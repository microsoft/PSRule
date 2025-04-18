// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Host;

/// <summary>
/// A dynamic variable $PSRule used during Rule execution.
/// </summary>
internal sealed class PSRuleVariable : PSVariable
{
    private const string VARIABLE_NAME = "PSRule";

    private readonly Runtime.PSRule _Value;

    public PSRuleVariable()
        : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
    {
        _Value = new Runtime.PSRule(LegacyRunspaceContext.CurrentThread);
    }

    public override object Value => _Value;
}
