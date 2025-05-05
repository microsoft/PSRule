// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions.Rules;
using PSRule.Rules;

namespace PSRule.Pipeline.Output;

internal sealed class WideOutputWriter : PipelineWriter
{
    internal WideOutputWriter(PipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
        : base(inner, option, shouldProcess) { }

    public override void WriteObject(object sendToPipeline, bool enumerateCollection)
    {
        if (sendToPipeline is InvokeResult result)
            WriteWideObject(result.AsRecord());
        else if (sendToPipeline is IEnumerable<IRuleV1> rules)
            WriteWideObject(rules);
        else
            base.WriteObject(sendToPipeline, enumerateCollection);
    }

    private void WriteWideObject<T>(IEnumerable<T> collection)
    {
        var typeName = string.Concat(typeof(T).FullName, "+Wide");
        foreach (var item in collection)
        {
            var o = PSObject.AsPSObject(item);
            o.TypeNames.Insert(0, typeName);
            base.WriteObject(o, false);

            if (item is RuleRecord record)
                WriteErrorInfo(record);
        }
    }
}
