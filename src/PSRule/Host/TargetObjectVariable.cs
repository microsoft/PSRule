// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Host;

/// <summary>
/// A dynamic variable used during Rule execution.
/// </summary>
internal sealed class TargetObjectVariable : PSVariable
{
    private const string VARIABLE_NAME = "TargetObject";

    public TargetObjectVariable()
        : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
    {

    }

    public override object Value => LegacyRunspaceContext.CurrentThread.TargetObject?.Value;
}
