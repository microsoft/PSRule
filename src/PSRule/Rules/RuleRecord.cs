// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Rules
{
    /// <summary>
    /// A detailed format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleId}, Outcome = {Outcome}")]
    [JsonObject]
    public sealed class RuleRecord : IRuleResult
    {
        private readonly TargetObject _TargetObject;

        internal RuleRecord(string runId, ResourceId ruleId, string @ref, TargetObject targetObject, string targetName, string targetType, ResourceTags tag, RuleHelpInfo info, Hashtable field, SeverityLevel level, RuleOutcome outcome = RuleOutcome.None, RuleOutcomeReason reason = RuleOutcomeReason.None)
        {
            _TargetObject = targetObject;
            RunId = runId;
            RuleId = ruleId.Value;
            RuleName = ruleId.Name;
            Ref = @ref;
            TargetObject = targetObject.Value;
            TargetName = targetName;
            TargetType = targetType;
            Outcome = outcome;
            OutcomeReason = reason;
            Info = info;
            Source = targetObject.Source.GetSourceInfo();
            Level = level;
            if (tag != null)
                Tag = tag.ToHashtable();

            if (field != null && field.Count > 0)
                Field = field;
        }

        /// <summary>
        /// A unique identifier for the run.
        /// </summary>
        [JsonProperty(PropertyName = "runId")]
        public string RunId { get; }

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

        public string Ref { get; }

        /// <summary>
        /// If the rule fails, how serious is the result.
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        public SeverityLevel Level { get; }

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
        public string TargetName { get; }

        /// <summary>
        /// The type of the target object.
        /// </summary>
        [JsonProperty(PropertyName = "targetType")]
        public string TargetType { get; }

        /// <summary>
        /// The current target object.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public PSObject TargetObject { get; }

        /// <summary>
        /// Custom data set by the rule for this target object.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public Hashtable Data => _TargetObject.GetData();

        /// <summary>
        /// A set of custom fields bound for the target object.
        /// </summary>
        [JsonProperty(PropertyName = "field")]
        public Hashtable Field { get; }

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

        /// <summary>
        /// Source of target object.
        /// </summary>
        [DefaultValue(null)]
        [JsonProperty(PropertyName = "source")]
        public TargetSourceInfo[] Source { get; }

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
            if (Reason != null || Reason.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (var i = 0; i < Reason.Length; i++)
                sb.AppendLine(Reason[i]);

            return sb.ToString();
        }

        internal bool HasSource()
        {
            return Source != null && Source.Length > 0;
        }
    }
}
