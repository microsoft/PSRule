// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Management.Automation;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json.Linq;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Runtime;

/// <summary>
/// A helper class to traverse object properties.
/// </summary>
internal static class ObjectHelper
{
    public static bool GetPath(IBindingContext bindingContext, object targetObject, string path, bool caseSensitive, out object value)
    {
        var expression = GetPathExpression(bindingContext, path);
        return expression.TryGet(targetObject, caseSensitive, out value);
    }

    public static bool GetPath(IBindingContext bindingContext, object targetObject, string path, bool caseSensitive, out object[] value)
    {
        var expression = GetPathExpression(bindingContext, path);
        return expression.TryGet(targetObject, caseSensitive, out value);
    }

    public static bool TryPropertyValue(object o, string propertyName, bool caseSensitive, out object? value)
    {
        value = null;
        var baseObject = ExpressionHelpers.GetBaseObject(o);
        if (baseObject == null || (baseObject is JValue jValue && jValue.Type == JTokenType.Null) ||
            baseObject is string || baseObject is int || baseObject is long || baseObject is float)
            return false;

        // Handle dictionaries and hash tables
        if (baseObject is IDictionary dictionary)
        {
            return TryDictionary(dictionary, propertyName, caseSensitive, out value);
        }
        // Handle JToken
        else if (baseObject is JObject jObject)
        {
            return TryPropertyValueFromJObject(jObject, propertyName, caseSensitive, out value);
        }
        // Handle PSObjects
        else if (o is PSObject psObject)
        {
            return TryPropertyValueFromPSObject(psObject, propertyName, caseSensitive, out value);
        }
        // Handle DynamicObjects
        else if (o is DynamicObject dynamicObject)
        {
            return TryPropertyValueFromDynamic(dynamicObject, propertyName, caseSensitive, out value);
        }
        else if (o is XmlNode xmlNode)
        {
            // Try attribute first.
            var item = xmlNode.Attributes?.GetNamedItem(propertyName);
            if (item != null)
            {
                value = item.Value;
                return true;
            }

            // Try elements next.
            var nodes = xmlNode.SelectNodes(propertyName);
            if (nodes != null && nodes.Count == 1)
            {
                value = nodes[0];
                return true;
            }

            // Try to get the value of the node.
            if (nodes != null && nodes.Count == 0 && string.Equals(propertyName, "InnerText", caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            {
                value = xmlNode.InnerText;
                return value != null;
            }

            value = nodes.Count > 1 ? nodes : null;
            return value != null;
        }

        // Handle all other CLR types
        var baseType = baseObject.GetType();
        return TryPropertyValueFromCLR(o, propertyName, baseType, caseSensitive, out value) ||
               TryFieldValue(o, propertyName, baseType, caseSensitive, out value) ||
               TryIndexerProperty(o, propertyName, baseType, out value);
    }

    public static bool TryIndexValue(object o, int index, out object? value)
    {
        value = null;
        var baseObject = ExpressionHelpers.GetBaseObject(o);
        if (baseObject == null || baseObject is string || baseObject is int || baseObject is long || baseObject is float)
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

    private static bool TryDictionary(IDictionary dictionary, string key, bool caseSensitive, out object? value)
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

    private static bool TryPropertyValueFromCLR(object targetObject, string propertyName, Type baseType, bool caseSensitive, out object? value)
    {
        value = null;
        var bindingFlags = caseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase;
        var propertyInfo = baseType.GetProperty(propertyName, bindingAttr: bindingFlags | BindingFlags.Instance | BindingFlags.Public);
        if (propertyInfo == null)
            return false;

        value = propertyInfo.GetValue(targetObject);
        return true;
    }

    private static bool TryFieldValue(object targetObject, string fieldName, Type baseType, bool caseSensitive, out object? value)
    {
        value = null;
        var bindingFlags = caseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase;
        var fieldInfo = baseType.GetField(fieldName, bindingAttr: bindingFlags | BindingFlags.Instance | BindingFlags.Public);
        if (fieldInfo == null)
            return false;

        value = fieldInfo.GetValue(targetObject);
        return true;
    }

    private static bool TryPropertyValueFromPSObject(PSObject targetObject, string propertyName, bool caseSensitive, out object? value)
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

    private static bool TryPropertyValueFromJObject(JObject targetObject, string propertyName, bool caseSensitive, out object? value)
    {
        value = null;
        if (!targetObject.TryGetValue(propertyName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase, out var result))
            return false;

        value = GetTokenValue(result);
        return true;
    }

    private static bool TryPropertyValueFromDynamic(DynamicObject targetObject, string propertyName, bool caseSensitive, out object? value)
    {
        return targetObject.TryGetMember(new DynamicPropertyBinder(propertyName, !caseSensitive), out value);
    }

    private static object? GetTokenValue(JToken o)
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

    private static bool TryIndexerProperty(object targetObject, object index, Type baseType, out object? value)
    {
        value = null;
        var properties = baseType.GetProperties();
        foreach (var property in GetIndexerProperties(baseType))
        {
            var parameters = property.GetIndexParameters();
            if (parameters.Length > 0)
            {
                try
                {
                    var converter = GetConverter(parameters[0].ParameterType);
                    var p1 = converter(index);
                    value = property.GetValue(targetObject, [p1]);
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
        var convertAttribute = targetType.GetCustomAttribute<TypeConverterAttribute>();
        if (convertAttribute != null)
        {
            var converterType = Type.GetType(convertAttribute.ConverterTypeName);
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
        var properties = baseType.GetProperties();
        foreach (var property in properties)
        {
            if (attribute != null)
            {
                if (property.Name == attribute.MemberName)
                    yield return property;
            }
            else
            {
                var parameters = property.GetIndexParameters();
                if (parameters.Length > 0)
                    yield return property;
            }
        }
    }

    private static bool TryEnumerableIndex(IEnumerable o, int index, out object? value)
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

    /// <summary>
    /// Get a token for the specified name either by creating or reading from cache.
    /// </summary>
    [DebuggerStepThrough]
    private static PathExpression GetPathExpression(IBindingContext bindingContext, string path)
    {
        // Try to load nameToken from cache
        if (bindingContext == null || !bindingContext.GetPathExpression(path, out var expression))
        {
            expression = PathExpression.Create(path);
            bindingContext?.CachePathExpression(path, expression);
        }
        return expression;
    }
}
