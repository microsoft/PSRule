// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Models;

/// <summary>
/// Options for the get rule command.
/// </summary>
public sealed class GetRuleOptions
{
    /// <summary>
    /// An optional workspace path to use with this command.
    /// </summary>
    public string? WorkspacePath { get; set; }

    /// <summary>
    /// The path to search for rules.
    /// </summary>
    public string[]? Path { get; set; }

    /// <summary>
    /// A list of modules to use.
    /// </summary>
    public string[]? Module { get; set; }

    /// <summary>
    /// The name of the rules to get.
    /// </summary>
    public string[]? Name { get; set; }

    /// <summary>
    /// A baseline to use.
    /// </summary>
    public string? Baseline { get; set; }

    /// <summary>
    /// Include rule dependencies in the output.
    /// </summary>
    public bool IncludeDependencies { get; set; }

    /// <summary>
    /// Do not restore modules before getting rules.
    /// </summary>
    public bool NoRestore { get; set; }
}