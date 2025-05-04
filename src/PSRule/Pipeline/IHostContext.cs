// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A host context for handling input and output emitted from the pipeline.
/// </summary>
public interface IHostContext : ILogger
{
    /// <summary>
    /// Configures the root path to use for caching artifacts including modules.
    /// Each artifact is in a subdirectory of the root path.
    /// </summary>
    string? CachePath { get; }

    /// <summary>
    /// Get the last exit code.
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    /// Determines if the pipeline is executing in a remote PowerShell session.
    /// </summary>
    bool InSession { get; }

    /// <summary>
    /// Determines if the pipeline encountered any errors.
    /// </summary>
    bool HadErrors { get; }

    /// <summary>
    /// Get the value of a PowerShell preference variables.
    /// These variables are commonly used to control logging output.
    /// Preference variables include: <c>ErrorActionPreference</c>, <c>WarningPreference</c>, <c>InformationPreference</c>, <c>VerbosePreference</c>, <c>DebugPreference</c>
    /// </summary>
    ActionPreference GetPreferenceVariable(string variableName);

    /// <summary>
    /// Get the value of a named variable.
    /// </summary>
    T? GetVariable<T>(string variableName);

    /// <summary>
    /// Set the value of a named variable.
    /// </summary>
    void SetVariable<T>(string variableName, T value);

    /// <summary>
    /// Write an object to output.
    /// </summary>
    /// <param name="o">The object to write to output.</param>
    /// <param name="enumerateCollection">Determines when the object is enumerable if it should be enumerated as more then one object.</param>
    void WriteObject(object o, bool enumerateCollection);

    /// <summary>
    /// Write a message to the host.
    /// </summary>
    void WriteHost(string message, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null, bool? noNewLine = null);

    /// <summary>
    /// Determines if a destructive action such as overwriting a file should be processed.
    /// </summary>
    bool ShouldProcess(string target, string action);

    /// <summary>
    /// Get the current working path.
    /// </summary>
    string GetWorkingPath(); 

    /// <summary>
    /// Set the terminating exit code of the pipeline.
    /// </summary>
    /// <param name="exitCode">The numerical exit code.</param>
    void SetExitCode(int exitCode);
}

#nullable restore
