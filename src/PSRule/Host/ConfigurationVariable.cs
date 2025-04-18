// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Host;

internal sealed class ConfigurationVariable : PSVariable
{
    private const string VARIABLE_NAME = "Configuration";
    private readonly Runtime.Configuration _Value;

    public ConfigurationVariable()
        : base(VARIABLE_NAME, null, ScopedItemOptions.ReadOnly)
    {
        _Value = new Runtime.Configuration(LegacyRunspaceContext.CurrentThread);
    }

    public override object Value => _Value;
}
