// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to create a PSRule pipeline that can be used via the .NET SDK.
    /// </summary>
    public static class CommandLineBuilder
    {
        /// <summary>
        /// Create a builder for an Invoke pipeline.
        /// </summary>
        /// <remarks>
        /// Invoke piplines process objects and produce records indicating the outcome of each rule.
        /// </remarks>
        /// <param name="module">The name of modules containing rules to process.</param>
        /// <param name="option">Options that configure PSRule.</param>
        /// <param name="hostContext">An implementation of a host context that will recieve output and results.</param>
        /// <returns>A builder object to configure the pipeline.</returns>
        public static IInvokePipelineBuilder Invoke(string[] module, PSRuleOption option, IHostContext hostContext)
        {
            var sourcePipeline = new SourcePipelineBuilder(hostContext, option, GetLocalPath());
            for (var i = 0; i < module.Length; i++)
                sourcePipeline.ModuleByName(module[i]);

            for (var i = 0; option.Include.Module != null && i < option.Include.Module.Length; i++)
                sourcePipeline.ModuleByName(option.Include.Module[i]);

            var source = sourcePipeline.Build();
            var pipeline = new InvokeRulePipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        /// <summary>
        /// Create a builder for an Assert pipeline.
        /// </summary>
        /// <remarks>
        /// Assert pipelines process objects with rules and produce text-based output suitable for output to a CI pipeline.
        /// </remarks>
        /// <param name="module">The name of modules containing rules to process.</param>
        /// <param name="option">Options that configure PSRule.</param>
        /// <param name="hostContext">An implementation of a host context that will recieve output and results.</param>
        /// <returns>A builder object to configure the pipeline.</returns>
        public static IInvokePipelineBuilder Assert(string[] module, PSRuleOption option, IHostContext hostContext)
        {
            var sourcePipeline = new SourcePipelineBuilder(hostContext, option, GetLocalPath());
            for (var i = 0; module != null && i < module.Length; i++)
                sourcePipeline.ModuleByName(module[i]);

            for (var i = 0; option.Include.Module != null && i < option.Include.Module.Length; i++)
                sourcePipeline.ModuleByName(option.Include.Module[i]);

            var source = sourcePipeline.Build();
            var pipeline = new AssertPipelineBuilder(source, hostContext);
            pipeline.Configure(option);
            return pipeline;
        }

        internal static string GetLocalPath()
        {
            return string.IsNullOrEmpty(AppContext.BaseDirectory) ? null : Environment.GetRootedBasePath(AppContext.BaseDirectory);
        }
    }
}
