// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;

namespace PSRule;

internal static class RuleOutcomeExtensions
{
    public static RuleOutcome GetWorstCase(this RuleOutcome o1, RuleOutcome o2)
    {
        if (o2 == RuleOutcome.Error || o1 == RuleOutcome.Error)
        {
            return RuleOutcome.Error;
        }
        else if (o2 == RuleOutcome.Fail || o1 == RuleOutcome.Fail)
        {
            return RuleOutcome.Fail;
        }
        else if (o2 == RuleOutcome.Pass || o1 == RuleOutcome.Pass)
        {
            return RuleOutcome.Pass;
        }
        return o2;
    }
}
