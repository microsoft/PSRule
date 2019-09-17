using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
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
        private readonly INamingConvention _NamingConvention;

        public FieldYamlTypeInspector()
        {
            _TypeResolver = new StaticTypeResolver();
            _NamingConvention = new CamelCaseNamingConvention();
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return GetPropertyDescriptor(type: type);
        }

        private IEnumerable<IPropertyDescriptor> GetPropertyDescriptor(Type type)
        {
            foreach (var f in SelectField(type: type))
            {
                yield return f;
            }

            foreach (var p in SelectProperty(type: type))
            {
                yield return p;
            }
        }

        private IEnumerable<Field> SelectField(Type type)
        {
            return type
                .GetRuntimeFields()
                .Where(f => !f.IsStatic && f.IsPublic)
                .Select(p => new Field(p, _TypeResolver, _NamingConvention));
        }

        private IEnumerable<Property> SelectProperty(Type type)
        {
            return type
                .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
                .Where(p => p.CanRead && p.Name != "TargetObject")
                .Select(p => new Property(p, _TypeResolver, _NamingConvention));
        }

        private sealed class Field : IPropertyDescriptor
        {
            private readonly FieldInfo _FieldInfo;
            private readonly ITypeResolver _TypeResolver;
            private readonly INamingConvention _NamingConvention;

            public Field(FieldInfo fieldInfo, ITypeResolver typeResolver, INamingConvention namingConvention)
            {
                _FieldInfo = fieldInfo;
                _TypeResolver = typeResolver;
                _NamingConvention = namingConvention;
                ScalarStyle = ScalarStyle.Any;
            }

            public string Name => _NamingConvention.Apply(_FieldInfo.Name);

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

        private sealed class Property : IPropertyDescriptor
        {
            private readonly PropertyInfo _PropertyInfo;
            private readonly ITypeResolver _TypeResolver;
            private readonly INamingConvention _NamingConvention;

            public Property(PropertyInfo propertyInfo, ITypeResolver typeResolver, INamingConvention namingConvention)
            {
                _PropertyInfo = propertyInfo;
                _TypeResolver = typeResolver;
                _NamingConvention = namingConvention;
                ScalarStyle = ScalarStyle.Any;
            }

            public string Name => _NamingConvention.Apply(_PropertyInfo.Name);

            public Type Type => _PropertyInfo.PropertyType;

            public Type TypeOverride { get; set; }

            public int Order { get; set; }

            public bool CanWrite => false;

            public ScalarStyle ScalarStyle { get; set; }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return _PropertyInfo.GetCustomAttributes(typeof(T), true).OfType<T>().FirstOrDefault();
            }

            public void Write(object target, object value)
            {
                throw new NotImplementedException();
            }

            public IObjectDescriptor Read(object target)
            {
                var propertyValue = _PropertyInfo.GetValue(target);
                var actualType = TypeOverride ?? _TypeResolver.Resolve(Type, propertyValue);
                return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
            }
        }
    }

    internal sealed class LanguageBlockDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer _Next;
        private readonly SpecFactory _Factory;

        public LanguageBlockDeserializer(INodeDeserializer next)
        {
            _Next = next;
            _Factory = new SpecFactory();
        }

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (typeof(ResourceObject).IsAssignableFrom(expectedType))
            {
                var comment = HostHelper.GetCommentMeta(PipelineContext.CurrentThread.Source.File.Path, parser.Current.Start.Line - 2, parser.Current.Start.Column);
                var resource = MapResource(parser, nestedObjectDeserializer, comment);
                value = new ResourceObject(resource);
                return true;
            }
            else
            {
                return _Next.Deserialize(parser, expectedType, nestedObjectDeserializer, out value);
            }
        }

        private IResource MapResource(IParser parser, Func<IParser, Type, object> nestedObjectDeserializer, CommentMetadata comment)
        {
            IResource result = null;
            string kind = null;
            ResourceMetadata metadata = null;

            if (parser.Accept<MappingStart>())
            {
                parser.MoveNext();
                while (!parser.Accept<MappingEnd>())
                {
                    // Read kind
                    var propertyName = parser.Allow<Scalar>().Value;

                    if (propertyName == "kind")
                    {
                        kind = parser.Allow<Scalar>().Value;
                    }
                    else if (propertyName == "metadata")
                    {
                        if (!TryMetadata(parser, nestedObjectDeserializer, out metadata))
                        {
                            parser.SkipThisAndNestedEvents();
                        }
                    }
                    else if (propertyName == "spec" && kind != null)
                    {
                        if (!TryResource(kind, parser, nestedObjectDeserializer, metadata, comment, out IResource resource))
                        {
                            parser.SkipThisAndNestedEvents();
                        }
                        result = resource;
                    }
                    else
                    {
                        parser.SkipThisAndNestedEvents();
                    }
                }
                parser.MoveNext();
            }
            return result;
        }

        private bool TryMetadata(IParser parser, Func<IParser, Type, object> nestedObjectDeserializer, out ResourceMetadata metadata)
        {
            metadata = null;
            if (parser.Accept<MappingStart>())
            {
                if (!_Next.Deserialize(parser, typeof(ResourceMetadata), nestedObjectDeserializer, out object value))
                    return false;

                metadata = (ResourceMetadata)value;
                return true;
            }
            return false;
        }

        private bool TryResource(string name, IParser parser, Func<IParser, Type, object> nestedObjectDeserializer, ResourceMetadata metadata, CommentMetadata comment, out IResource spec)
        {
            spec = null;
            if (_Factory.TryDescriptor(name, out ISpecDescriptor descriptor))
            {
                // Using object style
                if (parser.Accept<MappingStart>())
                {
                    if (!_Next.Deserialize(parser, descriptor.SpecType, nestedObjectDeserializer, out object value))
                        return false;

                    spec = descriptor.CreateInstance(PipelineContext.CurrentThread.Source.File, metadata, comment, value);
                    return true;
                }
            }
            return false;
        }
    }
}
