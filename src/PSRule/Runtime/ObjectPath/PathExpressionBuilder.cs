// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Dynamic;
using System.Management.Automation;
using System.Reflection;
using Newtonsoft.Json.Linq;
using PSRule.Resources;

namespace PSRule.Runtime.ObjectPath;

/// <summary>
/// A helper class to build an expression tree from path tokens.
/// </summary>
internal sealed class PathExpressionBuilder
{
    private const int DEFAULT_RECURSE_MAX_DEPTH = 100;
    private static readonly object[] DEFAULT_EMPTY_ARRAY = [];

    private readonly int _RecurseMaxDepth;

    public PathExpressionBuilder()
    {
        _RecurseMaxDepth = DEFAULT_RECURSE_MAX_DEPTH;
    }

    /// <summary>
    /// Determines if the output should be an array.
    /// </summary>
    public bool IsArray { get; private set; }

    /// <summary>
    /// Build a delegate function to evaluate the object path.
    /// </summary>
    public PathExpressionFn Build(IPathToken[] tokens)
    {
        return BuildSelector(new TokenReader(tokens));
    }

    private void UseArray()
    {
        IsArray = true;
    }

    private PathExpressionFn BuildSelector(ITokenReader reader)
    {
        if (!reader.Peak(out var token) || token.Type == PathTokenType.EndFilter || token.Type == PathTokenType.EndGroup)
            return Return;

        reader.Next(out token);
        switch (token.Type)
        {
            case PathTokenType.DotSelector:
                return DotSelector(reader, token.As<string>(), token.Option);

            case PathTokenType.DescendantSelector:
                return DescendantSelector(reader, token.As<string>(), token.Option);

            case PathTokenType.RootRef:
                return RootRef(reader);

            case PathTokenType.CurrentRef:
                return CurrentRef(reader);

            case PathTokenType.IndexWildSelector:
                return IndexWildSelector(reader);

            case PathTokenType.IndexSelector:
                return IndexSelector(reader, token.As<int>());

            case PathTokenType.StartFilter:
                return FilterSelector(reader);

            case PathTokenType.ArraySliceSelector:
                return ArraySliceSelector(reader, token.As<int?[]>());

            case PathTokenType.Boolean:
            case PathTokenType.Integer:
            case PathTokenType.String:
                return Literal(token.Arg);

            default:
                return Return;
        }
    }

