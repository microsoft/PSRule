// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Pipeline;

namespace PSRule
{
    internal sealed class TestWriter : PipelineWriter
    {
        internal List<ErrorRecord> Errors = new();
        internal List<string> Warnings = new();
        internal List<object> Information = new();
        internal List<object> Output = new();

        public TestWriter(PSRuleOption option)
            : base(null, option, null) { }

        public override void WriteError(ErrorRecord errorRecord)
        {
            Errors.Add(errorRecord);
        }

        public override void WriteWarning(string message)
        {
            Warnings.Add(message);
        }

        public override bool ShouldWriteError()
        {
            return true;
        }

        public override bool ShouldWriteWarning()
        {
            return true;
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (sendToPipeline == null)
                return;

            if (enumerateCollection && sendToPipeline is IEnumerable<object> enumerable)
            {
                Output.AddRange(enumerable);
            }
            else
            {
                Output.Add(sendToPipeline);
            }
        }

        public override bool ShouldWriteInformation()
        {
            return true;
        }

        public override void WriteInformation(InformationRecord informationRecord)
        {
            Information.Add(informationRecord.MessageData);
        }
    }
}
