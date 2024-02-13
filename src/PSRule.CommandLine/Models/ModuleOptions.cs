// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Models;

/// <summary>
/// 
/// </summary>
public sealed class ModuleOptions
{
    /// <summary>
    /// 
    /// </summary>
    public string[]? Path { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string[]? Module { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool SkipVerification { get; set; }
}
