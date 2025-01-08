// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using System.Reflection;
using PSRule.Configuration;
using PSRule.Converters;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Emitters;
using PSRule.Pipeline;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;
using IEmitter = YamlDotNet.Core.IEmitter;

namespace PSRule;

#nullable enable

/// <summary>
/// A YAML converter that allows short and full notation of suppression rules.
/// </summary>
internal sealed class SuppressionRuleYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(SuppressionRule);
    }

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var result = new SuppressionRule();
        if (parser.TryConsume<SequenceStart>(out _))
        {
            var targetNames = new List<string>();
            while (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
                targetNames.Add(scalar.Value);

            result.TargetName = [.. targetNames];
            parser.MoveNext();
        }
        else if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
            {
                var name = scalar.Value;
                if (name == "targetName" && parser.TryConsume<SequenceStart>(out _))
                {
                    var targetNames = new List<string>();
                    while (parser.TryConsume<Scalar>(out var item) && item != null)
                        targetNames.Add(item.Value);

                    result.TargetName = [.. targetNames];
                    parser.MoveNext();
                }
            }
            parser.MoveNext();
        }
        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
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

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var result = new FieldMap();
        if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
            {
                var fieldName = scalar.Value;
                if (parser.TryConsume<SequenceStart>(out _))
                {
                    var fields = new List<string>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
#pragma warning disable IDE0001
                        if (parser.TryConsume<Scalar>(out scalar) && scalar != null)
                            fields.Add(scalar.Value);
#pragma warning restore
                    }
                    result.Set(fieldName, [.. fields]);
                    parser.Require<SequenceEnd>();
                    parser.MoveNext();
                }
            }
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (type == typeof(FieldMap) && value == null)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new MappingEnd());
        }
        if (value is not FieldMap map)
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

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        // Handle empty objects
        if (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
        {
            return PSObject.AsPSObject(scalar.Value);
        }

        var result = new PSObject();
        if (parser.TryConsume<MappingStart>(out _))
        {
#pragma warning disable IDE0001
            while (parser.TryConsume<Scalar>(out scalar) && scalar != null)
            {
                var name = scalar.Value;
                var property = ReadNoteProperty(parser, name, rootDeserializer) ?? throw new NotImplementedException();
                result.Properties.Add(property);
            }
#pragma warning restore
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        Map(emitter, value);
    }

    private PSNoteProperty? ReadNoteProperty(IParser parser, string name, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<SequenceStart>(out _))
        {
            var values = new List<PSObject>();
            while (parser.Current is not SequenceEnd)
            {
                if (parser.Current is MappingStart)
                {
                    values.Add(PSObject.AsPSObject(ReadYaml(parser, typeof(PSObject), rootDeserializer)));
                }
                else if (parser.TryConsume<Scalar>(out var scalar))
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
            return new PSNoteProperty(name, ReadYaml(parser, typeof(PSObject), rootDeserializer));
        }
        else if (parser.TryConsume<Scalar>(out var scalar))
        {
            return new PSNoteProperty(name, scalar.Value);
        }
        return null;
    }
}

internal abstract class MappingTypeConverter
{
    protected void Map(IEmitter emitter, object? value)
    {
        if (value is null) return;

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

        value = ExpressionHelpers.GetBaseObject(value);
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
        var o = ExpressionHelpers.GetBaseObject(value);
        if (o is IDictionary d)
            foreach (DictionaryEntry kv in d)
                yield return new KeyValuePair<string, object>(kv.Key.ToString(), kv.Value);

        if (o is PSObject psObject)
            foreach (var p in psObject.Properties)
                yield return new KeyValuePair<string, object>(p.Name, p.Value);
    }
}

/// <summary>
/// A YAML resolver to convert any dictionary types to PSObjects instead.
/// </summary>
internal sealed class PSObjectYamlTypeResolver : INodeTypeResolver
{
    public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
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
    public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
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
/// A YAML type inspector to order properties alphabetically
/// </summary>
internal sealed class OrderedPropertiesTypeInspector(ITypeInspector innerTypeDescriptor) : ReflectionTypeInspector
{
    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return innerTypeDescriptor
            .GetProperties(type, container)
            .OrderBy(prop => prop.Name);
    }
}

/// <summary>
/// A YAML type inspector to read fields and properties from a type for serialization.
/// </summary>
internal sealed class FieldYamlTypeInspector : ReflectionTypeInspector
{
    private readonly ITypeResolver _TypeResolver;
    private readonly INamingConvention _NamingConvention;

