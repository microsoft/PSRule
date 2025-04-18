// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;

namespace PSRule.Pipeline;

#nullable enable

internal sealed class GetRuleHelpPipeline : RulePipeline, IPipeline
{
    internal GetRuleHelpPipeline(PipelineContext context, Source[] source)
        : base(context, source)
    {
        // Do nothing
    }

    public override void End()
    {
        Pipeline.Writer.WriteObject(HostHelper.GetRuleHelp(Context), true);
    }
}

#nullable restore
