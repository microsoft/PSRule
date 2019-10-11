// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Parser
{
    internal sealed class MetadataLexer : MarkdownLexer
    {
        public Dictionary<string, string> Process(TokenStream stream)
        {
            stream.MoveTo(0);

            // Look for yaml header

            return YamlHeader(stream);
        }
    }
}
