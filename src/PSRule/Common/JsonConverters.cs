// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PSRule.Configuration;
using PSRule.Converters;
using PSRule.Converters.Json;
using PSRule.Data;
using PSRule.Definitions.Baselines;
using PSRule.Emitters;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// A base <seealso cref="PSObject"/> converter.
/// </summary>
internal abstract class PSObjectBaseConverter : JsonConverter
{
    /// <summary>
    /// Skip JSON comments.
    /// </summary>
    protected static bool SkipComments(JsonReader reader)
    {
        var hasComments = false;
        while (reader.TokenType == JsonToken.Comment && reader.Read())
            hasComments = true;

        return hasComments;
    }

    protected static void ReadObject(PSObject value, JsonReader reader, bool bindTargetInfo, IFileInfo sourceInfo)
    {
        SkipComments(reader);
        var path = reader.Path;
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(Messages.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);

        string? name = null;
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
                    if (string.IsNullOrEmpty(name))
                    {
                        reader.Skip();
                    }
                    else if (name == PSRuleTargetInfo.PropertyName)
                    {
                        var targetInfo = ReadInfo(reader);
                        if (targetInfo != null)
                            value.SetTargetInfo(targetInfo);
                    }
                    break;

                case JsonToken.StartObject:
                    var item = new PSObject();
                    ReadObject(item, reader, bindTargetInfo: false, sourceInfo: null);
                    value.Properties.Add(new PSNoteProperty(name, value: item));
                    break;

                case JsonToken.StartArray:
                    var items = ReadArray(reader: reader);
                    value.Properties.Add(new PSNoteProperty(name, value: items));
                    break;

                case JsonToken.Comment:
                    break;

                default:
                    value.Properties.Add(new PSNoteProperty(name, value: reader.Value));
                    break;
            }
            if (!reader.Read() || reader.TokenType == JsonToken.None)
                throw new PipelineSerializationException(Messages.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);
        }
        if (bindTargetInfo)
        {
            value.UseTargetInfo(out var info);
            info.SetSource(sourceInfo?.Path, lineNumber, linePosition);
            if (string.IsNullOrEmpty(info.Path))
                info.Path = path;
        }
    }

    protected static PSRuleTargetInfo? ReadInfo(JsonReader reader)
    {
        if (!reader.Read() || reader.TokenType == JsonToken.None || reader.TokenType != JsonToken.StartObject)
            return null;

        var s = JsonSerializer.Create();
        return s.Deserialize<PSRuleTargetInfo>(reader);
    }

    protected static PSObject[] ReadArray(JsonReader reader)
    {
        SkipComments(reader);
        if (reader.TokenType != JsonToken.StartArray || !reader.Read())
            throw new PipelineSerializationException(Messages.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);

        var result = new List<PSObject>();

        // Read until the end of the array
        while (reader.TokenType != JsonToken.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var item = new PSObject();
                    ReadObject(item, reader, bindTargetInfo: false, sourceInfo: null);
                    result.Add(item);
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
                throw new PipelineSerializationException(Messages.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);
        }
        return result.ToArray();
    }
}

/// <summary>
/// A custom serializer to correctly convert PSObject properties to JSON instead of CLIXML.
/// </summary>
internal sealed class PSObjectJsonConverter : PSObjectBaseConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PSObject);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not PSObject obj)
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

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Create target object based on JObject
        var result = existingValue as PSObject ?? new PSObject();

        // Read tokens
        ReadObject(result, reader, bindTargetInfo: true, sourceInfo: null);
        return result;
    }

    /// <summary>
    /// Serialize a file system info object.
    /// </summary>
    private static bool WriteFileSystemInfo(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is not FileSystemInfo fileSystemInfo)
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
/// A custom serializer to convert PSObjects that may or may not be in a JSON array to an a PSObject array.
/// </summary>
internal sealed class PSObjectArrayJsonConverter : PSObjectBaseConverter
{
    private readonly IFileInfo _SourceInfo;

