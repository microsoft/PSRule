namespace PSRule.Pipeline
{
    public static class PipelineBuilder
    {
        public static InvokeRulePipelineBuilder Invoke()
        {
            return new InvokeRulePipelineBuilder();
        }

        public static GetRulePipelineBuilder Get()
        {
            return new GetRulePipelineBuilder();
        }
    }
}
