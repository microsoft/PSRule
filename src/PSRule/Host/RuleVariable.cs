// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Host;

/// <summary>
/// A dynamic variable $Rule used during Rule execution.
/// </summary>
internal sealed class RuleVariable : PSVariable
{
    private const string VARIABLE_NAME = "Rule";

    private readonly Rule _Value;

    public RuleVariable()
        : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
    {
        _Value = new Rule();
    }

    public override object Value => _Value;
}
