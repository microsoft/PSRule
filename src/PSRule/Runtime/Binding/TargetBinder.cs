// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

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
    public ITargetBindingResult Bind(TargetObject targetObject)
    {
        return _BindingContext.Bind(targetObject);
    }

    /// <inheritdoc/>
    public ITargetBindingResult Bind(object targetObject)
    {
        return _BindingContext.Bind(targetObject);
    }
}
