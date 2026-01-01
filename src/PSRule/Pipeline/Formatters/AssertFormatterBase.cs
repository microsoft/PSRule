// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Pipeline.Formatters;

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

    private static readonly TerminalSupport DefaultTerminalSupport = new(4);

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
            BreakIfUnbrokenInfo();

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
        Writer.LogVerbose(EventId.None, message);
    }

    protected sealed override void DoWriteInformation(InformationRecord informationRecord)
    {
        Writer.LogInformation(EventId.None, informationRecord.MessageData.ToString());
    }

    protected sealed override void DoWriteDebug(DebugRecord debugRecord)
    {
        Writer.LogDebug(EventId.None, debugRecord.Message);
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

        WriteLine(FormatterStrings.Banner.Replace("\\n", System.Environment.NewLine));
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
            foregroundColor: GetTerminalSupport().StartResultForegroundColor);
    }

    protected virtual TerminalSupport GetTerminalSupport()
    {
        return DefaultTerminalSupport;
    }

    private void Source(Source[] source)
    {
        if (!Option.Output.Banner.GetValueOrDefault(BannerFormat.Default).HasFlag(BannerFormat.Source))
            return;

        var version = Engine.GetVersion();
        if (!string.IsNullOrEmpty(version))
            WriteLineFormat(FormatterStrings.PSRuleVersion, version);

        var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; source != null && i < source.Length; i++)
        {
            if (source[i].Module != null && !list.Contains(source[i].Module.Name))
            {
                WriteLineFormat(FormatterStrings.ModuleVersion, source[i].Module.Name, source[i].Module.FullVersion);
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

        var repository = Option.Repository.Url;
        if (string.IsNullOrEmpty(repository))
            return;

        WriteLineFormat(FormatterStrings.Repository_Url, repository);
        if (GitHelper.TryHeadBranch(out var branch))
            WriteLineFormat(FormatterStrings.Repository_Branch, branch);

        if (GitHelper.TryRevision(out var revision))
            WriteLineFormat(FormatterStrings.Repository_Revision, revision);

        if (!string.IsNullOrEmpty(branch) || !string.IsNullOrEmpty(revision))
            LineBreak();
    }

    protected static string GetErrorMessage(RuleRecord record)
    {
        return string.IsNullOrEmpty(record.Ref) ?
            string.Format(
                Thread.CurrentThread.CurrentCulture,
                FormatterStrings.Result_ErrorDetail,
                record.TargetName,
                record.Info.Name,
                record.Error.Message
            ) :
            string.Format(
                Thread.CurrentThread.CurrentCulture,
                FormatterStrings.Result_ErrorDetailWithRef,
                record.TargetName,
                record.Info.Name,
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
                record.Info.Name,
                record.Info.Synopsis.Text
            ) :
            string.Format(
                Thread.CurrentThread.CurrentCulture,
                FormatterStrings.Result_FailDetailWithRef,
                record.TargetName,
                record.Info.Name,
                record.Info.Synopsis.Text,
                record.Ref
            );
    }

    private void FooterRuleCount(int total, int fail, int error)
    {
        if (!Option.Output.Footer.GetValueOrDefault(FooterFormat.Default).HasFlag(FooterFormat.RuleCount))
            return;

        BreakIfUnbrokenContent();
        WriteLineFormat(FormatterStrings.FooterRuleCount, total, fail, error);
    }

    private void FooterRunInfo()
    {
        if (PipelineContext.CurrentThread == null || !Option.Output.Footer.GetValueOrDefault(FooterFormat.Default).HasFlag(FooterFormat.RunInfo))
            return;

        var elapsed = PipelineContext.CurrentThread.RunTime.Elapsed;
        WriteLineFormat(FormatterStrings.FooterRunInfo, PipelineContext.CurrentThread.RunInstance, elapsed.ToString("c", Thread.CurrentThread.CurrentCulture));
    }

    protected void WriteStatus(string status, string statusIndent, ConsoleColor? statusForeground, ConsoleColor? statusBackground, ConsoleColor? messageForeground, ConsoleColor? messageBackground, string message, string? suffix = null)
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

    protected void WriteLine(string prefix, ConsoleColor? foregroundColor, string message, params object[] args)
    {
        var output = args == null || args.Length == 0 ? message : string.Format(Thread.CurrentThread.CurrentCulture, message, args);
        Writer.WriteHost(new HostInformationMessage { Message = string.Concat(prefix, output), ForegroundColor = foregroundColor });
    }

    protected void WriteLine(string message, string? prefix = null, ConsoleColor? foregroundColor = null)
    {
        var output = string.IsNullOrEmpty(prefix) ? message : string.Concat(prefix, message);
        Writer.WriteHost(new HostInformationMessage { Message = output, ForegroundColor = foregroundColor });
    }

    protected void WriteIndentedLine(string message, string indent, string? prefix = null, ConsoleColor? foregroundColor = null)
    {
        if (string.IsNullOrEmpty(message))
            return;

        var output = string.Concat(indent, prefix, message);
        Writer.WriteHost(new HostInformationMessage { Message = output, ForegroundColor = foregroundColor });
    }

    protected void WriteIndentedLines(string message, string indent, string? prefix = null, ConsoleColor? foregroundColor = null)
    {
        if (string.IsNullOrEmpty(message))
            return;

        var lines = message.SplitSemantic();
        for (var i = 0; i < lines.Length; i++)
            WriteIndentedLine(lines[i], indent, prefix, foregroundColor);
    }

    protected void WriteLineFormat(string message, params object[] args)
    {
        WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, message, args));
    }

    protected void WriteLines(string message, string? prefix = null, ConsoleColor? foregroundColor = null)
    {
        if (string.IsNullOrEmpty(message))
            return;

        var lines = message.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
        for (var i = 0; i < lines.Length; i++)
            WriteLine(lines[i], prefix, foregroundColor);
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
                    foregroundColor: GetTerminalSupport().SourceLocationForegroundColor
                );
        }
    }

    protected void WriteSynopsis(IDetailedRuleResultV2 record, bool shouldBreak = true)
    {
        if (!record.Info.Synopsis.HasValue)
            return;

        if (shouldBreak)
            LineBreak();

        WriteIndentedLine(
            message: record.Info.Synopsis.Text,
            indent: GetTerminalSupport().BodyIndent,
            prefix: GetTerminalSupport().SynopsisPrefix,
            foregroundColor: GetTerminalSupport().SynopsisForegroundColor
        );
    }

    protected void WriteRecommendation(IDetailedRuleResultV2 record)
    {
        if (!record.Info.Recommendation.HasValue)
            return;

        LineBreak();
        WriteLine(GetTerminalSupport().RecommendationHeading, foregroundColor: GetTerminalSupport().BodyForegroundColor);
        WriteIndentedLines(
            message: record.Info.Recommendation.Text,
            indent: GetTerminalSupport().BodyIndent,
            prefix: GetTerminalSupport().RecommendationPrefix,
            foregroundColor: GetTerminalSupport().BodyForegroundColor
        );
    }

    protected void WriteReason(RuleRecord record)
    {
        if (record.Reason != null && record.Reason.Length > 0)
        {
            LineBreak();
            WriteLine(GetTerminalSupport().ReasonHeading, foregroundColor: GetTerminalSupport().BodyForegroundColor);
            for (var i = 0; i < record.Reason.Length; i++)
            {
                WriteIndentedLine(
                    message: record.Reason[i],
                    indent: GetTerminalSupport().BodyIndent,
                    prefix: GetTerminalSupport().ReasonItemPrefix,
                    foregroundColor: GetTerminalSupport().BodyForegroundColor
                );
            }
        }
    }

    protected void WriteHelp(IDetailedRuleResultV2 record)
    {
        var link = record.Info?.GetOnlineHelpUri()?.ToString();
        if (!string.IsNullOrEmpty(link))
        {
            LineBreak();
            WriteLine(GetTerminalSupport().HelpHeading, foregroundColor: GetTerminalSupport().BodyForegroundColor);
            WriteIndentedLine(
                message: link,
                indent: GetTerminalSupport().BodyIndent,
                prefix: GetTerminalSupport().HelpLinkPrefix,
                foregroundColor: GetTerminalSupport().BodyForegroundColor
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
