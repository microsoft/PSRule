// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Pipeline.Output;

internal sealed class CsvOutputWriter : SerializationOutputWriter<object>, IBindingContext
{
    private const char COMMA = ',';
    private const char QUOTE = '"';

    private readonly StringBuilder _Builder;
    private readonly ResultFormat _OutputAs;
    private readonly string[] _DetailColumns;
    private readonly string[] _SummaryColumns;
    private readonly Dictionary<string, PathExpression> _PathExpressionCache = [];

    // Default columns for backward compatibility
    private static readonly string[] DefaultDetailColumns = ["RuleName", "TargetName", "TargetType", "Outcome", "OutcomeReason", "Synopsis", "Recommendation"];
    private static readonly string[] DefaultSummaryColumns = ["RuleName", "Pass", "Fail", "Outcome", "Synopsis", "Recommendation"];


    internal CsvOutputWriter(PipelineWriter inner, PSRuleOption option, ShouldProcess? shouldProcess)
        : base(inner, option, shouldProcess)
    {
        _Builder = new StringBuilder();
        _OutputAs = Option.Output.As ?? OutputOption.Default.As!.Value;

        // Use configured columns or default columns
        _DetailColumns = option.Output.CsvDetailedColumns ?? DefaultDetailColumns;
        _SummaryColumns = DefaultSummaryColumns; // Summary columns are not configurable yet
    }

    protected override string Serialize(object[] o)
    {
        WriteHeader();
        if (_OutputAs == ResultFormat.Detail)
        {
            WriteDetail(o);
        }
        else
        {
            WriteSummary(o);
        }
        return _Builder.ToString();
    }

    private void WriteSummary(object[] o)
    {
        for (var i = 0; i < o.Length; i++)
            if (o[i] is RuleSummaryRecord record)
                VisitSummaryRecord(record);
    }

    private void WriteDetail(object[] o)
    {
        for (var i = 0; i < o.Length; i++)
            if (o[i] is RuleRecord record)
                VisitRecord(record);
    }

    private void WriteHeader()
    {
        var columns = Option.Output.As == ResultFormat.Summary ? _SummaryColumns : _DetailColumns;

        for (var i = 0; i < columns.Length; i++)
        {
            if (i > 0)
                _Builder.Append(COMMA);

            _Builder.Append(GetColumnDisplayName(columns[i]));
        }
        _Builder.Append(System.Environment.NewLine);
    }

    private void VisitRecord(RuleRecord record)
    {
        if (record == null)
            return;

        var columns = _DetailColumns;

        for (var i = 0; i < columns.Length; i++)
        {
            if (i > 0)
                _Builder.Append(COMMA);

            var value = GetColumnValue(record, columns[i]);
            WriteColumn(value);
        }
        _Builder.Append(System.Environment.NewLine);
    }

    private void VisitSummaryRecord(RuleSummaryRecord record)
    {
        if (record == null)
            return;

        var columns = _SummaryColumns;

        for (var i = 0; i < columns.Length; i++)
        {
            if (i > 0)
                _Builder.Append(COMMA);

            var value = GetColumnValue(record, columns[i]);
            WriteColumn(value);
        }
        _Builder.Append(System.Environment.NewLine);
    }

    private void WriteColumn(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        _Builder.Append(QUOTE);
        _Builder.Append(value
            .Replace("\"", "\"\"").Replace("\r\n", " ")
            .Replace('\r', ' ').Replace('\n', ' ')
            .Replace("  ", " ")
        );
        _Builder.Append(QUOTE);
    }

    private void WriteColumn(InfoString value)
    {
        if (!value.HasValue)
            return;

        WriteColumn(value.Text);
    }

    /// <summary>
    /// Get the display name for a column in the CSV header.
    /// </summary>
    private static string GetColumnDisplayName(string column)
    {
        return column switch
        {
            "RuleName" => ViewStrings.RuleName,
            "TargetName" => ViewStrings.TargetName,
            "TargetType" => ViewStrings.TargetType,
            "Outcome" => ViewStrings.Outcome,
            "OutcomeReason" => ViewStrings.OutcomeReason,
            "Synopsis" => ViewStrings.Synopsis,
            "Recommendation" => ViewStrings.Recommendation,
            "Pass" => ViewStrings.Pass,
            "Fail" => ViewStrings.Fail,
            _ => column // Use the column name as-is for custom columns
        };
    }

    /// <summary>
    /// Get the value for a specific column from a record.
    /// </summary>
    private string GetColumnValue(object record, string column)
    {
        if (record == null) return string.Empty;

        return column switch
        {
            // Standard RuleRecord properties
            "RuleName" when record is RuleRecord ruleRecord => ruleRecord.RuleName,
            "TargetName" when record is RuleRecord ruleRecord => ruleRecord.TargetName,
            "TargetType" when record is RuleRecord ruleRecord => ruleRecord.TargetType,
            "Outcome" when record is RuleRecord ruleRecord => ruleRecord.Outcome.ToString(),
            "OutcomeReason" when record is RuleRecord ruleRecord => ruleRecord.OutcomeReason.ToString(),
            "Synopsis" when record is RuleRecord ruleRecord => ruleRecord.Info.Synopsis?.Text ?? string.Empty,
            "Recommendation" when record is RuleRecord ruleRecord => ruleRecord.Info.Recommendation?.Text ?? string.Empty,

            // Standard RuleSummaryRecord properties
            "RuleName" when record is RuleSummaryRecord summaryRecord => summaryRecord.RuleName,
            "Pass" when record is RuleSummaryRecord summaryRecord => summaryRecord.Pass.ToString(),
            "Fail" when record is RuleSummaryRecord summaryRecord => summaryRecord.Fail.ToString(),
            "Outcome" when record is RuleSummaryRecord summaryRecord => summaryRecord.Outcome.ToString(),
            "Synopsis" when record is RuleSummaryRecord summaryRecord => summaryRecord.Info.Synopsis ?? string.Empty,
            "Recommendation" when record is RuleSummaryRecord summaryRecord => summaryRecord.Info.Recommendation ?? string.Empty,

            // For any other column, try to get it via object path (for nested properties)
            _ => GetRecordColumnValue(record, column)
        };
    }

    /// <summary>
    /// Get a column value using object path notation for the record.
    /// </summary>
    private string GetRecordColumnValue(object record, string path)
    {
        try
        {
            if (ObjectHelper.GetPath(this, record, path, caseSensitive: false, out object value))
            {
                return value?.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // If we can't get the value via object path, ignore the error and return empty.
        }
        return string.Empty;
    }

    bool IBindingContext.GetPathExpression(string path, out PathExpression expression)
    {
        if (_PathExpressionCache.TryGetValue(path, out expression))
            return true;

        expression = PathExpression.Create(path);
        _PathExpressionCache[path] = expression;
        return true;
    }

    void IBindingContext.CachePathExpression(string path, PathExpression expression)
    {
        _PathExpressionCache[path] = expression;
    }
}
