// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;
using System.Threading;

namespace PSRule.Pipeline
{
    internal interface IAssertFormatter : ILogger
    {
        void Result(InvokeResult result);

        void Error(ErrorRecord errorRecord);

        void Warning(WarningRecord warningRecord);

        void End(int total, int fail, int error);
    }

    /// <summary>
    /// A helper to construct the pipeline for Assert-PSRule.
    /// </summary>
    internal sealed class AssertPipelineBuilder : InvokePipelineBuilderBase
    {
        private AssertWriter _Writer;

        internal AssertPipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext) { }

        /// <summary>
        /// A writer for outputting assertions.
        /// </summary>
        private sealed class AssertWriter : PipelineWriter
        {
            internal readonly IAssertFormatter _Formatter;
            private readonly PipelineWriter _InnerWriter;
            private readonly string _ResultVariableName;
            private readonly PSCmdlet _CmdletContext;
            private readonly EngineIntrinsics _ExecutionContext;
            private readonly List<RuleRecord> _Results;
            private int _ErrorCount;
            private int _FailCount;
            private int _TotalCount;
            private bool _PSError;

            internal AssertWriter(PSRuleOption option, Source[] source, PipelineWriter inner, PipelineWriter next, OutputStyle style, string resultVariableName, PSCmdlet cmdletContext, EngineIntrinsics executionContext)
                : base(inner, option)
            {
                _InnerWriter = next;
                _ResultVariableName = resultVariableName;
                _CmdletContext = cmdletContext;
                _ExecutionContext = executionContext;
                if (!string.IsNullOrEmpty(resultVariableName))
                    _Results = new List<RuleRecord>();

                if (style == OutputStyle.AzurePipelines)
                    _Formatter = new AzurePipelinesFormatter(source, inner);
                else if (style == OutputStyle.GitHubActions)
                    _Formatter = new GitHubActionsFormatter(source, inner);
                else if (style == OutputStyle.Plain)
                    _Formatter = new PlainFormatter(source, inner);
                else if (style == OutputStyle.Client)
                    _Formatter = new ClientFormatter(source, inner);
            }

            /// <summary>
            /// A base class for a formatter.
            /// </summary>
            private abstract class AssertFormatterBase : PipelineLoggerBase, IAssertFormatter
            {
                protected readonly ILogger Logger;

                private bool _UnbrokenContent;
                private bool _UnbrokenInfo;

                protected AssertFormatterBase(Source[] source, ILogger logger)
                {
                    Logger = logger;
                    Banner();
                    Source(source);
                }

                public void Error(ErrorRecord errorRecord)
                {
                    Error(errorRecord.Exception.Message);
                }

                public void Warning(WarningRecord warningRecord)
                {
                    Warning(warningRecord.Message);
                }

                public virtual void Result(InvokeResult result)
                {
                    StartObject(result, out RuleRecord[] records);
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (records[i].IsSuccess())
                            Pass(records[i]);
                        else if (records[i].Outcome == RuleOutcome.Error)
                            FailWithError(records[i]);
                        else
                            Fail(records[i]);
                    }
                }

                protected abstract void Pass(RuleRecord record);

                protected abstract void Fail(RuleRecord record);

                protected abstract void FailWithError(RuleRecord record);

                protected virtual void FailDetail(RuleRecord record)
                {
                    if (!string.IsNullOrEmpty(record.Recommendation))
                    {
                        LineBreak();
                        WriteLine(FormatterStrings.Recommend);
                        WriteLines(record.Recommendation, prefix: FormatterStrings.RecommendPrefix);
                    }
                    if (record.Reason != null && record.Reason.Length > 0)
                    {
                        LineBreak();
                        WriteLine(FormatterStrings.Reason);
                        for (var i = 0; i < record.Reason.Length; i++)
                        {
                            WriteLines(record.Reason[i], prefix: FormatterStrings.ReasonPrefix);
                        }
                    }
                    LineBreak();
                }

                protected virtual void ErrorDetail(RuleRecord record)
                {

                }

                protected abstract void Error(string message);

                protected abstract void Warning(string message);

                protected void Banner()
                {
                    WriteLine(FormatterStrings.Banner.Replace("\\n", Environment.NewLine));
                    LineBreak();
                }

                protected void StartObject(InvokeResult result, out RuleRecord[] records, ConsoleColor? forgroundColor = null)
                {
                    records = result.AsRecord();
                    if (records == null || records.Length == 0)
                        return;

                    BreakIfUnbrokenContent();
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat(" -> ", records[0].TargetName, " : ", records[0].TargetType), forgroundColor: forgroundColor);
                    LineBreak();
                }

