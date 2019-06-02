namespace PSRule.Parser
{
    internal static class MarkdownTokenExtensions
    {
        public static bool IsSingleLineEnding(this MarkdownToken token)
        {
            return token.Flag.HasFlag(MarkdownTokenFlag.LineEnding) ||
                token.Type == MarkdownTokenType.LineBreak;
        }

        public static bool IsPreservableLineEnding(this MarkdownToken token)
        {
            return (token.Flag.HasFlag(MarkdownTokenFlag.LineEnding) && token.Flag.HasFlag(MarkdownTokenFlag.Preserve)) ||
                token.Type == MarkdownTokenType.LineBreak;
        }

        public static bool IsDoubleLineEnding(this MarkdownToken token)
        {
            return token.Flag.HasFlag(MarkdownTokenFlag.LineBreak) && token.Type != MarkdownTokenType.LineBreak;
        }
    }
}
