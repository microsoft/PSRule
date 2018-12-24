using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Configuration
{
    /// <summary>
    /// A YAML converter that allows short and full notation of suppression rules.
    /// </summary>
    internal sealed class SuppressionRuleConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(SuppressionRule);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var result = new SuppressionRule();

            if (parser.Accept<SequenceStart>())
            {
                parser.MoveNext();

                var targetNames = new List<string>();

                while (!parser.Accept<SequenceEnd>())
                {
                    targetNames.Add(parser.Allow<Scalar>().Value);
                }

                result.TargetName = targetNames.ToArray();

                parser.MoveNext();
            }
            else if (parser.Accept<MappingStart>())
            {
                parser.MoveNext();

                while (!parser.Accept<MappingEnd>())
                {
                    var name = parser.Allow<Scalar>().Value;

                    if (name == "targetName" && parser.Accept<SequenceStart>())
                    {
                        parser.MoveNext();

                        var targetNames = new List<string>();

                        while (!parser.Accept<SequenceEnd>())
                        {
                            targetNames.Add(parser.Allow<Scalar>().Value);
                        }

                        result.TargetName = targetNames.ToArray();

                        parser.MoveNext();
                    }
                }

                parser.MoveNext();
            }

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
