// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Configuration;

namespace PSRule.Pipeline
{
    internal interface IBindingOption
    {
        FieldMap[] Field { get; }

        bool IgnoreCase { get; }

        string NameSeparator { get; }

        bool PreferTargetInfo { get; }

        string[] TargetName { get; }

        string[] TargetType { get; }

        bool UseQualifiedName { get; }
    }

    internal static class BindingOptionExtensions
    {
        [DebuggerStepThrough]
        public static StringComparer GetComparer(this IBindingOption option)
        {
            return option.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        }
    }
}
