// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Options;

namespace PSRule.Runtime.Scripting;

/// <summary>
/// A context that holds the state of a PowerShell runspace.
/// This is used to manage the lifecycle of the runspace and its associated resources.
/// </summary>
internal interface IRunspaceContext : IDisposable
{
    int ErrorCount { get; }

    ErrorRecord? LastError { get; }

    RestrictScriptSource RestrictScriptSource { get; }

    IResourceDiscoveryContext? ResourceContext { get; }

    void ResetErrorCount();

    PowerShell GetPowerShell();

    void EnterResourceContext(IResourceDiscoveryContext context);

    void ExitResourceContext(IResourceDiscoveryContext context);
}
