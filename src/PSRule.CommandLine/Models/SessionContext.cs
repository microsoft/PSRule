// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.CommandLine.Models;

/// <summary>
/// A context for a single invocation of a command.
/// </summary>
internal sealed class SessionContext(IHostContext parent) : IHostContext
{
    private readonly IHostContext _Parent = parent;

    public string? CachePath => _Parent.CachePath;

    public int ExitCode => _Parent.ExitCode;

    public bool InSession => _Parent.InSession;

    public bool HadErrors => _Parent.HadErrors;

    public string? WorkingPath { get; set; }

    public Action<InvokeResult>? OnWriteResult { get; set; }

    public ActionPreference GetPreferenceVariable(string variableName)
    {
        return _Parent.GetPreferenceVariable(variableName);
    }

    public T? GetVariable<T>(string variableName)
    {
        return _Parent.GetVariable<T>(variableName);
    }

    public string GetWorkingPath()
    {
        return WorkingPath ?? _Parent.GetWorkingPath();
    }

    public void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        _Parent.WriteObject(sendToPipeline, enumerateCollection);
    }

    public void WriteResult(InvokeResult result)
    {
        OnWriteResult?.Invoke(result);
        _Parent.WriteResult(result);
    }

    public void SetVariable<T>(string variableName, T value)
    {
        _Parent.SetVariable(variableName, value);
    }

    public bool ShouldProcess(string target, string action)
    {
        return _Parent.ShouldProcess(target, action);
    }

    public void WriteHost(string message, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null, bool? noNewLine = null)
    {
        _Parent.WriteHost(message, backgroundColor, foregroundColor, noNewLine);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _Parent.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _Parent.Log(logLevel, eventId, state, exception, formatter);
    }

    public void SetExitCode(int exitCode)
    {
        _Parent.SetExitCode(exitCode);
    }
}
