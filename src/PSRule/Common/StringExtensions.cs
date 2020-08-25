// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PSRule
{
    internal static class StringExtensions
    {
        public static bool IsUri(this string s)
        {
            return s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
