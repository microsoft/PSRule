using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;

namespace PSRule.Rules
{
    /// <summary>
    /// A detailed format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleId}, Outcome = {Outcome}")]
    public sealed class RuleRecord : IRuleRecord
    {
        internal RuleRecord(string ruleId, string ruleName, RuleOutcome outcome = RuleOutcome.None, RuleOutcomeReason reason = RuleOutcomeReason.None)
        {
            RuleId = ruleId;
            RuleName = ruleName;
            Outcome = outcome;
            OutcomeReason = reason;
        }

        /// <summary>
        /// A unique identifier for the rule.
        /// </summary>
        [JsonRequired]
        public string RuleId { get; private set; }

        /// <summary>
        /// The name of the rule.
        /// </summary>
        public string RuleName { get; private set; }

        /// <summary>
        /// The outcome after the rule processes an object.
        /// </summary>
        public RuleOutcome Outcome { get; internal set; }

        public RuleOutcomeReason OutcomeReason { get; internal set; }

        public string Message { get; internal set; }

        /// <summary>
        /// A name to identify the processed object.
        /// </summary>
        public string TargetName { get; internal set; }

        [JsonIgnore]
        public PSObject TargetObject { get; internal set; }

        [DefaultValue(null)]
        public Hashtable Tag { get; internal set; }

        public bool IsSuccess()
        {
            return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.None;
        }
    }
}
