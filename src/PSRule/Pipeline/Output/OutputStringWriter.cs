// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using PSRule.Configuration;

namespace PSRule.Pipeline.Output
{
    internal sealed class OutputStringWriter : StringWriter
    {
        private readonly Encoding _Encoding;

        public OutputStringWriter(PSRuleOption option)
        {
            _Encoding = option.Output.GetEncoding();
        }

        public override Encoding Encoding => _Encoding;
    }
}
