// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;
using PSRule.Definitions.Expressions;
using PSRule.Resources;

namespace PSRule.Converters.Json;

#nullable enable

/// <summary>
/// A custom converter for deserializing JSON into a language expression.
/// </summary>
internal sealed class LanguageExpressionJsonConverter : JsonConverter
{
    private const string OPERATOR_IF = "if";

    private readonly IResourceDiscoveryContext _Context;
    private readonly LanguageExpressionFactory _Factory;
    private readonly FunctionBuilder _FunctionBuilder;

    public LanguageExpressionJsonConverter(IResourceDiscoveryContext context)
    {
        _Context = context;
        _Factory = new LanguageExpressionFactory();
        _FunctionBuilder = new FunctionBuilder();
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(LanguageExpression).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var @operator = MapOperator(OPERATOR_IF, null, null, reader);
        return @operator?.Children.Count == 0 ? new LanguageIf(LanguageExpressionLambda.True) : new LanguageIf(@operator);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Map an operator.
    /// </summary>
    private LanguageOperator? MapOperator(string type, LanguageExpression.PropertyBag? properties, LanguageExpression? subselector, JsonReader reader)
    {
        if (TryExpression(type, properties, out LanguageOperator? result) && result != null)
        {
            reader.SkipComments(out _);

            // If and Not
            if (reader.TryConsume(JsonToken.StartObject))
            {
                var expression = MapExpression(reader);
                if (expression != null)
                    result.Add(expression);

                if (type != "if")
                    reader.Consume(JsonToken.EndObject);
            }
            // AllOf and AnyOf
            else if (reader.TryConsume(JsonToken.StartArray))
            {
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.SkipComments(out var hasComments) && hasComments)
                        continue;

                    if (reader.TryConsume(JsonToken.StartObject))
                    {
                        result.Add(MapExpression(reader));
                        reader.Consume(JsonToken.EndObject);
                    }
                    if (reader.TokenType == JsonToken.EndObject)
                        throw new PipelineSerializationException(Messages.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);
                }
                reader.Consume(JsonToken.EndArray);
                reader.SkipComments(out _);
            }
            result.Subselector = subselector;
        }
        return result;
    }

    private LanguageExpression? MapCondition(string type, LanguageExpression.PropertyBag properties, JsonReader reader)
    {
        if (TryExpression(type, null, out LanguageCondition? result) && result != null)
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                MapProperty(properties, reader, out _, out _);
            }
            result.Add(properties);
        }
        return result;
    }

    private LanguageExpression? MapExpression(JsonReader reader)
    {
        LanguageExpression? result = null;
        var properties = new LanguageExpression.PropertyBag();
        reader.SkipComments(out _);
        MapProperty(properties, reader, out var key, out var subselector);
        if (key != null && TryCondition(key))
        {
            result = MapCondition(key, properties, reader);
        }
        else if ((reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray) &&
            key != null && TryOperator(key))
        {
            var op = MapOperator(key, properties, subselector, reader);
            MapProperty(properties, reader, out _, out subselector);
            if (subselector != null && op != null)
            {
                op.Subselector = subselector;
            }
            result = op;
        }
        return result;
    }

    private ExpressionFnOuter MapFunction(string type, JsonReader reader)
    {
        _FunctionBuilder.Push();
        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.Value is string name)
            {
                reader.Consume(JsonToken.PropertyName);
                if (reader.TryConsume(JsonToken.StartObject))
                {
                    var child = MapFunction(name, reader);
                    _FunctionBuilder.Add(name, child);
                    reader.Consume(JsonToken.EndObject);
                }
                else if (reader.TryConsume(JsonToken.StartArray))
                {
                    var sequence = MapSequence(name, reader);
                    _FunctionBuilder.Add(name, sequence);
                    reader.Consume(JsonToken.EndArray);
                }
                else
                {
                    _FunctionBuilder.Add(name, reader.Value);
                    reader.Read();
                }
            }
        }
        var result = _FunctionBuilder.Pop();
        return result;
    }

    private object MapSequence(string name, JsonReader reader)
    {
        var result = new List<object>();
        while (reader.TokenType != JsonToken.EndArray)
        {
            if (reader.TryConsume(JsonToken.StartObject))
            {
                var child = MapFunction(name, reader);
                result.Add(child);
                reader.Consume(JsonToken.EndObject);
            }
            else
            {
                result.Add(reader.Value);
                reader.Read();
            }
        }
        return result.ToArray();
    }

    private void MapProperty(LanguageExpression.PropertyBag properties, JsonReader reader, out string? name, out LanguageExpression? subselector)
    {
        name = null;
        subselector = null;
        reader.SkipComments(out _);
        while (reader.TokenType == JsonToken.PropertyName && reader.Value != null)
        {
            var key = reader.Value.ToString();
            if (TryCondition(key) || TryOperator(key))
                name = key;

            if (reader.Read())
            {
                // value:
                if (TryValue(key, reader, out var value))
                {
                    properties[key] = value;
                    reader.Read();
                }
                else if (TryCondition(key) && reader.TryConsume(JsonToken.StartObject))
                {
                    if (TryFunction(reader, out var fn))
                        properties.Add(key, fn);

                    reader.Consume(JsonToken.EndObject);
                }
                // where:
                else if (TrySubSelector(key) && reader.TryConsume(JsonToken.StartObject))
                {
                    subselector = MapExpression(reader);
                    reader.Consume(JsonToken.EndObject);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    break;
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    if (!TryCondition(key))
                        break;

                    var objects = new List<string>();
                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        if (reader.SkipComments(out var hasComments) && hasComments)
                            continue;

                        var item = reader.ReadAsString();
                        if (!string.IsNullOrEmpty(item) && item != null)
                            objects.Add(item);
                    }
                    properties.Add(key, objects.ToArray());
                    reader.Consume(JsonToken.EndArray);
                }
                else
                {
                    properties.Add(key, reader.Value);
                    reader.Read();
                }
            }
            reader.SkipComments(out _);
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

    private bool TryValue(string key, JsonReader reader, out object? value)
    {
        value = null;
        if (key != "value")
            return false;

        if (reader.TryConsume(JsonToken.StartObject) &&
            TryFunction(reader, out var fn))
        {
            value = fn;
            return true;
        }
        return false;
    }

    private bool TryFunction(JsonReader reader, out ExpressionFnOuter? fn)
    {
        fn = null;
        if (!IsFunction(reader))
            return false;

        reader.Consume(JsonToken.PropertyName);
        reader.Consume(JsonToken.StartObject);
        fn = MapFunction("$", reader);
        if (fn == null)
            throw new Exception();

        reader.Consume(JsonToken.EndObject);
        return true;
    }

    private static bool IsFunction(JsonReader reader)
    {
        return reader.TokenType == JsonToken.PropertyName &&
            reader.Value is string s &&
            s == "$";
    }

    private bool TryExpression<T>(string type, LanguageExpression.PropertyBag? properties, out T? expression) where T : LanguageExpression
    {
        expression = null;
        if (_Factory.TryDescriptor(type, out var descriptor))
        {
            expression = (T)descriptor.CreateInstance(
                source: _Context.Source,
                properties: properties
            );
            return expression != null;
        }
        return false;
    }
}

#nullable restore
