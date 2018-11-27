using System.Management.Automation;

namespace PSRule.Rules
{
    public sealed class RuleResult
    {
        public string RuleName { get; set; }

        public bool Success { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string TargetName { get; set; }

        public PSObject TargetObject { get; internal set; }
    }
}
