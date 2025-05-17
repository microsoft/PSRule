// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;

namespace PSRule.Pipeline;

internal sealed class PipelineLogger : PipelineLoggerBase
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string InformationPreference = "InformationPreference";
    private const string DebugPreference = "DebugPreference";

    private HashSet<string>? _VerboseFilter;
    private HashSet<string>? _DebugFilter;

    private Action<string>? _OnWriteWarning;
    private Action<string>? _OnWriteVerbose;
    private Action<ErrorRecord>? _OnWriteError;
    private Action<InformationRecord>? _OnWriteInformation;
    private Action<string>? _OnWriteDebug;
    internal Action<object, bool>? OnWriteObject;

    private bool _LogError;
    private bool _LogWarning;
    private bool _LogVerbose;
    private bool _LogInformation;
    private bool _LogDebug;

    internal void UseCommandRuntime(PSCmdlet commandRuntime)
    {
        _OnWriteVerbose = commandRuntime.WriteVerbose;
        _OnWriteWarning = commandRuntime.WriteWarning;
        _OnWriteError = commandRuntime.WriteError;
        _OnWriteInformation = commandRuntime.WriteInformation;
        _OnWriteDebug = commandRuntime.WriteDebug;
        OnWriteObject = commandRuntime.WriteObject;
    }

    internal void UseExecutionContext(EngineIntrinsics executionContext)
    {
        _LogError = GetPreferenceVariable(executionContext, ErrorPreference);
        _LogWarning = GetPreferenceVariable(executionContext, WarningPreference);
        _LogVerbose = GetPreferenceVariable(executionContext, VerbosePreference);
        _LogInformation = GetPreferenceVariable(executionContext, InformationPreference);
        _LogDebug = GetPreferenceVariable(executionContext, DebugPreference);
    }

    internal void Configure(PSRuleOption option)
    {
        // Do nothing.
    }

    private static bool GetPreferenceVariable(EngineIntrinsics executionContext, string variableName)
    {
        var preference = (ActionPreference)executionContext.SessionState.PSVariable.GetValue(variableName);
        return preference != ActionPreference.Ignore &&
            !(preference == ActionPreference.SilentlyContinue &&
            (variableName == VerbosePreference || variableName == DebugPreference));
    }

    #region Internal logging methods

    /// <summary>
    /// Core methods to hand off to logger.
    /// </summary>
    /// <param name="errorRecord">A valid PowerShell error record.</param>
    protected override void DoWriteError(ErrorRecord errorRecord)
    {
        if (_OnWriteError == null)
            return;

        _OnWriteError(errorRecord);
    }

    /// <summary>
    /// Core method to hand off verbose messages to logger.
    /// </summary>
    /// <param name="message">A message to log.</param>
    protected override void DoWriteVerbose(string message)
    {
        if (_OnWriteVerbose == null)
            return;

        _OnWriteVerbose(message);
    }

    /// <summary>
    /// Core method to hand off warning messages to logger.
    /// </summary>
    /// <param name="message">A message to log</param>
    protected override void DoWriteWarning(string message)
    {
        if (_OnWriteWarning == null)
            return;

        _OnWriteWarning(message);
    }

    /// <summary>
    /// Core method to hand off information messages to logger.
    /// </summary>
    protected override void DoWriteInformation(InformationRecord informationRecord)
    {
        if (_OnWriteInformation == null)
            return;

        _OnWriteInformation(informationRecord);
    }

    /// <summary>
    /// Core method to hand off debug messages to logger.
    /// </summary>
    protected override void DoWriteDebug(DebugRecord debugRecord)
    {
        if (_OnWriteDebug == null)
            return;

        _OnWriteDebug(debugRecord.Message);
    }

    protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (OnWriteObject == null)
            return;

        OnWriteObject(sendToPipeline, enumerateCollection);
    }

    #endregion Internal logging methods

    public override bool ShouldWriteVerbose()
    {
        return _LogVerbose && (_VerboseFilter == null || ScopeName == null || _VerboseFilter.Contains(ScopeName));
    }

    public override bool ShouldWriteInformation()
    {
        return true;
    }

    public override bool ShouldWriteDebug()
    {
        return _LogDebug && (_DebugFilter == null || ScopeName == null || _DebugFilter.Contains(ScopeName));
    }
}
