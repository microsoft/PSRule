// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;

namespace PSRule.Definitions.Baselines;

/// <summary>
/// A specification for a V1 baseline resource.
/// </summary>
internal interface IBaselineV1Spec
{
    /// <summary>
    /// Allows configuration key/ values to be specified that can be used within rule definitions.
    /// </summary>
    ConfigurationOption Configuration { get; set; }

    /// <summary>
    /// Options for that affect which rules are executed by including and filtering discovered rules.
    /// </summary>
    RuleOption Rule { get; set; }

    /// <summary>
    /// Options that configure additional rule overrides.
    /// </summary>
    OverrideOption Override { get; set; }
}
