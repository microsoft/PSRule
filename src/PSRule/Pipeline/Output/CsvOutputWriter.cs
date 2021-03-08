// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Text;

namespace PSRule.Pipeline.Output
{
    internal sealed class CsvOutputWriter : SerializationOutputWriter<object>
    {
        private const char COMMA = ',';
        private const char QUOTE = '"';

        private readonly StringBuilder _Builder;

        internal CsvOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option)
        {
            _Builder = new StringBuilder();
        }

        protected override string Serialize(object[] o)
        {
            WriteHeader();
            if (Option.Output.As == ResultFormat.Detail)
                WriteDetail(o);
            else
                WriteSummary(o);

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
            _Builder.Append(ViewStrings.RuleName);
            _Builder.Append(COMMA);

            if (Option.Output.As == ResultFormat.Summary)
            {
                _Builder.Append(ViewStrings.Pass);
                _Builder.Append(COMMA);
                _Builder.Append(ViewStrings.Fail);
            }
            else
            {
                _Builder.Append(ViewStrings.TargetName);
                _Builder.Append(COMMA);
                _Builder.Append(ViewStrings.TargetType);
            }

            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.Outcome);

            if (Option.Output.As == ResultFormat.Detail)
            {
                _Builder.Append(COMMA);
                _Builder.Append(ViewStrings.OutcomeReason);
            }

            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.Synopsis);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.Recommendation);
            _Builder.Append(Environment.NewLine);
        }

        private void VisitRecord(RuleRecord record)
        {
            if (record == null)
                return;

            WriteColumn(record.RuleName);
            _Builder.Append(COMMA);
            WriteColumn(record.TargetName);
            _Builder.Append(COMMA);
            WriteColumn(record.TargetType);
            _Builder.Append(COMMA);
            WriteColumn(record.Outcome.ToString());
            _Builder.Append(COMMA);
            WriteColumn(record.OutcomeReason.ToString());
            _Builder.Append(COMMA);
            WriteColumn(record.Info.Synopsis);
            _Builder.Append(COMMA);
            WriteColumn(record.Info.Recommendation);
            _Builder.Append(Environment.NewLine);
        }

        private void VisitSummaryRecord(RuleSummaryRecord record)
        {
            if (record == null)
                return;

            WriteColumn(record.RuleName);
            _Builder.Append(COMMA);
            _Builder.Append(record.Pass.ToString());
            _Builder.Append(COMMA);
            _Builder.Append(record.Fail.ToString());
            _Builder.Append(COMMA);
            WriteColumn(record.Outcome.ToString());
            _Builder.Append(COMMA);
            WriteColumn(record.Info.Synopsis);
            _Builder.Append(COMMA);
            WriteColumn(record.Info.Recommendation);
            _Builder.Append(Environment.NewLine);
        }

        private void WriteColumn(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            _Builder.Append(QUOTE);
            _Builder.Append(value.Replace("\"", "\"\""));
            _Builder.Append(QUOTE);
        }
    }
}
