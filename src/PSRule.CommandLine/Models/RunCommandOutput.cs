// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.CommandLine.Models;

/// <summary>
/// Output from the <c>run</c> command.
/// </summary>
public sealed class RunCommandOutput(int exitCode, IReadOnlyCollection<InvokeResult> results)
{
    /// <summary>
    /// The exit code from the command.
    /// </summary>
    public int ExitCode { get; } = exitCode;

    /// <summary>
    /// The results from the command execution.
    /// This is a collection of <see cref="InvokeResult"/> objects that contain the outcome of executed rules.
    /// </summary>
    public IReadOnlyCollection<InvokeResult> Results { get; } = results;
}
