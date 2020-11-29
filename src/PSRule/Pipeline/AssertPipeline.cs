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
    internal interface IAssertFormatter : IPipelineWriter
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
                const string OUTPUT_SEPARATOR_BAR = "----------------------------";

                private readonly bool _VTSupport;

                protected readonly IPipelineWriter Writer;

                private bool _UnbrokenContent;
                private bool _UnbrokenInfo;

                protected AssertFormatterBase(Source[] source, IPipelineWriter writer, bool vtSupport)
                {
                    _VTSupport = vtSupport;
                    Writer = writer;
                    Banner();
                    Source(source);
                    Help(source);
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

                protected virtual void Pass(RuleRecord record)
                {
                    WritePass(FormatterStrings.Result_Pass, record);
                }

                protected virtual void Fail(RuleRecord record)
                {
                    WriteFail(FormatterStrings.Result_Fail, record);
                }

                protected virtual void FailWithError(RuleRecord record)
                {
                    WriteFailWithError(FormatterStrings.Result_Error, record);
                }

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
                    var link = record.Info?.GetOnlineHelpUri()?.ToString();
                    if (!string.IsNullOrEmpty(link))
                    {
                        LineBreak();
                        WriteLine(FormatterStrings.Help);
                        WriteLines(link, prefix: FormatterStrings.HelpLinkPrefix);
                    }
                    LineBreak();
                }

                protected virtual void ErrorDetail(RuleRecord record)
                {
                    
                }

                protected virtual void Error(string message)
                {
                    WriteErrorMessage(null, FormatterStrings.Result_Error, message);
                }

                protected virtual void Warning(string message)
                {
                    WriteWarningMessage(null, FormatterStrings.Result_Warning, message);
                }

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
                    WriteLineFormat(FormatterStrings.PSRuleVersion, version);
                    var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; source != null && i < source.Length; i++)
                    {
                        if (source[i].Module != null && !list.Contains(source[i].Module.Name))
                        {
                            WriteLineFormat(FormatterStrings.ModuleVersion, source[i].Module.Name, source[i].Module.Version);
                            list.Add(source[i].Module.Name);
                        }
                    }
                    LineBreak();
                }

                private void Help(Source[] source)
                {
                    WriteLine(OUTPUT_SEPARATOR_BAR);
                    WriteLine(FormatterStrings.HelpDocs);
                    WriteLine(FormatterStrings.HelpContribute);
                    WriteLine(FormatterStrings.HelpIssues);
                    var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; source != null && i < source.Length; i++)
                    {
                        if (source[i].Module != null && !list.Contains(source[i].Module.Name) && !string.IsNullOrEmpty(source[i].Module.ProjectUri))
                        {
                            WriteLineFormat(FormatterStrings.HelpModule, source[i].Module.Name, source[i].Module.ProjectUri);
                            list.Add(source[i].Module.Name);
                        }
                    }
                    WriteLine(OUTPUT_SEPARATOR_BAR);
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
                    Writer.WriteVerbose(message);
                }

                protected override void DoWriteInformation(InformationRecord informationRecord)
                {
                    Writer.WriteInformation(informationRecord);
                }

                protected override void DoWriteDebug(DebugRecord debugRecord)
                {
                    Writer.WriteDebug(debugRecord);
                }

                protected override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
                {
                    Writer.WriteObject(sendToPipeline, enumerateCollection);
                }

                public void End(int total, int fail, int error)
                {
                    LineBreak();
                    WriteLineFormat(FormatterStrings.Summary, total, fail, error);
                }

                protected void WriteLine(string prefix, ConsoleColor? forgroundColor, string message, params object[] args)
                {
                    var output = args == null || args.Length == 0 ? message : string.Format(Thread.CurrentThread.CurrentCulture, message, args);
                    Writer.WriteHost(new HostInformationMessage { Message = string.Concat(prefix, output), ForegroundColor = forgroundColor });
                }

                protected void WriteLine(string message, string prefix = null, ConsoleColor? forgroundColor = null)
                {
                    var output = string.IsNullOrEmpty(prefix) ? message : string.Concat(prefix, message);
                    Writer.WriteHost(new HostInformationMessage { Message = output, ForegroundColor = forgroundColor });
                }

                protected void WriteLineFormat(string message, params object[] args)
                {
                    WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, message, args));
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
                    Writer.WriteHost(new HostInformationMessage() { Message = string.Empty });
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

                protected ConsoleColor? GetErrorForeground()
                {
                    if (!_VTSupport)
                        return null;

                    return ConsoleColor.Red;
                }

                protected ConsoleColor? GetWarningForeground()
                {
                    if (!_VTSupport)
                        return null;

                    return ConsoleColor.Yellow;
                }

                protected ConsoleColor? GetPassForeground()
                {
                    if (!_VTSupport)
                        return null;

                    return ConsoleColor.Green;
                }

                protected ConsoleColor? GetFailForeground()
                {
                    if (!_VTSupport)
                        return null;

                    return ConsoleColor.Red;
                }

                protected void WriteErrorMessage(string prefix, string message, params object[] args)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(prefix, GetErrorForeground(), message, args);
                    UnbrokenInfo();
                }

                protected void WriteWarningMessage(string prefix, string message, params object[] args)
                {
                    BreakIfUnbrokenContent();
                    WriteLine(prefix, GetWarningForeground(), message, args);
                    UnbrokenInfo();
                }

                protected void WritePass(string message, RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(null, GetPassForeground(), message, record.RuleName);
                    UnbrokenContent();
                }

                protected void WriteFail(string message, RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(null, GetFailForeground(), message, record.RuleName);
                    FailDetail(record);
                }

                protected void WriteFailWithError(string message, RuleRecord record)
                {
                    BreakIfUnbrokenInfo();
                    WriteLine(null, GetFailForeground(), message, record.RuleName);
                    ErrorDetail(record);
                    UnbrokenContent();
                }
            }

            /// <summary>
            /// Client assert formatter.
            /// </summary>
            private sealed class ClientFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal ClientFormatter(Source[] source, IPipelineWriter logger)
                    : base(source, logger, true) { }

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

                protected override void ErrorDetail(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    LineBreak();
                    WriteLine(FormatterStrings.Message, forgroundColor: ConsoleColor.Red);
                    WriteLines(record.Error.Message, prefix: FormatterStrings.MessagePrefix, forgroundColor: ConsoleColor.Red);
                    LineBreak();
                    WriteLine(FormatterStrings.Position, forgroundColor: ConsoleColor.Cyan);
                    WriteLines(record.Error.PositionMessage, prefix: FormatterStrings.MessagePrefix, forgroundColor: ConsoleColor.Cyan);
                    LineBreak();
                    WriteLine(FormatterStrings.StackTrace, forgroundColor: ConsoleColor.Cyan);
                    WriteLines(record.Error.ScriptStackTrace, prefix: FormatterStrings.MessagePrefix, forgroundColor: ConsoleColor.Cyan);
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
                    var link = record.Info?.GetOnlineHelpUri()?.ToString();
                    if (!string.IsNullOrEmpty(link))
                    {
                        LineBreak();
                        WriteLine(FormatterStrings.Help, forgroundColor: ConsoleColor.Cyan);
                        WriteLines(link, prefix: FormatterStrings.HelpLinkPrefix, forgroundColor: ConsoleColor.Cyan);
                    }
                    LineBreak();
                }
            }

            /// <summary>
            /// Plain text assert formatter.
            /// </summary>
            private sealed class PlainFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal PlainFormatter(Source[] source, IPipelineWriter logger)
                    : base(source, logger, false) { }

                protected override void DoWriteError(ErrorRecord errorRecord)
                {
                    Error(errorRecord);
                }

                protected override void DoWriteWarning(string message)
                {
                    Warning(message);
                }

                protected override void ErrorDetail(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    LineBreak();
                    WriteLine(FormatterStrings.Message);
                    WriteLines(record.Error.Message, prefix: FormatterStrings.MessagePrefix);
                    LineBreak();
                    WriteLine(FormatterStrings.Position);
                    WriteLines(record.Error.PositionMessage, prefix: FormatterStrings.MessagePrefix);
                    LineBreak();
                    WriteLine(FormatterStrings.StackTrace);
                    WriteLines(record.Error.ScriptStackTrace, prefix: FormatterStrings.MessagePrefix);
                }
            }

            /// <summary>
            /// Formatter for Azure Pipelines.
            /// </summary>
            private sealed class AzurePipelinesFormatter : AssertFormatterBase, IAssertFormatter
            {
                private const string MESSAGE_PREFIX_ERROR = "##vso[task.logissue type=error]";
                private const string MESSAGE_PREFIX_WARNING = "##vso[task.logissue type=warning]";

                internal AzurePipelinesFormatter(Source[] source, IPipelineWriter logger)
                    : base(source, logger, false) { }

                protected override void Error(string message)
                {
                    WriteErrorMessage(MESSAGE_PREFIX_ERROR, message);
                }

                protected override void Warning(string message)
                {
                    WriteWarningMessage(MESSAGE_PREFIX_WARNING, message);
                }

                protected override void ErrorDetail(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    LineBreak();
                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Result_ErrorDetail, record.TargetName, record.RuleName, record.Error.Message));
                    LineBreak();
                    WriteLine(record.Error.PositionMessage);
                    LineBreak();
                    WriteLine(record.Error.ScriptStackTrace);
                }

                protected override void FailDetail(RuleRecord record)
                {
                    base.FailDetail(record);
                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Result_FailDetail, record.TargetName, record.RuleName, record.Info.Synopsis));
                    LineBreak();
                }
            }

            /// <summary>
            /// Formatter for GitHub Actions.
            /// </summary>
            private sealed class GitHubActionsFormatter : AssertFormatterBase, IAssertFormatter
            {
                private const string MESSAGE_PREFIX_ERROR = "::error::";
                private const string MESSAGE_PREFIX_WARNING = "::warning::";

                internal GitHubActionsFormatter(Source[] source, IPipelineWriter logger)
                    : base(source, logger, false) { }

                protected override void Error(string message)
                {
                    WriteErrorMessage(MESSAGE_PREFIX_ERROR, message);
                }

                protected override void Warning(string message)
                {
                    WriteWarningMessage(MESSAGE_PREFIX_WARNING, message);
                }

                protected override void ErrorDetail(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    LineBreak();
                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Result_ErrorDetail, record.TargetName, record.RuleName, record.Error.Message));
                    LineBreak();
                    WriteLine(record.Error.PositionMessage);
                    LineBreak();
                    WriteLine(record.Error.ScriptStackTrace);
                }

                protected override void FailDetail(RuleRecord record)
                {
                    base.FailDetail(record);
                    Error(string.Format(Thread.CurrentThread.CurrentCulture, FormatterStrings.Result_FailDetail, record.TargetName, record.RuleName, record.Info.Synopsis));
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