    public FieldYamlTypeInspector()
    {
        _TypeResolver = new StaticTypeResolver();
        _NamingConvention = CamelCaseNamingConvention.Instance;
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
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

        public Type? TypeOverride { get; set; }

        public int Order { get; set; }

        public bool CanWrite => false;

        public ScalarStyle ScalarStyle { get; set; }

        public bool AllowNulls => true;

        public bool Required => false;

        public Type? ConverterType => null;

        public void Write(object target, object? value)
        {
            throw new NotImplementedException();
        }

        public T? GetCustomAttribute<T>() where T : Attribute
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

        public Type? TypeOverride { get; set; }

        public int Order { get; set; }

        public bool CanWrite => false;

        public ScalarStyle ScalarStyle { get; set; }

        public bool AllowNulls => true;

        public bool Required => false;

        public Type? ConverterType => null;

        public T? GetCustomAttribute<T>() where T : Attribute
        {
            return _PropertyInfo.GetCustomAttributes(typeof(T), true).OfType<T>().FirstOrDefault();
        }

        public void Write(object target, object? value)
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

internal sealed class PSObjectYamlDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _Next;
    private readonly PSObjectYamlTypeConverter _Converter;
    private readonly IFileInfo? _SourceInfo;

    public PSObjectYamlDeserializer(INodeDeserializer next, IFileInfo sourceInfo)
    {
        _Next = next;
        _Converter = new PSObjectYamlTypeConverter();
        _SourceInfo = sourceInfo;
    }

    public PSObjectYamlDeserializer(INodeDeserializer next)
    {
        _Next = next;
        _Converter = new PSObjectYamlTypeConverter();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
    {
        if (expectedType == typeof(PSObject[]) && reader.Current is MappingStart)
        {
            var parser = reader as YamlEmitterParser;
            var fileInfo = parser?.Info ?? _SourceInfo;

            var lineNumber = (int)reader.Current.Start.Line;
            var linePosition = (int)reader.Current.Start.Column;
            value = _Converter.ReadYaml(reader, typeof(PSObject), rootDeserializer);
            if (value is PSObject pso)
            {
                pso.UseTargetInfo(out var info);
                info.SetSource(fileInfo?.Path, lineNumber, linePosition);
                value = new PSObject[] { pso };
                return true;
            }
            return false;
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }
    }
}

internal sealed class TargetObjectYamlDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _Next;
    private readonly PSObjectYamlTypeConverter _Converter;

    public TargetObjectYamlDeserializer(INodeDeserializer next)
    {
        _Next = next;
        _Converter = new PSObjectYamlTypeConverter();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
    {
        value = null;
        if (expectedType == typeof(TargetObject[]) && reader.Current is MappingStart)
        {
            if (TryGetTargetObject(reader, out var targetObject, rootDeserializer) && targetObject != null)
            {
                value = new TargetObject[] { targetObject };
                return true;
            }
            return false;
        }
        else if (expectedType == typeof(TargetObject[]) && reader.TryConsume<SequenceStart>(out _))
        {
            var result = new List<TargetObject>();
            while (reader.Current is MappingStart)
            {
                if (TryGetTargetObject(reader, out var targetObject, rootDeserializer) && targetObject != null)
                {
                    result.Add(targetObject);
                }
            }
            value = result.ToArray();
            return reader.TryConsume<SequenceEnd>(out _);
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }
    }

    private bool TryGetTargetObject(IParser reader, out TargetObject? value, ObjectDeserializer rootDeserializer)
    {
        value = null;
        var parser = reader as YamlEmitterParser;
        var fileInfo = parser?.Info;
        var lineNumber = (int?)reader.Current?.Start.Line;
        var linePosition = (int?)reader.Current?.Start.Column;

        if (_Converter.ReadYaml(reader, typeof(PSObject), rootDeserializer) is PSObject o)
        {
            o.UseTargetInfo(out var info);
            info.SetSource(fileInfo?.Path, lineNumber, linePosition);

            var targetObject = new TargetObject(o);

            value = targetObject;
            return true;
        }
        return false;
    }
}

internal sealed class InfoStringYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(InfoString);
    }

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        return parser.TryConsume<Scalar>(out var scalar) &&
            !string.IsNullOrEmpty(scalar.Value) ? new InfoString(scalar.Value) : new InfoString();
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is InfoString info && info.HasValue && info.Text != null)
            emitter.Emit(new Scalar(info.Text));
    }
}

/// <summary>
/// A converter for converting <see cref="EnumMap{T}"/> to/ from YAML.
/// </summary>
internal sealed class EnumMapYamlTypeConverter<T> : IYamlTypeConverter where T : struct, Enum
{
    public bool Accepts(Type type)
    {
        return typeof(EnumMap<T>).IsAssignableFrom(type);
    }

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var map = new EnumMap<T>();
        if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume<Scalar>(out var s1))
            {
                var propertyName = s1.Value;
                if (parser.TryConsume<Scalar>(out var s2) && TypeConverter.TryEnum<T>(s2.Value, convert: true, out var value) && value != null)
                    map.Add(propertyName, value.Value);
            }
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        else
        {
            parser.SkipThisAndNestedEvents();
        }
        return map;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (type == typeof(EnumMap<T>) && value == null)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new MappingEnd());
        }
        if (value is not EnumMap<T> map)
            return;

        emitter.Emit(new MappingStart());
        foreach (var kv in map)
        {
            emitter.Emit(new Scalar(kv.Key));
            emitter.Emit(new Scalar(kv.Value.ToString()));
        }
        emitter.Emit(new MappingEnd());
    }
}

#nullable restore
