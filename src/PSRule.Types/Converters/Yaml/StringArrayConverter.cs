// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml
{
    /// <summary>
    /// A YAML converter for deserializing a string array.
    /// </summary>
    public sealed class StringArrayConverter : IYamlTypeConverter
    {
        /// <inheritdoc/>
        bool IYamlTypeConverter.Accepts(Type type)
        {
            return type == typeof(string[]);
        }

        /// <inheritdoc/>
        object IYamlTypeConverter.ReadYaml(IParser parser, Type type)
        {
            if (parser.TryConsume<SequenceStart>(out _))
            {
                var result = new List<string>();
                while (parser.TryConsume<Scalar>(out var scalar))
                    result.Add(scalar.Value);

                parser.Consume<SequenceEnd>();
                return result.ToArray();
            }
            else if (parser.TryConsume<Scalar>(out var scalar))
            {
                return new string[] { scalar.Value };
            }
            return null;
        }

        /// <inheritdoc/>
        void IYamlTypeConverter.WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
