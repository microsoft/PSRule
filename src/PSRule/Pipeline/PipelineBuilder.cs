namespace PSRule.Pipeline
{
    public static class PipelineBuilder
    {
        public static RulePipelineBuilder Get()
        {
            return new RulePipelineBuilder();
        }
    }
}
