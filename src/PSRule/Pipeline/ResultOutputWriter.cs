// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline;

internal abstract class ResultOutputWriter<T>(IPipelineWriter inner, PSRuleOption option, ShouldProcess? shouldProcess) : PipelineWriter(inner, option, shouldProcess)
{
    private readonly List<T> _Result = [];

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is InvokeResult && Option.Output.As == ResultFormat.Summary)
        {
            base.WriteObject(sendToPipeline, enumerateCollection);
            return;
        }

        if (sendToPipeline is InvokeResult result)
        {
            Add(typeof(T) == typeof(RuleRecord) ? result.AsRecord() : result);
        }
        else
        {
            Add(sendToPipeline);
        }
        base.WriteObject(sendToPipeline, enumerateCollection);
    }

    protected void Add(object o)
    {
        if (o is T[] collection)
            _Result.AddRange(collection);
        else if (o is T item)
            _Result.Add(item);
    }

    /// <summary>
    /// Clear any buffers from the writer.
    /// </summary>
    protected virtual void Flush() { }

    protected T[] GetResults()
    {
        return [.. _Result];
    }
}
