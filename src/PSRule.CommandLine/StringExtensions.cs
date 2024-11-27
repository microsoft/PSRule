// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
}
