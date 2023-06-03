// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using PSRule.Definitions;
using PSRule.Resources;

namespace PSRule.Help
{
    internal abstract class HelpLexer : MarkdownLexer
    {
        protected const int RULE_NAME_HEADING_LEVEL = 1;
        protected const int RULE_ENTRIES_HEADING_LEVEL = 2;

        private const string Space = " ";

        protected readonly ResourceSet _Strings;

        protected HelpLexer(string culture)
        {
            var cultureInfo = string.IsNullOrEmpty(culture) ? Thread.CurrentThread.CurrentCulture : CultureInfo.GetCultureInfo(culture);
            _Strings = DocumentStrings.ResourceManager.GetResourceSet(cultureInfo, createIfNotExists: true, tryParents: true);
        }

        /// <summary>
        /// Read synopsis.
        /// </summary>
        protected bool Synopsis(TokenStream stream, IHelpDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, _Strings.GetString("Synopsis")))
                return false;

            doc.Synopsis = InfoString(stream);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read description.
        /// </summary>
        protected bool Description(TokenStream stream, IHelpDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, _Strings.GetString("Description")))
                return false;

            doc.Description = InfoString(stream, includeNonYamlFencedBlocks: true);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read links.
        /// </summary>
        protected bool Links(TokenStream stream, IHelpDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, _Strings.GetString("Links")))
                return false;

            var links = new List<Link>();
            stream.Next();
            while (stream.IsTokenType(
                MarkdownTokenType.Link,
                MarkdownTokenType.LinkReference,
                MarkdownTokenType.LineBreak,
                MarkdownTokenType.Text))
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

        protected static InfoString InfoString(TokenStream stream, bool includeNonYamlFencedBlocks = false)
        {
            stream.Next();
            var text = ReadText(stream, includeNonYamlFencedBlocks);
            return new InfoString(text, null);
        }

        protected static TextBlock TextBlock(TokenStream stream, bool includeNonYamlFencedBlocks = false)
        {
            var useBreak = stream.Current.IsDoubleLineEnding();
            stream.Next();
            var text = ReadText(stream, includeNonYamlFencedBlocks);
            return new TextBlock(text: text, formatOption: useBreak ? FormatOptions.LineBreak : FormatOptions.None);
        }

        /// <summary>
        /// Read tokens from the stream as text.
        /// </summary>
        private static string ReadText(TokenStream stream, bool includeNonYamlFencedBlocks)
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
                stringBuilder.Append(preserveEnding ? string.Concat(System.Environment.NewLine, System.Environment.NewLine) : System.Environment.NewLine);
            else if (token.IsSingleLineEnding())
                stringBuilder.Append(preserveEnding ? System.Environment.NewLine : Space);
        }
    }
}
