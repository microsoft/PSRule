// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to create a PowerShell-based pipeline for running PSRule.
/// </summary>
public static class PipelineBuilder
{
    /// <summary>
    /// Create a builder for an Assert pipeline.
    /// Used by Assert-PSRule.
    /// </summary>
    /// <remarks>
    /// Assert pipelines process objects with rules and produce text-based output suitable for output to a CI pipeline.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Assert(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new AssertPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for an Invoke pipeline.
    /// Used by Invoke-PSRule.
    /// </summary>
    /// <remarks>
    /// Invoke pipelines process objects and produce records indicating the outcome of each rule.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Invoke(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new InvokeRulePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a Test pipeline.
    /// Used by Test-PSRule.
    /// </summary>
    /// <remarks>
    /// Test pipelines process objects and true or false the outcome of each rule.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Test(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new TestPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a Get pipeline.
    /// Used by Get-PSRule.
    /// </summary>
    /// <remarks>
    /// Get pipelines list rules that are discovered by PSRule either in modules or as standalone rules.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IGetPipelineBuilder Get(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetRulePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a help pipeline.
    /// Used by Get-PSRuleHelp.
    /// </summary>
    /// <remarks>
    /// Gets command lines help content for all or specific rules.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IHelpPipelineBuilder GetHelp(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetRuleHelpPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder to define a list of rule sources.
    /// </summary>
    /// <param name="option">>Options that configure PSRule.</param>
    /// <param name="hostContext">>An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the source pipeline.</returns>
    public static ISourcePipelineBuilder RuleSource(PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new SourcePipelineBuilder(hostContext, option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a get baseline pipeline.
    /// Used by Get-PSRuleBaseline.
    /// </summary>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IPipelineBuilder GetBaseline(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetBaselinePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for an export baseline pipeline.
    /// Used by Export-PSRuleBaseline.
    /// </summary>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IPipelineBuilder ExportBaseline(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new ExportBaselinePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a target pipeline.
    /// Used by Get-PSRuleTarget.
    /// </summary>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IGetTargetPipelineBuilder GetTarget(PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetTargetPipelineBuilder(null, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }
}
