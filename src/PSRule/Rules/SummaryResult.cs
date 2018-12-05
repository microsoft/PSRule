using System.Diagnostics;

namespace PSRule.Rules
{
    /// <summary>
    /// A summary format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleId")]
    public sealed class SummaryResult : IRuleResult
    {
        internal SummaryResult(string ruleId)
        {
            RuleId = ruleId;
        }

        /// <summary>
        /// The number of rule passes.
        /// </summary>
        public int Pass { get; internal set; }

        /// <summary>
        /// The number of rule failures.
        /// </summary>
        public int Fail { get; internal set; }

        /// <summary>
        /// The unique identifer for the rule.
        /// </summary>
        public string RuleId { get; private set; }

        //TODO: Add tags
    }
}
