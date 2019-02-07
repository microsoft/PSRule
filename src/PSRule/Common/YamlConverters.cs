using PSRule.Configuration;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule
{
    /// <summary>
    /// A YAML converter that allows short and full notation of suppression rules.
    /// </summary>
    internal sealed class SuppressionRuleYamlTypeConverter : IYamlTypeConverter
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

    /// <summary>
    /// A YAML converter to deserialize a map/ object as a PSObject.
    /// </summary>
    internal sealed class PSObjectYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(PSObject);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var result = new PSObject();

            if (parser.Accept<MappingStart>())
            {
                parser.MoveNext();

                while (!parser.Accept<MappingEnd>())
                {
                    var name = parser.Allow<Scalar>().Value;

                    if (parser.Accept<SequenceStart>())
                    {
                        parser.MoveNext();

                        var values = new List<object>();

                        while (!parser.Accept<SequenceEnd>())
                        {
                            if (parser.Accept<MappingStart>())
                            {
                                values.Add(ReadYaml(parser, type));
                            }
                            else if (parser.Accept<Scalar>())
                            {
                                values.Add(parser.Allow<Scalar>().Value);
                            }
                        }

                        result.Properties.Add(new PSNoteProperty(name, values.ToArray()));

                        parser.MoveNext();
                    }
                    else if (parser.Accept<MappingStart>())
                    {
                        var value = ReadYaml(parser, type);
                        result.Properties.Add(new PSNoteProperty(name, value));
                    }
                    else if (parser.Accept<Scalar>())
                    {
                        result.Properties.Add(new PSNoteProperty(name, parser.Allow<Scalar>().Value));
                    }
                    else
                    {
                        throw new NotImplementedException();
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

    /// <summary>
    /// A YAML resolver to convert any dictionary types to PSObjects instead.
    /// </summary>
    internal sealed class PSObjectYamlTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (currentType == typeof(Dictionary<object, object>))
            {
                currentType = typeof(PSObject);

                return true;
            }

            if (nodeEvent is MappingStart)
            {
                currentType = typeof(PSObject);

                return true;
            }

            return false;
        }
    }
}
