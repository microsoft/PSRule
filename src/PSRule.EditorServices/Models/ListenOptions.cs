// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.EditorServices.Models;

/// <summary>
/// Options for the <c>listen</c> command.
/// </summary>
internal sealed class ListenOptions
{
    /// <summary>
    /// A specific path to use for the operation.
    /// </summary>
    public string[]? Path { get; set; }

    /// <summary>
    /// The name of a client named pipe to connect to.
    /// </summary>
    public string? Pipe { get; set; }

    /// <summary>
    /// Determine if the client will use stdio for communication.
    /// </summary>
    public bool Stdio { get; set; } = false;

    /// <summary>
    /// Determine if the listen command should wait for a debugger to attach.
    /// </summary>
    public bool WaitForDebugger { get; set; } = false;
}
