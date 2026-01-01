// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml;

/// <summary>
/// A type converter for <see cref="ResourceIdReference"/>.
/// </summary>
internal sealed class ResourceIdReferenceYamlConverter() : IYamlTypeConverter
{
    /// <inheritdoc/>
    public bool Accepts(Type type)
    {
        return type == typeof(ResourceIdReference) || type == typeof(ResourceIdReference?) || type == typeof(ResourceIdReference[]);
    }

    /// <inheritdoc/>
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<Scalar>(out var scalar) && scalar != null && scalar.Value != null)
        {
            if (!ConvertOne(scalar, out var value) || value == null)
                return null;

            return type == typeof(ResourceIdReference) ? value.Value : value;
        }

        if (type == typeof(ResourceIdReference[]) && parser.TryConsume<SequenceStart>(out _))
        {
            var list = new List<ResourceIdReference>();

            while (parser.TryConsume<Scalar>(out scalar))
            {
                if (ConvertOne(scalar, out var value) && value != null)
                {
                    list.Add(value.Value);
                }
            }

            parser.Consume<SequenceEnd>();

            return list.ToArray();
        }

        return null;
    }

    /// <inheritdoc/>
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is ResourceIdReference reference)
        {
            emitter.Emit(new Scalar(reference.Raw));
        }
        else if (value is ResourceIdReference[] references)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (var r in references)
            {
                emitter.Emit(new Scalar(r.Raw));
            }

            emitter.Emit(new SequenceEnd());
        }
    }

    private static bool ConvertOne(Scalar value, out ResourceIdReference? result)
    {
        return ResourceIdReference.TryParse(value.Value, out result);
    }
}
