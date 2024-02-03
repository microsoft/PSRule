// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Options;

/// <summary>
/// Configures where to allow PowerShell language features (such as rules and conventions) to run from.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RestrictScriptSource
{
    /// <summary>
    /// PowerShell language features are allowed from workspace and modules.
    /// </summary>
    Unrestricted = 0,

    /// <summary>
    /// PowerShell language features are allowed from loaded modules, but PowerShell files within the workspace are ignored.
    /// </summary>
    ModuleOnly = 1,

    /// <summary>
    /// No PowerShell language features are used during PSRule run.
    /// </summary>
    DisablePowerShell = 2
}
