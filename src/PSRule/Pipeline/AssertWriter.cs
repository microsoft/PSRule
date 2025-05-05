// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions.Rules;
using PSRule.Pipeline.Formatters;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// A writer for an Assert pipeline that formats results as text.
/// </summary>
internal sealed class AssertWriter : PipelineWriter
{
    internal readonly IAssertFormatter _Formatter;
    private readonly PipelineWriter _InnerWriter;
    private readonly string _ResultVariableName;
    private readonly IHostContext _HostContext;
    private readonly List<RuleRecord> _Results;
    private int _ErrorCount;
    private int _FailCount;
    private int _TotalCount;
    private bool _HadErrors;
    private bool _HadFailures;
    private bool _PSError;
    private SeverityLevel _Level;

    internal AssertWriter(PSRuleOption option, Source[] source, PipelineWriter inner, PipelineWriter next, OutputStyle style, string resultVariableName, IHostContext hostContext)
        : base(inner, option, hostContext.ShouldProcess)
    {
        _InnerWriter = next;
        _ResultVariableName = resultVariableName;
        _HostContext = hostContext;
        if (!string.IsNullOrEmpty(resultVariableName))
            _Results = [];

        _Formatter = GetFormatter(style, source, inner, option);
    }

    public override bool HadErrors => _HadErrors || base.HadErrors;

    public override bool HadFailures => _HadFailures || base.HadFailures;

    private static IAssertFormatter GetFormatter(OutputStyle style, Source[] source, PipelineWriter inner, PSRuleOption option)
    {
        if (style == OutputStyle.AzurePipelines)
            return new AzurePipelinesFormatter(source, inner, option);

        if (style == OutputStyle.GitHubActions)
            return new GitHubActionsFormatter(source, inner, option);

        if (style == OutputStyle.VisualStudioCode)
            return new VisualStudioCodeFormatter(source, inner, option);

        return style == OutputStyle.Plain ?
            new PlainFormatter(source, inner, option) :
            new ClientFormatter(source, inner, option);
    }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is not InvokeResult result)
            return;

        ProcessResult(result);
        _InnerWriter?.WriteObject(sendToPipeline, enumerateCollection);
    }

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            _HadErrors = true;

        if (!IsEnabled(logLevel))
            return;

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Information:
                base.Log(logLevel, eventId, state, exception, formatter);
                break;

            case LogLevel.Warning:
                _Formatter.Warning(new WarningRecord(formatter(state, exception)));
                break;

            case LogLevel.Error:
            case LogLevel.Critical:
                _PSError = true;
                var exitCode = eventId.Id == 0 ? 1 : eventId.Id;
                SetExitCode(exitCode);

                _Formatter.Error(new ErrorRecord(exception, "PSRule.Error", ErrorCategory.InvalidOperation, null));
                break;
        }
    }

    public override void Begin()
    {
        base.Begin();
        _Formatter.Begin();
    }

    public override void End(IPipelineResult result)
    {
        _Formatter.End(_TotalCount, _FailCount, _ErrorCount);
        base.End(result);
        try
        {
            if (_ErrorCount > 0)
            {
                _HadErrors = true;
                base.Log(LogLevel.Error, new EventId(0, "PSRule.Error"), ErrorCategory.InvalidOperation, new FailPipelineException(PSRuleResources.RuleErrorPipelineException), (s, e) => e!.Message);
            }
            else if (result.ShouldBreakFromFailure)
            {
                _HadFailures = true;
                base.Log(LogLevel.Error, new EventId(0, "PSRule.Fail"), ErrorCategory.InvalidData, new FailPipelineException(PSRuleResources.RuleFailPipelineException), (s, e) => e!.Message);
            }
            else if (_PSError)
            {
                _HadErrors = true;
                base.Log(LogLevel.Error, new EventId(0, "PSRule.Error"), ErrorCategory.InvalidOperation, new FailPipelineException(PSRuleResources.ErrorPipelineException), (s, e) => e!.Message);
            }
            else if (_FailCount > 0)
            {
                _HadFailures = true;
                base.WriteHost(new HostInformationMessage() { Message = PSRuleResources.RuleFailPipelineException });
            }

            if (_Results != null && _HostContext != null)
                _HostContext.SetVariable(_ResultVariableName, _Results.ToArray());
        }
        finally
        {
            _InnerWriter?.End(result);
        }
    }

    private void ProcessResult(InvokeResult result)
    {
        _Formatter.Result(result);
        _FailCount += result.Fail;
        _ErrorCount += result.Error;
        _TotalCount += result.Total;
        _Level = _Level.GetWorstCase(result.Level);
        _Results?.AddRange(result.AsRecord());
    }
}
