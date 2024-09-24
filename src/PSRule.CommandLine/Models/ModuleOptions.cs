// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Models;

/// <summary>
/// Options for the module command.
/// </summary>
public sealed class ModuleOptions
{
    /// <summary>
    /// A specific path to use for the operation.
    /// </summary>
    public string[]? Path { get; set; }

    /// <summary>
    /// The name of any specified modules.
    /// </summary>
    public string[]? Module { get; set; }

    /// <summary>
    /// Determines if the module is overridden if it already exists.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// The target module version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Determines if verification that the module exists is skipped.
    /// </summary>
    public bool SkipVerification { get; set; }

    /// <summary>
    /// Accept pre-release versions in addition to stable module versions.
    /// </summary>
    public bool Prerelease { get; set; }
}
