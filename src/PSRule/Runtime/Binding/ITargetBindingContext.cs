// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Runtime.Binding;

/// <summary>
/// A binding context specific to a language scope.
/// </summary>
internal interface ITargetBindingContext
{
    ITargetBindingResult Bind(object o);

    ITargetBindingResult Bind(TargetObject o);
}
