using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSRule.Pipeline
{
    internal sealed class CSVSerializer : PipelineWriter
    {
        private const char COMMA = ',';
        private const char QUOTE = '"';

        private readonly StringBuilder _Builder;
        private readonly List<InvokeResult> _Result;

        internal CSVSerializer(WriteOutput output)
            : base(output)
        {
            _Builder = new StringBuilder();
            _Result = new List<InvokeResult>();
        }

        public override void Write(object o, bool enumerate)
        {
            if (!(o is InvokeResult result))
                return;

            _Result.Add(result);
        }

        public override void End()
        {
            base.Write(Serialize(_Result.ToArray()), false);
        }

        internal string Serialize(IEnumerable<InvokeResult> o)
        {
            WriteHeader();
            foreach (var result in o)
            {
                foreach (var record in result.AsRecord())
                {
                    VisitRecord(record: record);
                }
            }
            return _Builder.ToString();
        }

        private void WriteHeader()
        {
            _Builder.Append(ViewStrings.RuleName);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.TargetName);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.TargetType);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.Outcome);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.OutcomeReason);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.Synopsis);
            _Builder.Append(COMMA);
            _Builder.Append(ViewStrings.Recommendation);
            _Builder.Append(Environment.NewLine);
        }

        private void VisitRecord(RuleRecord record)
        {
            if (record == null)
            {
                return;
            }

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

        private void WriteColumn(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            _Builder.Append(QUOTE);
            _Builder.Append(value.Replace("\"", "\"\""));
            _Builder.Append(QUOTE);
        }
    }
}
