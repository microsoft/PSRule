// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace PSRule.Options;

/// <summary>
/// Configures how the initial PowerShell sandbox for executing rules is created.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum SessionState
{
    /// <summary>
    /// Create the initial session state with all built-in cmdlets loaded.
    /// See <seealso href="https://docs.microsoft.com/en-us/powershell/scripting/developer/hosting/creating-an-initialsessionstate?view=powershell-7.2">CreateDefault</seealso>.
    /// </summary>
    BuiltIn = 0,

    /// <summary>
    /// Create the initial session state with only cmdlets loaded for hosting PowerShell.
    /// See <seealso href="https://docs.microsoft.com/en-us/powershell/scripting/developer/hosting/creating-an-initialsessionstate?view=powershell-7.2">CreateDefault2</seealso>.
    /// </summary>
    Minimal = 1
}
