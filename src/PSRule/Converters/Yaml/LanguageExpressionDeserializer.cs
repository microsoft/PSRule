// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Definitions.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSRule.Converters.Yaml;

#nullable enable

/// <summary>
/// A custom deserializer to convert YAML into a language expression.
/// </summary>
internal sealed class LanguageExpressionDeserializer : INodeDeserializer
{
    private const string OPERATOR_IF = "if";

    private readonly IResourceDiscoveryContext _Context;
    private readonly INodeDeserializer _Next;
    private readonly LanguageExpressionFactory _Factory;
    private readonly FunctionBuilder _FunctionBuilder;

    public LanguageExpressionDeserializer(IResourceDiscoveryContext context, INodeDeserializer next)
    {
        _Context = context;
        _Next = next;
        _Factory = new LanguageExpressionFactory();
        _FunctionBuilder = new FunctionBuilder();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
    {
        if (typeof(LanguageExpression).IsAssignableFrom(expectedType))
        {
            var @operator = MapOperator(OPERATOR_IF, null, null, reader, nestedObjectDeserializer);
            value = @operator?.Children.Count == 0 ? new LanguageIf(LanguageExpressionLambda.True) : new LanguageIf(@operator);
            return true;
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }
    }

    /// <summary>
    /// Map an operator.
    /// </summary>
    private LanguageOperator? MapOperator(string type, LanguageExpression.PropertyBag? properties, LanguageExpression? subselector, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        if (TryExpression(reader, type, properties, nestedObjectDeserializer, out LanguageOperator? result) && result != null)
        {
            // If and Not
            if (reader.TryConsume<MappingStart>(out _))
            {
                var expression = MapExpression(reader, nestedObjectDeserializer);
                if (expression != null)
                    result.Add(expression);

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
            result.Subselector = subselector;
        }
        return result;
    }

    private LanguageCondition? MapCondition(string type, LanguageExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        if (TryExpression(reader, type, null, nestedObjectDeserializer, out LanguageCondition? result) && result != null)
        {
            while (!reader.Accept<MappingEnd>(out var end) && end != null)
            {
                MapProperty(properties, reader, nestedObjectDeserializer, out _, out _);
            }
            result.Add(properties);
        }
        return result;
    }

    private LanguageExpression? MapExpression(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        LanguageExpression? result = null;
        var properties = new LanguageExpression.PropertyBag();
        MapProperty(properties, reader, nestedObjectDeserializer, out var key, out var subselector);
        if (key == null)
            return null;

        if (TryCondition(key))
        {
            result = MapCondition(key, properties, reader, nestedObjectDeserializer);
        }
        else if (TryOperator(key) && reader.Accept<MappingStart>(out _))
        {
            var op = MapOperator(key, properties, subselector, reader, nestedObjectDeserializer);
            MapProperty(properties, reader, nestedObjectDeserializer, out _, out _);
            result = op;
        }
        else if (TryOperator(key) && reader.Accept<SequenceStart>(out _))
        {
            var op = MapOperator(key, properties, subselector, reader, nestedObjectDeserializer);
            MapProperty(properties, reader, nestedObjectDeserializer, out _, out subselector);
            if (op != null && subselector != null)
                op.Subselector = subselector;

            result = op;
        }
        return result;
    }

    private ExpressionFnOuter MapFunction(string type, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        _FunctionBuilder.Push();
        string? name = null;
        while (!(reader.Accept<MappingEnd>(out _) || reader.Accept<SequenceEnd>(out _)))
        {
            if (reader.TryConsume<Scalar>(out var s) && s != null)
            {
                if (name != null)
                {
                    _FunctionBuilder.Add(name, s.Value);
                    name = null;
                }
                else
                {
                    name = s.Value;
                }
            }
            else if (reader.TryConsume<MappingStart>(out _) && name != null)
            {
                var child = MapFunction(name, reader, nestedObjectDeserializer);
                _FunctionBuilder.Add(name, child);
                name = null;
                reader.Consume<MappingEnd>();
            }
            else if (reader.TryConsume<SequenceStart>(out _) && name != null)
            {
                var sequence = MapSequence(name, reader, nestedObjectDeserializer);
                _FunctionBuilder.Add(name, sequence);
                name = null;
                reader.Consume<SequenceEnd>();
            }
        }
        var result = _FunctionBuilder.Pop();
        return result;
    }

    private object[] MapSequence(string name, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        var result = new List<object>();
        while (!reader.Accept<SequenceEnd>(out _))
        {
            if (reader.TryConsume<Scalar>(out var s))
                result.Add(s.Value);

            else if (reader.TryConsume<MappingStart>(out _))
            {
                var child = MapFunction(name, reader, nestedObjectDeserializer);
                result.Add(child);
                reader.Consume<MappingEnd>();
            }
        }
        return [.. result];
    }

    private void MapProperty(LanguageExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, out string? name, out LanguageExpression? subselector)
    {
        name = null;
        subselector = null;
        while (reader.TryConsume<Scalar>(out var scalar) && scalar != null)
        {
            var key = scalar.Value;
            if (TryCondition(key) || TryOperator(key))
                name = key;

            if (reader.TryConsume(out scalar) && scalar != null)
            {
                properties[key] = scalar.Value;
            }
            // value:
            else if (TryValue(key, reader, nestedObjectDeserializer, out var value))
            {
                properties[key] = value;
            }
            else if (TryCondition(key) && reader.TryConsume<MappingStart>(out _))
            {
                if (TryFunction(reader, nestedObjectDeserializer, out var fn))
                    properties[key] = fn;
            }
            else if (TryCondition(key) && reader.TryConsume<SequenceStart>(out _))
            {
                var objects = new List<string>();
                while (!reader.TryConsume<SequenceEnd>(out _))
                {
                    if (reader.TryConsume(out scalar) && scalar != null)
                    {
                        objects.Add(scalar.Value);
                    }
                }
                properties[key] = objects.ToArray();
            }
            // where:
            else if (TrySubSelector(key) && reader.TryConsume<MappingStart>(out _))
            {
                subselector = MapExpression(reader, nestedObjectDeserializer);
                reader.Consume<MappingEnd>();
            }
        }
    }

    private bool TrySubSelector(string key)
    {
        return _Factory.IsSubselector(key);
    }

    private bool TryOperator(string key)
    {
        return _Factory.IsOperator(key);
    }

    private bool TryCondition(string key)
    {
        return _Factory.IsCondition(key);
    }

    private bool TryValue(string key, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        if (key != "value")
            return false;

        if (reader.TryConsume<MappingStart>(out _) && TryFunction(reader, nestedObjectDeserializer, out var fn))
        {
            value = fn;
            return true;
        }
        reader.SkipThisAndNestedEvents();
        return false;
    }

    private bool TryFunction(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, out ExpressionFnOuter? fn)
    {
        fn = null;
        if (!IsFunction(reader))
            return false;

        reader.Consume<Scalar>();
        reader.Consume<MappingStart>();
        fn = MapFunction("$", reader, nestedObjectDeserializer);
        reader.Consume<MappingEnd>();
        reader.Consume<MappingEnd>();
        return true;
    }

    private static bool IsFunction(IParser reader)
    {
        return reader.Accept<Scalar>(out var scalar) || scalar?.Value == "$";
    }

    private bool TryExpression<T>(IParser reader, string type, LanguageExpression.PropertyBag? properties, Func<IParser, Type, object?> nestedObjectDeserializer, out T? expression) where T : LanguageExpression
    {
        expression = null;
        if (_Factory.TryDescriptor(type, out var descriptor))
        {
            expression = (T)descriptor.CreateInstance(_Context.Source, properties);
            return expression != null;
        }
        return false;
    }
}

#nullable restore
