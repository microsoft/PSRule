using PSRule.Rules;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipelineBuilder
    {
        internal InvokeRulePipelineBuilder()
        {

        }

        public InvokeRulePipeline Build(string[] path, RuleFilter filter, RuleResultOutcome outcome)
        {
            return new InvokeRulePipeline(path, filter, outcome);
        }
    }
}