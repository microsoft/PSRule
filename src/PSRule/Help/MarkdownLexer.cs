// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

internal abstract class MarkdownLexer
{
    protected MarkdownLexer() { }

    protected static bool IsHeading(MarkdownToken token, int level)
    {
        return token.Type == MarkdownTokenType.Header &&
            token.Depth == level;
    }

    protected static bool IsHeading(MarkdownToken token, int level, string text)
    {
        return token.Type == MarkdownTokenType.Header &&
            token.Depth == level &&
            string.Equals(text, token.Text, StringComparison.OrdinalIgnoreCase);
    }

    protected static Dictionary<string, string>? YamlHeader(TokenStream stream)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        while (!stream.EOF && stream.IsTokenType(MarkdownTokenType.YamlKeyValue))
        {
            metadata[stream.Current.Meta] = stream.Current.Text;
            stream.Next();
        }
        return metadata.Count == 0 ? null : metadata;
    }
}
