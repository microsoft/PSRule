// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions;
using PSRule.Definitions.Rules;

namespace PSRule.Rules;

public interface IRuleBlock : IRuleV1
{
    RuleProperties Default { get; }

    RuleOverride? Override { get; }

    IRuleHelpInfo Info { get; }

    Hashtable? Configuration { get; }

    ICondition Condition { get; }
}
