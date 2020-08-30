// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PSRule.Parser
{
    /// <summary>
    /// A lexer that interprets markdown as a rule.
    /// </summary>
    internal sealed class RuleLexer : MarkdownLexer
    {
        private const int RULE_NAME_HEADING_LEVEL = 1;
        private const int RULE_ENTRIES_HEADING_LEVEL = 2;

        private const string Space = " ";

        public RuleLexer()
        {
            // Do nothing
        }

        public RuleDocument Process(TokenStream stream)
        {
            // Look for yaml header
            stream.MoveTo(0);
            var metadata = YamlHeader(stream);
            RuleDocument doc = null;

            // Process sections
            while (!stream.EOF)
            {
                if (IsHeading(stream.Current, RULE_NAME_HEADING_LEVEL))
                {
                    doc = new RuleDocument(stream.Current.Text)
                    {
                        Annotations = TagSet.FromDictionary(metadata)
                    };
                }
                else if (doc != null && IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL))
                {
                    var matching = Synopsis(stream, doc) ||
                        Description(stream, doc) ||
                        Recommendation(stream, doc) ||
                        Notes(stream, doc) ||
                        Links(stream, doc);

                    if (matching)
                        continue;
                }

                // Skip the current token
                stream.Next();
            }
            return doc;
        }

        /// <summary>
        /// Read synopsis.
        /// </summary>
        private bool Synopsis(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Synopsis))
                return false;

            doc.Synopsis = TextBlock(stream);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read description.
        /// </summary>
        private bool Description(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Description))
                return false;

            doc.Description = TextBlock(stream);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read recommendation.
        /// </summary>
        private bool Recommendation(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Recommendation))
                return false;

            doc.Recommendation = TextBlock(stream);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read notes.
        /// </summary>
        private bool Notes(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Notes))
                return false;

            doc.Notes = TextBlock(stream);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read links.
        /// </summary>
        private bool Links(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Links))
                return false;

            var links = new List<Link>();
            stream.Next();
            while (stream.IsTokenType(MarkdownTokenType.Link, MarkdownTokenType.LinkReference, MarkdownTokenType.LineBreak, MarkdownTokenType.Text))
            {
                if (stream.IsTokenType(MarkdownTokenType.LineBreak) || stream.IsTokenType(MarkdownTokenType.Text))
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
            stream.SkipUntilHeader();
            doc.Links = links.ToArray();
            return true;
        }

        private static TextBlock TextBlock(TokenStream stream)
        {
            var useBreak = stream.Current.IsDoubleLineEnding();
            stream.Next();
            var text = ReadText(stream);
            return new TextBlock(text: text, formatOption: useBreak ? FormatOptions.LineBreak : FormatOptions.None);
        }

        /// <summary>
        /// Read tokens from the stream as text.
        /// </summary>
        private static string ReadText(TokenStream stream, bool includeNonYamlFencedBlocks = false)
        {
            var sb = new StringBuilder();
            while (stream.IsTokenType(MarkdownTokenType.Text, MarkdownTokenType.Link, MarkdownTokenType.FencedBlock, MarkdownTokenType.LineBreak))
            {
                if (stream.IsTokenType(MarkdownTokenType.Text))
                {
                    AppendEnding(sb, stream.Peak(-1));
                    sb.Append(stream.Current.Text);
                }
                else if (stream.IsTokenType(MarkdownTokenType.Link))
                {
                    AppendEnding(sb, stream.Peak(-1));
                    sb.Append(stream.Current.Meta);
                    if (!string.IsNullOrEmpty(stream.Current.Text))
                        sb.AppendFormat(Thread.CurrentThread.CurrentCulture, " ({0})", stream.Current.Text);
                }
                else if (stream.IsTokenType(MarkdownTokenType.LinkReference))
                {
                    AppendEnding(sb, stream.Peak(-1));
                    sb.Append(stream.Current.Meta);
                }
                else if (stream.IsTokenType(MarkdownTokenType.FencedBlock))
                {
                    // Only process fenced blocks if specified, and never process yaml blocks
                    if (!includeNonYamlFencedBlocks || string.Equals(stream.Current.Meta, "yaml", StringComparison.OrdinalIgnoreCase))
                    {
                        if (stream.PeakTokenType(-1) == MarkdownTokenType.LineBreak)
                            AppendEnding(sb, stream.Peak(-1));

                        break;
                    }
                    AppendEnding(sb, stream.Peak(-1), preserveEnding: true);
                    sb.Append(stream.Current.Text);
                }
                else if (stream.IsTokenType(MarkdownTokenType.LineBreak))
                    AppendEnding(sb, stream.Peak(-1));

                stream.Next();
            }

            if (stream.EOF && stream.Peak(-1).Flag.HasFlag(MarkdownTokens.Preserve) && stream.Peak(-1).Flag.HasFlag(MarkdownTokens.LineEnding))
                AppendEnding(sb, stream.Peak(-1));

            return sb.ToString();
        }

        private static void AppendEnding(StringBuilder stringBuilder, MarkdownToken token, bool preserveEnding = false)
        {
            if (token == null || stringBuilder.Length == 0 || !token.Flag.IsEnding())
                return;

            if (!preserveEnding && token.Flag.ShouldPreserve())
                preserveEnding = true;

            if (token.IsDoubleLineEnding())
                stringBuilder.Append(preserveEnding ? string.Concat(Environment.NewLine, Environment.NewLine) : Environment.NewLine);
            else if (token.IsSingleLineEnding())
                stringBuilder.Append(preserveEnding ? Environment.NewLine : Space);
        }
    }
}
