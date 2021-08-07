// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule
{
    internal static class ConditionResultExtensions
    {
        public static bool AllOf(this IConditionResult result)
        {
            return result.Count > 0 && result.Pass == result.Count;
        }

        public static bool AnyOf(this IConditionResult result)
        {
            return result.Pass > 0;
        }
    }
}
