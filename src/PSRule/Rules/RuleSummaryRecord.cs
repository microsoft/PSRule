// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using Newtonsoft.Json;
using PSRule.Definitions;

namespace PSRule.Rules;

/// <summary>
/// A summary format for rule results.
/// </summary>
[DebuggerDisplay("{RuleId}, Outcome = {Outcome}")]
public sealed class RuleSummaryRecord
{
    internal RuleSummaryRecord(string ruleId, string ruleName, IResourceTags tag, IRuleHelpInfo info)
    {
        RuleId = ruleId;
        RuleName = ruleName;
        Tag = tag?.ToHashtable();
        Info = info;
    }

    /// <summary>
    /// The unique identifier for the rule.
    /// </summary>
    [JsonRequired]
    public readonly string RuleId;

    /// <summary>
    /// The name of the rule.
    /// </summary>
    public readonly string RuleName;

    /// <summary>
    /// The number of rule passes.
    /// </summary>
    public int Pass { get; internal set; }

    /// <summary>
    /// The number of rule failures.
    /// </summary>
    public int Fail { get; internal set; }

    /// <summary>
    /// The number of rile errors.
    /// </summary>
    public int Error { get; internal set; }

    /// <summary>
    /// The aggregate outcome after the rule processes all objects.
    /// </summary>
    public RuleOutcome Outcome
    {
        get
        {
            if (Error > 0)
            {
                return RuleOutcome.Error;
            }
            else if (Fail > 0)
            {
                return RuleOutcome.Fail;
            }
            else if (Pass > 0)
            {
                return RuleOutcome.Pass;
            }
            return RuleOutcome.None;
        }
    }

    /// <summary>
    /// Tags associated with the rule.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "tag")]
    public Hashtable Tag { get; internal set; }

    /// <summary>
    /// Additional information associated with the rule.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "info")]
    public IRuleHelpInfo Info { get; internal set; }

    /// <summary>
    /// Determines if the overall outcome is successful or a failure.
    /// </summary>
    public bool IsSuccess()
    {
        return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.None;
    }

    internal void Add(RuleOutcome outcome)
    {
        if (outcome == RuleOutcome.Pass)
            Pass++;
        else if (outcome == RuleOutcome.Fail)
            Fail++;
        else if (outcome == RuleOutcome.Error)
            Error++;
    }
}
