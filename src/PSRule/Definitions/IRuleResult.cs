// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Rules;
using PSRule.Rules;

namespace PSRule.Definitions
{
    public interface IRuleResult : IResultRecord
    {
        string RunId { get; }

        RuleOutcome Outcome { get; }

        SeverityLevel Level { get; }

        long Time { get; }
    }
}
