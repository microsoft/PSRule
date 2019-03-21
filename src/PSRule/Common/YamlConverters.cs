using PSRule.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

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
            // Handle empty objects
            if (parser.Accept<Scalar>())
            {
                parser.Allow<Scalar>();
                return null;
            }

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

                        var values = new List<PSObject>();

                        while (!parser.Accept<SequenceEnd>())
                        {
                            if (parser.Accept<MappingStart>())
                            {
                                values.Add(PSObject.AsPSObject(ReadYaml(parser, type)));
                            }
                            else if (parser.Accept<Scalar>())
                            {
                                values.Add(PSObject.AsPSObject(parser.Allow<Scalar>().Value));
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

    /// <summary>
    /// A YAML type inspector to read fields and properties from a type for serialization.
    /// </summary>
    internal sealed class FieldYamlTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeResolver _TypeResolver;

        public FieldYamlTypeInspector()
        {
            _TypeResolver = new StaticTypeResolver();
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return type
                .GetRuntimeFields().Where(f => !f.IsStatic && f.IsPublic)
                .Select(p => new FieldDescriptor(p, _TypeResolver));
        }

        private sealed class FieldDescriptor : IPropertyDescriptor
        {
            private readonly FieldInfo _FieldInfo;
            private readonly ITypeResolver _TypeResolver;

            public FieldDescriptor(FieldInfo fieldInfo, ITypeResolver typeResolver)
            {
                _FieldInfo = fieldInfo;
                _TypeResolver = typeResolver;
                ScalarStyle = ScalarStyle.Any;
            }

            public string Name => _FieldInfo.Name;

            public Type Type => _FieldInfo.FieldType;

            public Type TypeOverride { get; set; }

            public int Order { get; set; }

            public bool CanWrite => false;

            public ScalarStyle ScalarStyle { get; set; }

            public void Write(object target, object value)
            {
                throw new NotImplementedException();
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return _FieldInfo.GetCustomAttributes(typeof(T), true).OfType<T>().FirstOrDefault();
            }

            public IObjectDescriptor Read(object target)
            {
                var propertyValue = _FieldInfo.GetValue(target);
                var actualType = TypeOverride ?? _TypeResolver.Resolve(Type, propertyValue);
                return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
            }
        }
    }
}
