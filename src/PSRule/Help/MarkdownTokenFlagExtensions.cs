// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Help;

internal static class MarkdownTokenFlagExtensions
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
