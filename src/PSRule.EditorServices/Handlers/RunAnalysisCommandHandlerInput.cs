// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Input to the <c>runAnalysis</c> command handler.
/// </summary>
public sealed class RunAnalysisCommandHandlerInput
{
    /// <summary>
    /// The workspace path that the command is running in.
    /// </summary>
    public string? WorkspacePath { get; set; }

    /// <summary>
    /// The path for the input files to analyze.
    /// </summary>
    public string[]? InputPath { get; set; }
}
