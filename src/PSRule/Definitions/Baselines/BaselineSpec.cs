// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;

namespace PSRule.Definitions.Baselines;

#pragma warning disable CS8618 // Keyword required is not supported in .NET Standard 2.0

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

    /// <inheritdoc/>
    public OverrideOption Override { get; set; }
}

#pragma warning restore CS8618
