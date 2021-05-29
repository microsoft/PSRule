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

                style = GetStyle(style);
                _Formatter = GetFormatter(GetStyle(style), source, inner, option);
            }

            private static IAssertFormatter GetFormatter(OutputStyle style, Source[] source, PipelineWriter inner, PSRuleOption option)
            {
                switch (style)
                {
                    case OutputStyle.AzurePipelines: return new AzurePipelinesFormatter(source, inner, option);
                    case OutputStyle.GitHubActions: return new GitHubActionsFormatter(source, inner, option);
                    case OutputStyle.VisualStudioCode: return new VisualStudioCodeFormatter(source, inner, option);
                    case OutputStyle.Plain: return new PlainFormatter(source, inner, option);
                    default: return new ClientFormatter(source, inner, option);
                }
            }

            private static OutputStyle GetStyle(OutputStyle style)
            {
                if (style != OutputStyle.Detect)
                    return style;

                if (EnvironmentHelper.Default.TryBool("TF_BUILD", out bool azp) && azp)
                    return OutputStyle.AzurePipelines;

                if (EnvironmentHelper.Default.TryBool("GITHUB_ACTIONS", out bool gh) && gh)
                    return OutputStyle.GitHubActions;

                if (EnvironmentHelper.Default.TryString("TERM_PROGRAM", out string term) && term == "vscode")
                    return OutputStyle.VisualStudioCode;

                return OutputStyle.Client;
            }

            internal sealed class TerminalSupport
            {
                public TerminalSupport(int indent)
                {
                    BodyIndent = new string(' ', indent);
                    MessageIdent = BodyIndent;
                    StartResultIndent = FormatterStrings.StartObjectPrefix;
                    SourceLocationPrefix = "| ";
                    SynopsisPrefix = FormatterStrings.SynopsisPrefix;
                    PassStatus = FormatterStrings.Result_Pass;
                    FailStatus = FormatterStrings.Result_Fail;
                    WarningStatus = FormatterStrings.Result_Warning;
                    ErrorStatus = FormatterStrings.Result_Error;
                    RecommendationHeading = FormatterStrings.Recommend;
                    RecommendationPrefix = "| ";
                    ReasonHeading = FormatterStrings.Reason;
                    ReasonItemPrefix = "| - ";
                    HelpHeading = FormatterStrings.Help;
                    HelpLinkPrefix = "| - ";
                }

                public string BodyIndent { get; }

                public string MessageIdent { get; internal set; }

                public string StartResultIndent { get; internal set; }

                public ConsoleColor? StartResultForegroundColor { get; internal set; }

                public string SourceLocationPrefix { get; internal set; }

                public ConsoleColor? SourceLocationForegroundColor { get; internal set; }

                public string SynopsisPrefix { get; internal set; }

                public ConsoleColor? SynopsisForegroundColor { get; internal set; }

                public string PassStatus { get; internal set; }

                public ConsoleColor? PassForegroundColor { get; internal set; }

                public ConsoleColor? PassBackgroundColor { get; internal set; }

                public ConsoleColor? PassStatusBackgroundColor { get; internal set; }

                public ConsoleColor? PassStatusForegroundColor { get; internal set; }

                public string FailStatus { get; internal set; }

                public ConsoleColor? FailForegroundColor { get; internal set; }

                public ConsoleColor? FailBackgroundColor { get; internal set; }

                public ConsoleColor? FailStatusBackgroundColor { get; internal set; }

                public ConsoleColor? FailStatusForegroundColor { get; internal set; }

                public string WarningStatus { get; internal set; }

                public ConsoleColor? WarningForegroundColor { get; internal set; }

                public ConsoleColor? WarningBackgroundColor { get; internal set; }

                public ConsoleColor? WarningStatusBackgroundColor { get; internal set; }

                public ConsoleColor? WarningStatusForegroundColor { get; internal set; }

                public string ErrorStatus { get; internal set; }

                public ConsoleColor? ErrorForegroundColor { get; internal set; }

                public ConsoleColor? ErrorBackgroundColor { get; internal set; }

                public ConsoleColor? ErrorStatusBackgroundColor { get; internal set; }

                public ConsoleColor? ErrorStatusForegroundColor { get; internal set; }

                public ConsoleColor? BodyForegroundColor { get; internal set; }

                public string RecommendationHeading { get; internal set; }

                public string RecommendationPrefix { get; internal set; }

                public string ReasonHeading { get; internal set; }

                public string ReasonItemPrefix { get; internal set; }

                public string HelpHeading { get; internal set; }

                public string HelpLinkPrefix { get; internal set; }
            }

            /// <summary>
            /// A base class for a formatter.
            /// </summary>
            private abstract class AssertFormatterBase : PipelineLoggerBase, IAssertFormatter
            {
                private const string OUTPUT_SEPARATOR_BAR = "----------------------------";

                protected readonly IPipelineWriter Writer;
                protected readonly PSRuleOption Option;

                private bool _UnbrokenContent;
                private bool _UnbrokenInfo;
                private bool _UnbrokenObject;

                private static readonly TerminalSupport DefaultTerminalSupport = new TerminalSupport(4);

                protected AssertFormatterBase(Source[] source, IPipelineWriter writer, PSRuleOption option)
                {
                    Writer = writer;
                    Option = option;
                    Banner();
                    Source(source);
                    SupportLinks(source);
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
                    if ((Option.Output.Outcome.Value & result.Outcome) != result.Outcome)
                        return;

                    StartResult(result, out RuleRecord[] records);
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
                    if (Option.Output.As == ResultFormat.Detail && (Option.Output.Outcome.Value & RuleOutcome.Pass) == RuleOutcome.Pass)
                        WritePass(record);
                }

                protected virtual void Fail(RuleRecord record)
                {
                    if ((Option.Output.Outcome.Value & RuleOutcome.Fail) == RuleOutcome.Fail)
                        WriteFail(record);
                }

                protected virtual void FailWithError(RuleRecord record)
                {
                    if ((Option.Output.Outcome.Value & RuleOutcome.Error) == RuleOutcome.Error)
                        WriteFailWithError(record);
                }

                protected virtual void FailDetail(RuleRecord record)
                {
                    WriteSourceLocation(record, shouldBreak: false);
                    WriteRecommendation(record);
                    WriteReason(record);
                    WriteHelp(record);
                    LineBreak();
                }

                protected virtual void ErrorDetail(RuleRecord record)
                {

                }

                protected virtual void Error(string message)
                {
                    WriteErrorMessage(GetTerminalSupport().MessageIdent, message);
                }

                protected virtual void Warning(string message)
                {
                    WriteWarningMessage(GetTerminalSupport().MessageIdent, message);
                }

                protected void Banner()
                {
                    if (!Option.Output.Banner.GetValueOrDefault(BannerFormat.Default).HasFlag(BannerFormat.Title))
                        return;

                    WriteLine(FormatterStrings.Banner.Replace("\\n", Environment.NewLine));
                    LineBreak();
                }

                protected void StartResult(InvokeResult result, out RuleRecord[] records)
                {
                    records = result.AsRecord();
                    if (records == null || records.Length == 0)
                        return;

                    BreakIfUnbrokenContent();
                    BreakIfUnbrokenInfo();
                    WriteStartResult(result);
                    UnbrokenObject();
                }

                private void WriteStartResult(InvokeResult result)
                {
                    WriteLine(string.Concat(GetTerminalSupport().StartResultIndent, result.TargetName, " : ", result.TargetType, " [", result.Pass, "/", result.Total, "]"), forgroundColor: GetTerminalSupport().StartResultForegroundColor);
                }

                protected virtual TerminalSupport GetTerminalSupport()
                {
                    return DefaultTerminalSupport;
                }

                private void Source(Source[] source)
                {
                    if (!Option.Output.Banner.GetValueOrDefault(BannerFormat.Default).HasFlag(BannerFormat.Source))
                        return;

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

                private void SupportLinks(Source[] source)
                {
                    if (!Option.Output.Banner.GetValueOrDefault(BannerFormat.Default).HasFlag(BannerFormat.SupportLinks))
                        return;

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

                protected void WriteStatus(string status, string statusIndent, ConsoleColor? statusForeground, ConsoleColor? statusBackground, ConsoleColor? messageForeground, ConsoleColor? messageBackground, string message)
                {
                    var output = message;
                    if (statusForeground != null || statusBackground != null)
                    {
                        Writer.WriteHost(new HostInformationMessage { Message = statusIndent, NoNewLine = true });
                        Writer.WriteHost(new HostInformationMessage { Message = status, ForegroundColor = statusForeground, BackgroundColor = statusBackground, NoNewLine = true });
                        Writer.WriteHost(new HostInformationMessage { Message = " ", NoNewLine = true });
                    }
                    else
                    {
                        output = string.Concat(status, output);
                    }
                    Writer.WriteHost(new HostInformationMessage { Message = output, ForegroundColor = messageForeground, BackgroundColor = messageBackground });
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

                protected void WriteIndentedLine(string message, string indent, string prefix = null, ConsoleColor? forgroundColor = null)
                {
                    if (string.IsNullOrEmpty(message))
                        return;

                    var output = string.Concat(indent, prefix, message);
                    Writer.WriteHost(new HostInformationMessage { Message = output, ForegroundColor = forgroundColor });
                }

                protected void WriteIndentedLines(string message, string indent, string prefix = null, ConsoleColor? forgroundColor = null)
                {
                    if (string.IsNullOrEmpty(message))
                        return;

                    var lines = message.SplitSemantic();
                    for (var i = 0; i < lines.Length; i++)
                        WriteIndentedLine(lines[i], indent, prefix, forgroundColor);
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
                    _UnbrokenContent = _UnbrokenInfo = _UnbrokenObject = false;
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

                protected void BreakIfUnbrokenObject()
                {
                    if (!_UnbrokenObject)
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

                protected void UnbrokenObject()
                {
                    _UnbrokenObject = true;
                }

                protected void WriteSourceLocation(RuleRecord record, bool shouldBreak = true)
                {
                    if (record.Source != null && record.Source.Length > 0)
                    {
                        if (shouldBreak)
                            LineBreak();

                        for (var i = 0; i < record.Source.Length; i++)
                            WriteIndentedLine(
                                record.Source[i].ToString(FormatterStrings.SourceAt, useRelativePath: true),
                                GetTerminalSupport().BodyIndent,
                                GetTerminalSupport().SourceLocationPrefix,
                                forgroundColor: GetTerminalSupport().SourceLocationForegroundColor
                            );
                    }
                }

                protected void WriteSynopsis(RuleRecord record, bool shouldBreak = true)
                {
                    if (!string.IsNullOrEmpty(record.Info.Synopsis))
                    {
                        if (shouldBreak)
                            LineBreak();

                        WriteIndentedLine(
                            record.Info.Synopsis,
                            GetTerminalSupport().BodyIndent,
                            GetTerminalSupport().SynopsisPrefix,
                            forgroundColor: GetTerminalSupport().SynopsisForegroundColor
                        );
                    }
                }

                protected void WriteRecommendation(RuleRecord record)
                {
                    if (!string.IsNullOrEmpty(record.Recommendation))
                    {
                        LineBreak();
                        WriteLine(GetTerminalSupport().RecommendationHeading, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                        WriteIndentedLines(
                            record.Recommendation,
                            GetTerminalSupport().BodyIndent,
                            GetTerminalSupport().RecommendationPrefix,
                            forgroundColor: GetTerminalSupport().BodyForegroundColor
                        );
                    }
                }

                protected void WriteReason(RuleRecord record)
                {
                    if (record.Reason != null && record.Reason.Length > 0)
                    {
                        LineBreak();
                        WriteLine(GetTerminalSupport().ReasonHeading, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                        for (var i = 0; i < record.Reason.Length; i++)
                        {
                            WriteIndentedLine(
                                record.Reason[i],
                                GetTerminalSupport().BodyIndent,
                                GetTerminalSupport().ReasonItemPrefix,
                                forgroundColor: GetTerminalSupport().BodyForegroundColor
                            );
                        }
                    }
                }

                protected void WriteHelp(RuleRecord record)
                {
                    var link = record.Info?.GetOnlineHelpUri()?.ToString();
                    if (!string.IsNullOrEmpty(link))
                    {
                        LineBreak();
                        WriteLine(GetTerminalSupport().HelpHeading, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                        WriteIndentedLine(
                            link,
                            GetTerminalSupport().BodyIndent,
                            GetTerminalSupport().HelpLinkPrefix,
                            forgroundColor: GetTerminalSupport().BodyForegroundColor
                        );
                    }
                }

                protected void WriteErrorMessage(string indent, string message)
                {
                    BreakIfUnbrokenObject();
                    BreakIfUnbrokenContent();
                    WriteStatus(GetTerminalSupport().ErrorStatus, indent, GetTerminalSupport().ErrorStatusForegroundColor, GetTerminalSupport().ErrorStatusBackgroundColor, GetTerminalSupport().ErrorForegroundColor, GetTerminalSupport().ErrorBackgroundColor, message);
                    UnbrokenInfo();
                }

                protected void WriteWarningMessage(string indent, string message)
                {
                    BreakIfUnbrokenObject();
                    BreakIfUnbrokenContent();
                    WriteStatus(GetTerminalSupport().WarningStatus, indent, GetTerminalSupport().WarningStatusForegroundColor, GetTerminalSupport().WarningStatusBackgroundColor, GetTerminalSupport().WarningForegroundColor, GetTerminalSupport().WarningBackgroundColor, message);
                    UnbrokenInfo();
                }

                protected void WritePass(RuleRecord record)
                {
                    BreakIfUnbrokenObject();
                    BreakIfUnbrokenInfo();
                    WriteStatus(GetTerminalSupport().PassStatus, GetTerminalSupport().BodyIndent, GetTerminalSupport().PassStatusForegroundColor, GetTerminalSupport().PassStatusBackgroundColor, GetTerminalSupport().PassForegroundColor, GetTerminalSupport().PassBackgroundColor, record.RuleName);
                    UnbrokenContent();
                }

                protected void WriteFail(RuleRecord record)
                {
                    BreakIfUnbrokenObject();
                    BreakIfUnbrokenInfo();
                    WriteStatus(GetTerminalSupport().FailStatus, GetTerminalSupport().BodyIndent, GetTerminalSupport().FailStatusForegroundColor, GetTerminalSupport().FailStatusBackgroundColor, GetTerminalSupport().FailForegroundColor, GetTerminalSupport().FailBackgroundColor, record.RuleName);
                    FailDetail(record);
                }

                protected void WriteFailWithError(RuleRecord record)
                {
                    BreakIfUnbrokenObject();
                    BreakIfUnbrokenInfo();
                    WriteStatus(GetTerminalSupport().ErrorStatus, GetTerminalSupport().BodyIndent, GetTerminalSupport().ErrorStatusForegroundColor, GetTerminalSupport().ErrorStatusBackgroundColor, GetTerminalSupport().ErrorForegroundColor, GetTerminalSupport().ErrorBackgroundColor, record.RuleName);
                    ErrorDetail(record);
                    UnbrokenContent();
                }
            }

            /// <summary>
            /// Client assert formatter.
            /// </summary>
            private sealed class ClientFormatter : AssertFormatterBase, IAssertFormatter
            {
                private readonly TerminalSupport _TerminalSupport;

                internal ClientFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
                    : base(source, logger, option)
                {
                    _TerminalSupport = new TerminalSupport(4)
                    {
                        StartResultForegroundColor = ConsoleColor.Green,
                        SourceLocationForegroundColor = ConsoleColor.Red,
                        SynopsisForegroundColor = ConsoleColor.Red,
                        ErrorForegroundColor = ConsoleColor.Red,
                        FailForegroundColor = ConsoleColor.Red,
                        PassForegroundColor = ConsoleColor.Green,
                        WarningForegroundColor = ConsoleColor.Yellow,
                        BodyForegroundColor = ConsoleColor.Cyan,
                    };
                }

                protected override TerminalSupport GetTerminalSupport()
                {
                    return _TerminalSupport;
                }

                protected override void ErrorDetail(RuleRecord record)
                {
                    if (record.Error == null)
                        return;

                    LineBreak();
                    WriteLine(FormatterStrings.Message, forgroundColor: GetTerminalSupport().ErrorForegroundColor);
                    WriteIndentedLine(record.Error.Message, GetTerminalSupport().BodyIndent, forgroundColor: GetTerminalSupport().ErrorForegroundColor);
                    LineBreak();
                    WriteLine(FormatterStrings.Position, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                    WriteIndentedLine(record.Error.PositionMessage, GetTerminalSupport().BodyIndent, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                    LineBreak();
                    WriteLine(FormatterStrings.StackTrace, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                    WriteIndentedLine(record.Error.ScriptStackTrace, GetTerminalSupport().BodyIndent, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                }
            }

            /// <summary>
            /// Plain text assert formatter.
            /// </summary>
            private sealed class PlainFormatter : AssertFormatterBase, IAssertFormatter
            {
                internal PlainFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
                    : base(source, logger, option) { }

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
                    WriteLine(FormatterStrings.Message, forgroundColor: GetTerminalSupport().ErrorForegroundColor);
                    WriteIndentedLine(record.Error.Message, GetTerminalSupport().BodyIndent, forgroundColor: GetTerminalSupport().ErrorForegroundColor);
                    LineBreak();
                    WriteLine(FormatterStrings.Position, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                    WriteIndentedLine(record.Error.PositionMessage, GetTerminalSupport().BodyIndent, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                    LineBreak();
                    WriteLine(FormatterStrings.StackTrace, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                    WriteIndentedLine(record.Error.ScriptStackTrace, GetTerminalSupport().BodyIndent, forgroundColor: GetTerminalSupport().BodyForegroundColor);
                }
            }

            /// <summary>
            /// Formatter for Azure Pipelines.
            /// </summary>
            private sealed class AzurePipelinesFormatter : AssertFormatterBase, IAssertFormatter
            {
                private const string MESSAGE_PREFIX_ERROR = "##vso[task.logissue type=error]";
                private const string MESSAGE_PREFIX_WARNING = "##vso[task.logissue type=warning]";

                private readonly TerminalSupport _TerminalSupport;

                internal AzurePipelinesFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
                    : base(source, logger, option)
                {
                    _TerminalSupport = new TerminalSupport(4)
                    {
                        MessageIdent = string.Empty,
                        ErrorStatus = MESSAGE_PREFIX_ERROR,
                        WarningStatus = MESSAGE_PREFIX_WARNING,
                    };
                }

                protected override TerminalSupport GetTerminalSupport()
                {
                    return _TerminalSupport;
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

                private readonly TerminalSupport _TerminalSupport;

                internal GitHubActionsFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
                    : base(source, logger, option)
                {
                    _TerminalSupport = new TerminalSupport(4)
                    {
                        MessageIdent = string.Empty,
                        ErrorStatus = MESSAGE_PREFIX_ERROR,
                        WarningStatus = MESSAGE_PREFIX_WARNING,
                    };
                }

                protected override TerminalSupport GetTerminalSupport()
                {
                    return _TerminalSupport;
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
            /// Visual Studio Code assert formatter.
            /// </summary>
            private sealed class VisualStudioCodeFormatter : AssertFormatterBase, IAssertFormatter
            {
                private readonly TerminalSupport _TerminalSupport;

                internal VisualStudioCodeFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
                    : base(source, logger, option)
                {
                    _TerminalSupport = new TerminalSupport(2)
                    {
                        StartResultIndent = FormatterStrings.VSCode_StartObjectPrefix,
                        StartResultForegroundColor = ConsoleColor.Green,
                        SourceLocationForegroundColor = ConsoleColor.Cyan,
                        SourceLocationPrefix = null,
                        SynopsisPrefix = null,
                        SynopsisForegroundColor = ConsoleColor.Cyan,
                        ErrorStatus = FormatterStrings.VSCode_Error,
                        ErrorForegroundColor = ConsoleColor.Red,
                        ErrorStatusForegroundColor = ConsoleColor.Black,
                        ErrorStatusBackgroundColor = ConsoleColor.Red,
                        FailStatus = FormatterStrings.VSCode_Fail,
                        FailForegroundColor = ConsoleColor.Red,
                        FailStatusForegroundColor = ConsoleColor.Black,
                        FailStatusBackgroundColor = ConsoleColor.Red,
                        PassStatus = FormatterStrings.VSCode_Pass,
                        PassForegroundColor = ConsoleColor.Green,
                        PassStatusForegroundColor = ConsoleColor.Black,
                        PassStatusBackgroundColor = ConsoleColor.Green,
                        WarningStatus = FormatterStrings.VSCode_Warning,
                        WarningForegroundColor = ConsoleColor.Yellow,
                        WarningStatusForegroundColor = ConsoleColor.Black,
                        WarningStatusBackgroundColor = ConsoleColor.Yellow,
                        BodyForegroundColor = ConsoleColor.White,
                        RecommendationHeading = FormatterStrings.VSCode_Recommend,
                        RecommendationPrefix = null,
                        ReasonHeading = FormatterStrings.VSCode_Reason,
                        ReasonItemPrefix = "- ",
                        HelpHeading = FormatterStrings.VSCode_Help,
                        HelpLinkPrefix = "- ",
                    };
                }

                protected override TerminalSupport GetTerminalSupport()
                {
                    return _TerminalSupport;
                }

                protected override void FailDetail(RuleRecord record)
                {
                    WriteSynopsis(record, shouldBreak: true);
                    WriteSourceLocation(record, shouldBreak: true);
                    WriteRecommendation(record);
                    WriteReason(record);
                    WriteHelp(record);
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

        public sealed override IPipeline Build()
        {
            if (!RequireModules() || !RequireSources())
                return null;

            return new InvokeRulePipeline(PrepareContext(BindTargetNameHook, BindTargetTypeHook, BindFieldHook), Source, PrepareWriter(), RuleOutcome.Processed);
        }
    }
}
