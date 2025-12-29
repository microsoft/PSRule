// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline;

internal abstract class SerializationOutputWriter<T>(IPipelineWriter inner, PSRuleOption option, ShouldProcess? shouldProcess)
    : ResultOutputWriter<T>(inner, option, shouldProcess)
{
    public sealed override void End(IPipelineResult result)
    {
        var results = GetResults();
        base.WriteObject(Serialize(results), false);
        ProcessError(results);
        Flush();
        base.End(result);
    }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is InvokeResult && Option.Output.As == ResultFormat.Summary)
        {
            base.WriteObject(sendToPipeline, enumerateCollection);
            return;
        }

        if (sendToPipeline is InvokeResult result)
        {
            Add(result.AsRecord());
            return;
        }
        Add(sendToPipeline);
    }

    protected abstract string Serialize(T[] o);

    private void ProcessError(T[] results)
    {
        for (var i = 0; i < results.Length; i++)
        {
            if (results[i] is RuleRecord record)
                WriteErrorInfo(record);
        }
    }
}