                private void Source(Source[] source)
                {
                    var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.PSRuleVersion, version));
                    var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; source != null && i < source.Length; i++)
                    {
                        if (source[i].Module != null && !list.Contains(source[i].Module.Name))
                        {
                            WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.ModuleVersion, source[i].Module.Name, source[i].Module.Version));
                            list.Add(source[i].Module.Name);
                        }
                    }
                    LineBreak();
                }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                protected override void DoWriteVerbose(string message)
                {
                    Logger.WriteVerbose(message);
                }

                protected override void DoWriteInformation(InformationRecord informationRecord)
                {
                    Logger.WriteInformation(informationRecord);
                }

                protected override void DoWriteDebug(DebugRecord debugRecord)
                {
                    Logger.WriteDebug(debugRecord);
                }

                protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
                {
                    Logger.WriteObject(sendToPipeline, enumerateCollection);
                }

                public void End(int total, int fail, int error)
                {
                    LineBreak();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Summary, total, fail, error));
                }

                protected void WriteLine(string message, string prefix = null, ConsoleColor? forgroundColor = null)
                {
                    var output = string.IsNullOrEmpty(prefix) ? message : string.Concat(prefix, message);
                    Logger.WriteHost(new HostInformationMessage { Message = output, ForegroundColor = forgroundColor });
                }

                protected void WriteLines(string message, string prefix = null, ConsoleColor? forgroundColor = null)
                {
                    if (string.IsNullOrEmpty(message))
                        return;

                    var lines = message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    for (var i = 0; i < lines.Length; i++)
                        WriteLine(lines[i], prefix, forgroundColor);
                }

                protected void LineBreak()
                {
                    Logger.WriteHost(new HostInformationMessage() { Message = string.Empty });
                    _UnbrokenContent = false;
                    _UnbrokenInfo = false;
                }

                protected void BreakIfUnbrokenInfo()
                {
                    if (!_UnbrokenInfo)
                        return;

                    LineBreak();
                }

                protected void BreakIfUnbrokenContent()
                {
                    if (!_UnbrokenContent)
                        return;

                    LineBreak();
                }

                protected void UnbrokenInfo()
                {
                    _UnbrokenInfo = true;
                }

                protected void UnbrokenContent()
                {
                    _UnbrokenContent = true;
                }
            }

            /// <summary>
            /// Client assert formatter.
            /// </summary>
            private sealed class ClientFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal ClientFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                public override void Result(InvokeResult result)
                {
                    StartObject(result, out RuleRecord[] records, forgroundColor: ConsoleColor.Green);
                    for (var i = 0; i < records.Length; i++)
                    {
                        if (records[i].IsSuccess())
                            Pass(records[i]);
                        else if (records[i].Outcome == RuleOutcome.Error)
                            FailWithError(records[i]);
                        else
                            Fail(records[i]);
                    }
                }

                protected override void Error(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Client_Error, message), forgroundColor: ConsoleColor.Red);
                    UnbrokenInfo();
                }

                protected override void Warning(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Client_Warning, message), forgroundColor: ConsoleColor.Yellow);
                    UnbrokenInfo();
                }

                protected override void Pass(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Client_Pass, record.RuleName), forgroundColor: ConsoleColor.Green);
                    UnbrokenContent();
                }

                protected override void Fail(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Client_Fail, record.RuleName), forgroundColor: ConsoleColor.Red);
                    FailDetail(record);
                }

                protected override void FailWithError(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Client_Error, record.RuleName), forgroundColor: ConsoleColor.Red);
                    ErrorDetail(record);
                    UnbrokenContent();
                }

                protected override void FailDetail(RuleRecord record)
                {
                    LineBreak();
                    WriteLine(FormatterStrings.Recommend, forgroundColor: ConsoleColor.Cyan);
                    WriteLines(record.Recommendation, prefix: FormatterStrings.RecommendPrefix, forgroundColor: ConsoleColor.Cyan);
                    if (record.Reason != null && record.Reason.Length > 0)
                    {
                        LineBreak();
                        WriteLine(FormatterStrings.Reason, forgroundColor: ConsoleColor.Cyan);
                        for (var i = 0; i < record.Reason.Length; i++)
                        {
                            WriteLines(record.Reason[i], prefix: FormatterStrings.ReasonPrefix, forgroundColor: ConsoleColor.Cyan);
                        }
                    }
                    LineBreak();
                }
            }

            /// <summary>
            /// Plain text assert formatter.
            /// </summary>
            private sealed class PlainFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal PlainFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                protected override void Error(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Plain_Error, message));
                    UnbrokenInfo();
                }

                protected override void Warning(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Plain_Warning, message));
                    UnbrokenInfo();
                }

                protected override void Pass(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Plain_Pass, record.RuleName));
                    UnbrokenContent();
                }

                protected override void Fail(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Plain_Fail, record.RuleName));
                    FailDetail(record);
                }

                protected override void FailWithError(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Plain_Fail, record.RuleName));
                    ErrorDetail(record);
                    UnbrokenContent();
                }
            }

            /// <summary>
            /// Formatter for Azure Pipelines.
            /// </summary>
            private sealed class AzurePipelinesFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal AzurePipelinesFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                private string GetReason(RuleRecord record)
                {
                    return string.Join(" ", record.Reason);
                }

                protected override void Error(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Concat("##vso[task.logissue type=error]", message));
                    UnbrokenInfo();
                }

                protected override void Warning(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Concat("##vso[task.logissue type=warning]", message));
                    UnbrokenInfo();
                }

                protected override void Pass(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat("    [+] ", record.RuleName));
                    UnbrokenContent();
                }

                protected override void Fail(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat("    [-] ", record.RuleName));
                    FailDetail(record);
                }

                protected override void FailWithError(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat("    [-] ", record.RuleName));
                    ErrorDetail(record);
                    UnbrokenContent();
                }

                protected override void FailDetail(RuleRecord record)
                {
                    base.FailDetail(record);
                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.AzurePipelines_Fail, record.TargetName, record.RuleName, record.Info.Synopsis));
                    LineBreak();
                }

                protected override void ErrorDetail(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.AzurePipelines_Error, record.TargetName, record.RuleName, record.Error.Message));
                    LineBreak();
                    WriteLine(record.Error.ScriptStackTrace);
                }
            }

            /// <summary>
            /// Formatter for GitHub Actions.
            /// </summary>
            private sealed class GitHubActionsFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal GitHubActionsFormatter(Source[] source, ILogger logger)
                    : base(source, logger) { }

                private string GetReason(RuleRecord record)
                {
                    return string.Join(" ", record.Reason);
                }

                protected override void Error(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Concat("::error::", message));
                    UnbrokenInfo();
                }

                protected override void Warning(string message)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(string.Concat("::warning::", message));
                    UnbrokenInfo();
                }

                protected override void Pass(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat("    [+] ", record.RuleName));
                    UnbrokenContent();
                }

                protected override void Fail(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat("    [-] ", record.RuleName));
                    FailDetail(record);
                }

                protected override void FailWithError(RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(string.Concat("    [-] ", record.RuleName));
                    ErrorDetail(record);
                    UnbrokenContent();
                }

                protected override void FailDetail(RuleRecord record)
                {
                    base.FailDetail(record);
                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.GitHubActions_Fail, record.TargetName, record.RuleName, record.Info.Synopsis));
                    LineBreak();
                }
            }

            public override void WriteObject(object sendToPipeline, bool enumerateCollection)
            {
                if (!(sendToPipeline is InvokeResult result))
                    return;

                ProcessResult(result);
                if (_InnerWriter != null)
                    _InnerWriter.WriteObject(sendToPipeline, enumerateCollection);
            }

            public override void WriteWarning(string message)
            {
                var warningPreference = GetPreferenceVariable(_ExecutionContext.SessionState, WarningPreference);
                if (warningPreference == ActionPreference.Ignore || warningPreference == ActionPreference.SilentlyContinue)
                    return;

                _Formatter.Warning(new WarningRecord(message));
            }

            public override void WriteError(ErrorRecord errorRecord)
            {
                var errorPreference = GetPreferenceVariable(_ExecutionContext.SessionState, ErrorPreference);
                if (errorPreference == ActionPreference.Ignore || errorPreference == ActionPreference.SilentlyContinue)
                    return;

                _PSError = true;
                _Formatter.Error(errorRecord);
            }

            public override void End()
            {
                _Formatter.End(_TotalCount, _FailCount, _ErrorCount);
                base.End();
                try
                {
                    if (_ErrorCount > 0)
                        base.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.RuleErrorPipelineException), "PSRule.Error", ErrorCategory.InvalidOperation, null));
                    else if (_FailCount > 0)
                        base.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.RuleFailPipelineException), "PSRule.Fail", ErrorCategory.InvalidData, null));
                    else if (_PSError)
                        base.WriteError(new ErrorRecord(new FailPipelineException(PSRuleResources.ErrorPipelineException), "PSRule.Error", ErrorCategory.InvalidOperation, null));

                    if (_Results != null && _CmdletContext != null)
                        _CmdletContext.SessionState.PSVariable.Set(_ResultVariableName, _Results.ToArray());
                }
                finally
                {
                    if (_InnerWriter != null)
                        _InnerWriter.End();
                }
            }

            private void ProcessResult(InvokeResult result)
            {
                _Formatter.Result(result);
                _FailCount += result.Fail;
                _ErrorCount += result.Error;
                _TotalCount += result.Total;
                if (_Results != null)
                    _Results.AddRange(result.AsRecord());
            }
        }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            base.Configure(option);
            Option.Output.As = ResultFormat.Detail;
            Option.Output.Outcome = RuleOutcome.Processed;
            return this;
        }

        protected override PipelineWriter PrepareWriter()
        {
            return GetWriter();
        }

        private AssertWriter GetWriter()
        {
            if (_Writer == null)
            {
                var next = ShouldOutput() ? base.PrepareWriter() : null;
                _Writer = new AssertWriter(
                    Option,
                    Source,
                    GetOutput(),
                    next,
                    Option.Output.Style ?? OutputOption.Default.Style.Value,
                    _ResultVariableName,
                    HostContext.CmdletContext,
                    HostContext.ExecutionContext
                );
            }
            return _Writer;
        }

        private bool ShouldOutput()
        {
            return !(string.IsNullOrEmpty(Option.Output.Path) ||
                Option.Output.Format == OutputFormat.Wide ||
                Option.Output.Format == OutputFormat.None);
        }
    }
}
