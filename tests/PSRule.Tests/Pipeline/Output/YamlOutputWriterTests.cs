// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Output;

/// <summary>
/// Tests for <see cref="YamlOutputWriter"/>.
/// </summary>
public sealed class YamlOutputWriterTests : OutputWriterBaseTests
{

    [Fact]
    public void Yaml()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new YamlOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        Assert.Equal(@"- detail:
    reason: []
  info:
    moduleName: TestModule
    recommendation: >-
      Recommendation for rule 001

      over two lines.
  level: Error
  outcome: Pass
  outcomeReason: Processed
  ruleName: rule-001
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 500
- detail:
    reason: []
  field:
    field-data: Custom field data
  info:
    annotations:
      annotation-data: Custom annotation
    moduleName: TestModule
    recommendation: Recommendation for rule 002
  level: Error
  outcome: Fail
  outcomeReason: Processed
  ref: rid-002
  ruleName: rule-002
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 1000
- detail:
    reason: []
  field:
    field-data: Custom field data
  info:
    annotations:
      annotation-data: Custom annotation
    moduleName: TestModule
    recommendation: Recommendation for rule 002
  level: Warning
  outcome: Fail
  outcomeReason: Processed
  ref: rid-003
  ruleName: rule-002
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 1000
- detail:
    reason: []
  field:
    field-data: Custom field data
  info:
    annotations:
      annotation-data: Custom annotation
    moduleName: TestModule
    recommendation: Recommendation for rule 002
  level: Information
  outcome: Fail
  outcomeReason: Processed
  ref: rid-004
  ruleName: rule-002
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 1000
", output.Output.OfType<string>().FirstOrDefault());
    }

}
