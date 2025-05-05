// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;
using PSRule.Rules;

namespace PSRule.CommandLine.Models;

/// <summary>
/// Options for the run command.
/// </summary>
public sealed class RunOptions
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
    /// The name of the rules to run.
    /// </summary>
    public string[]? Name { get; set; }

    /// <summary>
    /// A baseline to use.
    /// </summary>
    public string? Baseline { get; set; }

    /// <summary>
    /// A list of formats to enable.
    /// </summary>
    public string[]? Formats { get; set; }

    /// <summary>
    /// A list of conventions to use.
    /// </summary>
    public string[]? Convention { get; set; }

    /// <summary>
    /// Only show output with the specified outcome.
    /// </summary>
    public RuleOutcome? Outcome { get; set; }

    /// <summary>
    /// The input path to search for input files.
    /// </summary>
    public string[]? InputPath { get; set; }

    /// <summary>
    /// The output path to write output files.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// The format to write output files.
    /// </summary>
    public OutputFormat? OutputFormat { get; set; }

    /// <summary>
    /// Do not restore modules before running rules.
    /// </summary>
    public bool NoRestore { get; set; }

    /// <summary>
    /// The path to write the job summary file.
    /// </summary>
    public string? JobSummaryPath { get; set; }

    /// <summary>
    /// Determine the rule severity level at which to break the pipeline.
    /// </summary>
    public BreakLevel? BreakLevel { get; set; }
}
