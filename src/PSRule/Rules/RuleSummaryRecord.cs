using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace PSRule.Rules
{
    /// <summary>
    /// A summary format for rule results.
    /// </summary>
    [DebuggerDisplay("{RuleId}, Outcome = {Outcome}")]
    public sealed class RuleSummaryRecord : IRuleRecord
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
        /// The unique identifier for the rule.
        /// </summary>
        [JsonRequired]
        public string RuleId { get; private set; }

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

        [DefaultValue(null)]
        public Hashtable Tag { get; internal set; }

        public bool IsSuccess()
        {
            return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.None;
        }
    }
}
