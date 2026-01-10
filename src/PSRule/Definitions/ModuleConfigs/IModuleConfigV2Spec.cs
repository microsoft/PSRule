// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A specification for a V2 module configuration.
/// </summary>
internal interface IModuleConfigV2Spec : IModuleConfigSpec
{
    BindingOption? Binding { get; }

    CapabilityOption? Capabilities { get; }

    ConfigurationOption? Configuration { get; }

    ConventionOption? Convention { get; }

    OutputOption? Output { get; }

    RuleOption? Rule { get; }
}
