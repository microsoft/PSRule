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

        public string RuleName { get; set; }

        public bool Success { get; set; }

        public RuleResultOutcome Status { get; set; }

        public string Message { get; set; }

        public string TargetName { get; set; }

        public PSObject TargetObject { get; internal set; }
    }
}
