// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.Selectors;

#nullable enable

/// <summary>
/// A specification for a V2 selector resource.
/// </summary>
internal interface ISelectorV2Spec : ISelectorSpec
{
    /// <summary>
    /// An optional type pre-condition before the selector is evaluated.
    /// </summary>
    string[]? Type { get; }

    /// <summary>
    /// An expression. If the expression evaluates as true the target object is selected.
    /// </summary>
    LanguageIf? If { get; }
}

#nullable restore
