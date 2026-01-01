// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Definitions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml;

/// <summary>
/// A YAML converter for de/serializing <see cref="StringArrayMap"/>.
/// </summary>
public sealed class StringMapYamlConverter<TValue> : IYamlTypeConverter where TValue : class
{
    /// <inheritdoc/>
    bool IYamlTypeConverter.Accepts(Type type)
    {
        return type.IsSubclassOf(typeof(StringMap<TValue>));
    }

    /// <inheritdoc/>
    object? IYamlTypeConverter.ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var result = new StringArrayMap();
        if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume<Scalar>(out var scalar))
            {
#pragma warning disable IDE0001
                var key = scalar.Value;
                if (parser.TryConsume<SequenceStart>(out _))
                {
                    var values = new List<string>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {

                        if (parser.TryConsume<Scalar>(out scalar))
                        {
                            values.Add(scalar.Value);
                        }
                    }
                    result[key] = [.. values];
                    parser.Require<SequenceEnd>();
                    parser.MoveNext();
                }
                else if (parser.TryConsume<Scalar>(out scalar))
                {
                    result[key] = [scalar.Value];
                }
#pragma warning restore IDE0001
            }
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        return result;
    }

    /// <inheritdoc/>
    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is not StringMap<TValue> map)
            return;

        emitter.Emit(new MappingStart());
        foreach (var field in map)
        {
            emitter.Emit(new Scalar(field.Key));
            emitter.Emit(new MappingStart());


            emitter.Emit(new MappingEnd());
        }
        emitter.Emit(new MappingEnd());
    }
}
