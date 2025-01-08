// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Host;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml;

#nullable enable

/// <summary>
/// A custom deserializer to convert YAML into a <see cref="ResourceObject"/>.
/// </summary>
internal sealed class ResourceNodeDeserializer(IResourceDiscoveryContext context, INodeDeserializer next) : INodeDeserializer
{
    private const string FIELD_API_VERSION = "apiVersion";
    private const string FIELD_KIND = "kind";
    private const string FIELD_METADATA = "metadata";
    private const string FIELD_SPEC = "spec";

    private readonly IResourceDiscoveryContext _Context = context;
    private readonly INodeDeserializer _Next = next;
    private readonly SpecFactory _Factory = new();

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
    {
        if (typeof(ResourceObject).IsAssignableFrom(expectedType))
        {
            var comment = reader.Current == null ? null : GetCommentMetadata(reader.Current.Start.Line - 2, reader.Current.Start.Column);
            var resource = MapResource(reader, nestedObjectDeserializer, comment, rootDeserializer, out var apiVersion, out var kind);
            value = new ResourceObject(resource, apiVersion, kind);
            return true;
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }
    }

    private IResource? MapResource(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, CommentMetadata? comment, ObjectDeserializer rootDeserializer, out string? apiVersion, out string? kind)
    {
        IResource? result = null;
        apiVersion = null;
        kind = null;
        ResourceMetadata? metadata = null;
        if (reader.TryConsume<MappingStart>(out var mappingStart) && mappingStart != null)
        {
            var extent = GetSourceExtent(mappingStart.Start.Line, mappingStart.Start.Column);
            while (reader.TryConsume<Scalar>(out var scalar) && scalar != null)
            {
                // Read apiVersion
                if (TryApiVersion(reader, scalar, out var apiVersionValue))
                {
                    apiVersion = apiVersionValue;
                }
                // Read kind
                else if (TryKind(reader, scalar, out var kindValue))
                {
                    kind = kindValue;
                }
                // Read metadata
                else if (TryMetadata(reader, scalar, nestedObjectDeserializer, out var metadataValue, rootDeserializer))
                {
                    metadata = metadataValue;
                }
                // Read spec
                else if (kind != null && apiVersion != null && TrySpec(reader, scalar, apiVersion, kind, nestedObjectDeserializer, metadata, comment, extent, out var resource, rootDeserializer))
                {
                    result = resource;
                }
                else
                {
                    reader.SkipThisAndNestedEvents();
                }
            }
            reader.Require<MappingEnd>();
            reader.MoveNext();
        }
        return result;
    }

    private static bool TryApiVersion(IParser reader, Scalar scalar, out string? apiVersion)
    {
        apiVersion = null;
        if (scalar.Value == FIELD_API_VERSION)
        {
            apiVersion = reader.Consume<Scalar>().Value;
            return true;
        }
        return false;
    }

    private static bool TryKind(IParser reader, Scalar scalar, out string? kind)
    {
        kind = null;
        if (scalar.Value == FIELD_KIND)
        {
            kind = reader.Consume<Scalar>().Value;
            return true;
        }
        return false;
    }

    private CommentMetadata? GetCommentMetadata(long line, long column)
    {
        return _Context == null || _Context.Source == null ? null : HostHelper.GetCommentMeta(_Context.Source, (int)line, (int)column);
    }

    private SourceExtent GetSourceExtent(long? line, long? column)
    {
        return new SourceExtent(_Context.Source, (int?)line, (int?)column);
    }

    private bool TryMetadata(IParser reader, Scalar scalar, Func<IParser, Type, object?> nestedObjectDeserializer, out ResourceMetadata? metadata, ObjectDeserializer rootDeserializer)
    {
        metadata = null;
        if (scalar.Value != FIELD_METADATA)
            return false;

        if (reader.Current is MappingStart)
        {
            if (!_Next.Deserialize(reader, typeof(ResourceMetadata), nestedObjectDeserializer, out var value, rootDeserializer) || value is not ResourceMetadata metadata_value)
                return false;

            metadata = metadata_value;
            return true;
        }
        return false;
    }

    private bool TrySpec(IParser reader, Scalar scalar, string apiVersion, string kind, Func<IParser, Type, object?> nestedObjectDeserializer, ResourceMetadata? metadata, CommentMetadata? comment, ISourceExtent extent, out IResource? spec, ObjectDeserializer rootDeserializer)
    {
        spec = null;
        return scalar.Value == FIELD_SPEC && TryResource(reader, apiVersion, kind, nestedObjectDeserializer, metadata, comment, extent, out spec, rootDeserializer);
    }

    private bool TryResource(IParser reader, string apiVersion, string kind, Func<IParser, Type, object?> nestedObjectDeserializer, ResourceMetadata? metadata, CommentMetadata? comment, ISourceExtent extent, out IResource? spec, ObjectDeserializer rootDeserializer)
    {
        spec = null;
        if (_Factory.TryDescriptor(apiVersion, kind, out var descriptor) && reader.Current is MappingStart)
        {
            if (!_Next.Deserialize(reader, descriptor.SpecType, nestedObjectDeserializer, out var value, rootDeserializer))
                return false;

            spec = descriptor.CreateInstance(extent.File, metadata, comment, extent, value);
            return true;
        }
        return false;
    }
}

#nullable restore
