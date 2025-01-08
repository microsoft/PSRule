// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Options;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml;

/// <summary>
/// a YAML converter for <see cref="CapabilityOption"/>.
/// </summary>
public sealed class CapabilityOptionYamlConverter : IYamlTypeConverter
{
    /// <inheritdoc/>
    bool IYamlTypeConverter.Accepts(Type type)
    {
        return type == typeof(CapabilityOption);
    }

    /// <inheritdoc/>
    object? IYamlTypeConverter.ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<SequenceStart>(out _))
        {
            var items = new List<string>();
            while (parser.TryConsume<Scalar>(out var scalar))
            {
                items.Add(scalar.Value);
            }

            parser.Consume<SequenceEnd>();
            return new CapabilityOption
            {
                Items = [.. items]
            };
        }
        return null;
    }

    /// <inheritdoc/>
    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
        if (value is CapabilityOption option && option.Items != null)
        {
            foreach (var item in option.Items)
            {
                emitter.Emit(new Scalar(item));
            }
        }
        emitter.Emit(new SequenceEnd());
    }
}
