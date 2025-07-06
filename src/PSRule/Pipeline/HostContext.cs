// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// A base class for host context instances, by default this is a no-op implementation.
/// </summary>
public abstract class HostContext : IHostContext
{
    /// <summary>
    /// A preference variable for error handling action.
    /// </summary>
    protected const string ErrorPreference = "ErrorActionPreference";

    /// <summary>
    /// A preference variable for warning handling action.
    /// </summary>
    protected const string WarningPreference = "WarningPreference";

    /// <summary>
    /// A preference variable for verbose handling action.
    /// </summary>
    protected const string VerbosePreference = "VerbosePreference";

    /// <summary>
    /// A preference variable for information handling action.
    /// </summary>
    protected const string InformationPreference = "InformationPreference";

    /// <summary>
    /// A preference variable for debug handling action.
    /// </summary>
    protected const string DebugPreference = "DebugPreference";

    /// <inheritdoc/>
    public virtual string? CachePath => null;

    /// <inheritdoc/>
    public int ExitCode { get; private set; }

    /// <inheritdoc/>
    public virtual bool InSession => false;

    /// <inheritdoc/>
    public virtual bool HadErrors { get; private set; }

    /// <inheritdoc/>
    public virtual ActionPreference GetPreferenceVariable(string variableName)
    {
        return variableName == ErrorPreference ||
            variableName == WarningPreference ? ActionPreference.Continue : ActionPreference.Ignore;
    }

    /// <inheritdoc/>
    public virtual T? GetVariable<T>(string variableName)
    {
        return default;
    }

    /// <inheritdoc/>
    public virtual void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is IResultRecord record)
            Record(record);
        //else if (enumerateCollection)
        //    foreach (var item in record)
    }

    /// <inheritdoc/>
    public virtual void WriteResult(InvokeResult result)
    {
        // No-op, override to write results to the host.
    }

    /// <inheritdoc/>
    public virtual void WriteHost(string message, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null, bool? noNewLine = null)
    {

    }

    /// <inheritdoc/>
    public virtual void SetVariable<T>(string variableName, T value)
    {

    }

    /// <inheritdoc/>
    public abstract bool ShouldProcess(string target, string action);

    /// <summary>
    /// Handle record objects emitted from the pipeline.
    /// </summary>
    public virtual void Record(IResultRecord record)
    {

    }

    /// <inheritdoc/>
    public virtual string GetWorkingPath()
    {
        return Directory.GetCurrentDirectory();
    }

    /// <inheritdoc/>
    public virtual bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    /// <inheritdoc/>
    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            HadErrors = true;
    }

    /// <inheritdoc/>
    public virtual void SetExitCode(int exitCode)
    {
        if (exitCode == 0) return;

        ExitCode = exitCode;
    }
}
