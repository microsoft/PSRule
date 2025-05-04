// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// The host context used to wrap the parent PowerShell runtime when executing PowerShell-based cmdlet.
/// </summary>
public sealed class PSHostContext : IHostContext
{
    private const string Source = "PSRule";
    private const string HostTag = "PSHOST";

    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string InformationPreference = "InformationPreference";
    private const string DebugPreference = "DebugPreference";

    internal readonly PSCmdlet CmdletContext;
    internal readonly EngineIntrinsics ExecutionContext;

    /// <summary>
    /// Create an instance of a PowerShell-based host context.
    /// </summary>
    public PSHostContext(PSCmdlet commandRuntime, EngineIntrinsics executionContext)
    {
        InSession = false;
        CmdletContext = commandRuntime;
        ExecutionContext = executionContext;
        InSession = executionContext != null && executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") != null;
    }

    /// <inheritdoc/>
    public string? CachePath { get; }

    /// <inheritdoc/>
    public int ExitCode { get; private set; }

    /// <inheritdoc/>
    public bool InSession { get; }

    /// <inheritdoc/>
    public bool HadErrors { get; private set; }

    /// <inheritdoc/>
    public ActionPreference GetPreferenceVariable(string variableName)
    {
        return ExecutionContext == null
            ? ActionPreference.SilentlyContinue
            : (ActionPreference)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
    }

    /// <inheritdoc/>
    public T? GetVariable<T>(string variableName)
    {
        return ExecutionContext == null ? default : (T)ExecutionContext.SessionState.PSVariable.GetValue(variableName);
    }

    /// <inheritdoc/>
    public void SetVariable<T>(string variableName, T value)
    {
        CmdletContext.SessionState.PSVariable.Set(variableName, value);
    }

    /// <inheritdoc/>
    public bool ShouldProcess(string target, string action)
    {
        return CmdletContext == null || CmdletContext.ShouldProcess(target, action);
    }

    /// <inheritdoc/>
    public void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        CmdletContext.WriteObject(sendToPipeline, enumerateCollection);
    }

    /// <inheritdoc/>
    public void WriteHost(string message, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null, bool? noNewLine = null)
    {
        var record = new InformationRecord(new HostInformationMessage
        {
            Message = message,
            BackgroundColor = backgroundColor,
            ForegroundColor = foregroundColor,
            NoNewLine = noNewLine
        }, Source);
        record.Tags.Add(HostTag);

        CmdletContext.WriteInformation(record);
    }

    /// <inheritdoc/>
    public string GetWorkingPath()
    {
        return ExecutionContext.SessionState.Path.CurrentFileSystemLocation.Path;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
            return false;

        var preference = logLevel switch
        {
            LogLevel.Trace => GetPreferenceVariable(VerbosePreference),
            LogLevel.Debug => GetPreferenceVariable(DebugPreference),
            LogLevel.Information => GetPreferenceVariable(InformationPreference),
            LogLevel.Warning => GetPreferenceVariable(WarningPreference),
            LogLevel.Error => GetPreferenceVariable(ErrorPreference),
            _ => ActionPreference.SilentlyContinue
        };

        return preference != ActionPreference.Ignore && !(preference == ActionPreference.SilentlyContinue && (
            logLevel == LogLevel.Trace ||
            logLevel == LogLevel.Debug)
        );
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            HadErrors = true;

        if (!IsEnabled(logLevel))
            return;

        switch (logLevel)
        {
            case LogLevel.Trace:
                CmdletContext.WriteVerbose(formatter(state, exception));
                break;

            case LogLevel.Debug:
                CmdletContext.WriteDebug(formatter(state, exception));
                break;

            case LogLevel.Information:
                CmdletContext.WriteInformation(new InformationRecord(formatter(state, exception), null));
                break;

            case LogLevel.Warning:
                CmdletContext.WriteWarning(formatter(state, exception));
                break;

            case LogLevel.Error:
            case LogLevel.Critical:
                var errorRecord = new ErrorRecord(exception, eventId.Name, ErrorCategory.NotSpecified, state);
                CmdletContext.WriteError(errorRecord);
                break;
        }
    }

    /// <inheritdoc/>
    public void SetExitCode(int exitCode)
    {
        if (exitCode == 0) return;

        ExitCode = exitCode;
    }
}

#nullable restore
