// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A specification for a V2 module configuration.
/// </summary>
internal sealed class ModuleConfigV2Spec : Spec, IModuleConfigV2Spec
{
    public BindingOption? Binding { get; set; }

    public CapabilityOption? Capabilities { get; set; }

    public ConfigurationOption? Configuration { get; set; }

    public ConventionOption? Convention { get; set; }

    public OutputOption? Output { get; set; }

    public RuleOption? Rule { get; set; }
}
