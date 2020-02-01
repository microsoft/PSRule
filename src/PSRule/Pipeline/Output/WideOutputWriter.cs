// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline.Output
{
    internal sealed class WideOutputWriter : PipelineWriter
    {
        internal WideOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option) { }

        public override void WriteObject(object o, bool enumerate)
        {
            if (o is InvokeResult result)
                WriteWideObject(result.AsRecord());
            else if (o is IEnumerable<Rule> rules)
                WriteWideObject(rules);
            else
                base.WriteObject(o, enumerate);
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
}
