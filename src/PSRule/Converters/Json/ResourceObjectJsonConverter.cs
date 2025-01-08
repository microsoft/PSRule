// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Converters.Json;

#nullable enable

/// <summary>
/// A custom deserializer to convert JSON into a <see cref="ResourceObject"/>.
/// </summary>
internal sealed class ResourceObjectJsonConverter(IResourceDiscoveryContext context) : JsonConverter
{
    private const string FIELD_API_VERSION = "apiVersion";
    private const string FIELD_KIND = "kind";
    private const string FIELD_METADATA = "metadata";
    private const string FIELD_SPEC = "spec";
    private const string FIELD_SYNOPSIS = "Synopsis: ";

    public override bool CanRead => true;

    public override bool CanWrite => false;

    private readonly IResourceDiscoveryContext _Context = context;
    private readonly SpecFactory _Factory = new();

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ResourceObject);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var resource = MapResource(reader, serializer, out var apiVersion, out var kind);
        return new ResourceObject(resource, apiVersion, kind);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private IResource? MapResource(JsonReader reader, JsonSerializer serializer, out string? apiVersion, out string? kind)
    {
        reader.GetSourceExtent(_Context.Source, out var extent);
        reader.SkipComments(out _);
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

        IResource? result = null;
        apiVersion = null;
        kind = null;
        ResourceMetadata? metadata = null;
        CommentMetadata? comment = null;

        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value!.ToString();

                // Read apiVersion
                if (TryApiVersion(reader, propertyName, apiVersion: out var apiVersionValue))
                {
                    apiVersion = apiVersionValue;
                }

                // Read kind
                else if (TryKind(reader, propertyName, kind: out var kindValue))
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
                else if (kind != null && apiVersion != null && TrySpec(
                    reader: reader,
                    serializer: serializer,
                    propertyName: propertyName,
                    apiVersion: apiVersion,
                    kind: kind,
                    metadata: metadata,
                    comment: comment,
                    extent: extent,
                    spec: out var specValue))
                {
                    result = specValue;

                    // Break out of loop if result is populated
                    // Needed so we don't read more than we have to
                    if (result != null && reader.TokenType == JsonToken.EndObject)
                        break;
                }
                else
                {
                    reader.Skip();
                }
            }

            else if (reader.TokenType == JsonToken.Comment)
            {
                var commentLine = reader.Value!.ToString().TrimStart();
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

    private static bool TryApiVersion(JsonReader reader, string propertyName, out string? apiVersion)
    {
        apiVersion = null;
        if (propertyName == FIELD_API_VERSION)
        {
            apiVersion = reader.ReadAsString();
            return true;
        }
        return false;
    }

    private static bool TryKind(JsonReader reader, string propertyName, out string? kind)
    {
        kind = null;
        if (propertyName == FIELD_KIND)
        {
            kind = reader.ReadAsString();
            return true;
        }
        return false;
    }

    private static bool TryMetadata(JsonReader reader, JsonSerializer serializer, string propertyName, out ResourceMetadata? metadata)
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
        ResourceMetadata? metadata,
        CommentMetadata? comment,
        ISourceExtent extent,
        out IResource? spec)
    {
        spec = null;
        if (propertyName == FIELD_SPEC && _Factory.TryDescriptor(apiVersion: apiVersion, name: kind, descriptor: out var descriptor))
        {
            if (reader.Read() && reader.TokenType == JsonToken.StartObject)
            {
                reader.SkipComments(out _);
                var deserializedSpec = serializer.Deserialize(reader, objectType: descriptor.SpecType);
                spec = descriptor.CreateInstance(
                    source: _Context.Source,
                    metadata: metadata,
                    comment: comment,
                    extent: extent,
                    spec: deserializedSpec
                );
                return true;
            }
        }
        return false;
    }
}

#nullable restore
