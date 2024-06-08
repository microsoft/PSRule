// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Configuration;

internal static class BindingOptionExtensions
{
    [DebuggerStepThrough]
    public static StringComparer GetComparer(this BindingOption option)
    {
        return option.IgnoreCase.GetValueOrDefault(BindingOption.Default.IgnoreCase.Value) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }
}
