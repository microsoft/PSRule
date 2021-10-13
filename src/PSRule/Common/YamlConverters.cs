// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Expressions;
using PSRule.Host;
using PSRule.Runtime;
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
            if (parser.TryConsume<SequenceStart>(out _))
            {
                var targetNames = new List<string>();
                while (parser.TryConsume(out Scalar scalar))
                    targetNames.Add(scalar.Value);

                result.TargetName = targetNames.ToArray();
                parser.MoveNext();
            }
            else if (parser.TryConsume<MappingStart>(out _))
            {
                while (parser.TryConsume(out Scalar scalar))
                {
                    var name = scalar.Value;
                    if (name == "targetName" && parser.TryConsume<SequenceStart>(out _))
                    {
                        var targetNames = new List<string>();
                        while (parser.TryConsume(out Scalar item))
                            targetNames.Add(item.Value);

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
    /// A YAML converter for de/serializing a field map.
    /// </summary>
    internal sealed class FieldMapYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(FieldMap);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var result = new FieldMap();
            if (parser.TryConsume<MappingStart>(out _))
            {
                while (parser.TryConsume(out Scalar scalar))
                {
                    var fieldName = scalar.Value;
                    if (parser.TryConsume<SequenceStart>(out _))
                    {
                        var fields = new List<string>();
                        while (!parser.Accept<SequenceEnd>(out _))
                        {
                            if (parser.TryConsume<Scalar>(out scalar))
                                fields.Add(scalar.Value);
                        }
                        result.Set(fieldName, fields.ToArray());
                        parser.Require<SequenceEnd>();
                        parser.MoveNext();
                    }
                }
                parser.Require<MappingEnd>();
                parser.MoveNext();
            }
            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (type == typeof(FieldMap) && value == null)
            {
                emitter.Emit(new MappingStart());
                emitter.Emit(new MappingEnd());
            }
            if (!(value is FieldMap map))
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

    /// <summary>
    /// A YAML converter to deserialize a map/ object as a PSObject.
    /// </summary>
    internal sealed class PSObjectYamlTypeConverter : MappingTypeConverter, IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(PSObject);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            // Handle empty objects
            if (parser.TryConsume(out Scalar scalar))
            {
                return PSObject.AsPSObject(scalar.Value);
            }

            var result = new PSObject();
            if (parser.TryConsume<MappingStart>(out _))
            {
                while (parser.TryConsume(out scalar))
                {
                    var name = scalar.Value;
                    var property = ReadNoteProperty(parser, name);
                    if (property == null)
                        throw new NotImplementedException();

                    result.Properties.Add(property);
                }
                parser.Require<MappingEnd>();
                parser.MoveNext();
            }
            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            Map(emitter, value);
        }

        private PSNoteProperty ReadNoteProperty(IParser parser, string name)
        {
            if (parser.TryConsume<SequenceStart>(out _))
            {
                var values = new List<PSObject>();
                while (!(parser.Current is SequenceEnd))
                {
                    if (parser.Current is MappingStart)
                    {
                        values.Add(PSObject.AsPSObject(ReadYaml(parser, typeof(PSObject))));
                    }
                    else if (parser.TryConsume(out Scalar scalar))
                    {
                        values.Add(PSObject.AsPSObject(scalar.Value));
                    }
                }
                parser.Require<SequenceEnd>();
                parser.MoveNext();
                return new PSNoteProperty(name, values.ToArray());
            }
            else if (parser.Current is MappingStart)
            {
                return new PSNoteProperty(name, ReadYaml(parser, typeof(PSObject)));
            }
            else if (parser.TryConsume(out Scalar scalar))
            {
                return new PSNoteProperty(name, scalar.Value);
            }
            return null;
        }
    }

    internal abstract class MappingTypeConverter
    {
        protected void Map(IEmitter emitter, object value)
        {
            emitter.Emit(new MappingStart());
            foreach (var kv in GetKV(value))
            {
                emitter.Emit(new Scalar(kv.Key));
                Primitive(emitter, kv.Value);
            }
            emitter.Emit(new MappingEnd());
        }

        protected void Primitive(IEmitter emitter, object value)
        {
            if (value == null)
                return;

            value = GetBaseObject(value);
            if (value is string s)
            {
                emitter.Emit(new Scalar(s));
                return;
            }

            if (value is int || value is long || value is bool)
            {
                emitter.Emit(new Scalar(null, null, value.ToString(), ScalarStyle.Plain, false, false));
                return;
            }

            if (value is IEnumerable enumerable)
            {
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                foreach (var item in enumerable)
                    Primitive(emitter, item);

                emitter.Emit(new SequenceEnd());
                return;
            }

            if (value is PSObject || value is IDictionary)
            {
                Map(emitter, value);
                return;
            }

            emitter.Emit(new Scalar(value.ToString()));
        }

        private static IEnumerable<KeyValuePair<string, object>> GetKV(object value)
        {
            var o = GetBaseObject(value);
            if (o is IDictionary d)
                foreach (DictionaryEntry kv in d)
                    yield return new KeyValuePair<string, object>(kv.Key.ToString(), kv.Value);

            if (o is PSObject psObject)
                foreach (var p in psObject.Properties)
                    yield return new KeyValuePair<string, object>(p.Name, p.Value);
        }

        private static object GetBaseObject(object value)
        {
            return value is PSObject psObject && psObject.BaseObject != null && !(psObject.BaseObject is PSCustomObject) ? psObject.BaseObject : value;
        }
    }

    /// <summary>
    /// A YAML converter to serialize Baseline
    /// </summary>
    internal sealed class BaselineYamlTypeConverter : BaselineMappingConverter, IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Baseline);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (type == typeof(Baseline) && value == null)
            {
                emitter.Emit(new MappingStart());
                emitter.Emit(new MappingEnd());
                return;
            }

            if (!(value is Baseline baseline))
                return;

            MapBaseline(emitter, baseline);
        }
    }

    /// <summary>
    /// This class provides encapsulation for baseline mapping
    /// </summary>
    internal abstract class BaselineMappingConverter
    {
        protected static void MapBaseline(IEmitter emitter, Baseline baseline)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new Comment($"Synopsis: {baseline.Synopsis}", isInline: false));

            MapPropertyName(emitter, nameof(baseline.ApiVersion));
            emitter.Emit(new Scalar(baseline.ApiVersion));

            MapPropertyName(emitter, nameof(baseline.Kind));
            string kind = Enum.GetName(typeof(ResourceKind), baseline.Kind);
            emitter.Emit(new Scalar(kind));

            MapPropertyName(emitter, nameof(baseline.Metadata));
            MapResourceMetadata(emitter, baseline.Metadata);

            MapPropertyName(emitter, nameof(baseline.Spec));
            MapBaselineSpec(emitter, baseline.Spec);

            emitter.Emit(new MappingEnd());
        }

        private static void MapPropertyName(IEmitter emitter, string propertyName)
        {
            emitter.Emit(new Scalar(CamelCaseNamingConvention.Instance.Apply(propertyName)));
        }

        private static void MapResourceMetadata(IEmitter emitter, ResourceMetadata resourceMetadata)
        {
            emitter.Emit(new MappingStart());

            MapPropertyName(emitter, nameof(resourceMetadata.Annotations));

            emitter.Emit(new MappingStart());
            foreach (KeyValuePair<string, object> kvp in resourceMetadata.Annotations)
            {
                emitter.Emit(new Scalar(kvp.Key));
                emitter.Emit(new Scalar(kvp.Value.ToString()));
            }
            emitter.Emit(new MappingEnd());

            MapPropertyName(emitter, nameof(resourceMetadata.Name));
            emitter.Emit(new Scalar(resourceMetadata.Name));

            MapPropertyName(emitter, nameof(resourceMetadata.Tags));

            emitter.Emit(new MappingStart());
            foreach (KeyValuePair<string, string> kvp in resourceMetadata.Tags)
            {
                emitter.Emit(new Scalar(kvp.Key));
                emitter.Emit(new Scalar(kvp.Value));
            }
            emitter.Emit(new MappingEnd());

            emitter.Emit(new MappingEnd());
        }

        private static void MapBaselineSpec(IEmitter emitter, BaselineSpec baselineSpec)
        {
            emitter.Emit(new MappingStart());

            MapPropertyName(emitter, nameof(baselineSpec.Binding));
            MapBindingOption(emitter, baselineSpec.Binding);

            MapPropertyName(emitter, nameof(baselineSpec.Configuration));
            MapConfigurationOption(emitter, baselineSpec.Configuration);

            MapPropertyName(emitter, nameof(baselineSpec.Convention));
            MapConventionOption(emitter, baselineSpec.Convention);

            MapPropertyName(emitter, nameof(baselineSpec.Rule));
            MapRuleOption(emitter, baselineSpec.Rule);

            emitter.Emit(new MappingEnd());
        }

        private static void MapBindingOption(IEmitter emitter, BindingOption bindingOption)
        {
            emitter.Emit(new MappingStart());

            if (bindingOption?.Field != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.Field));

                emitter.Emit(new MappingStart());

                foreach (KeyValuePair<string, string[]> kvp in bindingOption.Field)
                {
                    emitter.Emit(new Scalar(kvp.Key));
                    MapStringArraySequence(emitter, kvp.Value);
                }

                emitter.Emit(new MappingEnd());
            }

            if ((bindingOption?.IgnoreCase).HasValue)
            {
                MapPropertyName(emitter, nameof(bindingOption.IgnoreCase));
                emitter.Emit(new Scalar(bindingOption.IgnoreCase.ToString()));
            }

            if (bindingOption?.NameSeparator != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.NameSeparator));
                emitter.Emit(new Scalar(bindingOption.NameSeparator));
            }

            if ((bindingOption?.PreferTargetInfo).HasValue)
            {
                MapPropertyName(emitter, nameof(bindingOption.PreferTargetInfo));
                emitter.Emit(new Scalar(bindingOption.PreferTargetInfo.ToString()));
            }

            if (bindingOption?.TargetName != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.TargetName));
                MapStringArraySequence(emitter, bindingOption.TargetName);
            }

            if (bindingOption?.TargetType != null)
            {
                MapPropertyName(emitter, nameof(bindingOption.TargetType));
                MapStringArraySequence(emitter, bindingOption.TargetType);
            }

            if ((bindingOption?.UseQualifiedName).HasValue)
            {
                MapPropertyName(emitter, nameof(bindingOption.UseQualifiedName));
                emitter.Emit(new Scalar(bindingOption.UseQualifiedName.ToString()));
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapConfigurationOption(IEmitter emitter, ConfigurationOption configurationOption)
        {
            emitter.Emit(new MappingStart());

            if (configurationOption != null)
            {
                foreach (KeyValuePair<string, object> kvp in configurationOption)
                {
                    emitter.Emit(new Scalar(kvp.Key));

                    if (kvp.Value is PSObject[] psObjects)
                    {
                        MapPSObjectArraySequence(emitter, psObjects);
                    }
                    else
                    {
                        emitter.Emit(new Scalar(kvp.Value.ToString()));
                    }
                }
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapConventionOption(IEmitter emitter, ConventionOption conventionOption)
        {
            emitter.Emit(new MappingStart());

            if (conventionOption?.Include != null)
            {
                MapPropertyName(emitter, nameof(conventionOption.Include));
                MapStringArraySequence(emitter, conventionOption.Include);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapRuleOption(IEmitter emitter, RuleOption ruleOption)
        {
            emitter.Emit(new MappingStart());

            if (ruleOption?.Exclude != null)
            {
                MapPropertyName(emitter, nameof(ruleOption.Exclude));
                MapStringArraySequence(emitter, ruleOption.Exclude);
            }

            if (ruleOption?.Include != null)
            {
                MapPropertyName(emitter, nameof(ruleOption.Include));
                MapStringArraySequence(emitter, ruleOption.Include);
            }

            if ((ruleOption?.IncludeLocal).HasValue)
            {
                MapPropertyName(emitter, nameof(ruleOption.IncludeLocal));
                emitter.Emit(new Scalar(ruleOption.IncludeLocal.ToString()));
            }

            if (ruleOption?.Tag != null)
            {
                MapPropertyName(emitter, nameof(ruleOption.Tag));
                MapHashtable(emitter, ruleOption.Tag);
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapHashtable(IEmitter emitter, Hashtable hashtable)
        {
            emitter.Emit(new MappingStart());

            foreach (DictionaryEntry entry in hashtable)
            {
                emitter.Emit(new Scalar(entry.Key.ToString()));

                if (entry.Value is string entryValue)
                {
                    emitter.Emit(new Scalar(entryValue));
                }

                else if (entry.Value is string[] entryValues)
                {
                    MapStringArraySequence(emitter, entryValues);
                }

                else if (entry.Value is PSObject[] psObjects)
                {
                    MapPSObjectArraySequence(emitter, psObjects);
                }

                else
                {
                    emitter.Emit(new Scalar(entry.Value.ToString()));
                }
            }

            emitter.Emit(new MappingEnd());
        }

        private static void MapStringArraySequence(IEmitter emitter, string[] sequence)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (string item in sequence)
            {
                emitter.Emit(new Scalar(item));
            }

            emitter.Emit(new SequenceEnd());
        }

        private static void MapPSObjectArraySequence(IEmitter emitter, PSObject[] sequence)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));

            foreach (PSObject obj in sequence)
            {
                IEnumerable<PSPropertyInfo> noteProperties = obj.Properties
                    .Where(prop => prop.MemberType == PSMemberTypes.NoteProperty);

                if (noteProperties.Any())
                {
                    emitter.Emit(new MappingStart());

                    foreach (PSPropertyInfo propertyInfo in noteProperties)
                    {
                        emitter.Emit(new Scalar(propertyInfo.Name));
                        emitter.Emit(new Scalar(propertyInfo.Value.ToString()));
                    }

                    emitter.Emit(new MappingEnd());
                }
                else
                {
                    emitter.Emit(new Scalar(obj.BaseObject.ToString()));
                }
            }

            emitter.Emit(new SequenceEnd());
        }
    }

    /// <summary>
    /// A YAML resolver to convert any dictionary types to PSObjects instead.
    /// </summary>
    internal sealed class PSObjectYamlTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (nodeEvent is SequenceStart)
            {
                currentType = typeof(PSObject[]);
                return true;
            }
            else if (currentType == typeof(Dictionary<object, object>) || nodeEvent is MappingStart)
            {
                currentType = typeof(PSObject);
                return true;
            }
            return false;
        }
    }

    internal sealed class PSOptionYamlTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (currentType == typeof(object) && nodeEvent is SequenceStart)
            {
                currentType = typeof(PSObject[]);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A YAML type inspector to sort properties by name
    /// </summary>
    internal sealed class SortedPropertyYamlTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeDescriptor;

        public SortedPropertyYamlTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this._innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return _innerTypeDescriptor
                .GetProperties(type, container)
                .OrderBy(p => p.Name);
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
            _NamingConvention = CamelCaseNamingConvention.Instance;
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
                .Where(f => !f.IsStatic && f.IsPublic && !f.IsDefined(typeof(YamlIgnoreAttribute), true))
                .Select(p => new Field(p, _TypeResolver, _NamingConvention));
        }

        private IEnumerable<Property> SelectProperty(Type type)
        {
            return type
                .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
                .Where(p => p.CanRead && IsAllowedProperty(p.Name) && !p.IsDefined(typeof(YamlIgnoreAttribute), true))
                .Select(p => new Property(p, _TypeResolver, _NamingConvention));
        }

        private static bool IsAllowedProperty(string name)
        {
            return !(name == "TargetObject" || name == "Exception");
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
        private const string FIELD_APIVERSION = "apiVersion";
        private const string FIELD_KIND = "kind";
        private const string FIELD_METADATA = "metadata";
        private const string FIELD_SPEC = "spec";

        private readonly INodeDeserializer _Next;
        private readonly SpecFactory _Factory;

        public LanguageBlockDeserializer(INodeDeserializer next)
        {
            _Next = next;
            _Factory = new SpecFactory();
        }

        bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (typeof(ResourceObject).IsAssignableFrom(expectedType))
            {
                var comment = HostHelper.GetCommentMeta(RunspaceContext.CurrentThread.Source.File.Path, reader.Current.Start.Line - 2, reader.Current.Start.Column);
                var resource = MapResource(reader, nestedObjectDeserializer, comment);
                value = new ResourceObject(resource);
                return true;
            }
            else
            {
                return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }
        }

        private IResource MapResource(IParser reader, Func<IParser, Type, object> nestedObjectDeserializer, CommentMetadata comment)
        {
            IResource result = null;
            string apiVersion = null;
            string kind = null;
            ResourceMetadata metadata = null;
            if (reader.TryConsume<MappingStart>(out _))
            {
                while (reader.TryConsume(out Scalar scalar))
                {
                    // Read apiVersion
                    if (TryApiVersion(reader, scalar, out string apiVersionValue))
                    {
                        apiVersion = apiVersionValue;
                    }
                    // Read kind
                    else if (TryKind(reader, scalar, out string kindValue))
                    {
                        kind = kindValue;
                    }
                    // Read metadata
                    else if (TryMetadata(reader, scalar, nestedObjectDeserializer, out ResourceMetadata metadataValue))
                    {
                        metadata = metadataValue;
                    }
                    // Read spec
                    else if (kind != null && TrySpec(reader, scalar, apiVersion, kind, nestedObjectDeserializer, metadata, comment, out IResource resource))
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

        private static bool TryApiVersion(IParser reader, Scalar scalar, out string kind)
        {
            kind = null;
            if (scalar.Value == FIELD_APIVERSION)
            {
                kind = reader.Consume<Scalar>().Value;
                return true;
            }
            return false;
        }

        private static bool TryKind(IParser reader, Scalar scalar, out string kind)
        {
            kind = null;
            if (scalar.Value == FIELD_KIND)
            {
                kind = reader.Consume<Scalar>().Value;
                return true;
            }
            return false;
        }

        private bool TryMetadata(IParser reader, Scalar scalar, Func<IParser, Type, object> nestedObjectDeserializer, out ResourceMetadata metadata)
        {
            metadata = null;
            if (scalar.Value != FIELD_METADATA)
                return false;

            if (reader.Current is MappingStart)
            {
                if (!_Next.Deserialize(reader, typeof(ResourceMetadata), nestedObjectDeserializer, out object value))
                    return false;

                metadata = (ResourceMetadata)value;
                return true;
            }
            return false;
        }

        private bool TrySpec(IParser reader, Scalar scalar, string apiVersion, string kind, Func<IParser, Type, object> nestedObjectDeserializer, ResourceMetadata metadata, CommentMetadata comment, out IResource spec)
        {
            spec = null;
            if (scalar.Value != FIELD_SPEC)
                return false;

            return TryResource(reader, apiVersion, kind, nestedObjectDeserializer, metadata, comment, out spec);
        }

        private bool TryResource(IParser reader, string apiVersion, string kind, Func<IParser, Type, object> nestedObjectDeserializer, ResourceMetadata metadata, CommentMetadata comment, out IResource spec)
        {
            spec = null;
            if (_Factory.TryDescriptor(apiVersion, kind, out ISpecDescriptor descriptor) && reader.Current is MappingStart)
            {
                if (!_Next.Deserialize(reader, descriptor.SpecType, nestedObjectDeserializer, out object value))
                    return false;

                spec = descriptor.CreateInstance(RunspaceContext.CurrentThread.Source.File, metadata, comment, value);
                if (string.IsNullOrEmpty(apiVersion))
                    spec.SetApiVersionIssue();

                return true;
            }
            return false;
        }
    }

    internal sealed class LanguageExpressionDeserializer : INodeDeserializer
    {
        private const string OPERATOR_IF = "if";

        private readonly INodeDeserializer _Next;
        private readonly LanguageExpressionFactory _Factory;

        public LanguageExpressionDeserializer(INodeDeserializer next)
        {
            _Next = next;
            _Factory = new LanguageExpressionFactory();
        }

        bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (typeof(LanguageExpression).IsAssignableFrom(expectedType))
            {
                var resource = MapOperator(OPERATOR_IF, reader, nestedObjectDeserializer);
                value = new LanguageIf(resource);
                return true;
            }
            else
            {
                return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }
        }

        private LanguageExpression MapOperator(string type, IParser reader, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            if (TryExpression(reader, type, nestedObjectDeserializer, out LanguageOperator result))
            {
                // If and Not
                if (reader.TryConsume<MappingStart>(out _))
                {
                    result.Add(MapExpression(reader, nestedObjectDeserializer));
                    reader.Require<MappingEnd>();
                    reader.MoveNext();
                }
                // AllOf and AnyOf
                else if (reader.TryConsume<SequenceStart>(out _))
                {
                    while (reader.TryConsume<MappingStart>(out _))
                    {
                        result.Add(MapExpression(reader, nestedObjectDeserializer));
                        reader.Require<MappingEnd>();
                        reader.MoveNext();
                    }
                    reader.Require<SequenceEnd>();
                    reader.MoveNext();
                }
            }
            return result;
        }

        private LanguageExpression MapCondition(string type, LanguageExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            if (TryExpression(reader, type, nestedObjectDeserializer, out LanguageCondition result))
            {
                while (!reader.Accept(out MappingEnd end))
                {
                    MapProperty(properties, reader, nestedObjectDeserializer, out _);
                }
                result.Add(properties);
            }
            return result;
        }

        private LanguageExpression MapExpression(IParser reader, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            LanguageExpression result = null;
            var properties = new LanguageExpression.PropertyBag();
            MapProperty(properties, reader, nestedObjectDeserializer, out string key);
            if (key != null && TryCondition(key))
            {
                result = MapCondition(key, properties, reader, nestedObjectDeserializer);
            }
            else if (TryOperator(key) && reader.Accept(out MappingStart mapping))
            {
                result = MapOperator(key, reader, nestedObjectDeserializer);
            }
            else if (TryOperator(key) && reader.Accept(out SequenceStart sequence))
            {
                result = MapOperator(key, reader, nestedObjectDeserializer);
            }
            return result;
        }

        private void MapProperty(LanguageExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object> nestedObjectDeserializer, out string name)
        {
            name = null;
            while (reader.TryConsume(out Scalar scalar))
            {
                string key = scalar.Value;
                if (TryCondition(key) || TryOperator(key))
                    name = key;

                if (reader.TryConsume(out scalar))
                {
                    properties[key] = scalar.Value;
                }
                else if (TryCondition(key) && reader.TryConsume<SequenceStart>(out _))
                {
                    var objects = new List<string>();
                    while (!reader.TryConsume<SequenceEnd>(out _))
                    {
                        if (reader.TryConsume(out scalar))
                        {
                            objects.Add(scalar.Value);
                        }
                    }
                    properties[key] = objects.ToArray();
                }
            }
        }

        private bool TryOperator(string key)
        {
            return _Factory.IsOperator(key);
        }

        private bool TryCondition(string key)
        {
            return _Factory.IsCondition(key);
        }

        private bool TryExpression<T>(IParser reader, string type, Func<IParser, Type, object> nestedObjectDeserializer, out T expression) where T : LanguageExpression
        {
            expression = null;
            if (_Factory.TryDescriptor(type, out ILanguageExpresssionDescriptor descriptor))
            {
                expression = (T)descriptor.CreateInstance(RunspaceContext.CurrentThread.Source.File, null);
                return expression != null;
            }
            return false;
        }
    }

    internal sealed class PSObjectYamlDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer _Next;
        private readonly PSObjectYamlTypeConverter _Converter;
        private readonly TargetSourceInfo _SourceInfo;

        public PSObjectYamlDeserializer(INodeDeserializer next, TargetSourceInfo sourceInfo)
        {
            _Next = next;
            _Converter = new PSObjectYamlTypeConverter();
            _SourceInfo = sourceInfo;
        }

        bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (expectedType == typeof(PSObject[]) && reader.Current is MappingStart)
            {
                int lineNumber = reader.Current.Start.Line;
                int linePosition = reader.Current.Start.Column;
                value = _Converter.ReadYaml(reader, typeof(PSObject));
                if (value is PSObject pso)
                {
                    pso.UseTargetInfo(out PSRuleTargetInfo info);
                    info.SetSource(_SourceInfo?.File, lineNumber, linePosition);
                    value = new PSObject[] { pso };
                    return true;
                }
                return false;
            }
            else
            {
                return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }
        }
    }
}
