// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Parser
{
    internal static class MarkdownTokenExtensions
    {
        public static bool IsSingleLineEnding(this MarkdownToken token)
        {
            return token.Flag.HasFlag(MarkdownTokens.LineEnding) ||
                token.Type == MarkdownTokenType.LineBreak;
        }

        public static bool IsPreservableLineEnding(this MarkdownToken token)
        {
            return (token.Flag.HasFlag(MarkdownTokens.LineEnding) && token.Flag.HasFlag(MarkdownTokens.Preserve)) ||
                token.Type == MarkdownTokenType.LineBreak;
        }

        public static bool IsDoubleLineEnding(this MarkdownToken token)
        {
            return token.Flag.HasFlag(MarkdownTokens.LineBreak) && token.Type != MarkdownTokenType.LineBreak;
        }
    }
}
