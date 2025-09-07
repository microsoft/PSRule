// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions.Rules;
using PSRule.Rules;

namespace PSRule.Definitions;

/// <summary>
/// Rule results for PSRule V2.
/// </summary>
public interface IRuleResultV2 : IResultRecord
{
    /// <summary>
    /// A unique identifier for the run.
    /// </summary>
    string RunId { get; }

    /// <summary>
    /// Help info for the rule.
    /// </summary>
    IRuleHelpInfo Info { get; }

    /// <summary>
    /// The outcome after the rule processes an object.
    /// </summary>
    RuleOutcome Outcome { get; }

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    SeverityLevel Level { get; }

    /// <summary>
    /// Tags set for the rule.
    /// </summary>
    Hashtable Tag { get; }

    /// <summary>
    /// The execution time of the rule in millisecond.
    /// </summary>
    long Time { get; }
}
