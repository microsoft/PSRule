// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Get a command results that does not include any output.
/// </summary>
public sealed class CommandResult(int exitCode)
{
    /// <summary>
    /// A successful command result with no additional output.
    /// </summary>
    public static readonly CommandResult Success = new(0);

    /// <summary>
    /// The numeric exit code of the command.
    /// </summary>
    public int ExitCode { get; } = exitCode;
}
