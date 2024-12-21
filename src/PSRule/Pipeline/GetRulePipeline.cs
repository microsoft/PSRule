// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;

namespace PSRule.Pipeline;

internal sealed class GetRulePipeline : RulePipeline, IPipeline
{
    private readonly bool _IncludeDependencies;

    internal GetRulePipeline(
        PipelineContext pipeline,
        Source[] source,
        bool includeDependencies
    )
        : base(pipeline, source)
    {
        _IncludeDependencies = includeDependencies;
    }

    public override void End()
    {
        Pipeline.Writer.WriteObject(HostHelper.GetRule(Context, _IncludeDependencies), true);
        Pipeline.Writer.End(Result);
    }
}
