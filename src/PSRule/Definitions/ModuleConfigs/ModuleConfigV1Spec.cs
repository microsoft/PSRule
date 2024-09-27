// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A specification for a module configuration.
/// </summary>
internal sealed class ModuleConfigV1Spec : Spec
{
    public BindingOption Binding { get; set; }

    public ConfigurationOption Configuration { get; set; }

    public ConventionOption Convention { get; set; }

    public OutputOption Output { get; set; }

    public RuleOption Rule { get; set; }
}
