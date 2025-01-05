// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Output;

/// <summary>
/// Tests for <see cref="CsvOutputWriter"/>.
/// </summary>
public sealed class CsvOutputWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void Csv()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new CsvOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = output.Output.OfType<string>().FirstOrDefault();

        Assert.Equal(@"RuleName,TargetName,TargetType,Outcome,Synopsis,Recommendation
""rule-001"",""TestObject1"",""TestType"",""Pass"",""Processed"",""This is rule 001."",""Recommendation for rule 001 over two lines.""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
", actual);
    }
}
