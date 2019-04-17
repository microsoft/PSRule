using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
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
        internal RuleRecord(string ruleId, string ruleName, PSObject targetObject, string targetName, string targetType, TagSet tag, RuleOutcome outcome = RuleOutcome.None, RuleOutcomeReason reason = RuleOutcomeReason.None, string message = null)
        {
            RuleId = ruleId;
            RuleName = ruleName;
            TargetObject = targetObject;
            TargetName = targetName;
            TargetType = targetType;
            
            if (tag != null)
            {
                Tag = tag.ToHashtable();
            }

            Outcome = outcome;
            OutcomeReason = reason;
            Message = message;
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

        [JsonProperty(PropertyName = "message")]
        public string Message { get; internal set; }

        /// <summary>
        /// A name to identify the processed object.
        /// </summary>
        [JsonProperty(PropertyName = "targetName")]
        public string TargetName { get; internal set; }

        /// <summary>
        /// The type of the processed object.
        /// </summary>
        [JsonProperty(PropertyName = "targetType")]
        public string TargetType { get; internal set; }

        [JsonIgnore]
        [YamlIgnore]
        public PSObject TargetObject { get; internal set; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = "tag")]
        public Hashtable Tag { get; internal set; }

        [DefaultValue(0f)]
        [JsonProperty(PropertyName = "time")]
        public float Time { get; internal set; }

        public bool IsSuccess()
        {
            return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.None;
        }

        public bool IsProcessed()
        {
            return Outcome == RuleOutcome.Processed;
        }
    }
}
