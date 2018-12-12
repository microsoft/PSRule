using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace PSRule.Rules
{
    /// <summary>
    /// A summary format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleId")]
    public sealed class RuleSummaryRecord : IRuleResult
    {
        internal RuleSummaryRecord(string ruleId)
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
        /// The number of rile errors.
        /// </summary>
        public int Error { get; internal set; }

        /// <summary>
        /// The unique identifer for the rule.
        /// </summary>
        [JsonRequired]
        public string RuleId { get; private set; }

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
                    return RuleOutcome.Failed;
                }
                else if (Pass > 0)
                {
                    return RuleOutcome.Passed;
                }

                return RuleOutcome.None;
            }
        }

        [DefaultValue(null)]
        public Hashtable Tag { get; internal set; }
    }
}
