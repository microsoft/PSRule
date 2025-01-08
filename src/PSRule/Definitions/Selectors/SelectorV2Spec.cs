// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.Selectors;

#nullable enable

/// <summary>
/// A specification for a V2 selector resource.
/// </summary>
internal sealed class SelectorV2Spec : Spec, ISelectorV2Spec
{
    /// <inheritdoc/>
    public string[]? Type { get; set; }

    /// <inheritdoc/>
    public LanguageIf? If { get; set; }
}

#nullable restore
