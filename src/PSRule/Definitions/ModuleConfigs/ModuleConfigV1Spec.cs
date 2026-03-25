// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A specification for a V1 module configuration.
/// </summary>
internal sealed class ModuleConfigV1Spec : Spec, IModuleConfigV1Spec
{
    public BindingOption? Binding { get; set; }

    public ConfigurationOption? Configuration { get; set; }

    public ConventionOption? Convention { get; set; }

    public OutputOption? Output { get; set; }

    public RuleOption? Rule { get; set; }
}
