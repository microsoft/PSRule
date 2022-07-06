// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline
{
    public static class CommandLineBuilder
    {
        public static IInvokePipelineBuilder Invoke(string[] module, PSRuleOption option, IHostContext hostContext)
        {
            var sourcePipeline = new SourcePipelineBuilder(hostContext, option);
            for (var i = 0; i < module.Length; i++)
                sourcePipeline.ModuleByName(module[i]);

            var source = sourcePipeline.Build();
            var pipeline = new InvokeRulePipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        public static IInvokePipelineBuilder Assert(string[] module, PSRuleOption option, IHostContext hostContext)
        {
            var sourcePipeline = new SourcePipelineBuilder(hostContext, option);
            for (var i = 0; i < module.Length; i++)
                sourcePipeline.ModuleByName(module[i]);

            var source = sourcePipeline.Build();
            var pipeline = new AssertPipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }
    }
}
