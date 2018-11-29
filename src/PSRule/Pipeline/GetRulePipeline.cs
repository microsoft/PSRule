using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipeline : RulePipeline
    {
        internal GetRulePipeline(PSRuleOption option, string[] path, RuleFilter filter)
            : base(option, path, filter)
        {

        }

        public IEnumerable<Rule> Process()
        {
            return HostHelper.GetRule(_Option, null, _Path, _Filter);
        }
    }
}
