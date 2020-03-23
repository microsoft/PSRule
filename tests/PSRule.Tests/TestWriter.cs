// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline;
using System.Collections.Generic;

namespace PSRule
{
    internal sealed class TestWriter : PipelineWriter
    {
        internal List<string> Warnings = new List<string>();

        public TestWriter(PSRuleOption option)
            : base(null, option) { }

        public override void WriteWarning(string message)
        {
            Warnings.Add(message);
        }

        public override bool ShouldWriteWarning()
        {
            return true;
        }
    }
}
