// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.Selectors;

#nullable enable

/// <summary>
/// A specification for a V1 selector resource.
/// </summary>
internal sealed class SelectorV1Spec : Spec, ISelectorV1Spec
{
    /// <inheritdoc/>
    public LanguageIf? If { get; set; }
}

#nullable restore
