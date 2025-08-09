// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Runtime.Binding;

/// <summary>
/// Builds a TargetBinder.
/// </summary>
internal sealed class TargetBinderBuilder
{
    private readonly HashSet<string>? _TypeFilter;
    private readonly BindTargetMethod? _BindTargetName;
    private readonly BindTargetMethod? _BindTargetType;
    private readonly BindTargetMethod? _BindField;

    public TargetBinderBuilder(BindTargetMethod? bindTargetName, BindTargetMethod? bindTargetType, BindTargetMethod? bindField, string[]? typeFilter)
    {
        _BindTargetName = bindTargetName;
        _BindTargetType = bindTargetType;
        _BindField = bindField;
        if (typeFilter != null && typeFilter.Length > 0)
            _TypeFilter = new HashSet<string>(typeFilter, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Build a TargetBinder.
    /// </summary>
    public ITargetBinder Build(BindingOption? bindingOption)
    {
        return new TargetBinder(new TargetBindingContext(bindingOption, _BindTargetName, _BindTargetType, _BindField, _TypeFilter));
    }
}
