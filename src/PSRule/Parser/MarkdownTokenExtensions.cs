namespace PSRule.Parser
{
    internal static class MarkdownTokenExtensions
    {
        public static bool IsSingleLineEnding(this MarkdownToken token)
        {
            return token.Flag.HasFlag(MarkdownTokenFlags.LineEnding) ||
                token.Type == MarkdownTokenType.LineBreak;
        }

        public static bool IsPreservableLineEnding(this MarkdownToken token)
        {
            return (token.Flag.HasFlag(MarkdownTokenFlags.LineEnding) && token.Flag.HasFlag(MarkdownTokenFlags.Preserve)) ||
                token.Type == MarkdownTokenType.LineBreak;
        }

        public static bool IsDoubleLineEnding(this MarkdownToken token)
        {
            return token.Flag.HasFlag(MarkdownTokenFlags.LineBreak) && token.Type != MarkdownTokenType.LineBreak;
        }
    }
}
