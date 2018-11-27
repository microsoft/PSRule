using PSRule.Rules;

namespace PSRule.Pipeline
{
    public sealed class GetRulePipelineBuilder
    {
        internal GetRulePipelineBuilder()
        {

        }

        public GetRulePipeline Build(string[] path, RuleFilter filter)
        {
            return new GetRulePipeline(path, filter);
        }
    }
}
