// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Output;

/// <summary>
/// Tests for <see cref="JsonOutputWriter"/>.
/// </summary>
public sealed class JsonOutputWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void Output_WithIndent_ShouldReturnIndentedResults()
    {
        var option = GetOption();
        option.Output.JsonIndent = 2;
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));

        var writer = new JsonOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        Assert.Equal(@"[
  {
    ""detail"": {},
    ""info"": {
      ""displayName"": ""Rule 001"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-001"",
      ""recommendation"": ""Recommendation for rule 001\r\nover two lines."",
      ""synopsis"": ""This is rule 001.""
    },
    ""level"": ""Error"",
    ""outcome"": ""Pass"",
    ""outcomeReason"": ""Processed"",
    ""ruleName"": ""rule-001"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 500
  },
  {
    ""detail"": {},
    ""field"": {
      ""field-data"": ""Custom field data""
    },
    ""info"": {
      ""annotations"": {
        ""annotation-data"": ""Custom annotation""
      },
      ""displayName"": ""Rule 002"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-002"",
      ""recommendation"": ""Recommendation for rule 002"",
      ""synopsis"": ""This is rule 002.""
    },
    ""level"": ""Error"",
    ""outcome"": ""Fail"",
    ""outcomeReason"": ""Processed"",
    ""ref"": ""rid-002"",
    ""ruleName"": ""rule-002"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 1000
  },
  {
    ""detail"": {},
    ""field"": {
      ""field-data"": ""Custom field data""
    },
    ""info"": {
      ""annotations"": {
        ""annotation-data"": ""Custom annotation""
      },
      ""displayName"": ""Rule 002"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-002"",
      ""recommendation"": ""Recommendation for rule 002"",
      ""synopsis"": ""This is rule 002.""
    },
    ""level"": ""Warning"",
    ""outcome"": ""Fail"",
    ""outcomeReason"": ""Processed"",
    ""ref"": ""rid-003"",
    ""ruleName"": ""rule-002"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 1000
  },
  {
    ""detail"": {},
    ""field"": {
      ""field-data"": ""Custom field data""
    },
    ""info"": {
      ""annotations"": {
        ""annotation-data"": ""Custom annotation""
      },
      ""displayName"": ""Rule 002"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-002"",
      ""recommendation"": ""Recommendation for rule 002"",
      ""synopsis"": ""This is rule 002.""
    },
    ""level"": ""Information"",
    ""outcome"": ""Fail"",
    ""outcomeReason"": ""Processed"",
    ""ref"": ""rid-004"",
    ""ruleName"": ""rule-002"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 1000
  }
]", output.Output.OfType<string>().FirstOrDefault());
    }

    [Fact]
    public void Output_WithNoRecords_ShouldReturnEmptyArray()
    {
        var option = GetOption();
        option.Output.JsonIndent = 2;
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        var writer = new JsonOutputWriter(output, option, null);

        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        Assert.Equal("[]", output.Output.OfType<string>().FirstOrDefault());
    }
}