    public PSObjectArrayJsonConverter(IFileInfo sourceInfo)
    {
        _SourceInfo = sourceInfo;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PSObject[]);
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        SkipComments(reader);
        if (reader.TokenType == JsonToken.Comment && !reader.Read())
            return Array.Empty<PSObject>();

        if (reader.TokenType != JsonToken.StartObject && reader.TokenType != JsonToken.StartArray)
            throw new PipelineSerializationException(Messages.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);

        var parser = reader as JsonEmitterParser;
        var fileInfo = parser?.Info ?? _SourceInfo;

        var result = new List<PSObject>();
        var isArray = reader.TokenType == JsonToken.StartArray;

        if (isArray)
            reader.Read();

        while (reader.TokenType != JsonToken.None && (!isArray || (isArray && reader.TokenType != JsonToken.EndArray)))
        {
            if (SkipComments(reader))
                continue;

            var value = new PSObject();
            ReadObject(value, reader, bindTargetInfo: true, sourceInfo: fileInfo);
            result.Add(value);

            // Consume the EndObject token.
            reader.Read();
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

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue(Enum.GetName(typeof(ErrorCategory), value));
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
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

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        BaselineJsonSerializationMapper.MapBaseline(writer, serializer, value as Baseline);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A contract resolver to order properties alphabetically
/// </summary>
internal sealed class OrderedPropertiesContractResolver : DefaultContractResolver
{
    public OrderedPropertiesContractResolver() : base()
    {
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = true,
            OverrideSpecifiedNames = true
        };
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return base
            .CreateProperties(type, memberSerialization)
            .OrderBy(prop => prop.PropertyName)
            .OrderBy(prop => prop.Order)
            .ToList();
    }
}

/// <summary>
/// A JSON converter for de/serializing a field map.
/// </summary>
internal sealed class FieldMapJsonConverter : JsonConverter
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(FieldMap);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var fieldMap = existingValue as FieldMap ?? new FieldMap();
        ReadFieldMap(fieldMap, reader);
        return fieldMap;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not FieldMap map) return;

        writer.WriteStartObject();
        foreach (var field in map)
        {
            writer.WritePropertyName(field.Key);
            serializer.Serialize(writer, field.Value);
        }
        writer.WriteEndObject();
    }

    private static void ReadFieldMap(FieldMap map, JsonReader reader)
    {
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

        string? propertyName = null;
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
                    if (SkipComments(reader))
                        continue;

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

    /// <summary>
    /// Skip JSON comments.
    /// </summary>
    private static bool SkipComments(JsonReader reader)
    {
        var hasComments = false;
        while (reader.TokenType == JsonToken.Comment && reader.Read())
            hasComments = true;

        return hasComments;
    }
}

/// <summary>
/// A JSON converter that handles string to string array.
/// </summary>
internal sealed class StringArrayJsonConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(string[]).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TryConsume(JsonToken.StartArray))
        {
            reader.SkipComments(out _);
            var result = new List<string>();
            while (reader.TryConsume(JsonToken.String, out var s_object) && s_object is string s)
            {
                result.Add(s);
                reader.SkipComments(out _);
            }
            return result.ToArray();
        }
        else if (reader.TokenType == JsonToken.String && reader.Value is string s)
        {
            return new string[] { s };
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A converter for converting <see cref="EnumMap{T}"/> to/ from JSON.
/// </summary>
internal sealed class EnumMapJsonConverter<T> : JsonConverter where T : struct, Enum
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(EnumMap<T>).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var map = existingValue as EnumMap<T> ?? new EnumMap<T>();
        ReadMap(map, reader);
        return map;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static void ReadMap(EnumMap<T> map, JsonReader reader)
    {
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

        string? propertyName = null;
        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = reader.Value.ToString();
            }
            else if (reader.TokenType == JsonToken.String && TypeConverter.TryEnum<T>(reader.Value, convert: true, out var value) && value != null)
            {
                map.Add(propertyName, value.Value);
            }
            reader.Read();
        }
    }
}

internal sealed class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<string, TValue>) || objectType == typeof(IDictionary<string, TValue>);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        existingValue ??= new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
        serializer.Deserialize<Dictionary<string, TValue>>(reader);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
