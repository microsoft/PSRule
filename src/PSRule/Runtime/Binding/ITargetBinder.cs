// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

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
    ITargetBindingResult Bind(ITargetObject targetObject);
}

#nullable restore
