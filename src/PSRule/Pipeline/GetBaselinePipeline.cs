// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Host;

namespace PSRule.Pipeline;

internal sealed class GetBaselinePipeline : RulePipeline
{
    private readonly IResourceFilter _Filter;

    internal GetBaselinePipeline(
        PipelineContext pipeline,
        Source[] source,
        IResourceFilter filter
    )
        : base(pipeline, source)
    {
        _Filter = filter;
    }

    public override void End()
    {
        Pipeline.Writer.WriteObject(HostHelper.GetBaseline(Source, Context).Where(Match), true);
        Pipeline.Writer.End(Result);
    }

    private bool Match(Baseline baseline)
    {
        return _Filter == null || _Filter.Match(baseline);
    }
}
