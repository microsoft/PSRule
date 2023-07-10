// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace PSRule
{
    internal static class StringExtensions
    {
        public static bool IsUri(this string s)
        {
            return s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public static string[] SplitSemantic(this string s, int width = 80)
        {
            if (s == null)
                return null;

            if (s.Length <= 80)
                return new string[] { s };

            var result = new List<string>();
            var pos = 0;
            while (pos < s.Length)
            {
                var i = pos + width - 1;
                if (i >= s.Length)
                    i = s.Length - 1;

                var breaks = s.IndexOfAny(new char[] { '\r', '\n' }, pos, i - pos);
                if (breaks > -1)
                    i = breaks;
                else
                {
                    while (!IsSemanticStopChar(s[i]) && i > pos)
                        i--;
                }

                if (i == pos)
                {
                    // move forward
                }
                if (char.IsPunctuation(s[i]))
                    i++;

                while (i > pos && i < s.Length && IsLineBreak(s[i]))
                    i--;

                if (pos != i)
                    result.Add(s.Substring(pos, i - pos));

                while (i < s.Length && IsLineBreak(s[i]))
                    i++;

                pos = i + 1;
            }
            return result.ToArray();
        }

        private static bool IsSemanticStopChar(char c)
        {
            return char.IsWhiteSpace(c) || (char.IsPunctuation(c) && char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.DashPunctuation);
        }

        private static bool IsLineBreak(char c)
        {
            return char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.LineSeparator;
        }

        public static string ToCamelCase(this string str)
        {
            return !string.IsNullOrEmpty(str) ? char.ToLowerInvariant(str[0]) + str.Substring(1) : string.Empty;
        }

        public static bool Contains(this string source, string value, StringComparison comparison)
        {
            return source?.IndexOf(value, comparison) >= 0;
        }

        public static string Replace(this string s, string oldString, string newString, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(oldString) || s.Length < oldString.Length)
                return s;

            if (caseSensitive)
                return s.Replace(oldString, newString);

            var sb = new StringBuilder(s.Length);
            var pos = 0;
            var replaceWithEmpty = string.IsNullOrEmpty(newString);
            int indexAt;
            while ((indexAt = s.IndexOf(oldString, pos, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                sb.Append(s, pos, indexAt - pos);
                if (!replaceWithEmpty)
                    sb.Append(newString);

                pos = indexAt + oldString.Length;
            }
            if (pos < s.Length)
                sb.Append(s, pos, s.Length - pos);

            return sb.ToString();
        }
    }
}
