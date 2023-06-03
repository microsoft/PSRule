// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PSRule.Data;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml
{
    /// <summary>
    /// A YAML converter for de/serializing <see cref="StringArrayMap"/>.
    /// </summary>
    public sealed class StringArrayMapConverter : IYamlTypeConverter
    {
        /// <inheritdoc/>
        bool IYamlTypeConverter.Accepts(Type type)
        {
            return type == typeof(StringArrayMap);
        }

        /// <inheritdoc/>
        object IYamlTypeConverter.ReadYaml(IParser parser, Type type)
        {
            var result = new StringArrayMap();
            if (parser.TryConsume<MappingStart>(out _))
            {
                while (parser.TryConsume(out Scalar scalar))
                {
                    var key = scalar.Value;
                    if (parser.TryConsume<SequenceStart>(out _))
                    {
                        var values = new List<string>();
                        while (!parser.Accept<SequenceEnd>(out _))
                        {
                            if (parser.TryConsume(out scalar))
                                values.Add(scalar.Value);
                        }
                        result[key] = values.ToArray();
                        parser.Require<SequenceEnd>();
                        parser.MoveNext();
                    }
                    else if (parser.TryConsume(out scalar))
                    {
                        result[key] = new string[] { scalar.Value };
                    }
                }
                parser.Require<MappingEnd>();
                parser.MoveNext();
            }
            return result;
        }

        /// <inheritdoc/>
        void IYamlTypeConverter.WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (type == typeof(StringArrayMap) && value == null)
            {
                emitter.Emit(new MappingStart());
                emitter.Emit(new MappingEnd());
            }
            if (value is not StringArrayMap map)
                return;

            emitter.Emit(new MappingStart());
            foreach (var field in map)
            {
                emitter.Emit(new Scalar(field.Key));
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                for (var i = 0; i < field.Value.Length; i++)
                {
                    emitter.Emit(new Scalar(field.Value[i]));
                }
                emitter.Emit(new SequenceEnd());
            }
            emitter.Emit(new MappingEnd());
        }
    }
}
