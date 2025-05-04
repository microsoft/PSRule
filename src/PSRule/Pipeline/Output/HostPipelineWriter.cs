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
    private readonly IHostContext _HostContext;

    internal Action<object, bool> OnWriteObject;

    private bool _HadErrors;
    private bool _HadFailures;

    internal HostPipelineWriter(IHostContext hostContext, PSRuleOption option, ShouldProcess shouldProcess)
        : base(null, option, shouldProcess)
    {
        _HostContext = hostContext;
        if (hostContext != null)
        {
            UseCommandRuntime(hostContext);
        }
    }

    public override bool HadErrors => _HadErrors || _HostContext.HadErrors;

    public override bool HadFailures => _HadFailures;

    public override void Begin()
    {
        // Do nothing.
    }

    private void UseCommandRuntime(IHostContext hostContext)
    {
        if (hostContext == null)
            return;

        OnWriteObject = hostContext.WriteObject;
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

    public override void WriteResult(InvokeResult result)
    {
        if (result == null) return;

        ProcessRecord(result.AsRecord());
    }

    public override void WriteHost(HostInformationMessage info)
    {
        _HostContext?.WriteHost(info.Message, info.BackgroundColor, info.ForegroundColor, info.NoNewLine);
    }

    public override bool IsEnabled(LogLevel logLevel)
    {
        return _HostContext?.IsEnabled(logLevel) ?? false;
    }

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            _HadErrors = true;

        if (!IsEnabled(logLevel) || _HostContext == null)
            return;

        _HostContext.Log(logLevel, eventId, state, exception, formatter);
    }

    public override void SetExitCode(int exitCode)
    {
        _HostContext?.SetExitCode(exitCode);
    }

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
