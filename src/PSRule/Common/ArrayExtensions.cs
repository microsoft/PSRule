// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Common;

/// <summary>
/// Extension methods for arrays.
/// </summary>
internal static class ArrayExtensions
{
    internal static object? Last(this Array array)
    {
        return array.Length > 0 ? array.GetValue(array.Length - 1) : null;
    }

    internal static object? First(this Array array)
    {
        return array.Length > 0 ? array.GetValue(0) : null;
    }
}
