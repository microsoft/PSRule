// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace PSRule;

/// <summary>
/// Extension methods for strings.
/// </summary>
public static class StringExtensions
{
    private const string HTTP_SCHEME = "http://";
    private const string HTTPS_SCHEME = "https://";

    private static readonly char[] LINE_STOPCHARACTERS = new char[] { '\r', '\n' };

    /// <summary>
    /// Determine if the string is a URL.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns>Returns <c>true</c> if the string starts with a http:// or https://.</returns>
    public static bool IsURL(this string s)
    {
        return s.StartsWith(HTTP_SCHEME, StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith(HTTPS_SCHEME, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Split a string semantically based on a maximum width.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="width">The maximum width to split lines along.</param>
    /// <returns>Returns an array of strings that have been semantically split.</returns>
    public static string[]? SplitSemantic(this string s, int width = 80)
    {
        if (s == null)
            return null;

        if (s.Length <= width)
            return new string[] { s };

        var result = new List<string>();
        var pos = 0;
        while (pos < s.Length)
        {
            var i = pos + width - 1;
            if (i >= s.Length)
                i = s.Length - 1;

            var breaks = s.IndexOfAny(LINE_STOPCHARACTERS, pos, i - pos);
            if (breaks > -1)
            {
                i = breaks;
            }
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

    /// <summary>
    /// Convert a string to camel case.
    /// </summary>
    /// <param name="s">The input string to convert.</param>
    /// <returns>The converted string.</returns>
    public static string ToCamelCase(this string s)
    {
        return !string.IsNullOrEmpty(s) ? char.ToLowerInvariant(s[0]) + s.Substring(1) : string.Empty;
    }

    /// <summary>
    /// Determine if the string contains a specific substring.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="value"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    public static bool Contains(this string s, string value, StringComparison comparison)
    {
        return s?.IndexOf(value, comparison) >= 0;
    }

    /// <summary>
    /// Replace an old string with a new string.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="oldString"></param>
    /// <param name="newString"></param>
    /// <param name="caseSensitive"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private static bool IsSemanticStopChar(char c)
    {
        return char.IsWhiteSpace(c) || (char.IsPunctuation(c) && char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.DashPunctuation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private static bool IsLineBreak(char c)
    {
        return char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.LineSeparator;
    }
}
