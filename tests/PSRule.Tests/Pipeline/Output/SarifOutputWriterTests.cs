// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Output;

#nullable enable

/// <summary>
/// Tests for <see cref="SarifOutputWriter"/>.
/// </summary>
public sealed class SarifOutputWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void Output_WhenDefaultOptions_ShouldReturnStandardProperties()
    {
        var option = GetOption();
        var output = new TestWriter(option);
        var invokeResult = new InvokeResult(GetRun());
        invokeResult.Add(GetPass());
        invokeResult.Add(GetFail());

        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(invokeResult, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault()!);
        Assert.NotNull(actual);

        var results = actual["runs"]?[0]?["results"];
        Assert.NotNull(results);

        var driver = actual["runs"]?[0]?["tool"]?["driver"];
        Assert.NotNull(driver);
        Assert.Equal("PSRule", driver["name"]?.Value<string>());
        Assert.Equal("0.0.1", driver["semanticVersion"]?.Value<string>()?.Split('+')[0]);
        Assert.Equal("0130215d-58eb-4887-b6fa-31ed02500569", driver["guid"]?.Value<string>());
        Assert.Equal("Microsoft Corporation", driver["organization"]?.Value<string>());
        Assert.Equal("https://aka.ms/ps-rule", driver["informationUri"]?.Value<string>());

        var automationDetails = actual["runs"]?[0]?["automationDetails"];
        Assert.NotNull(automationDetails);
        Assert.Equal("run-001", automationDetails["id"]?.Value<string>());
        Assert.Equal("00000000-0000-0000-0000-000000000000", automationDetails["correlationGuid"]?.Value<string>());
        Assert.Equal("Test run", automationDetails["description"]?["text"]?.Value<string>());

        var invocations = actual["runs"]?[0]?["invocations"];
        Assert.NotNull(invocations);
        Assert.Single(invocations);
        Assert.True(invocations[0]?["executionSuccessful"]?.Value<bool>());

        var originalUriBaseIds = actual["runs"]?[0]?["originalUriBaseIds"];
        Assert.NotNull(originalUriBaseIds);
        Assert.NotNull(originalUriBaseIds["REPO_ROOT"]);

        var rules = driver["rules"];
        Assert.NotNull(rules);

        // Contain pass rule by no results
        var rule = rules.Where(r => r["id"]?.Value<string>() == "TestModule\\rule-001").FirstOrDefault();
        Assert.NotNull(rule);

        var result = results.Where(r => r["ruleId"]?.Value<string>() == "TestModule\\rule-001").FirstOrDefault();
        Assert.Null(result);

        // Contains fail rule
        rule = rules.Where(r => r["id"]?.Value<string>() == "rid-002").FirstOrDefault();
        Assert.NotNull(rule);

        result = results.Where(r => r["ruleId"]?.Value<string>() == "rid-002").FirstOrDefault();
        Assert.NotNull(result);
    }

    [Fact]
    public void Output_WhenRepositoryOptionSet_ShouldReturnVersionControlProperties()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());

        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault()!);
        Assert.NotNull(actual);

        var driver = actual["runs"]?[0]?["tool"]?["driver"];
        Assert.NotNull(driver);
        Assert.Equal("PSRule", driver["name"]?.Value<string>());
        Assert.Equal("0.0.1", driver["semanticVersion"]?.Value<string>()?.Split('+')[0]);
        Assert.Equal("0130215d-58eb-4887-b6fa-31ed02500569", driver["guid"]?.Value<string>());

        var versionControl = actual["runs"]?[0]?["versionControlProvenance"];
        Assert.NotNull(versionControl);
        Assert.Equal("https://github.com/microsoft/PSRule.UnitTest", versionControl[0]?["repositoryUri"]?.Value<string>());
    }

    [Fact]
    public void Output_WhenDefaultOptions_ShouldNotReturnPassingResults()
    {
        var option = GetOption();
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));

        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault()!);
        Assert.NotNull(actual);

        // Fail with error
        Assert.Equal("rid-002", actual["runs"]?[0]?["results"]?[0]?["ruleId"]?.Value<string>());
        Assert.Equal("error", actual["runs"]?[0]?["results"]?[0]?["level"]?.Value<string>());

        // Fail with warning (default value is omitted)
        Assert.Equal("rid-003", actual["runs"]?[0]?["results"]?[1]?["ruleId"]?.Value<string>());
        Assert.Null(actual["runs"]?[0]?["results"]?[1]?["level"]);

        // Fail with note
        Assert.Equal("rid-004", actual["runs"]?[0]?["results"]?[2]?["ruleId"]?.Value<string>());
        Assert.Equal("note", actual["runs"]?[0]?["results"]?[2]?["level"]?.Value<string>());
    }

    [Fact]
    public void Output_WhenNotProblemsOnly_ShouldReturnAllResults()
    {
        var option = GetOption();
        option.Output.SarifProblemsOnly = false;
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));

        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault()!);
        Assert.NotNull(actual);

        // Pass
        Assert.Equal("TestModule\\rule-001", actual["runs"]?[0]?["results"]?[0]?["ruleId"]?.Value<string>());
        Assert.Equal("none", actual["runs"]?[0]?["results"]?[0]?["level"]?.Value<string>());

        // Fail with error
        Assert.Equal("rid-002", actual["runs"]?[0]?["results"]?[1]?["ruleId"]?.Value<string>());
        Assert.Equal("error", actual["runs"]?[0]?["results"]?[1]?["level"]?.Value<string>());
        Assert.Equal("Custom annotation", actual["runs"]?[0]?["results"]?[1]?["properties"]?["annotations"]?["annotation-data"]?.Value<string>());
        Assert.Equal("Custom field data", actual["runs"]?[0]?["results"]?[1]?["properties"]?["fields"]?["field-data"]?.Value<string>());

        // Fail with warning
        Assert.Equal("rid-003", actual["runs"]?[0]?["results"]?[2]?["ruleId"]?.Value<string>());
        Assert.Null(actual["runs"]?[0]?["results"]?[2]?["level"]);

        // Fail with note
        Assert.Equal("rid-004", actual["runs"]?[0]?["results"]?[3]?["ruleId"]?.Value<string>());
        Assert.Equal("note", actual["runs"]?[0]?["results"]?[3]?["level"]?.Value<string>());

        // Check options
        Assert.Equal(option.Repository.Url, actual["runs"]?[0]?["properties"]?["options"]?["workspace"]?["Repository"]?["Url"]?.Value<string>());
        Assert.False(actual["runs"]?[0]?["properties"]?["options"]?["workspace"]?["Output"]?["SarifProblemsOnly"]?.Value<bool>());
    }

    [Fact]
    public void Output_WhenRuleLevelIsOverridden_ShouldReturnOverrideInSarifFormat()
    {
        var option = GetOption();
        var output = new TestWriter(option);
        var result = new InvokeResult(GetRun());
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning, overrideLevel: SeverityLevel.Information));
        result.Add(GetFail("rid-004", SeverityLevel.Information, overrideLevel: SeverityLevel.Warning));

        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var doc = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault()!);
        Assert.NotNull(doc);

        // Fail with error
        var actual = doc["runs"]?[0]?["results"]?.Where(r => r["ruleId"]?.Value<string>() == "rid-002").FirstOrDefault();
        Assert.NotNull(actual);
        Assert.Equal("error", actual["level"]?.Value<string>());

        // Fail with note
        actual = doc["runs"]?[0]?["results"]?.Where(r => r["ruleId"]?.Value<string>() == "rid-003").FirstOrDefault();
        Assert.NotNull(actual);
        Assert.Equal("note", actual["level"]?.Value<string>());

        var ruleDefault = doc["runs"]?[0]?["tool"]?["driver"]?["rules"]?.Where(r => r["id"]?.Value<string>() == "rid-003").FirstOrDefault();
        Assert.NotNull(ruleDefault);
        Assert.Null(ruleDefault["defaultConfiguration"]);

        var ruleOverride = doc["runs"]?[0]?["invocations"]?[0]?["ruleConfigurationOverrides"]?.Where(r => r["descriptor"]?["id"]?.Value<string>() == "rid-003").FirstOrDefault();
        Assert.Equal("note", actual["level"]?.Value<string>());

        // Fail with warning (default value is omitted)
        actual = doc["runs"]?[0]?["results"]?.Where(r => r["ruleId"]?.Value<string>() == "rid-004").FirstOrDefault();
        Assert.NotNull(actual);
        Assert.Null(actual["level"]);

        ruleDefault = doc["runs"]?[0]?["tool"]?["driver"]?["rules"]?.Where(r => r["id"]?.Value<string>() == "rid-004").FirstOrDefault();
        Assert.NotNull(ruleDefault);
        Assert.Equal("note", ruleDefault["defaultConfiguration"]?["level"]?.Value<string>());

        ruleOverride = doc["runs"]?[0]?["invocations"]?[0]?["ruleConfigurationOverrides"]?.Where(r => r["descriptor"]?["id"]?.Value<string>() == "rid-004").FirstOrDefault();
        Assert.NotNull(ruleOverride);
        Assert.Null(actual["level"]);
    }
}

#nullable restore
