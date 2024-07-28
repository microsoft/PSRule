// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Definitions.Baselines;

/// <summary>
/// A specification for a V1 baseline resource.
/// </summary>
internal interface IBaselineV1Spec
{
    /// <summary>
    /// Options that affect property binding.
    /// </summary>
    BindingOption Binding { get; set; }

    /// <summary>
    /// Allows configuration key/ values to be specified that can be used within rule definitions.
    /// </summary>
    ConfigurationOption Configuration { get; set; }

    /// <summary>
    /// Options that configure conventions.
    /// </summary>
    ConventionOption Convention { get; set; }

    /// <summary>
    /// Options for that affect which rules are executed by including and filtering discovered rules.
    /// </summary>
    RuleOption Rule { get; set; }
}
