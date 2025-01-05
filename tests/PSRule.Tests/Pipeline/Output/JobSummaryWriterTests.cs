// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Output;

/// <summary>
/// Tests for <see cref="JobSummaryWriter"/>.
/// </summary>
public sealed class JobSummaryWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void JobSummary()
    {
        using var stream = new MemoryStream();
        var option = GetOption();
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        var context = GetPipelineContext(option: option, writer: output, resourceCache: GetResourceCache());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning, ruleId: "TestModule\\Rule-003"));
        var writer = new JobSummaryWriter(output, option, null, outputPath: "reports/summary.md", stream: stream);
        writer.Begin();
        writer.WriteObject(result, false);
        context.RunTime.Stop();
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var s = reader.ReadToEnd().Replace(System.Environment.NewLine, "\r\n");
        Assert.Equal($"# PSRule result summary\r\n\r\n❌ PSRule completed with an overall result of 'Fail' with 3 rule(s) and 1 target(s) in {context.RunTime.Elapsed}.\r\n\r\n## Analysis\r\n\r\nThe following results were reported with fail or error results.\r\n\r\nName | Target name | Synopsis\r\n---- | ----------- | --------\r\nrule-002 | TestObject1 | This is rule 002.\r\nRule-003 | TestObject1 | This is rule 002.\r\n", s);
    }
}
