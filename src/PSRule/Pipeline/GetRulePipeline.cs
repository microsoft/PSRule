using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipeline : RulePipeline
    {
        internal GetRulePipeline(PSRuleOption option, RuleSource[] source, RuleFilter filter, PipelineContext context)
            : base(context, option, source, filter)
        {
            // Do nothing
        }

        public IEnumerable<Rule> Process()
        {
            return HostHelper.GetRule(_Option, _Source, _Filter);
        }
    }
}
