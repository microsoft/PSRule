// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule
{
    /// <summary>
    /// A custom serializer to correctly convert PSObject properties to JSON instead of CLIXML.
    /// </summary>
    internal sealed class PSObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PSObject);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is PSObject obj))
                throw new ArgumentException(message: PSRuleResources.SerializeNullPSObject, paramName: nameof(value));

            if (WriteFileSystemInfo(writer, value, serializer) || WriteBaseObject(writer, obj, serializer))
                return;

            writer.WriteStartObject();
            foreach (var property in obj.Properties)
            {
                // Ignore properties that are not readable or can cause race condition
                if (!property.IsGettable || property.Value is PSDriveInfo || property.Value is ProviderInfo || property.Value is DirectoryInfo)
                    continue;

                writer.WritePropertyName(property.Name);
                serializer.Serialize(writer, property.Value);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Create target object based on JObject
            var result = existingValue as PSObject ?? new PSObject();

            // Read tokens
            ReadObject(value: result, reader: reader);
            return result;
        }

        private void ReadObject(PSObject value, JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

            reader.Read();
            string name = null;

            // Read each token
            while (reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        name = reader.Value.ToString();
                        break;

                    case JsonToken.StartObject:
                        var child = new PSObject();
                        ReadObject(value: child, reader: reader);
                        value.Properties.Add(new PSNoteProperty(name: name, value: child));
                        break;

                    case JsonToken.StartArray:
                        var items = new List<PSObject>();
                        reader.Read();
                        var item = new PSObject();

                        while (reader.TokenType != JsonToken.EndArray)
                        {
                            ReadObject(value: item, reader: reader);
                            items.Add(item);
                            reader.Read();
                        }

                        value.Properties.Add(new PSNoteProperty(name: name, value: items.ToArray()));
                        break;

                    case JsonToken.Comment:
                        break;

                    default:
                        value.Properties.Add(new PSNoteProperty(name: name, value: reader.Value));
                        break;
                }
                reader.Read();
            }
        }

        /// <summary>
        /// Serialize a file system info object.
        /// </summary>
        private static bool WriteFileSystemInfo(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is FileSystemInfo fileSystemInfo))
                return false;

            serializer.Serialize(writer, fileSystemInfo.FullName);
            return true;
        }

        /// <summary>
        /// Serialize the base object.
        /// </summary>
        private static bool WriteBaseObject(JsonWriter writer, PSObject value, JsonSerializer serializer)
        {
            if (value.BaseObject == null || value.HasNoteProperty())
                return false;

            serializer.Serialize(writer, value.BaseObject);
            return true;
        }
    }

    /// <summary>
    /// A custom serializer to convert PSObjects that may or maynot be in a JSON array to an a PSObject array.
    /// </summary>
    internal sealed class PSObjectArrayJsonConverter : JsonConverter
    {
        private readonly TargetSourceInfo _SourceInfo;

        public PSObjectArrayJsonConverter(TargetSourceInfo sourceInfo)
        {
            _SourceInfo = sourceInfo;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PSObject[]);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject && reader.TokenType != JsonToken.StartArray)
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

            var result = new List<PSObject>();
            var isArray = reader.TokenType == JsonToken.StartArray;

            if (isArray)
                reader.Read();

            while (reader.TokenType != JsonToken.None && (!isArray || (isArray && reader.TokenType != JsonToken.EndArray)))
            {
                var value = ReadObject(reader, bindTargetInfo: true, _SourceInfo);
                result.Add(value);

                // Consume the EndObject token
                reader.Read();
            }
            return result.ToArray();
        }

        private static PSObject ReadObject(JsonReader reader, bool bindTargetInfo, TargetSourceInfo sourceInfo)
        {
            if (reader.TokenType != JsonToken.StartObject || !reader.Read())
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

            var result = new PSObject();
            string name = null;
            var lineNumber = 0;
            var linePosition = 0;

            if (bindTargetInfo && reader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo())
            {
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }

            // Read each token
            while (reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        name = reader.Value.ToString();
                        if (name == PSRuleTargetInfo.PropertyName)
                        {
                            var targetInfo = ReadInfo(reader);
                            if (targetInfo != null)
                                result.SetTargetInfo(targetInfo);
                        }
                        break;

                    case JsonToken.StartObject:
                        var value = ReadObject(reader, bindTargetInfo: false, sourceInfo: null);
                        result.Properties.Add(new PSNoteProperty(name, value: value));
                        break;

                    case JsonToken.StartArray:
                        var items = ReadArray(reader: reader);
                        result.Properties.Add(new PSNoteProperty(name, value: items));
                        break;

                    case JsonToken.Comment:
                        break;

                    default:
                        result.Properties.Add(new PSNoteProperty(name, value: reader.Value));
                        break;
                }
                if (!reader.Read() || reader.TokenType == JsonToken.None)
                    throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);
            }
            if (bindTargetInfo)
            {
                result.UseTargetInfo(out var info);
                info.SetSource(sourceInfo?.File, lineNumber, linePosition);
            }
            return result;
        }

        private static PSRuleTargetInfo ReadInfo(JsonReader reader)
        {
            if (!reader.Read() || reader.TokenType == JsonToken.None || reader.TokenType != JsonToken.StartObject)
                return null;

            var s = JsonSerializer.Create();
            return s.Deserialize<PSRuleTargetInfo>(reader);
        }

        private static PSObject[] ReadArray(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartArray || !reader.Read())
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

            var result = new List<PSObject>();

            // Read until the end of the array
            while (reader.TokenType != JsonToken.EndArray)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        result.Add(ReadObject(reader, bindTargetInfo: false, sourceInfo: null));
                        break;

                    case JsonToken.StartArray:
                        result.Add(PSObject.AsPSObject(ReadArray(reader)));
                        break;

                    case JsonToken.Null:
                        result.Add(null);
                        break;

                    case JsonToken.Comment:
                        break;

                    default:
                        result.Add(PSObject.AsPSObject(reader.Value));
                        break;
                }
                if (!reader.Read() || reader.TokenType == JsonToken.None)
                    throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// A custom serializer to convert ErrorCategory to a string.
    /// </summary>
    internal sealed class ErrorCategoryJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ErrorCategory);
        }

        public override bool CanWrite => true;

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Enum.GetName(typeof(ErrorCategory), value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A custom serializer to convert Baseline object to JSON
    /// </summary>
    internal sealed class BaselineJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Baseline);
        }

        public override bool CanWrite => true;

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BaselineJsonSerializationMapper.MapBaseline(writer, serializer, value as Baseline);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A contract resolver to order properties alphabetically
    /// </summary>
    internal sealed class OrderedPropertiesContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base
                .CreateProperties(type, memberSerialization)
                .OrderBy(prop => prop.PropertyName)
                .ToList();
        }
    }

    /// <summary>
    /// A customer deserializer to convert JSON into ResourceObject
    /// </summary>
    internal sealed class ResourceObjectJsonConverter : JsonConverter
    {
        private const string FIELD_APIVERSION = "apiVersion";
        private const string FIELD_KIND = "kind";
        private const string FIELD_METADATA = "metadata";
        private const string FIELD_SPEC = "spec";
        private const string FIELD_SYNOPSIS = "Synopsis: ";

        public override bool CanRead => true;

        public override bool CanWrite => false;

        private readonly SpecFactory _Factory;

        public ResourceObjectJsonConverter()
        {
            _Factory = new SpecFactory();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ResourceObject);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var resource = MapResource(reader, serializer);
            return new ResourceObject(resource);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private IResource MapResource(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            {
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);
            }

            IResource result = null;
            string apiVersion = null;
            string kind = null;
            ResourceMetadata metadata = null;
            CommentMetadata comment = null;

            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value.ToString();

                    // Read apiVersion
                    if (TryApiVersion(reader: reader, propertyName: propertyName, apiVersion: out var apiVersionValue))
                    {
                        apiVersion = apiVersionValue;
                    }

                    // Read kind
                    else if (TryKind(reader: reader, propertyName: propertyName, kind: out var kindValue))
                    {
                        kind = kindValue;
                    }

                    // Read metadata
                    else if (TryMetadata(
                        reader: reader,
                        serializer: serializer,
                        propertyName: propertyName,
                        metadata: out var metadataValue))
                    {
                        metadata = metadataValue;
                    }

                    // Try Spec
                    else if (kind != null && TrySpec(
                        reader: reader,
                        serializer: serializer,
                        propertyName: propertyName,
                        apiVersion: apiVersion,
                        kind: kind,
                        metadata: metadata,
                        comment: comment,
                        spec: out var specValue))
                    {
                        result = specValue;

                        // Break out of loop if result is populated
                        // Needed so we don't read more than we have to
                        if (result != null && reader.TokenType == JsonToken.EndObject)
                        {
                            break;
                        }
                    }

                    else
                    {
                        reader.Skip();
                    }
                }

                else if (reader.TokenType == JsonToken.Comment)
                {
                    var commentLine = reader.Value.ToString().TrimStart();

                    if (commentLine.Length > FIELD_SYNOPSIS.Length && commentLine.StartsWith(FIELD_SYNOPSIS))
                    {
                        comment = new CommentMetadata
                        {
                            Synopsis = commentLine.Substring(FIELD_SYNOPSIS.Length)
                        };
                    }
                }

                reader.Read();
            }

            return result;
        }

        private static bool TryApiVersion(JsonReader reader, string propertyName, out string apiVersion)
        {
            apiVersion = null;

            if (propertyName == FIELD_APIVERSION)
            {
                apiVersion = reader.ReadAsString();
                return true;
            }

            return false;
        }

        private static bool TryKind(JsonReader reader, string propertyName, out string kind)
        {
            kind = null;

            if (propertyName == FIELD_KIND)
            {
                kind = reader.ReadAsString();
                return true;
            }

            return false;
        }

        private static bool TryMetadata(JsonReader reader, JsonSerializer serializer, string propertyName, out ResourceMetadata metadata)
        {
            metadata = null;

            if (propertyName == FIELD_METADATA)
            {
                if (reader.Read() && reader.TokenType == JsonToken.StartObject)
                {
                    metadata = serializer.Deserialize<ResourceMetadata>(reader);
                    return true;
                }
            }

            return false;
        }

        private bool TrySpec(
            JsonReader reader,
            JsonSerializer serializer,
            string propertyName,
            string apiVersion,
            string kind,
            ResourceMetadata metadata,
            CommentMetadata comment,
            out IResource spec)
        {
            spec = null;

            if (propertyName == FIELD_SPEC && _Factory.TryDescriptor(
                apiVersion: apiVersion,
                name: kind,
                descriptor: out var descriptor))
            {
                if (reader.Read() && reader.TokenType == JsonToken.StartObject)
                {
                    var deserializedSpec = serializer.Deserialize(reader, objectType: descriptor.SpecType);

                    spec = descriptor.CreateInstance(
                        source: RunspaceContext.CurrentThread.Source.File,
                        metadata: metadata,
                        comment: comment,
                        spec: deserializedSpec
                    );

                    if (string.IsNullOrEmpty(apiVersion))
                    {
                        spec.SetApiVersionIssue();
                    }

                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// A JSON converter for de/serializing a field map.
    /// </summary>
    internal sealed class FieldMapJsonConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FieldMap);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var fieldMap = existingValue as FieldMap ?? new FieldMap();

            ReadFieldMap(fieldMap, reader);

            return fieldMap;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static void ReadFieldMap(FieldMap map, JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            {
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);
            }

            string propertyName = null;

            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    propertyName = reader.Value.ToString();
                }

                else if (reader.TokenType == JsonToken.StartArray)
                {
                    var items = new List<string>();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        var item = reader.ReadAsString();

                        if (!string.IsNullOrEmpty(item))
                        {
                            items.Add(item);
                        }
                    }

                    map.Set(propertyName, items.ToArray());
                }

                reader.Read();
            }
        }
    }

    /// <summary>
    /// A JSON converter for deserializing Language Expressions
    /// </summary>
    internal sealed class LanguageExpressionJsonConverter : JsonConverter
    {
        private const string OPERATOR_IF = "if";

        public override bool CanRead => true;

        public override bool CanWrite => false;

        private readonly LanguageExpressionFactory _Factory;

        public LanguageExpressionJsonConverter()
        {
            _Factory = new LanguageExpressionFactory();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(LanguageExpression).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var expression = MapOperator(OPERATOR_IF, reader);
            return new LanguageIf(expression);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private LanguageExpression MapOperator(string type, JsonReader reader)
        {
            if (TryExpression(type, out LanguageOperator result))
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    result.Add(MapExpression(reader));
                    reader.Read();
                }

                else if (reader.TokenType == JsonToken.StartArray && reader.Read())
                {
                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        result.Add(MapExpression(reader));
                        reader.Read();
                    }
                    reader.Read();
                }
            }

            return result;
        }

        private LanguageExpression MapCondition(string type, LanguageExpression.PropertyBag properties, JsonReader reader)
        {
            if (TryExpression(type, out LanguageCondition result))
            {
                while (reader.TokenType != JsonToken.EndObject)
                {
                    MapProperty(properties, reader, out _);
                }
                result.Add(properties);
            }
            return result;
        }

        private LanguageExpression MapExpression(JsonReader reader)
        {
            LanguageExpression result = null;

            var properties = new LanguageExpression.PropertyBag();

            MapProperty(properties, reader, out var key);

            if (key != null && TryCondition(key))
            {
                result = MapCondition(key, properties, reader);
            }

            else if (TryOperator(key) &&
                (reader.TokenType == JsonToken.StartObject ||
                 reader.TokenType == JsonToken.StartArray))
            {
                result = MapOperator(key, reader);
            }

            return result;
        }

        private void MapProperty(LanguageExpression.PropertyBag properties, JsonReader reader, out string name)
        {
            if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            {
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);
            }

            name = null;

            while (reader.TokenType != JsonToken.EndObject)
            {
                var key = reader.Value.ToString();

                if (TryCondition(key) || TryOperator(key))
                {
                    name = key;
                }

                if (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        break;
                    }

                    else if (reader.TokenType == JsonToken.StartArray)
                    {
                        if (!TryCondition(key))
                        {
                            break;
                        }

                        var objects = new List<string>();

                        while (reader.TokenType != JsonToken.EndArray)
                        {
                            var value = reader.ReadAsString();

                            if (!string.IsNullOrEmpty(value))
                            {
                                objects.Add(value);
                            }
                        }

                        properties.Add(key, objects.ToArray());
                    }

                    else
                    {
                        properties.Add(key, reader.Value);
                    }
                }

                reader.Read();
            }
        }

        private bool TryOperator(string key)
        {
            return _Factory.IsOperator(key);
        }

        private bool TryCondition(string key)
        {
            return _Factory.IsCondition(key);
        }

        private bool TryExpression<T>(string type, out T expression) where T : LanguageExpression
        {
            expression = null;

            if (_Factory.TryDescriptor(type, out var descriptor))
            {
                expression = (T)descriptor.CreateInstance(
                    source: RunspaceContext.CurrentThread.Source.File,
                    properties: null
                );

                return expression != null;
            }

            return false;
        }
    }
}
