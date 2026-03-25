// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A specification for a V1 module configuration.
/// </summary>
internal interface IModuleConfigV1Spec : IModuleConfigSpec
{
    BindingOption? Binding { get; }

    ConfigurationOption? Configuration { get; }

    ConventionOption? Convention { get; }

    OutputOption? Output { get; }

    RuleOption? Rule { get; }
}
