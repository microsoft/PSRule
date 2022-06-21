// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using PSRule.Definitions.Rules;
using PSRule.Rules;

namespace PSRule.Definitions
{
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
        IRuleHelpInfoV2 Info { get; }

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

    public interface IDetailedRuleResultV2 : IRuleResultV2
    {
        /// <summary>
        /// Custom data set by the rule for this target object.
        /// </summary>
        Hashtable Data { get; }

        /// <summary>
        /// Detailed information about the rule result.
        /// </summary>
        IResultDetailV2 Detail { get; }

        /// <summary>
        /// A set of custom fields bound for the target object.
        /// </summary>
        Hashtable Field { get; }

        string TargetName { get; }

        string TargetType { get; }
    }

    /// <summary>
    /// Detailed information about the rule result.
    /// </summary>
    public interface IResultDetailV2
    {
        /// <summary>
        /// Any reasons for the result.
        /// </summary>
        IEnumerable<IResultReasonV2> Reason { get; }
    }

    /// <summary>
    /// A reason for the rule result.
    /// </summary>
    public interface IResultReasonV2
    {
        /// <summary>
        /// The object path that failed.
        /// </summary>
        string Path { get; }

        string Message { get; }

        string Format();
    }
}
