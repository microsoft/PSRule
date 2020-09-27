// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Resources;
using PSRule.Rules;
using System.Linq;
using System.Text;

namespace PSRule.Pipeline.Output
{
    internal sealed class MarkdownOutputWriter : SerializationOutputWriter<InvokeResult>
    {
        private const char Hash = '#';
        private const char Space = ' ';

        private readonly StringBuilder _Builder;

        private bool _LineBreak = true;

        internal MarkdownOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option)
        {
            _Builder = new StringBuilder();
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!(sendToPipeline is InvokeResult result))
                return;

            Add(result);
        }

        protected override string Serialize(InvokeResult[] o)
        {
            if (Option.Output.As == ResultFormat.Detail)
                AsDetail(o);
            else
                AsSummary(o);

            return _Builder.ToString();
        }

        private void AsDetail(InvokeResult[] o)
        {
            Header();
            for (var i = 0; i < o.Length; i++)
                if (o[i].Total > 0)
                    DetailResult(o[i]);
        }

        private void AsSummary(InvokeResult[] o)
        {
            Header();
            var ruleGroup = o.SelectMany(result => result.AsRecord()).GroupBy(record => record.RuleId).ToArray();
            for (var i = 0; i < ruleGroup.Length; i++)
            {
                RuleSummary(ruleGroup[i].ToArray());
            }
        }

        private void DetailResult(InvokeResult invokeResult)
        {
            var records = invokeResult.AsRecord();

            Section(2, records[0].TargetName, " : ", records[0].TargetType);
            LineBreak();

            for (var i = 0; i < records.Length; i++)
            {
                CheckedItem(records[i].IsSuccess(), records[i].RuleName);
                if (!records[i].IsSuccess())
                {
                    LineBreak();
                    AppendLine(records[i].Recommendation);
                    LineBreak();
                }
            }
        }

        private void CheckedItem(bool ticked, string text)
        {
            if (ticked)
                AppendLine("- [X] ", text);
            else
                AppendLine("- [ ] ", text);
        }

        private void RuleSummary(RuleRecord[] records)
        {
            if (records.Length == 0)
                return;

            Section(2, records[0].Info.DisplayName);
            LineBreak();
            AppendLine("> ", records[0].RuleName);
            LineBreak();
            AppendLine(records[0].Info.Synopsis);
            LineBreak();
            AppendLine(records[0].Info.Description);
            LineBreak();

            // Info block
            AppendLine("```yaml");
            foreach (var key in records[0].Info.Annotations.Keys)
            {
                if (key.ToString() != "online version")
                {
                    AppendLine(key.ToString(), ": ", records[0].Info.Annotations[key].ToString());
                }
            }
            AppendLine("```");
            LineBreak();

            // Add recommendation
            AppendLine("**", ReportStrings.Markdown_Recommendation, "**:");
            LineBreak();
            AppendLine(records[0].Recommendation);
            LineBreak();

            // Add links
            if (records[0]?.Info?.Links?.Length > 0)
            {
                AppendLine("**", ReportStrings.Markdown_Links, "**:");
                LineBreak();
                for (var i = 0; i < records[0].Info.Links.Length; i++)
                {
                    Link(records[0].Info.Links[i]);
                }
            }
            LineBreak();

            // Get padding
            var padding = new int[3] { 0, 0, 0 };
            GetColumnPadding(padding, ReportStrings.Markdown_TargetName, ReportStrings.Markdown_TargetType, ReportStrings.Markdown_Outcome);
            for (var i = 0; i < records.Length; i++)
                GetColumnPadding(padding, records[i].TargetName, records[i].TargetType, records[i].Outcome.ToString());

            // Build results table
            LineBreak();
            AppendLine("**", ReportStrings.Markdown_Results, "**:");
            LineBreak();
            AppendLine(ReportStrings.Markdown_ResultText);
            LineBreak();
            AppendColumn(ReportStrings.Markdown_TargetName, Space, padding[0]);
            Append(" | ");
            AppendColumn(ReportStrings.Markdown_TargetType, Space, padding[1]);
            Append(" | ");
            AppendColumn(ReportStrings.Markdown_Outcome, Space, padding[2]);
            LineEnd();
            AppendColumn('-', ReportStrings.Markdown_TargetName.Length, Space, padding[0]);
            Append(" | ");
            AppendColumn('-', ReportStrings.Markdown_TargetType.Length, Space, padding[1]);
            Append(" | ");
            Append('-', ReportStrings.Markdown_Outcome.Length);
            LineEnd();
            for (var i = 0; i < records.Length; i++)
            {
                AppendColumn(records[i].TargetName, Space, padding[0]);
                Append(" | ");
                AppendColumn(records[i].TargetType, Space, padding[1]);
                Append(" | ");
                Append(records[i].Outcome.ToString());
                LineEnd();
            }
        }

        private void Link(RuleHelpInfo.Link link)
        {
            AppendLine("- [", link.Name, "](", link.Uri, ")");
        }

        private static void GetColumnPadding(int[] padding, string targetName, string targetType, string outcome)
        {
            if (targetName.Length > padding[0])
                padding[0] = targetName.Length;

            if (targetType.Length > padding[1])
                padding[1] = targetType.Length;

            if (outcome.Length > padding[2])
                padding[2] = outcome.Length;
        }

        private void Header()
        {
            Section(1, "PSRule");
        }

        private void Section(int depth, params string[] name)
        {
            LineBreak();
            Append(Hash, depth);
            Append(Space);
            AppendLine(name);
        }

        private void Append(string text)
        {
            _LineBreak = false;
            _Builder.Append(text);
        }

        private void Append(char c, int count = 1)
        {
            _LineBreak = false;
            _Builder.Append(c, count);
        }

        private void AppendColumn(string s, char padding, int width)
        {
            Append(s);
            if (s.Length < width)
                _Builder.Append(padding, width - s.Length);
        }

        private void AppendColumn(char c, int count, char padding, int width)
        {
            Append(c, count);
            if (count < width)
                _Builder.Append(padding, width - count);
        }

        private void AppendLine(params string[] text)
        {
            if (text == null || text.Length == 0)
                return;

            var hasContent = false;
            for (var i = 0; i < text.Length; i++)
            {
                if (!string.IsNullOrEmpty(text[i]))
                {
                    Append(text[i]);
                    hasContent = true;
                }
            }
            if (!hasContent)
                return;

            _LineBreak = false;
            LineEnd();
        }

        private void LineEnd()
        {
            _Builder.AppendLine();
        }

        private void LineBreak()
        {
            if (_LineBreak)
                return;

            _Builder.AppendLine();
            _LineBreak = true;
        }
    }
}
