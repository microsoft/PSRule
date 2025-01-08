// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Runtime.Binding;

/// <summary>
/// Responsible for handling binding for a given target object.
/// </summary>
internal sealed class TargetBinder : ITargetBinder
{
    private readonly ITargetBindingContext _BindingContext;

    internal TargetBinder(ITargetBindingContext bindingContext)
    {
        _BindingContext = bindingContext;
    }

    /// <inheritdoc/>
    public ITargetBindingResult Bind(ITargetObject targetObject)
    {
        return _BindingContext.Bind(targetObject);
    }
}
