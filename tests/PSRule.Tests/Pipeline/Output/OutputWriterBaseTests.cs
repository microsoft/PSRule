// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline.Runs;
using PSRule.Rules;

namespace PSRule.Pipeline.Output;

public abstract class OutputWriterBaseTests : ContextBaseTests
{
    protected internal static IRun GetRun()
    {
        return new Run("run-001", new InfoString("Test run", null), Guid.Empty.ToString(), new EmptyRuleGraph());
    }

    protected static RuleRecord GetPass()
    {
        var run = GetRun();
        return new RuleRecord
        (
            ruleId: ResourceId.Parse("TestModule\\rule-001"),
            @ref: null,
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject1",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: new RuleHelpInfo
            (
                "rule-001",
                "Rule 001",
                "TestModule",
                synopsis: new InfoString("This is rule 001."),
                recommendation: new InfoString("Recommendation for rule 001\r\nover two lines.")
            ),
            field: [],
            @default: new RuleProperties
            {
                Level = SeverityLevel.Error
            },
            extent: null,
            outcome: RuleOutcome.Pass,
            reason: RuleOutcomeReason.Processed
        )
        {
            RunId = run.Id,
            Time = 500
        };
    }

    protected static RuleRecord GetFail(string ruleRef = "rid-002", SeverityLevel level = SeverityLevel.Error, SeverityLevel? overrideLevel = null, string synopsis = "This is rule 002.", string ruleId = "TestModule\\rule-002")
    {
        var run = GetRun();
        var info = new RuleHelpInfo(
            "rule-002",
            "Rule 002",
            "TestModule",
            synopsis: new InfoString(synopsis),
            recommendation: new InfoString("Recommendation for rule 002")
        );

        info.Annotations = new Hashtable
        {
            ["annotation-data"] = "Custom annotation"
        };

        var ruleOverride = overrideLevel == null ? null : new RuleOverride
        {
            Level = overrideLevel,
        };

        return new RuleRecord(
            ruleId: ResourceId.Parse(ruleId),
            @ref: ruleRef,
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject1",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: info,
            field: new Hashtable
            {
                ["field-data"] = "Custom field data"
            },
            @default: new RuleProperties
            {
                Level = level
            },
            @override: ruleOverride,
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
