using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipeline : RulePipeline
    {
        private readonly IDictionary<string, RuleBlock> _RuleBlock;

        internal InvokeRulePipeline(string[] path, RuleFilter filter)
            : base(path, filter)
        {
            _RuleBlock = HostHelper.GetRuleBlock(_Context, _Path, _Filter);
        }

        public IEnumerable<RuleResult> Process(PSObject o)
        {
            var results = new List<RuleResult>();

            foreach (var rule in _RuleBlock.Values.ToArray())
            {
                var result = HostHelper.InvokeRuleBlock(null, rule, o);

                results.Add(result);
            }

            return results;
        }
    }
}