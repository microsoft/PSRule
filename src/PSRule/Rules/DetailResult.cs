using System.Collections;
using System.Diagnostics;
using System.Management.Automation;

namespace PSRule.Rules
{
    /// <summary>
    /// A detailed format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleName")]
    public sealed class DetailResult : IRuleResult
    {
        internal DetailResult(string ruleId)
        {
            RuleName = ruleId;
            Status = RuleOutcome.None;
        }

        public string RuleName { get; private set; }

        public bool Success { get; internal set; }

        /// <summary>
        /// The outcome of the processing an object.
        /// </summary>
        public RuleOutcome Status { get; internal set; }

        public string Message { get; internal set; }

        /// <summary>
        /// A name to identify the processed object.
        /// </summary>
        public string TargetName { get; internal set; }

        public PSObject TargetObject { get; internal set; }

        public Hashtable Tag { get; internal set; }
    }
}
