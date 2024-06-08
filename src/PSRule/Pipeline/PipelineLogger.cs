// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;

namespace PSRule.Pipeline;

internal abstract class PipelineLoggerBase : IPipelineWriter
{
    private const string Source = "PSRule";
    private const string HostTag = "PSHOST";

    protected string ScopeName { get; private set; }

    public bool HadErrors { get; private set; }

    public bool HadFailures { get; private set; }

    #region Logging

    public void WriteError(ErrorRecord errorRecord)
    {
        HadErrors = true;
        if (!ShouldWriteError() || errorRecord == null)
            return;

        DoWriteError(errorRecord);
    }

    public void WriteVerbose(string message)
    {
        if (!ShouldWriteVerbose() || string.IsNullOrEmpty(message))
            return;

        DoWriteVerbose(message);
    }

    public void WriteDebug(DebugRecord debugRecord)
    {
        if (!ShouldWriteDebug())
            return;

        DoWriteDebug(debugRecord);
    }

    public void WriteDebug(string text, params object[] args)
    {
        if (string.IsNullOrEmpty(text) || !ShouldWriteDebug())
            return;

        text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
        DoWriteDebug(new DebugRecord(text));
    }

    public void WriteInformation(InformationRecord informationRecord)
    {
        if (!ShouldWriteInformation())
            return;

        DoWriteInformation(informationRecord);
    }

    public void WriteHost(HostInformationMessage info)
    {
        var record = new InformationRecord(info, Source);
        record.Tags.Add(HostTag);
        DoWriteInformation(record);
    }

    public void WriteWarning(string message)
    {
        if (!ShouldWriteWarning())
            return;

        DoWriteWarning(message);
    }

    public void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        DoWriteObject(sendToPipeline, enumerateCollection);
    }

    public void EnterScope(string scopeName)
    {
        ScopeName = scopeName;
    }

    public void ExitScope()
    {
        ScopeName = null;
    }

    #endregion Logging

    public virtual bool ShouldWriteError()
    {
        return true;
    }

    public virtual bool ShouldWriteWarning()
    {
        return true;
    }

    public virtual bool ShouldWriteVerbose()
    {
        return true;
    }

    public virtual bool ShouldWriteInformation()
    {
        return true;
    }

    public virtual bool ShouldWriteDebug()
    {
        return true;
    }

    public virtual void Begin()
    {

    }

    public virtual void End(IPipelineResult result)
    {

    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        // Do nothing, but allow override.
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    protected abstract void DoWriteError(ErrorRecord errorRecord);

    protected abstract void DoWriteVerbose(string message);

    protected abstract void DoWriteWarning(string message);

    protected abstract void DoWriteInformation(InformationRecord informationRecord);

    protected abstract void DoWriteDebug(DebugRecord debugRecord);

    protected abstract void DoWriteObject(object sendToPipeline, bool enumerateCollection);
}

internal sealed class PipelineLogger : PipelineLoggerBase
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string InformationPreference = "InformationPreference";
    private const string DebugPreference = "DebugPreference";

    private HashSet<string> _VerboseFilter;
    private HashSet<string> _DebugFilter;

    private Action<string> _OnWriteWarning;
    private Action<string> _OnWriteVerbose;
    private Action<ErrorRecord> _OnWriteError;
    private Action<InformationRecord> _OnWriteInformation;
    private Action<string> _OnWriteDebug;
    internal Action<object, bool> OnWriteObject;

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
        if (option.Logging.LimitVerbose != null && option.Logging.LimitVerbose.Length > 0)
            _VerboseFilter = new HashSet<string>(option.Logging.LimitVerbose);

        if (option.Logging.LimitDebug != null && option.Logging.LimitDebug.Length > 0)
            _DebugFilter = new HashSet<string>(option.Logging.LimitDebug);
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
