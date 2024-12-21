// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline;

/// <summary>
/// A pipeline that gets target objects through the pipeline.
/// </summary>
internal sealed class GetTargetPipeline : RulePipeline
{
    internal GetTargetPipeline(PipelineContext context)
        : base(context, null) { }

    public override void Process(PSObject sourceObject)
    {
        try
        {
            Pipeline.Reader.Enqueue(sourceObject);
            while (Pipeline.Reader.TryDequeue(out var next))
            {
                // TODO: Temporary workaround to cast interface
                if (next is TargetObject to)
                    Pipeline.Writer.WriteObject(to.Value, false);
            }
        }
        catch (Exception)
        {
            End();
            throw;
        }
    }
}
