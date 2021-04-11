// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Management.Automation;
using System.Reflection;
using System.Threading;

namespace PSRule.Runtime
{
    /// <summary>
    /// A helper class to traverse object properties.
    /// </summary>
    internal static class ObjectHelper
    {
        private sealed class NameTokenStream
        {
            private const char Separator = '.';
            private const char Quoted = '\'';
            private const char OpenIndex = '[';
            private const char CloseIndex = ']';

            private readonly string Name;
            private readonly int Last;

            private bool inQuote;
            private bool inIndex;

            public int Position = -1;
            public char Current;

            public NameTokenStream(string name)
            {
                Name = name;
                Last = Name.Length - 1;
            }

            /// <summary>
            /// Find the start of the sequence.
            /// </summary>
            /// <returns>Return true when more characters follow.</returns>
            public bool Next()
            {
                if (Position < Last)
                {
                    Position++;
                    if (Name[Position] == Separator && Position > 0)
                    {
                        Position++;
                    }
                    else if (Name[Position] == Quoted)
                    {
                        Position++;
                        inQuote = true;
                    }
                    Current = Name[Position];
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Find the end of the sequence and return the index.
            /// </summary>
            /// <returns>The index of the sequence end.</returns>
            public int IndexOf(out NameTokenType tokenType)
            {
                tokenType = Position == 0 && Current == Separator ? NameTokenType.Self : NameTokenType.Field;
                if (tokenType == NameTokenType.Self)
                    return Position;

                while (Position < Last)
                {
                    Position++;
                    Current = Name[Position];

                    if (inQuote)
                    {
                        if (Current == Quoted)
                        {
                            inQuote = false;
                            return Position - 1;
                        }
                    }
                    else if (Current == Separator)
                    {
                        return Position - 1;
                    }
                    else if (inIndex)
                    {
                        if (Current == CloseIndex)
                        {
                            tokenType = NameTokenType.Index;
                            inIndex = false;
                            return Position - 1;
                        }
                    }
                    else if (Current == OpenIndex)
                    {
                        // Next token will be an Index
                        inIndex = true;

                        // Return end of token
                        return Position - 1;
                    }
                }
                return Position;
            }

            public NameToken Get()
            {
                var token = new NameToken();
                NameToken result = token;
                while (Next())
                {
                    var start = Position;
                    if (start > 0)
                    {
                        token.Next = new NameToken();
                        token = token.Next;
                    }

                    // Jump to the next separator or end
                    var end = IndexOf(out NameTokenType tokenType);
                    token.Type = tokenType;
                    if (tokenType == NameTokenType.Field)
                    {
                        token.Name = Name.Substring(start, end - start + 1);
                    }
                    else if (tokenType == NameTokenType.Index)
                    {
                        token.Index = int.Parse(Name.Substring(start, end - start + 1), Thread.CurrentThread.CurrentCulture);
                    }
                }
                return result;
            }
        }

        private sealed class DynamicPropertyBinder : GetMemberBinder
        {
            internal DynamicPropertyBinder(string name, bool ignoreCase)
                : base(name, ignoreCase) { }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                return null;
            }
        }

        public static bool GetField(PSObject targetObject, string name, bool caseSensitive, out object value)
        {
            if (targetObject.BaseObject is IDictionary dictionary)
                return TryDictionary(dictionary, name, caseSensitive, out value);

            return TryPropertyValue(targetObject, name, caseSensitive, out value);
        }

        public static bool GetField(IBindingContext bindingContext, object targetObject, string name, bool caseSensitive, out object value)
        {
            // Try to load nameToken from cache
            if (bindingContext == null || !bindingContext.GetNameToken(expression: name, nameToken: out NameToken nameToken))
            {
                nameToken = GetNameToken(expression: name);
                if (bindingContext != null)
                    bindingContext.CacheNameToken(expression: name, nameToken: nameToken);
            }
            return GetField(targetObject: targetObject, token: nameToken, caseSensitive: caseSensitive, value: out value);
        }

        private static bool GetField(object targetObject, NameToken token, bool caseSensitive, out object value)
        {
            value = null;
            var baseObject = GetBaseObject(targetObject);
            if (baseObject == null)
                return false;

            var baseType = baseObject.GetType();
            object field = null;
            bool foundField = false;

            // Handle this object
            if (token.Type == NameTokenType.Self)
            {
                field = baseObject;
                foundField = true;
            }
            // Handle dictionaries and hashtables
            else if (token.Type == NameTokenType.Field && baseObject is IDictionary dictionary)
            {
                if (TryDictionary(dictionary, token.Name, caseSensitive, out field))
                    foundField = true;
            }
            // Handle PSObjects
            else if (token.Type == NameTokenType.Field && targetObject is PSObject psObject)
            {
                if (TryPropertyValue(psObject, token.Name, caseSensitive, out field))
                    foundField = true;
            }
            // Handle DynamicObjects
            else if (token.Type == NameTokenType.Field && targetObject is DynamicObject dynamicObject)
            {
                if (TryPropertyValue(dynamicObject, token.Name, caseSensitive, out field))
                    foundField = true;
            }
            // Handle all other CLR types
            else if (token.Type == NameTokenType.Field)
            {
                if (TryPropertyValue(targetObject, token.Name, baseType, caseSensitive, out field) ||
                    TryFieldValue(targetObject, token.Name, baseType, caseSensitive, out field) ||
                    TryIndexerProperty(targetObject, token.Name, baseType, out field))
                    foundField = true;
            }
            // Handle array indexes
            else if (token.Type == NameTokenType.Index && baseType.IsArray && baseObject is Array array && token.Index < array.Length)
            {
                field = array.GetValue(token.Index);
                foundField = true;
            }
            // Handle IList
            else if (token.Type == NameTokenType.Index && baseObject is IList list && token.Index < list.Count)
            {
                field = list[token.Index];
                foundField = true;
            }
            // Handle IEnumerable
            else if (token.Type == NameTokenType.Index && baseObject is IEnumerable enumerable && TryEnumerableIndex(enumerable, token.Index, out object element))
            {
                field = element;
                foundField = true;
            }
            else if (token.Type == NameTokenType.Index)
            {
                if (TryIndexerProperty(targetObject, token.Index, baseType, out field))
                    foundField = true;
            }

            if (foundField)
            {
                if (token.Next == null)
                {
                    value = field;
                    return true;
                }
                else
                {
                    return GetField(targetObject: field, token: token.Next, caseSensitive: caseSensitive, value: out value);
                }
            }
            return false;
        }

        private static bool TryEnumerableIndex(IEnumerable o, int index, out object value)
        {
            value = null;
            var e = o.GetEnumerator();
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

        private static bool TryPropertyValue(DynamicObject targetObject, string propertyName, bool caseSensitive, out object value)
        {
            if (!targetObject.TryGetMember(new DynamicPropertyBinder(propertyName, !caseSensitive), out value))
                return false;

            return true;
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
                foreach(PropertyInfo pi in properties)
                {
                    var p = pi.GetIndexParameters();
                    if (p.Length > 0)
                        yield return pi;
                }
            }
        }

        private static NameToken GetNameToken(string expression)
        {
            var stream = new NameTokenStream(expression);
            return stream.Get();
        }

        private static object GetBaseObject(object value)
        {
            if (value == null)
                return null;

            if (value is PSObject ovalue)
            {
                var baseObject = ovalue.BaseObject;
                if (baseObject != null)
                    return baseObject;
            }
            return value;
        }
    }
}
