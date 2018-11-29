using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipelineBuilder
    {
        internal GetRulePipelineBuilder()
        {

        }

        public GetRulePipeline Build(PSRuleOption option, string[] path, RuleFilter filter)
        {
            return new GetRulePipeline(option, path, filter);
        }
    }
}
