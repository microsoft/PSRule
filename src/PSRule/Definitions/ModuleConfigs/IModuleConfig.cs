// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// A base interface for a module configuration resource.
/// </summary>
internal interface IModuleConfig : IResource<IModuleConfigSpec>, IScopeConfig
{
}
