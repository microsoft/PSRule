// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Parser
{
    internal sealed class MetadataLexer : MarkdownLexer
    {
        public MetadataLexer() { }

        public Dictionary<string, string> Process(TokenStream stream)
        {
            // Look for yaml header
            stream.MoveTo(0);
            return YamlHeader(stream);
        }
    }
}
