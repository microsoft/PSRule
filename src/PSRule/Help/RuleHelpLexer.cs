// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Resources;

namespace PSRule.Help
{
    /// <summary>
    /// A lexer that interprets markdown as rule help.
    /// </summary>
    internal sealed class RuleHelpLexer : HelpLexer
    {
        public RuleHelpLexer() { }

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
                        Annotations = ResourceTags.FromDictionary(metadata)
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
        /// Read recommendation.
        /// </summary>
        private static bool Recommendation(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Recommendation))
                return false;

            doc.Recommendation = InfoString(stream);
            stream.SkipUntilHeader();
            return true;
        }

        /// <summary>
        /// Read notes.
        /// </summary>
        private static bool Notes(TokenStream stream, RuleDocument doc)
        {
            if (!IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL, DocumentStrings.Notes))
                return false;

            doc.Notes = TextBlock(stream, includeNonYamlFencedBlocks: true);
            stream.SkipUntilHeader();
            return true;
        }
    }
}
