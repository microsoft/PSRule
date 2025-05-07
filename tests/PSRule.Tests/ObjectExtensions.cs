// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule;

#nullable enable

/// <summary>
/// Extension to make working with anonymous objects easier in tests.
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    /// Get the value of a property from an object.
    /// </summary>
    public static object? PropertyValue(this object o, string propertyName, bool caseSensitive = false)
    {
        return ObjectHelper.TryPropertyValue(o, propertyName, caseSensitive, out var result) ? result : default;
    }

    /// <summary>
    /// Get the value of a property from an object.
    /// </summary>
    public static T? PropertyValue<T>(this object o, string propertyName)
    {
        var result = o.PropertyValue(propertyName);
        if (result == null)
            return default;

        if (result is T t)
            return t;

        var actualType = result.GetType();
        if (typeof(T).IsAssignableFrom(actualType))
            return (T)result;

        if (typeof(T) == typeof(int) && Converters.TypeConverter.TryInt(result, true, out var intValue))
            return (T)(object)intValue;

        return default;
    }
}

#nullable disable
