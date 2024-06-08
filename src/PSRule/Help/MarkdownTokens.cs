// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

[Flags()]
internal enum MarkdownTokens
{
    None = 0,

    Italic = 1,

    Bold = 2,

    Code = 4,

    LineEnding = 8,

    LineBreak = 16,

    Preserve = 32,

    // Accelerators
    PreserveLineEnding = 40
}
