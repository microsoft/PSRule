// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Host;

/// <summary>
/// An assertion helper variable $Assert used during Rule execution.
/// </summary>
internal sealed class AssertVariable : PSVariable
{
    private const string VARIABLE_NAME = "Assert";
    private readonly Assert _Value;

    public AssertVariable()
        : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
    {
        _Value = new Assert();
    }

    public override object Value => _Value;
}
