// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters
{
    internal interface IAssertFormatter : IPipelineWriter
    {
        void Result(InvokeResult result);

        void Error(ErrorRecord errorRecord);

        void Warning(WarningRecord warningRecord);

        void End(int total, int fail, int error);
    }

    /// <summary>
    /// Configures formatted output.
    /// </summary>
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
            InformationStatus = FormatterStrings.Result_Information;
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

        public string InformationStatus { get; internal set; }

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
    internal abstract class AssertFormatterBase : PipelineLoggerBase, IAssertFormatter
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
            RepositoryInfo();
        }

        #region IAssertFormatter

        public void Error(ErrorRecord errorRecord)
        {
            Error(errorRecord.Exception.Message);
        }

        public void Warning(WarningRecord warningRecord)
        {
            Warning(warningRecord.Message);
        }

        public void Result(InvokeResult result)
        {
            if ((Option.Output.Outcome.Value & result.Outcome) != result.Outcome)
                return;

            StartResult(result, out var records);
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

        public void End(int total, int fail, int error)
        {
            if (Option.Output.Footer.GetValueOrDefault(FooterFormat.Default) != FooterFormat.None)
                LineBreak();

            FooterRuleCount(total, fail, error);
            FooterRunInfo();
        }

        #endregion IAssertFormatter

        #region PipelineLoggerBase

        protected sealed override void DoWriteError(ErrorRecord errorRecord)
        {
            Error(errorRecord);
        }

        protected sealed override void DoWriteWarning(string message)
        {
            Warning(message);
        }

        protected sealed override void DoWriteVerbose(string message)
        {
            Writer.WriteVerbose(message);
        }

        protected sealed override void DoWriteInformation(InformationRecord informationRecord)
        {
            Writer.WriteInformation(informationRecord);
        }

        protected sealed override void DoWriteDebug(DebugRecord debugRecord)
        {
            Writer.WriteDebug(debugRecord);
        }

        protected sealed override void DoWriteObject(object sendToPipeline, bool enumerateCollection)
        {
            Writer.WriteObject(sendToPipeline, enumerateCollection);
        }

        #endregion PipelineLoggerBase

        /// <summary>
        /// Occurs when a rule passes.
        /// </summary>
        private void Pass(RuleRecord record)
        {
            if (Option.Output.As != ResultFormat.Detail || !Option.Output.Outcome.Value.HasFlag(RuleOutcome.Pass))
                return;

            BreakIfUnbrokenObject();
            BreakIfUnbrokenInfo();
            WriteStatus(
                status: GetTerminalSupport().PassStatus,
                statusIndent: GetTerminalSupport().BodyIndent,
                statusForeground: GetTerminalSupport().PassStatusForegroundColor,
                statusBackground: GetTerminalSupport().PassStatusBackgroundColor,
                messageForeground: GetTerminalSupport().PassForegroundColor,
                messageBackground: GetTerminalSupport().PassBackgroundColor,
                message: record.RuleName,
                suffix: record.Ref
            );
            UnbrokenContent();
        }

        /// <summary>
        /// Occurs when a rule fails.
        /// </summary>
        private void Fail(RuleRecord record)
        {
            if (!Option.Output.Outcome.Value.HasFlag(RuleOutcome.Fail))
                return;

            BreakIfUnbrokenObject();
            BreakIfUnbrokenInfo();
            WriteStatus(
                status: GetTerminalSupport().FailStatus,
                statusIndent: GetTerminalSupport().BodyIndent,
                statusForeground: GetTerminalSupport().FailStatusForegroundColor,
                statusBackground: GetTerminalSupport().FailStatusBackgroundColor,
                messageForeground: GetTerminalSupport().FailForegroundColor,
                messageBackground: GetTerminalSupport().FailBackgroundColor,
                message: record.RuleName,
                suffix: record.Ref
            );
            FailDetail(record);
        }

        /// <summary>
        /// Occurs when a rule raises an error.
        /// </summary>
        private void FailWithError(RuleRecord record)
        {
            if (!Option.Output.Outcome.Value.HasFlag(RuleOutcome.Error))
                return;

            BreakIfUnbrokenObject();
            BreakIfUnbrokenInfo();
            WriteStatus(
                status: GetTerminalSupport().ErrorStatus,
                statusIndent: GetTerminalSupport().BodyIndent,
                statusForeground: GetTerminalSupport().ErrorStatusForegroundColor,
                statusBackground: GetTerminalSupport().ErrorStatusBackgroundColor,
                messageForeground: GetTerminalSupport().ErrorForegroundColor,
                messageBackground: GetTerminalSupport().ErrorBackgroundColor,
                message: record.RuleName,
                suffix: record.Ref
            );
            ErrorDetail(record);
            UnbrokenContent();
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

        protected void Error(string message)
        {
            WriteErrorMessage(GetTerminalSupport().MessageIdent, message);
        }

        protected void Warning(string message)
        {
            WriteWarningMessage(GetTerminalSupport().MessageIdent, message);
        }

        protected void Information(string message)
        {
            WriteInformationMessage(GetTerminalSupport().MessageIdent, message);
        }

        private void Banner()
        {
            if (!Option.Output.Banner.GetValueOrDefault(BannerFormat.Default).HasFlag(BannerFormat.Title))
                return;

            WriteLine(FormatterStrings.Banner.Replace("\\n", Environment.NewLine));
            LineBreak();
        }

        private void StartResult(InvokeResult result, out RuleRecord[] records)
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
            WriteLine(
                message: string.Concat(
                    GetTerminalSupport().StartResultIndent,
                    result.TargetName,
                    " : ",
                    result.TargetType,
                    " [",
                    result.Pass,
                    "/",
                    result.Total,
                    "]"),
                forgroundColor: GetTerminalSupport().StartResultForegroundColor);
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

        private void RepositoryInfo()
        {
            if (!Option.Output.Banner.GetValueOrDefault(BannerFormat.Default).HasFlag(BannerFormat.RepositoryInfo))
                return;

            if (!GitHelper.TryRepository(out var repository))
                return;

            WriteLineFormat(FormatterStrings.Repository_Url, repository);
            if (GitHelper.TryHeadBranch(out var branch))
                WriteLineFormat(FormatterStrings.Repository_Branch, branch);

            if (GitHelper.TryRevision(out var revision))
                WriteLineFormat(FormatterStrings.Repository_Revision, revision);

            if (!string.IsNullOrEmpty(repository) || !string.IsNullOrEmpty(branch) || !string.IsNullOrEmpty(revision))
                LineBreak();
        }

        protected static string GetErrorMessage(RuleRecord record)
        {
            return string.IsNullOrEmpty(record.Ref) ?
                string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    FormatterStrings.Result_ErrorDetail,
                    record.TargetName,
                    record.RuleName,
                    record.Error.Message
                ) :
                string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    FormatterStrings.Result_ErrorDetailWithRef,
                    record.TargetName,
                    record.RuleName,
                    record.Error.Message,
                    record.Ref
                );
        }

        protected static string GetFailMessage(RuleRecord record)
        {
            return string.IsNullOrEmpty(record.Ref) ?
                string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    FormatterStrings.Result_FailDetail,
                    record.TargetName,
                    record.RuleName,
                    record.Info.Synopsis
                ) :
                string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    FormatterStrings.Result_FailDetailWithRef,
                    record.TargetName,
                    record.RuleName,
                    record.Info.Synopsis,
                    record.Ref
                );
        }

        private void FooterRuleCount(int total, int fail, int error)
        {
            if (!Option.Output.Footer.GetValueOrDefault(FooterFormat.Default).HasFlag(FooterFormat.RuleCount))
                return;

            WriteLineFormat(FormatterStrings.FooterRuleCount, total, fail, error);
        }

        private void FooterRunInfo()
        {
            if (PipelineContext.CurrentThread == null || !Option.Output.Footer.GetValueOrDefault(FooterFormat.Default).HasFlag(FooterFormat.RunInfo))
                return;

            var elapsed = PipelineContext.CurrentThread.RunTime.Elapsed;
            WriteLineFormat(FormatterStrings.FooterRunInfo, PipelineContext.CurrentThread.RunId, elapsed.ToString("c", Thread.CurrentThread.CurrentCulture));
        }

        protected void WriteStatus(string status, string statusIndent, ConsoleColor? statusForeground, ConsoleColor? statusBackground, ConsoleColor? messageForeground, ConsoleColor? messageBackground, string message, string suffix = null)
        {
            var output = message;
            if (statusForeground != null || statusBackground != null)
            {
                Writer.WriteHost(new HostInformationMessage { Message = statusIndent, NoNewLine = true });
                Writer.WriteHost(new HostInformationMessage
                {
                    Message = status,
                    ForegroundColor = statusForeground,
                    BackgroundColor = statusBackground,
                    NoNewLine = true
                });
                Writer.WriteHost(new HostInformationMessage { Message = " ", NoNewLine = true });
                output = string.IsNullOrEmpty(suffix) ? output : string.Concat(output, " (", suffix, ")");
            }
            else
            {
                output = string.IsNullOrEmpty(suffix) ? string.Concat(status, output) : string.Concat(status, output, " (", suffix, ")"); ;
            }
            Writer.WriteHost(new HostInformationMessage
            {
                Message = output,
                ForegroundColor = messageForeground,
                BackgroundColor = messageBackground
            });
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
                        message: record.Source[i].ToString(FormatterStrings.SourceAt, useRelativePath: true),
                        indent: GetTerminalSupport().BodyIndent,
                        prefix: GetTerminalSupport().SourceLocationPrefix,
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
                    message: record.Info.Synopsis,
                    indent: GetTerminalSupport().BodyIndent,
                    prefix: GetTerminalSupport().SynopsisPrefix,
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
                    message: record.Recommendation,
                    indent: GetTerminalSupport().BodyIndent,
                    prefix: GetTerminalSupport().RecommendationPrefix,
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
                        message: record.Reason[i],
                        indent: GetTerminalSupport().BodyIndent,
                        prefix: GetTerminalSupport().ReasonItemPrefix,
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
                    message: link,
                    indent: GetTerminalSupport().BodyIndent,
                    prefix: GetTerminalSupport().HelpLinkPrefix,
                    forgroundColor: GetTerminalSupport().BodyForegroundColor
                );
            }
        }

        protected void WriteErrorMessage(string indent, string message)
        {
            BreakIfUnbrokenObject();
            BreakIfUnbrokenContent();
            WriteStatus(
                status: GetTerminalSupport().ErrorStatus,
                statusIndent: indent,
                statusForeground: GetTerminalSupport().ErrorStatusForegroundColor,
                statusBackground: GetTerminalSupport().ErrorStatusBackgroundColor,
                messageForeground: GetTerminalSupport().ErrorForegroundColor,
                messageBackground: GetTerminalSupport().ErrorBackgroundColor,
                message: message);
            UnbrokenInfo();
        }

        protected void WriteWarningMessage(string indent, string message)
        {
            BreakIfUnbrokenObject();
            BreakIfUnbrokenContent();
            WriteStatus(
                status: GetTerminalSupport().WarningStatus,
                statusIndent: indent,
                statusForeground: GetTerminalSupport().WarningStatusForegroundColor,
                statusBackground: GetTerminalSupport().WarningStatusBackgroundColor,
                messageForeground: GetTerminalSupport().WarningForegroundColor,
                messageBackground: GetTerminalSupport().WarningBackgroundColor,
                message: message
            );
            UnbrokenInfo();
        }

        protected void WriteInformationMessage(string indent, string message)
        {
            BreakIfUnbrokenObject();
            BreakIfUnbrokenContent();
            WriteStatus(
                status: GetTerminalSupport().InformationStatus,
                statusIndent: indent,
                statusForeground: GetTerminalSupport().WarningStatusForegroundColor,
                statusBackground: GetTerminalSupport().WarningStatusBackgroundColor,
                messageForeground: GetTerminalSupport().WarningForegroundColor,
                messageBackground: GetTerminalSupport().WarningBackgroundColor,
                message: message
            );
            UnbrokenInfo();
        }
    }
}
