// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule;

internal static class ReasonExtensions
{
    internal static string[] GetStrings(this IList<ResultReason> reason)
    {
        if (reason == null || reason.Count == 0)
            return Array.Empty<string>();

        var result = new string[reason.Count];
        for (var i = 0; i < reason.Count; i++)
            result[i] = reason[i].ToString();

        return result;
    }

}
