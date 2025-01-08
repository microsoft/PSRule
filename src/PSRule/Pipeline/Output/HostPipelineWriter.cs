// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline.Output;

/// <summary>
/// An output writer that returns output to the host PowerShell runspace.
/// </summary>
internal sealed class HostPipelineWriter : PipelineWriter
{
    private const string Source = "PSRule";
    private const string HostTag = "PSHOST";

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

    private HashSet<string> _VerboseFilter;
    private HashSet<string> _DebugFilter;

    private string _ScopeName;

    internal HostPipelineWriter(IHostContext hostContext, PSRuleOption option, ShouldProcess shouldProcess)
        : base(null, option, shouldProcess)
    {
        if (hostContext != null)
        {
            UseCommandRuntime(hostContext);
            UseExecutionContext(hostContext);
        }
    }

    public override void Begin()
    {
        if (Option.Logging.LimitVerbose != null && Option.Logging.LimitVerbose.Length > 0)
            _VerboseFilter = new HashSet<string>(Option.Logging.LimitVerbose);

        if (Option.Logging.LimitDebug != null && Option.Logging.LimitDebug.Length > 0)
            _DebugFilter = new HashSet<string>(Option.Logging.LimitDebug);
    }

    private void UseCommandRuntime(IHostContext hostContext)
    {
        if (hostContext == null)
            return;

        _OnWriteVerbose = hostContext.Verbose;
        _OnWriteWarning = hostContext.Warning;
        _OnWriteError = hostContext.Error;
        _OnWriteInformation = hostContext.Information;
        _OnWriteDebug = hostContext.Debug;
        OnWriteObject = hostContext.Object;
    }

    private void UseExecutionContext(IHostContext hostContext)
    {
        if (hostContext == null)
            return;

        _LogError = GetPreferenceVariable(hostContext, ErrorPreference);
        _LogWarning = GetPreferenceVariable(hostContext, WarningPreference);
        _LogVerbose = GetPreferenceVariable(hostContext, VerbosePreference);
        _LogInformation = GetPreferenceVariable(hostContext, InformationPreference);
        _LogDebug = GetPreferenceVariable(hostContext, DebugPreference);
    }

    private static bool GetPreferenceVariable(IHostContext hostContext, string variableName)
    {
        var preference = hostContext.GetPreferenceVariable(variableName);
        return preference != ActionPreference.Ignore && !(preference == ActionPreference.SilentlyContinue && (
            variableName == VerbosePreference ||
            variableName == DebugPreference)
        );
    }

    #region Internal logging methods

    /// <summary>
    /// Core methods to hand off to logger.
    /// </summary>
    /// <param name="errorRecord">A valid PowerShell error record.</param>
    public override void WriteError(ErrorRecord errorRecord)
    {
        if (_OnWriteError == null || !ShouldWriteError())
            return;

        _OnWriteError(errorRecord);
    }

    /// <summary>
    /// Core method to hand off verbose messages to logger.
    /// </summary>
    /// <param name="message">A message to log.</param>
    public override void WriteVerbose(string message)
    {
        if (_OnWriteVerbose == null || !ShouldWriteVerbose())
            return;

        _OnWriteVerbose(message);
    }

    /// <summary>
    /// Core method to hand off warning messages to logger.
    /// </summary>
    /// <param name="message">A message to log</param>
    public override void WriteWarning(string message)
    {
        if (_OnWriteWarning == null || !ShouldWriteWarning())
            return;

        _OnWriteWarning(message);
    }

    /// <summary>
    /// Core method to hand off information messages to logger.
    /// </summary>
    public override void WriteInformation(InformationRecord informationRecord)
    {
        if (_OnWriteInformation == null || !ShouldWriteInformation())
            return;

        _OnWriteInformation(informationRecord);
    }

    /// <summary>
    /// Core method to hand off debug messages to logger.
    /// </summary>
    public override void WriteDebug(string text, params object[] args)
    {
        if (_OnWriteDebug == null || string.IsNullOrEmpty(text) || !ShouldWriteDebug())
            return;

        text = args == null || args.Length == 0 ? text : string.Format(Thread.CurrentThread.CurrentCulture, text, args);
        _OnWriteDebug(text);
    }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (OnWriteObject == null || (sendToPipeline is InvokeResult && Option.Output.As == ResultFormat.Summary))
            return;

        if (sendToPipeline is InvokeResult result)
            ProcessRecord(result.AsRecord());
        else
            OnWriteObject(sendToPipeline, enumerateCollection);
    }

    public override void WriteHost(HostInformationMessage info)
    {
        if (_OnWriteInformation == null)
            return;

        var record = new InformationRecord(info, Source);
        record.Tags.Add(HostTag);
        _OnWriteInformation(record);
    }

    public override bool ShouldWriteVerbose()
    {
        return _LogVerbose && (_VerboseFilter == null || _ScopeName == null || _VerboseFilter.Contains(_ScopeName));
    }

    public override bool ShouldWriteInformation()
    {
        return _LogInformation;
    }

    public override bool ShouldWriteDebug()
    {
        return _LogDebug && (_DebugFilter == null || _ScopeName == null || _DebugFilter.Contains(_ScopeName));
    }

    public override bool ShouldWriteError()
    {
        return _LogError;
    }

    public override bool ShouldWriteWarning()
    {
        return _LogWarning;
    }

    public override void EnterScope(string scopeName)
    {
        _ScopeName = scopeName;
    }

    public override void ExitScope()
    {
        _ScopeName = null;
    }

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            HadErrors = true;

        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
        {
            WriteError(new ErrorRecord(exception, eventId.Id.ToString(), ErrorCategory.InvalidOperation, null));
        }
        else if (logLevel == LogLevel.Warning)
        {
            WriteWarning(formatter(state, exception));
        }
        else if (logLevel == LogLevel.Information)
        {
            WriteInformation(new InformationRecord(formatter(state, exception), null));
        }
        else if (logLevel == LogLevel.Debug || logLevel == LogLevel.Trace)
        {
            WriteDebug(formatter(state, exception));
        }
    }

    #endregion Internal logging methods

    private void ProcessRecord(RuleRecord[] records)
    {
        if (records == null || records.Length == 0)
            return;

        for (var i = 0; i < records.Length; i++)
        {
            OnWriteObject(records[i], false);
            WriteErrorInfo(records[i]);
        }
    }
}
