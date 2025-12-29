// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

internal sealed class ResourceHelpLexer : HelpLexer
{
    public ResourceHelpLexer(string culture) : base(culture) { }

    public ResourceHelpDocument Process(TokenStream stream)
    {
        // Look for yaml header
        stream.MoveTo(0);
        var metadata = YamlHeader(stream);
        ResourceHelpDocument? doc = null;

        // Process sections
        while (!stream.EOF)
        {
            if (IsHeading(stream.Current, RULE_NAME_HEADING_LEVEL))
            {
                doc = new ResourceHelpDocument(stream.Current.Text)
                {
                    //Annotations = ResourceTags.FromDictionary(metadata)
                };
            }
            else if (doc != null && IsHeading(stream.Current, RULE_ENTRIES_HEADING_LEVEL))
            {
                var matching = Synopsis(stream, doc) ||
                    Description(stream, doc) ||
                    Links(stream, doc);

                if (matching)
                    continue;
            }

            // Skip the current token
            stream.Next();
        }
        return doc;
    }
}
