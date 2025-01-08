// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Runtime.Binding;

/// <summary>
/// A binding context specific to a language scope.
/// </summary>
internal interface ITargetBindingContext
{
    ITargetBindingResult Bind(ITargetObject o);
}
