// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters;

/// <summary>
/// Client assert formatter.
/// </summary>
internal sealed class ClientFormatter : AssertFormatterBase, IAssertFormatter
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
