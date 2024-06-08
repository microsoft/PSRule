// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

internal static class TokenStreamExtensions
{
    /// <summary>
    /// Add a header.
    /// </summary>
    public static void Header(this TokenStream stream, int depth, string text, SourceExtent extent, bool lineBreak)
    {
        stream.Add(new MarkdownToken()
        {
            Depth = depth,
            Extent = extent,
            Text = text,
            Type = MarkdownTokenType.Header,
            Flag = lineBreak ? MarkdownTokens.LineBreak : MarkdownTokens.LineEnding | MarkdownTokens.Preserve
        });
    }

    public static void YamlKeyValue(this TokenStream stream, string key, string value)
    {
        stream.Add(new MarkdownToken()
        {
            Meta = key,
            Text = value,
            Type = MarkdownTokenType.YamlKeyValue
        });
    }

    /// <summary>
    /// Add a code fence.
    /// </summary>
    public static void FencedBlock(this TokenStream stream, string meta, string text, SourceExtent extent, bool lineBreak)
    {
        stream.Add(new MarkdownToken()
        {
            Extent = extent,
            Meta = meta,
            Text = text,
            Type = MarkdownTokenType.FencedBlock,
            Flag = (lineBreak ? MarkdownTokens.LineBreak : MarkdownTokens.LineEnding) | MarkdownTokens.Preserve
        });
    }

    /// <summary>
    /// Add a line break.
    /// </summary>
    public static void LineBreak(this TokenStream stream, int count)
    {
        // Ignore line break at the very start of file
        if (stream.Count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.LineBreak, Flag = MarkdownTokens.LineBreak });
        }
    }

    public static void Text(this TokenStream stream, string text, MarkdownTokens flag = MarkdownTokens.None)
    {
        if (MergeText(stream.Current, text, flag))
        {
            return;
        }

        stream.Add(new MarkdownToken() { Type = MarkdownTokenType.Text, Text = text, Flag = flag });
    }

    private static bool MergeText(MarkdownToken current, string text, MarkdownTokens flag)
    {
        // Only allow merge if the previous token was text
        if (current == null || current.Type != MarkdownTokenType.Text)
        {
            return false;
        }

        if (current.Flag.ShouldPreserve())
        {
            return false;
        }

        // If the previous token was text, lessen the break but still don't allow merging
        if (current.Flag.HasFlag(MarkdownTokens.LineBreak) && !current.Flag.ShouldPreserve())
        {
            return false;
        }

        // Text must have the same flags set
        if (current.Flag.HasFlag(MarkdownTokens.Italic) != flag.HasFlag(MarkdownTokens.Italic))
        {
            return false;
        }

        if (current.Flag.HasFlag(MarkdownTokens.Bold) != flag.HasFlag(MarkdownTokens.Bold))
        {
            return false;
        }

        if (current.Flag.HasFlag(MarkdownTokens.Code) != flag.HasFlag(MarkdownTokens.Code))
        {
            return false;
        }

        if (!current.Flag.IsEnding())
        {
            current.Text = string.Concat(current.Text, text);
        }
        else if (current.Flag == MarkdownTokens.LineEnding)
        {
            return false;
        }

        // Take on the ending of the merged token
        current.Flag = flag;

        return true;
    }

    public static void Link(this TokenStream stream, string text, string uri)
    {
        stream.Add(new MarkdownToken() { Type = MarkdownTokenType.Link, Meta = text, Text = uri });
    }

    public static void LinkReference(this TokenStream stream, string text, string linkRef)
    {
        stream.Add(new MarkdownToken() { Type = MarkdownTokenType.LinkReference, Meta = text, Text = linkRef });
    }

    public static void LinkReferenceDefinition(this TokenStream stream, string text, string linkTarget)
    {
        stream.Add(new MarkdownToken() { Type = MarkdownTokenType.LinkReferenceDefinition, Meta = text, Text = linkTarget });
    }

    /// <summary>
    /// Add a marker for the start of a paragraph.
    /// </summary>
    public static void ParagraphStart(this TokenStream stream)
    {
        stream.Add(new MarkdownToken() { Type = MarkdownTokenType.ParagraphStart });
    }

    /// <summary>
    /// Add a marker for the end of a paragraph.
    /// </summary>
    public static void ParagraphEnd(this TokenStream stream)
    {
        if (stream.Count > 0)
        {
            if (stream.Current.Type == MarkdownTokenType.ParagraphStart)
            {
                stream.Pop();

                return;
            }

            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.ParagraphEnd });
        }
    }

    public static IEnumerable<MarkdownToken> GetSection(this TokenStream stream, string header)
    {
        return stream.Count == 0
            ? Enumerable.Empty<MarkdownToken>()
            : stream
                // Skip until we reach the header
                .SkipWhile(token => token.Type != MarkdownTokenType.Header || token.Text != header)

                // Get all tokens to the next header
                .Skip(1)
                .TakeWhile(token => token.Type != MarkdownTokenType.Header);
    }

    public static IEnumerable<MarkdownToken> GetSections(this TokenStream stream)
    {
        return stream.Count == 0
            ? Enumerable.Empty<MarkdownToken>()
            : stream
                // Skip until we reach the header
                .SkipWhile(token => token.Type != MarkdownTokenType.Header)

                // Get all tokens to the next header
                .Skip(1)
                .TakeWhile(token => token.Type != MarkdownTokenType.Header);
    }
}
