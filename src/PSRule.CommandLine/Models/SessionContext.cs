// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule.CommandLine.Models;

/// <summary>
/// A context for a single invocation of a command.
/// </summary>
internal sealed class SessionContext(IHostContext parent) : IHostContext
{
    private readonly IHostContext _Parent = parent;

    public bool InSession => _Parent.InSession;

    public bool HadErrors => _Parent.HadErrors;

    public string? CachePath => _Parent.CachePath;

    public string? WorkingPath { get; set; }

    public Action<object>? GetResultOutput { get; set; }

    public void Debug(string text)
    {
        _Parent.Debug(text);
    }

    public void Error(ErrorRecord errorRecord)
    {
        _Parent.Error(errorRecord);
    }

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

    public void Information(InformationRecord informationRecord)
    {
        _Parent.Information(informationRecord);
    }

    public void Object(object sendToPipeline, bool enumerateCollection)
    {
        GetResultOutput?.Invoke(sendToPipeline);
        _Parent.Object(sendToPipeline, enumerateCollection);
    }

    public void SetVariable<T>(string variableName, T value)
    {
        _Parent.SetVariable(variableName, value);
    }

    public bool ShouldProcess(string target, string action)
    {
        return _Parent.ShouldProcess(target, action);
    }

    public void Verbose(string text)
    {
        _Parent.Verbose(text);
    }

    public void Warning(string text)
    {
        _Parent.Warning(text);
    }
}
