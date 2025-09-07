// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Options;

namespace PSRule.Definitions;

/// <summary>
/// A context that is used for discovery of resources defined as script blocks.
/// </summary>
internal interface IScriptResourceDiscoveryContext : IResourceDiscoveryContext
{
    PowerShell? GetPowerShell();

    RestrictScriptSource RestrictScriptSource { get; }
}