    private PathExpressionFn FilterSelector(ITokenReader reader)
    {
        UseArray();
        var filter = BuildExpression(reader, PathTokenType.EndFilter);
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            var result = new List<object>();
            var success = 0;
            foreach (var i in GetAll(input))
            {
                if (!filter(context, i))
                    continue;

                if (!next(context, i, out var items, out _))
                    continue;

                success++;
                result.AddRange(items);
            }
            value = success > 0 ? result.ToArray() : null;
            enumerable = value != null;
            return success > 0;
        };
    }

    private PathExpressionFn IndexSelector(ITokenReader reader, int index)
    {
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            value = null;
            enumerable = false;
            return ObjectHelper.TryIndexValue(input, index, out var item) && next(context, item, out value, out enumerable);
        };
    }

    private PathExpressionFn IndexWildSelector(ITokenReader reader)
    {
        UseArray();
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            var result = new List<object>();
            var success = 0;
            foreach (var i in GetAll(input))
            {
                if (!next(context, i, out var items, out _))
                    continue;

                success++;
                result.AddRange(items);
            }
            value = success > 0 ? result.ToArray() : null;
            enumerable = value != null;
            return success > 0;
        };
    }

    private PathExpressionFn ArraySliceSelector(ITokenReader reader, int?[] arg)
    {
        UseArray();
        var next = BuildSelector(reader);
        var step = arg[2].GetValueOrDefault(1);
        var start = arg[0].GetValueOrDefault(step >= 0 ? 0 : -1);
        var end = arg[1];
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            var result = new List<object>();
            var currentIndex = start;
            while ((!end.HasValue || (step > 0 && currentIndex < end) || (step < 0 && currentIndex > end)) && ObjectHelper.TryIndexValue(input, currentIndex, out var slice))
            {
                currentIndex += step;
                if (!next(context, slice, out var items, out _))
                    continue;

                result.AddRange(items);
            }
            enumerable = true;
            value = result.ToArray();
            return true;
        };
    }

    private PathExpressionFn DotSelector(ITokenReader reader, string memberName, PathTokenOption option)
    {
        var caseSensitiveFlag = option == PathTokenOption.CaseSensitive;
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            value = null;
            enumerable = false;
            var caseSensitive = context.CaseSensitive != caseSensitiveFlag;
            return ObjectHelper.TryPropertyValue(input, memberName, caseSensitive, out var item) && next(context, item, out value, out enumerable);
        };
    }

    private PathExpressionFn DescendantSelector(ITokenReader reader, string memberName, PathTokenOption option)
    {
        var caseSensitiveFlag = option == PathTokenOption.CaseSensitive;
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            var caseSensitive = context.CaseSensitive != caseSensitiveFlag;
            var result = new List<object>();
            var success = 0;
            foreach (var i in GetAllRecurse(input, memberName, caseSensitive, 0))
            {
                if (!next(context, i, out var items, out _))
                    continue;

                success++;
                result.AddRange(items);
            }
            value = success > 0 ? result.ToArray() : null;
            enumerable = value != null;
            return success > 0;
        };
    }

    private PathExpressionFn CurrentRef(ITokenReader reader)
    {
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) => next(context, input, out value, out enumerable);
    }

    private PathExpressionFn RootRef(ITokenReader reader)
    {
        var next = BuildSelector(reader);
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) => next(context, context.Input, out value, out enumerable);
    }

    private PathExpressionFilterFn BuildExpression(ITokenReader reader, PathTokenType stop)
    {
        var result = new Stack<PathExpressionFilterFn>(4);
        while (reader.Next(out var token) && token.Type != stop)
        {
            if (token.Type == PathTokenType.LogicalOperator && token.As<FilterOperator>() == FilterOperator.Or)
                continue;

            if (token.Type == PathTokenType.LogicalOperator && token.As<FilterOperator>() == FilterOperator.And)
            {
                var left = result.Pop();
                var right = BuildBasicExpression(reader);
                result.Push((IPathExpressionContext context, object input) =>
                {
                    // All expression must return true
                    return left(context, input) && right(context, input);
                });
                continue;
            }
            result.Push(BuildBasicExpression(reader));
        }
        var expressions = result.ToArray();
        return (IPathExpressionContext context, object input) =>
        {
            // Any one expression returns true
            for (var i = 0; i < expressions.Length; i++)
                if (expressions[i](context, input))
                    return true;

            return false;
        };
    }

    private PathExpressionFilterFn BuildBasicExpression(ITokenReader reader)
    {
        var token = reader.Current;
        switch (token.Type)
        {
            case PathTokenType.NotOperator:
                if (reader.Consume(PathTokenType.StartGroup))
                    return NotCondition(BuildExpression(reader, PathTokenType.EndGroup));

                return NotCondition(ExistCondition(BuildSelector(reader)));

            case PathTokenType.StartGroup:
                return BuildExpression(reader, PathTokenType.EndGroup);

            default:
                return BuildRelationExpression(reader);
        }
    }

    private PathExpressionFilterFn BuildRelationExpression(ITokenReader reader)
    {
        var left = BuildSelector(reader);
        if (reader.Current.Type == PathTokenType.ComparisonOperator)
        {
            var op = reader.Current;
            var right = BuildSelector(reader);
            return BinaryCondition(left, right, op.As<FilterOperator>());
        }
        return ExistCondition(left);
    }

    private static PathExpressionFilterFn ExistCondition(PathExpressionFn next)
    {
        return (IPathExpressionContext context, object input) => next(context, input, out _, out _);
    }

    private static PathExpressionFilterFn NotCondition(PathExpressionFilterFn next)
    {
        return (IPathExpressionContext context, object input) => !next(context, input);
    }

    private static PathExpressionFilterFn BinaryCondition(PathExpressionFn left, PathExpressionFn right, FilterOperator op)
    {
        return (IPathExpressionContext context, object input) =>
        {
            if (!left(context, input, out var leftValue, out _) || !right(context, input, out var rightValue, out _))
                return false;

            var operand1 = leftValue.FirstOrDefault();
            var operand2 = rightValue.FirstOrDefault();

            // Get the specific operator
            switch (op)
            {
                case FilterOperator.Equal:
                    return ExpressionHelpers.Equal(operand1, operand2, context.CaseSensitive);

                case FilterOperator.NotEqual:
                    return !ExpressionHelpers.Equal(operand1, operand2, context.CaseSensitive);

                case FilterOperator.Less:
                    return ExpressionHelpers.CompareNumeric(operand1, operand2, convert: false, compare: out var compare, value: out _) && compare < 0;

                case FilterOperator.LessOrEqual:
                    return ExpressionHelpers.CompareNumeric(operand1, operand2, convert: false, compare: out compare, value: out _) && compare <= 0;

                case FilterOperator.Greater:
                    return ExpressionHelpers.CompareNumeric(operand1, operand2, convert: false, compare: out compare, value: out _) && compare > 0;

                case FilterOperator.GreaterOrEqual:
                    return ExpressionHelpers.CompareNumeric(operand1, operand2, convert: false, compare: out compare, value: out _) && compare >= 0;

                case FilterOperator.RegEx:
                    return ExpressionHelpers.Match(operand1, operand2, context.CaseSensitive);
            }
            return false;
        };
    }

    private static bool Return(IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable)
    {
        // Unwrap primitive types
        if (input is JValue jValue && (jValue.Type == JTokenType.String || jValue.Type == JTokenType.Integer || jValue.Type == JTokenType.Boolean))
            input = jValue.Value;

        if (input is PSObject pso && pso.BaseObject is IEnumerable e && pso.BaseObject is not string)
            input = e.Cast<object>().ToArray();

        enumerable = false;
        if (input is object[] eo)
        {
            enumerable = true;
            value = eo;
        }
        else
            value = [input];

        return true;
    }

    private static PathExpressionFn Literal(object arg)
    {
        var isEnumerable = arg is object[];
        var result = isEnumerable ? arg as object[] : [arg];
        return (IPathExpressionContext context, object input, out IEnumerable<object> value, out bool enumerable) =>
        {
            value = result;
            enumerable = isEnumerable;
            return true;
        };
    }

    #region Enumerators

    private static IEnumerable<object> GetAll(object o)
    {
        var baseObject = ExpressionHelpers.GetBaseObject(o);
        if (IsSimpleType(baseObject))
            return DEFAULT_EMPTY_ARRAY;

        if (baseObject is JObject jObject)
            return GetAllField(jObject);

        if (baseObject is JArray jArray)
            return GetAllIndex(jArray);

        return baseObject is IEnumerable ? GetAllIndex(baseObject) : GetAllField(baseObject);
    }

    private static bool IsSimpleType(object o)
    {
        return o is string || (o != null && o.GetType().IsValueType);
    }

    private IEnumerable<object> GetAllRecurse(object o, string fieldName, bool caseSensitive, int depth)
    {
        if (depth > _RecurseMaxDepth)
            throw new ObjectPathEvaluateException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.ObjectPathRecurseMaxDepth, _RecurseMaxDepth, fieldName));

        foreach (var i in GetAll(o))
        {
            if (ObjectHelper.TryPropertyValue(i, fieldName, caseSensitive, out var value))
            {
                yield return value;
            }
            else
            {
                foreach (var c in GetAllRecurse(i, fieldName, caseSensitive, depth + 1))
                    yield return c;
            }
        }
    }

    private static IEnumerable<object> GetAllIndex(object o)
    {
        if (o is IEnumerable enumerable)
            foreach (var i in enumerable)
                yield return i;
    }

    private static IEnumerable<object> GetAllField(object o)
    {
        var baseObject = ExpressionHelpers.GetBaseObject(o);
        if (baseObject == null)
            yield break;

        // Handle dictionaries and hash tables
        if (baseObject is IDictionary dictionary)
        {
            foreach (var value in dictionary.Values)
                yield return value;
        }
        // Handle PSObjects
        else if (o is PSObject psObject)
        {
            foreach (var property in psObject.Properties)
                yield return property.Value;
        }
        // Handle JObject
        else if (o is JObject jObject)
        {
            foreach (var property in jObject.Properties())
                yield return property.Value;
        }
        // Handle DynamicObjects
        else if (o is DynamicObject)
        {

        }
        // Handle all other CLR types
        else
        {
            var baseType = baseObject.GetType();
            var properties = baseType.GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; properties != null && i < properties.Length; i++)
                yield return properties[i].GetValue(baseObject);

            var fields = baseType.GetFields(bindingAttr: BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; fields != null && i < fields.Length; i++)
                yield return fields[i].GetValue(baseObject);
        }
    }

    #endregion Enumerators
}
