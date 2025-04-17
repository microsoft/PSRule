// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Output from the <c>runAnalysis</c> command handler.
/// </summary>
public sealed class RunAnalysisCommandHandlerOutput(int exitCode)
{
    /// <summary>
    /// The exit code from the command.
    /// </summary>
    public int ExitCode { get; } = exitCode;
}
