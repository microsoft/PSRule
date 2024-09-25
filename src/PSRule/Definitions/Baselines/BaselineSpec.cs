// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Definitions.Baselines;

/// <summary>
/// A specification for a V1 baseline resource.
/// </summary>
public sealed class BaselineSpec : Spec, IBaselineV1Spec
{
    /// <inheritdoc/>
    public ConfigurationOption Configuration { get; set; }

    /// <inheritdoc/>
    public ConventionOption Convention { get; set; }

    /// <inheritdoc/>
    public RuleOption Rule { get; set; }
}
