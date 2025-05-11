// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// A specification for a V1 suppression group resource.
/// </summary>
internal sealed class SuppressionGroupV1Spec : Spec, ISuppressionGroupV1Spec
{
    /// <inheritdoc/>
    public DateTime? ExpiresOn { get; set; }

    /// <inheritdoc/>
    public string[]? Rule { get; set; }

    /// <inheritdoc/>
    public LanguageIf? If { get; set; }
}
