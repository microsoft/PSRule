using System;
using System.Collections.Generic;

namespace PSRule.Parser
{
    internal abstract class MarkdownLexer
    {
        protected bool IsHeading(MarkdownToken token, int level)
        {
            return token.Type == MarkdownTokenType.Header &&
                token.Depth == level;
        }

        protected bool IsHeading(MarkdownToken token, int level, string text)
        {
            return token.Type == MarkdownTokenType.Header &&
                token.Depth == level &&
                string.Equals(text, token.Text, StringComparison.OrdinalIgnoreCase);
        }

        protected Dictionary<string, string> YamlHeader(TokenStream stream)
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
}
