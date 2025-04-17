// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace PSRule.CommandLine.Models;

/// <summary>
/// Output from the <c>run</c> command.
/// </summary>
public sealed class RunCommandOutput(int exitCode)
{
    /// <summary>
    /// The exit code from the command.
    /// </summary>
    public int ExitCode { get; } = exitCode;
}
