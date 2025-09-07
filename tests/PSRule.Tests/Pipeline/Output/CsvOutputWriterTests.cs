// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Linq;
using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Rules;

namespace PSRule.Pipeline.Output;

/// <summary>
/// Tests for <see cref="CsvOutputWriter"/>.
/// </summary>
public sealed class CsvOutputWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void Output_WithDefaults_ShouldReturnRows()
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

        Assert.Equal(@"RuleName,TargetName,TargetType,Outcome,OutcomeReason,Synopsis,Recommendation
""rule-001"",""TestObject1"",""TestType"",""Pass"",""Processed"",""This is rule 001."",""Recommendation for rule 001 over two lines.""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
", actual);
    }

    [Fact]
    public void Output_WithCustomColumns_ShouldReturnSpecifiedColumns()
    {
        var option = GetOption();
        option.Output.CsvDetailedColumns = ["RuleName", "TargetName", "Outcome", "Synopsis", "Info.Recommendation", "Info.Annotations.severity", "Field.resourceGroup", "Tag.category"];
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFailWithCustomProperties());
        var writer = new CsvOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = output.Output.OfType<string>().FirstOrDefault();

        Assert.Equal(@"RuleName,TargetName,Outcome,Synopsis,Info.Recommendation,Info.Annotations.severity,Field.resourceGroup,Tag.category
""rule-001"",""TestObject1"",""Pass"",""This is rule 001."",""Recommendation for rule 001 over two lines."",,,
""rule-003"",""TestObject1"",""Fail"",""This is rule 003."",""Recommendation for rule 003"",""High"",""myResourceGroup"",""security""
", actual);
    }

    private static RuleRecord GetFailWithCustomProperties()
    {
        var run = GetRun();
        var info = new RuleHelpInfo(
            "rule-003",
            "Rule 003",
            "TestModule",
            synopsis: new InfoString("This is rule 003."),
            recommendation: new InfoString("Recommendation for rule 003")
        );

        info.Annotations = new Hashtable
        {
            ["severity"] = "High"
        };

        var tags = new ResourceTags();
        tags["category"] = "security";

        return new RuleRecord(
            ruleId: ResourceId.Parse("TestModule\\rule-003"),
            @ref: "rid-003",
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject1",
            targetType: "TestType",
            tag: tags,
            info: info,
            field: new Hashtable
            {
                ["resourceGroup"] = "myResourceGroup"
            },
            @default: new RuleProperties
            {
                Level = SeverityLevel.Error
            },
            extent: null,
            outcome: RuleOutcome.Fail,
            reason: RuleOutcomeReason.Processed
        )
        {
            RunId = run.Id,
            Time = 1000
        };
    }
}
