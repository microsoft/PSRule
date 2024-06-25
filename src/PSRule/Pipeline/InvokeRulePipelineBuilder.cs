// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline;

/// <summary>
/// A helper to construct the pipeline for Invoke-PSRule.
/// </summary>
internal sealed class InvokeRulePipelineBuilder : InvokePipelineBuilderBase
{
    internal InvokeRulePipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }
}
