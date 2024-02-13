// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;

namespace PSRule.CommandLine.Models;

/// <summary>
/// 
/// </summary>
public sealed class RunOptions
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
    public string? Baseline { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public RuleOutcome? Outcome { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string[]? InputPath { get; set; }
}
