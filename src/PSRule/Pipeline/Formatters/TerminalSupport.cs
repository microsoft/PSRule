// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;

namespace PSRule.Pipeline.Formatters;

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
