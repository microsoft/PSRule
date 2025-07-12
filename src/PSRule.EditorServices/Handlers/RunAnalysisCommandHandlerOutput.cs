// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Output from the <c>runAnalysis</c> command handler.
/// </summary>
public sealed class RunAnalysisCommandHandlerOutput(int exitCode, IEnumerable<RunAnalysisCommandHandlerRecord>? problems = null)
{
    /// <summary>
    /// The exit code from the command.
    /// </summary>
    public int ExitCode { get; } = exitCode;

    /// <summary>
    /// The problems found during the analysis.
    /// This is a collection of <see cref="RunAnalysisCommandHandlerRecord"/> objects that contain details about each problem.
    /// </summary>
    public IEnumerable<RunAnalysisCommandHandlerRecord>? Problems { get; } = problems;
}
