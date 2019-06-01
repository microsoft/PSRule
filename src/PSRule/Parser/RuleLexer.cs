using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSRule.Parser
{
    /// <summary>
    /// A lexer that inteprets markdown as a rule.
    /// </summary>
    internal sealed class RuleLexer : MarkdownLexer
    {
        private const int RULE_NAME_HEADING_LEVEL = 1;
        private const int RULE_ENTRIES_HEADING_LEVEL = 2;

        private readonly bool _PreserveFormatting;

        public RuleLexer(bool preserveFomatting)
        {
            _PreserveFormatting = preserveFomatting;
        }

        public RuleDocument Process(TokenStream stream)
        {
            stream.MoveTo(0);

            // Look for yaml header
            var metadata = YamlHeader(stream);

            RuleDocument doc = null;

            // Process sections
            while (!stream.EOF)
            {
                if (IsHeading(stream.Current, RULE_NAME_HEADING_LEVEL))
                {
                    doc = new RuleDocument
                    {
                        Name = stream.Current.Text
                    };

                    doc.Annotations = TagSet.FromDictionary(metadata);
                }
                else if (doc != null)
                {
                    if (IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL))
                    {
                        var matching = Synopsis(stream, doc) ||
                            Recommendation(stream, doc) ||
                            Notes(stream, doc) ||
                            RelatedLinks(stream, doc);

                        if (matching)
                        {
                            continue;
                        }
                    }
                }

                // Skip the current token
                stream.Next();
            }

            return doc;
        }

        /// <summary>
        /// Read Synopsis.
        /// </summary>
        private bool Synopsis(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Synopsis))
            {
                return false;
            }

            doc.Synopsis = SectionBody(stream);
            stream.SkipUntil(MarkdownTokenType.Header);

            return true;
        }

        /// <summary>
        /// Process recommendations.
        /// </summary>
        private bool Recommendation(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Recommendation))
            {
                return false;
            }

            stream.Next();

            var recommendations = new List<RuleRecommendation>();

            if (!stream.EOF)
            {
                var hasLineBreak = stream.Current.IsDoubleLineEnding();
                var recommendation = new RuleRecommendation
                {
                    Title = "default",
                    FormatOption = hasLineBreak ? SectionFormatOption.LineBreakAfterHeader : SectionFormatOption.None,
                    Introduction = SimpleTextSection(stream),
                    Code = RecommendationBlock(stream),
                    Remarks = SimpleTextSection(stream)
                };

                stream.SkipUntil(MarkdownTokenType.Header);

                recommendations.Add(recommendation);
            }

            doc.Recommendation = recommendations.ToArray();

            return true;
        }

        /// <summary>
        /// Read Notes.
        /// </summary>
        private bool Notes(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Notes))
            {
                return false;
            }

            doc.Notes = SectionBody(stream);
            stream.SkipUntil(MarkdownTokenType.Header);

            return true;
        }

        private bool RelatedLinks(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Links))
            {
                return false;
            }

            List<Link> links = new List<Link>();

            stream.Next();

            while (stream.IsTokenType(MarkdownTokenType.Link, MarkdownTokenType.LinkReference, MarkdownTokenType.LineBreak))
            {
                if (stream.IsTokenType(MarkdownTokenType.LineBreak))
                {
                    stream.Next();

                    continue;
                }

                var link = new Link
                {
                    Name = stream.Current.Meta,
                    Uri = stream.Current.Text
                };

                // Update link to point to resolved target
                if (stream.IsTokenType(MarkdownTokenType.LinkReference))
                {
                    var target = stream.ResolveLinkTarget(link.Uri);
                    link.Uri = target.Text;
                }

                links.Add(link);

                stream.Next();
            }

            stream.SkipUntil(MarkdownTokenType.Header);

            doc.Links = links.ToArray();

            return true;
        }

        private Body SectionBody(TokenStream stream)
        {
            var useBreak = stream.Current.IsDoubleLineEnding();

            stream.Next();

            var text = SimpleTextSection(stream);

            return new Body(text, useBreak ? SectionFormatOption.LineBreakAfterHeader : SectionFormatOption.None);
        }

        private string SimpleTextSection(TokenStream stream, bool includeNonYamlFencedBlocks = false)
        {
            var sb = new StringBuilder();

            while (stream.IsTokenType(MarkdownTokenType.Text, MarkdownTokenType.Link, MarkdownTokenType.FencedBlock, MarkdownTokenType.LineBreak))
            {
                if (stream.IsTokenType(MarkdownTokenType.Text))
                {
                    AppendEnding(sb, stream.Peak(-1), _PreserveFormatting);
                    sb.Append(stream.Current.Text);
                }
                else if (stream.IsTokenType(MarkdownTokenType.Link))
                {
                    AppendEnding(sb, stream.Peak(-1), _PreserveFormatting);
                    sb.Append(stream.Current.Meta);

                    if (!string.IsNullOrEmpty(stream.Current.Text))
                    {
                        sb.AppendFormat(" ({0})", stream.Current.Text);
                    }
                }
                else if (stream.IsTokenType(MarkdownTokenType.LinkReference))
                {
                    AppendEnding(sb, stream.Peak(-1), _PreserveFormatting);

                    sb.Append(stream.Current.Meta);
                }
                else if (stream.IsTokenType(MarkdownTokenType.FencedBlock))
                {
                    // Only process fenced blocks if specified, and never process yaml blocks
                    if (!includeNonYamlFencedBlocks || string.Equals(stream.Current.Meta, "yaml", StringComparison.OrdinalIgnoreCase))
                    {
                        if (stream.PeakTokenType(-1) == MarkdownTokenType.LineBreak)
                        {
                            AppendEnding(sb, stream.Peak(-1), _PreserveFormatting);
                        }

                        break;
                    }

                    AppendEnding(sb, stream.Peak(-1), preserveEnding: true);
                    sb.Append(stream.Current.Text);
                }
                else if (stream.IsTokenType(MarkdownTokenType.LineBreak))
                {
                    AppendEnding(sb, stream.Peak(-1), _PreserveFormatting);
                }

                stream.Next();
            }

            if (stream.EOF && stream.Peak(-1).Flag.HasFlag(MarkdownTokenFlag.Preserve) && stream.Peak(-1).Flag.HasFlag(MarkdownTokenFlag.LineEnding))
            {
                AppendEnding(sb, stream.Peak(-1));
            }

            return sb.ToString();
        }

        private void AppendEnding(StringBuilder stringBuilder, MarkdownToken token, bool preserveEnding = false)
        {
            if (token == null || stringBuilder.Length == 0 || !token.Flag.IsEnding())
            {
                return;
            }

            if (!preserveEnding && token.Flag.ShouldPreserve())
            {
                preserveEnding = true;
            }

            if (token.IsDoubleLineEnding())
            {
                stringBuilder.Append(preserveEnding ? string.Concat(Environment.NewLine, Environment.NewLine) : Environment.NewLine);
            }
            else if (token.IsSingleLineEnding())
            {
                stringBuilder.Append(preserveEnding ? Environment.NewLine : " ");
            }
        }

        private static CodeBlock[] RecommendationBlock(TokenStream stream)
        {
            List<CodeBlock> blocks = new List<CodeBlock>();

            foreach (var token in stream.CaptureWhile(MarkdownTokenType.FencedBlock))
            {
                var block = new CodeBlock(token.Text, token.Meta);

                blocks.Add(block);
            }

            return blocks.ToArray();
        }
    }
}
