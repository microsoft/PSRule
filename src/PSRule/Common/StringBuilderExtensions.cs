// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace PSRule.Common
{
    internal static class StringBuilderExtensions
    {
        private const char Backtick = '`';
        private const char BracketOpen = '[';
        private const char BracketClose = ']';
        private const char ParenthesesOpen = '(';
        private const char ParenthesesClose = ')';
        private const char AngleOpen = '<';
        private const char AngleClose = '>';
        private const char Backslash = '\\';

        public static void AppendMarkdownText(this StringBuilder builder, string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (IsEscapableCharacter(value[i]))
                    builder.Append(Backslash);

                builder.Append(value[i]);
            }
        }

        private static bool IsEscapableCharacter(char c)
        {
            return c == Backslash || c == BracketOpen || c == ParenthesesOpen || c == AngleOpen || c == AngleClose || c == Backtick || c == BracketClose || c == ParenthesesClose;
        }
    }
}
