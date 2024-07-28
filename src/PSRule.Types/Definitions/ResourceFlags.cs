// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Additional flags that indicate the status of the resource.
/// </summary>
[Flags]
public enum ResourceFlags
{
    /// <summary>
    /// No flags are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// The resource is obsolete.
    /// </summary>
    Obsolete = 1
}
