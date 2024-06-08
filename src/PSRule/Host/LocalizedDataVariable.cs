// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Host;

/// <summary>
/// A dynamic variable $LocalizedData used during Rule execution.
/// </summary>
internal sealed class LocalizedDataVariable : PSVariable
{
    private const string VARIABLE_NAME = "LocalizedData";

    private readonly LocalizedData _Value;

    public LocalizedDataVariable()
        : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
    {
        _Value = new LocalizedData();
    }

    public override object Value => _Value;
}
