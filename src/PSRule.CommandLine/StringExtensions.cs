// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.CommandLine;

/// <summary>
/// String extension methods.
/// </summary>
public static class StringExtensions
{
    private const char QUOTE = '"';
    private const char APOSTROPHE = '\'';

    /// <summary>
    /// Remove paired single and double quotes from the start and end of a string.
    /// </summary>
    public static string? TrimQuotes(this string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 2)
            return value;

        if (value[0] == QUOTE && value[value.Length - 1] == QUOTE)
            return value.Substring(1, value.Length - 2);

        return value[0] == APOSTROPHE && value[value.Length - 1] == APOSTROPHE ? value.Substring(1, value.Length - 2) : value;
    }

    /// <summary>
    /// Convert a string to <see cref="OutputFormat"/>.
    /// </summary>
    public static OutputFormat ToOutputFormat(this string? value)
    {
        return value != null && Enum.TryParse<OutputFormat>(value, true, out var result) ? result : OutputFormat.None;
    }

    /// <summary>
    /// Convert string arguments to flags of <see cref="RuleOutcome"/>.
    /// </summary>
    public static RuleOutcome? ToRuleOutcome(this string[]? s)
    {
        var result = RuleOutcome.None;
        for (var i = 0; s != null && i < s.Length; i++)
        {
            if (Enum.TryParse(s[i], ignoreCase: true, result: out RuleOutcome flag))
                result |= flag;
        }
        return result == RuleOutcome.None ? null : result;
    }
}
