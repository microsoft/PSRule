using System;
using System.Diagnostics;

namespace PSRule.Parser
{
    public enum MarkdownTokenType : byte
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
    public enum MarkdownTokenFlag
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
        public static bool IsEnding(this MarkdownTokenFlag flag)
        {
            return flag.HasFlag(MarkdownTokenFlag.LineEnding) || flag.HasFlag(MarkdownTokenFlag.LineBreak);
        }

        public static bool IsLineBreak(this MarkdownTokenFlag flag)
        {
            return flag.HasFlag(MarkdownTokenFlag.LineBreak);
        }

        public static bool ShouldPreserve(this MarkdownTokenFlag flag)
        {
            return flag.HasFlag(MarkdownTokenFlag.Preserve);
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

        //public MarkdownTokenEnding Ending { get; set; }

        public MarkdownTokenFlag Flag { get; set; }
    }
}
