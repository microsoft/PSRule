// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// The source location of the resource.
/// </summary>
public sealed class ResourceExtent
{
    /// <summary>
    /// The file where the resource is located.
    /// </summary>
    public string? File { get; set; }

    /// <summary>
    /// The name of the module if the resource is contained within a module.
    /// </summary>
    public string? Module { get; set; }
}
