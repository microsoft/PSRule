// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Runtime.Binding;

#nullable enable

/// <summary>
/// Responsible for handling binding for a given target object.
/// </summary>
internal interface ITargetBinder
{
    /// <summary>
    /// Bind to an object.
    /// </summary>
    ITargetBindingResult Bind(TargetObject targetObject);

    /// <summary>
    /// Bind to an object.
    /// </summary>
    ITargetBindingResult Bind(object targetObject);
}

#nullable restore
