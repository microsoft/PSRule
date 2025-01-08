// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.Selectors;

#nullable enable

/// <summary>
/// A specification for a V1 selector resource.
/// </summary>
internal interface ISelectorV1Spec : ISelectorSpec
{
    /// <summary>
    /// An expression. If the expression evaluates as true the target object is selected.
    /// </summary>
    LanguageIf? If { get; }
}

#nullable restore
