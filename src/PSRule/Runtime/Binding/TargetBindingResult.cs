// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Runtime.Binding;

internal sealed class TargetBindingResult : ITargetBindingResult
{
    public TargetBindingResult(string targetName, string targetNamePath, string targetType, string targetTypePath, bool shouldFilter, Hashtable? field)
    {
        TargetName = targetName;
        TargetNamePath = targetNamePath;
        TargetType = targetType;
        TargetTypePath = targetTypePath;
        ShouldFilter = shouldFilter;
        Field = field;
    }

    /// <inheritdoc/>
    public string TargetName { get; }

    /// <inheritdoc/>
    public string TargetNamePath { get; }

    /// <inheritdoc/>
    public string TargetType { get; }

    /// <inheritdoc/>
    public string TargetTypePath { get; }

    /// <inheritdoc/>
    public bool ShouldFilter { get; }

    /// <inheritdoc/>
    public Hashtable? Field { get; }
}
