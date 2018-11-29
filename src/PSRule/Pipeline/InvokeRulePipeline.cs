using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipeline : RulePipeline
    {
        private readonly RuleResultOutcome _Outcome;
        private readonly IDictionary<string, RuleBlock> _RuleBlock;

        internal InvokeRulePipeline(PSRuleOption option, string[] path, RuleFilter filter, RuleResultOutcome outcome)
            : base(option, path, filter)
        {
            _Outcome = outcome;
            _RuleBlock = HostHelper.GetRuleBlock(_Option, _Context, _Path, _Filter);
        }

        public IEnumerable<RuleResult> Process(PSObject o)
        {
            var results = new List<RuleResult>();

            foreach (var rule in _RuleBlock.Values.ToArray())
            {
                var result = HostHelper.InvokeRuleBlock(_Option, null, rule, o);

                if (_Outcome == RuleResultOutcome.All | (result.Status & _Outcome) > 0)
                {
                    results.Add(result);
                }
            }

            return results;
        }
    }
}