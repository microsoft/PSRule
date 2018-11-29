using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipelineBuilder
    {
        internal InvokeRulePipelineBuilder()
        {

        }

        public InvokeRulePipeline Build(PSRuleOption option, string[] path, RuleFilter filter, RuleResultOutcome outcome)
        {
            return new InvokeRulePipeline(option, path, filter, outcome);
        }
    }
}
