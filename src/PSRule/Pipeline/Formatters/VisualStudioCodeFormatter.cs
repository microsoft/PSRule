// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters
{
    /// <summary>
    /// Visual Studio Code assert formatter.
    /// </summary>
    internal sealed class VisualStudioCodeFormatter : AssertFormatterBase, IAssertFormatter
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
}
