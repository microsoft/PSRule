// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    /// <summary>
    /// A detailed format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleId}, Outcome = {Outcome}")]
    [JsonObject]
    public sealed class RuleRecord
    {
        internal RuleRecord(string ruleId, string ruleName, PSObject targetObject, string targetName, string targetType, TagSet tag, RuleHelpInfo info, Hashtable field, RuleOutcome outcome = RuleOutcome.None, RuleOutcomeReason reason = RuleOutcomeReason.None)
        {
            RuleId = ruleId;
            RuleName = ruleName;
            TargetObject = targetObject;
            TargetName = targetName;
            TargetType = targetType;
            Outcome = outcome;
            OutcomeReason = reason;
            Info = info;
            if (tag != null)
                Tag = tag.ToHashtable();
            if (field != null && field.Count > 0)
                Field = field;

            // Limit allocations for most scenarios. Runtime calls GetData().
            Data = null;
        }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public readonly string RuleId;

        /// <summary>
        /// The name of the rule.
        /// </summary>
        [JsonProperty(PropertyName = "ruleName")]
        public readonly string RuleName;

        /// <summary>
        /// The outcome after the rule processes an object.
        /// </summary>
        [JsonProperty(PropertyName = "outcome")]
        public RuleOutcome Outcome { get; internal set; }

        [JsonProperty(PropertyName = "outcomeReason")]
        public RuleOutcomeReason OutcomeReason { get; internal set; }

        [JsonIgnore]
        [YamlIgnore]
        public string Recommendation => Info.Recommendation ?? Info.Synopsis;

        /// <summary>
        /// The reason for the failed condition.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public string[] Reason { get; internal set; }

        /// <summary>
        /// A name to identify the target object.
        /// </summary>
        [JsonProperty(PropertyName = "targetName")]
        public string TargetName { get; internal set; }

        /// <summary>
        /// The type of the target object.
        /// </summary>
        [JsonProperty(PropertyName = "targetType")]
        public string TargetType { get; internal set; }

        /// <summary>
        /// The current target object.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public PSObject TargetObject { get; internal set; }

        /// <summary>
        /// Custom data set by the rule for this target object.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public Hashtable Data { get; private set; }

        /// <summary>
        /// A set of custom fields bound for the target object.
        /// </summary>
        [JsonProperty(PropertyName = "field")]
        public Hashtable Field { get; internal set; }

        /// <summary>
        /// Tags set for the current rule.
        /// </summary>
        [DefaultValue(null)]
        [JsonProperty(PropertyName = "tag")]
        public Hashtable Tag { get; }

        /// <summary>
        /// Help info for the current rule.
        /// </summary>
        [DefaultValue(null)]
        [JsonProperty(PropertyName = "info")]
        public RuleHelpInfo Info { get; }

        /// <summary>
        /// The execution time of the rule in millisecond.
        /// </summary>
        [DefaultValue(0f)]
        [JsonProperty(PropertyName = "time")]
        public long Time { get; internal set; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = "error")]
        public ErrorInfo Error { get; internal set; }

        public bool IsSuccess()
        {
            return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.None;
        }

        public bool IsProcessed()
        {
            return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.Fail || Outcome == RuleOutcome.Error;
        }

        public string GetReasonViewString()
        {
            var sb = new StringBuilder();
            foreach (var item in Reason)
                sb.AppendLine(item);

            return sb.ToString();
        }

        /// <summary>
        /// Safe call to Data.
        /// </summary>
        internal Hashtable GetData()
        {
            if (Data == null)
                Data = new Hashtable();

            return Data;
        }
    }
}
