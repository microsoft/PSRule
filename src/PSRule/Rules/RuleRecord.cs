using System.Collections;
using System.Diagnostics;
using System.Management.Automation;

namespace PSRule.Rules
{
    /// <summary>
    /// A detailed format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleName")]
    public sealed class RuleRecord : IRuleResult
    {
        internal RuleRecord(string ruleId)
        {
            RuleId = ruleId;
            RuleName = ruleId;
            Outcome = RuleOutcome.None;
        }

        /// <summary>
        /// A unique identifer for the rule.
        /// </summary>
        public string RuleId { get; private set; }

        public string RuleName { get; private set; }

        public bool Success { get; internal set; }

        /// <summary>
        /// The outcome of the processing an object.
        /// </summary>
        public RuleOutcome Outcome { get; internal set; }

        public string Message { get; internal set; }

        /// <summary>
        /// A name to identify the processed object.
        /// </summary>
        public string TargetName { get; internal set; }

        public PSObject TargetObject { get; internal set; }

        public Hashtable Tag { get; internal set; }
    }
}
