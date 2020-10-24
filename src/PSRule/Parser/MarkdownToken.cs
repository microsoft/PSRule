// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace PSRule.Parser
{
    public enum MarkdownTokenType
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

    [Flags()]
    public enum MarkdownTokens
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

    public static class MarkdownTokenFlagExtensions
    {
        public static bool IsEnding(this MarkdownTokens flags)
        {
            return flags.HasFlag(MarkdownTokens.LineEnding) || flags.HasFlag(MarkdownTokens.LineBreak);
        }

        public static bool IsLineBreak(this MarkdownTokens flags)
        {
            return flags.HasFlag(MarkdownTokens.LineBreak);
        }

        public static bool ShouldPreserve(this MarkdownTokens flags)
        {
            return flags.HasFlag(MarkdownTokens.Preserve);
        }
    }

    [DebuggerDisplay("Type = {Type}, Text = {Text}")]
    public sealed class MarkdownToken
    {
        public SourceExtent Extent { get; set; }

        public MarkdownTokenType Type { get; set; }

        public string Text { get; set; }

        public string Meta { get; set; }

        public int Depth { get; set; }

        public MarkdownTokens Flag { get; set; }
    }
}
