// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Get a command result, including the exit code and return value.
/// </summary>
public sealed class CommandResult<T>(int exitCode)
{
    /// <summary>
    /// The numeric exit code of the command.
    /// </summary>
    public int ExitCode { get; } = exitCode;

    /// <summary>
    /// The return value of the command.
    /// </summary>
    public T? Value { get; set; }
}
