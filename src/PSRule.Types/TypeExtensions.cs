// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace PSRule;

internal static class TypeExtensions
{
    public static bool TryGetPropertyInfo(this Type type, string propertyName, out PropertyInfo? value)
    {
        value = null;
        if (type == null || propertyName == null)
            return false;

        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo == null)
            return false;

        value = propertyInfo;
        return true;
    }
}
