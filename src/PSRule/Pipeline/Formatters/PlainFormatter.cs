// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters;

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
        WriteLine(FormatterStrings.Message, foregroundColor: GetTerminalSupport().ErrorForegroundColor);
        WriteIndentedLine(record.Error.Message, GetTerminalSupport().BodyIndent, foregroundColor: GetTerminalSupport().ErrorForegroundColor);
        LineBreak();
        WriteLine(FormatterStrings.Position, foregroundColor: GetTerminalSupport().BodyForegroundColor);
        WriteIndentedLine(record.Error.PositionMessage, GetTerminalSupport().BodyIndent, foregroundColor: GetTerminalSupport().BodyForegroundColor);
        LineBreak();
        WriteLine(FormatterStrings.StackTrace, foregroundColor: GetTerminalSupport().BodyForegroundColor);
        WriteIndentedLine(record.Error.ScriptStackTrace, GetTerminalSupport().BodyIndent, foregroundColor: GetTerminalSupport().BodyForegroundColor);
    }
}
