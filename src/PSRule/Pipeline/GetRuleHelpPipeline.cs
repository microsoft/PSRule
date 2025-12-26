// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Rules;

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
        var blocks = Pipeline.ResourceCache.OfType<IRuleV1>().ToRuleDependencyTargetCollection(Context, skipDuplicateName: true);

        Pipeline.Writer.WriteObject(blocks.GetAll().ToRuleHelp(Context), true);
    }
}

#nullable restore
