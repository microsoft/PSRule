// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters
{
    /// <summary>
    /// Plain text assert formatter.
    /// </summary>
    internal sealed class PlainFormatter : AssertFormatterBase, IAssertFormatter
    {
        internal PlainFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
            : base(source, logger, option) { }

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
}
