// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace PSRule.Runtime.ObjectPath
{
    /// <summary>
    /// A helper class to build an expression tree from path tokens.
    /// </summary>
    internal sealed class PathExpressionBuilder
    {
        private sealed class DynamicPropertyBinder : GetMemberBinder
        {
            internal DynamicPropertyBinder(string name, bool ignoreCase)
                : base(name, ignoreCase) { }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                return null;
            }
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
            if (!reader.Peak(out IPathToken token) || token.Type == PathTokenType.EndFilter || token.Type == PathTokenType.EndGroup)
                return Return;

            reader.Next(out token);
            switch (token.Type)
            {
                case PathTokenType.DotSelector:
                    return DotSelector(reader, token.As<string>(), token.Option);

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
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) =>
            {
                var result = new List<object>();
                var success = 0;
                foreach (var i in GetAll(input))
                {
                    if (!filter(context, i))
                        continue;

                    if (!next(context, i, out IEnumerable<object> items))
                        continue;

                    success++;
                    result.AddRange(items);
                }
                value = success > 0 ? result.ToArray() : null;
                return success > 0;
            };
        }

        private PathExpressionFn IndexSelector(ITokenReader reader, int index)
        {
            var next = BuildSelector(reader);
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) =>
            {
                value = null;
                if (!TryGetIndex(input, index, out object item))
                    return false;

                return next(context, item, out value);
            };
        }

        private PathExpressionFn IndexWildSelector(ITokenReader reader)
        {
            UseArray();
            var next = BuildSelector(reader);
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) =>
            {
                var result = new List<object>();
                var success = 0;
                foreach (var i in GetAll(input))
                {
                    if (!next(context, i, out IEnumerable<object> items))
                        continue;

                    success++;
                    result.AddRange(items);
                }
                value = success > 0 ? result.ToArray() : null;
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
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) =>
            {
                var result = new List<object>();
                var currentIndex = start;
                while ((!end.HasValue || (step > 0 && currentIndex < end) || (step < 0 && currentIndex > end)) && TryGetIndex(input, currentIndex, out object slice))
                {
                    currentIndex += step;
                    if (!next(context, slice, out IEnumerable<object> items))
                        continue;

                    result.AddRange(items);
                }
                value = result.ToArray();
                return true;
            };
        }

        private PathExpressionFn DotSelector(ITokenReader reader, string memberName, PathTokenOption option)
        {
            var caseSensitiveFlag = option == PathTokenOption.CaseSensitive;
            var next = BuildSelector(reader);
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) =>
            {
                value = null;
                var caseSensitive = context.CaseSensitive != caseSensitiveFlag;
                if (!TryGetField(input, memberName, caseSensitive, out object item))
                    return false;

                return next(context, item, out value);
            };
        }

        private PathExpressionFn CurrentRef(ITokenReader reader)
        {
            var next = BuildSelector(reader);
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) => next(context, input, out value);
        }

        private PathExpressionFn RootRef(ITokenReader reader)
        {
            var next = BuildSelector(reader);
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) => next(context, context.Input, out value);
        }

        private PathExpressionFilterFn BuildExpression(ITokenReader reader, PathTokenType stop)
        {
            var result = new Stack<PathExpressionFilterFn>(4);
            while (reader.Next(out IPathToken token) && token.Type != stop)
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
            return (IPathExpressionContext context, object input) => next(context, input, out _);
        }

        private static PathExpressionFilterFn NotCondition(PathExpressionFilterFn next)
        {
            return (IPathExpressionContext context, object input) => !next(context, input);
        }

        private static PathExpressionFilterFn BinaryCondition(PathExpressionFn left, PathExpressionFn right, FilterOperator op)
        {
            return (IPathExpressionContext context, object input) =>
            {
                if (!left(context, input, out IEnumerable<object> leftValue) || !right(context, input, out IEnumerable<object> rightValue))
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
                        return ExpressionHelpers.CompareNumeric(operand1, operand2, convert: false, compare: out int compare, value: out _) && compare < 0;

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

        private static bool Return(IPathExpressionContext context, object input, out IEnumerable<object> value)
        {
            // Unwrap primitive types
            if (input is JValue jValue && (jValue.Type == JTokenType.String || jValue.Type == JTokenType.Integer || jValue.Type == JTokenType.Boolean))
                input = jValue.Value;

            value = new object[] { input };
            return true;
        }

        private static PathExpressionFn Literal(object arg)
        {
            var result = new object[] { arg };
            return (IPathExpressionContext context, object input, out IEnumerable<object> value) =>
            {
                value = result;
                return true;
            };
        }

        #region Enumerators

        private static IEnumerable<object> GetAll(object o)
        {
            var baseObject = GetBaseObject(o);
            if (baseObject is IEnumerable)
                return GetAllIndex(baseObject);

            return GetAllField(baseObject);
        }

        private static IEnumerable<object> GetAllIndex(object o)
        {
            if (o is IEnumerable enumerable)
                foreach (var i in enumerable)
                    yield return i;
        }

        private static IEnumerable<object> GetAllField(object o)
        {
            var baseObject = GetBaseObject(o);
            if (baseObject == null)
                yield break;

            // Handle dictionaries and hashtables
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
            // Handle DynamicObjects
            else if (o is DynamicObject dynamicObject)
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

        #region Lookup

        private static bool TryGetField(object o, string fieldName, bool caseSensitive, out object value)
        {
            value = null;
            var baseObject = GetBaseObject(o);
            if (baseObject == null || (baseObject is JValue jValue && jValue.Type == JTokenType.Null))
                return false;

            // Handle dictionaries and hashtables
            if (baseObject is IDictionary dictionary)
            {
                return TryDictionary(dictionary, fieldName, caseSensitive, out value);
            }
            // Handle JToken
            else if (baseObject is JObject jObject)
            {
                return TryPropertyValue(jObject, fieldName, caseSensitive, out value);
            }
            // Handle PSObjects
            else if (o is PSObject psObject)
            {
                return TryPropertyValue(psObject, fieldName, caseSensitive, out value);
            }
            // Handle DynamicObjects
            else if (o is DynamicObject dynamicObject)
            {
                return TryPropertyValue(dynamicObject, fieldName, caseSensitive, out value);
            }
            // Handle all other CLR types
            var baseType = baseObject.GetType();
            return TryPropertyValue(o, fieldName, baseType, caseSensitive, out value) ||
                   TryFieldValue(o, fieldName, baseType, caseSensitive, out value) ||
                   TryIndexerProperty(o, fieldName, baseType, out value);
        }

        private static bool TryGetIndex(object o, int index, out object value)
        {
            value = null;
            var baseObject = GetBaseObject(o);
            if (baseObject == null)
                return false;

            // Handle array indexes
            if (baseObject is Array array && index < array.Length)
            {
                if (index < 0)
                    index = array.Length + index;

                if (index < 0 || index >= array.Length)
                    return false;

                value = array.GetValue(index);
                return true;
            }
            // Handle IList
            else if (baseObject is IList list && index < list.Count)
            {
                if (index < 0)
                    index = list.Count + index;

                if (index < 0 || index >= list.Count)
                    return false;

                value = list[index];
                return true;
            }
            // Handle IEnumerable
            else if (baseObject is IEnumerable enumerable)
            {
                return TryEnumerableIndex(enumerable, index, out value);
            }
            // Handle all other CLR types
            return TryIndexerProperty(o, index, baseObject.GetType(), out value);
        }

        #endregion Lookup

        private static bool TryEnumerableIndex(IEnumerable o, int index, out object value)
        {
            value = null;
            var e = o.GetEnumerator();
            if (index < 0)
            {
                var items = new List<object>();
                while (e.MoveNext())
                    items.Add(e.Current);

                index = items.Count + index;
                if (index < 0 || index >= items.Count)
                    return false;

                value = items[index];
                return true;
            }

            for (var i = 0; e.MoveNext(); i++)
            {
                if (i == index)
                {
                    value = e.Current;
                    return true;
                }
            }
            return false;
        }

        private static bool TryDictionary(IDictionary dictionary, string key, bool caseSensitive, out object value)
        {
            value = null;
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            foreach (var k in dictionary.Keys)
            {
                if (comparer.Equals(key, k))
                {
                    value = dictionary[k];
                    return true;
                }
            }
            return false;
        }

        private static bool TryPropertyValue(object targetObject, string propertyName, Type baseType, bool caseSensitive, out object value)
        {
            value = null;
            var bindingFlags = caseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase;
            var propertyInfo = baseType.GetProperty(propertyName, bindingAttr: bindingFlags | BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null)
                return false;

            value = propertyInfo.GetValue(targetObject);
            return true;
        }

        private static bool TryPropertyValue(PSObject targetObject, string propertyName, bool caseSensitive, out object value)
        {
            value = null;
            var p = targetObject.Properties[propertyName];
            if (p == null)
                return false;

            if (caseSensitive && !StringComparer.Ordinal.Equals(p.Name, propertyName))
                return false;

            value = p.Value;
            return true;
        }

        private static bool TryPropertyValue(JObject targetObject, string propertyName, bool caseSensitive, out object value)
        {
            value = null;
            if (!targetObject.TryGetValue(propertyName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase, out JToken result))
                return false;

            value = GetTokenValue(result);
            return true;
        }

        private static bool TryPropertyValue(DynamicObject targetObject, string propertyName, bool caseSensitive, out object value)
        {
            if (!targetObject.TryGetMember(new DynamicPropertyBinder(propertyName, !caseSensitive), out value))
                return false;

            return true;
        }

        private static object GetTokenValue(JToken o)
        {
            if (o == null || o.Type == JTokenType.Null)
                return null;

            if (o.Type == JTokenType.String)
                return o.Value<string>();

            if (o.Type == JTokenType.Boolean)
                return o.Value<bool>();

            if (o.Type == JTokenType.Integer)
                return o.Value<long>();

            return o;
        }

        private static bool TryFieldValue(object targetObject, string fieldName, Type baseType, bool caseSensitive, out object value)
        {
            value = null;
            var bindingFlags = caseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase;
            var fieldInfo = baseType.GetField(fieldName, bindingAttr: bindingFlags | BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo == null)
                return false;

            value = fieldInfo.GetValue(targetObject);
            return true;
        }

        private static bool TryIndexerProperty(object targetObject, object index, Type baseType, out object value)
        {
            value = null;
            var properties = baseType.GetProperties();
            foreach (PropertyInfo pi in GetIndexerProperties(baseType))
            {
                var p = pi.GetIndexParameters();
                if (p.Length > 0)
                {
                    try
                    {
                        var converter = GetConverter(p[0].ParameterType);
                        var p1 = converter(index);
                        value = pi.GetValue(targetObject, new object[] { p1 });
                        return true;
                    }
                    catch
                    {
                        // Discard converter exceptions
                    }
                }
            }
            return false;
        }

        private static Converter<object, object> GetConverter(Type targetType)
        {
            var convertAtribute = targetType.GetCustomAttribute<TypeConverterAttribute>();
            if (convertAtribute != null)
            {
                var converterType = Type.GetType(convertAtribute.ConverterTypeName);
                if (converterType.IsSubclassOf(typeof(TypeConverter)))
                {
                    var converter = (TypeConverter)Activator.CreateInstance(converterType);
                    return s => converter.ConvertFrom(s);
                }
                else if (converterType.IsSubclassOf(typeof(PSTypeConverter)))
                {
                    var converter = (PSTypeConverter)Activator.CreateInstance(converterType);
                    return s => converter.ConvertFrom(s, targetType, Thread.CurrentThread.CurrentCulture, true);
                }
            }
            return s => Convert.ChangeType(s, targetType);
        }

        private static IEnumerable<PropertyInfo> GetIndexerProperties(Type baseType)
        {
            var attribute = baseType.GetCustomAttribute<DefaultMemberAttribute>();
            if (attribute != null)
            {
                var property = baseType.GetProperty(attribute.MemberName);
                yield return property;
            }
            else
            {
                var properties = baseType.GetProperties();
                foreach (PropertyInfo pi in properties)
                {
                    var p = pi.GetIndexParameters();
                    if (p.Length > 0)
                        yield return pi;
                }
            }
        }

        private static object GetBaseObject(object value)
        {
            return value is PSObject ovalue && ovalue.BaseObject != null && !(ovalue.BaseObject is PSCustomObject) ? ovalue.BaseObject : value;
        }
    }
}
