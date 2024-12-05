// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A host context for handling input and output emitted from the pipeline.
/// </summary>
public interface IHostContext
{
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
    T GetVariable<T>(string variableName);

    /// <summary>
    /// Set the value of a named variable.
    /// </summary>
    void SetVariable<T>(string variableName, T value);

    /// <summary>
    /// Handle an error reported by the pipeline.
    /// </summary>
    void Error(ErrorRecord errorRecord);

    /// <summary>
    /// Handle a warning reported by the pipeline.
    /// </summary>
    void Warning(string text);

    /// <summary>
    /// Handle an informational record reported by the pipeline.
    /// </summary>
    void Information(InformationRecord informationRecord);

    /// <summary>
    /// Handle a verbose message reported by the pipeline.
    /// </summary>
    void Verbose(string text);

    /// <summary>
    /// Handle a debug message reported by the pipeline.
    /// </summary>
    void Debug(string text);

    /// <summary>
    /// Handle an object emitted from the pipeline.
    /// </summary>
    void Object(object sendToPipeline, bool enumerateCollection);

    /// <summary>
    /// Determines if a destructive action such as overwriting a file should be processed.
    /// </summary>
    bool ShouldProcess(string target, string action);

    /// <summary>
    /// Get the current working path.
    /// </summary>
    string GetWorkingPath();

    /// <summary>
    /// Configures the root path to use for caching artifacts including modules.
    /// Each artifact is in a subdirectory of the root path.
    /// </summary>
    string? CachePath { get; }
}

#nullable restore
