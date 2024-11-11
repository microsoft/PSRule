// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Models;

/// <summary>
/// Options for the restore command.
/// </summary>
public sealed class RestoreOptions
{
    /// <summary>
    /// 
    /// </summary>
    public string[]? Path { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Write output from the restore operation.
    /// </summary>
    public bool WriteOutput { get; set; } = true;
}
