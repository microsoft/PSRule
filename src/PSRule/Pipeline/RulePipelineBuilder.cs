namespace PSRule.Pipeline
{
    public sealed class RulePipelineBuilder
    {
        internal RulePipelineBuilder()
        {

        }

        public IRulePipeline Build()
        {
            return new RulePipeline();
        }
    }
}
