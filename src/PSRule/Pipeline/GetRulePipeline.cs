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
        PipelineReader reader,
        IPipelineWriter writer,
        bool includeDependencies
    )
        : base(pipeline, source, reader, writer)
    {
        _IncludeDependencies = includeDependencies;
    }

    public override void End()
    {
        Writer.WriteObject(HostHelper.GetRule(Source, Context, _IncludeDependencies), true);
        Writer.End();
    }
}
