// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

internal enum MarkdownTokenType
{
    None = 0,

    Text,

    Header,

    FencedBlock,

    LineBreak,

    ParagraphStart,

    ParagraphEnd,

    LinkReference,

    Link,

    LinkReferenceDefinition,

    YamlKeyValue
}
