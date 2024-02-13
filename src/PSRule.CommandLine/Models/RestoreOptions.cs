// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Models;

/// <summary>
/// 
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
}
