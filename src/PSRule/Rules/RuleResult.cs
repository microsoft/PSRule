using System.Collections;
using System.Management.Automation;

namespace PSRule.Rules
{
    public sealed class RuleResult
    {
        public RuleResult(string ruleId)
        {
            RuleName = ruleId;
            Status = RuleResultOutcome.None;
        }

        public string RuleName { get; private set; }

        public bool Success { get; internal set; }

        public RuleResultOutcome Status { get; internal set; }

        public string Message { get; internal set; }

        public string TargetName { get; internal set; }

        public PSObject TargetObject { get; internal set; }

        public Hashtable Tag { get; internal set; }
    }
}
