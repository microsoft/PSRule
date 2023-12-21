// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Text;
using PSRule.Configuration;
using PSRule.Pipeline;

namespace PSRule;

internal sealed class TestAssertWriter : PipelineWriter
{
    private readonly StringBuilder _Output;

    public TestAssertWriter(PSRuleOption option)
        : base(null, option, null)
    {
        _Output = new StringBuilder();
    }

    public string Output => _Output.ToString();

    public void Clear()
    {
        _Output.Clear();
    }

    public override void WriteHost(HostInformationMessage info)
    {
        if (info.NoNewLine.GetValueOrDefault(false))
            _Output.Append(info.Message);
        else
            _Output.AppendLine(info.Message);
    }
}
