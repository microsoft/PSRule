// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Rules;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Pipeline.Output;
using PSRule.Runtime;
using YamlDotNet.Serialization;

namespace PSRule;

#nullable enable

public sealed class BaselineTests : ContextBaseTests
{
    private const string BaselineYamlFileName = "Baseline.Rule.yaml";
    private const string BaselineJsonFileName = "Baseline.Rule.jsonc";

    [Fact]
    public void ReadBaselineYaml()
    {
        var baseline = GetBaselines(GetSource(BaselineYamlFileName));
        Assert.NotNull(baseline);
        Assert.Equal(6, baseline.Length);

        // TestBaseline1
        Assert.Equal("TestBaseline1", baseline[0].Name);
        Assert.Equal("github.com/microsoft/PSRule/v1", baseline[0].ApiVersion);
        Assert.Equal("value", baseline[0].Metadata.Annotations["key"]);
        Assert.False(baseline[0].Obsolete);
        Assert.Equal("This is an example baseline", baseline[0].Info.Synopsis.Text);

        var config = baseline[0].Spec.Configuration["key2"] as Array;
        Assert.NotNull(config);
        Assert.Equal(2, config.Length);
        Assert.IsType<PSObject>(config.GetValue(0));
        var pso = config.GetValue(0) as PSObject;
        Assert.Equal("abc", pso.PropertyValue<string>("value1"));
        pso = config.GetValue(1) as PSObject;
        Assert.Equal("def", pso.PropertyValue<string>("value2"));

        // TestBaseline4
        Assert.Equal("TestBaseline4", baseline[3].Name);
        Assert.Null(baseline[3].Info.Synopsis.Text);
        Assert.NotNull(baseline[3].Spec);
        Assert.Equal(SeverityLevel.Warning, baseline[3].Spec.Override?.Level?["rule1"]);

        // TestBaseline5
        Assert.Equal("TestBaseline5", baseline[4].Name);
        Assert.Equal("github.com/microsoft/PSRule/v1", baseline[4].ApiVersion);
        Assert.True(baseline[4].Obsolete);
        Assert.Equal("This is an example obsolete baseline", baseline[4].Info.Synopsis.Text);

        // TestBaseline6
        Assert.Equal("TestBaseline6", baseline[5].Name);
        var labels = baseline[5].Spec.Rule.Labels;
        Assert.True(labels.Contains("framework.v1/control", ["*"]));
        Assert.True(labels.Contains("framework.v1/control", ["c-1"]));
        Assert.False(labels.Contains("framework.v1/control", ["c-3"]));
        Assert.False(labels.Contains("framework.v3/control", ["*"]));
    }

    [Fact]
    public void ReadBaselineJson()
    {
        var baseline = GetBaselines(GetSource(BaselineJsonFileName));
        Assert.NotNull(baseline);
        Assert.Equal(6, baseline.Length);

        // TestBaseline1
        Assert.Equal("TestBaseline1", baseline[0].Name);
        Assert.Equal("github.com/microsoft/PSRule/v1", baseline[0].ApiVersion);
        Assert.Equal("value", baseline[0].Metadata.Annotations["key"]);
        Assert.False(baseline[0].Obsolete);
        Assert.Equal("This is an example baseline", baseline[0].Info.Synopsis.Text);

        var config = (JArray)baseline[0].Spec.Configuration["key2"];
        Assert.NotNull(config);
        Assert.Equal(2, config.Count);
        Assert.True(config[0].Type == JTokenType.Object);
        Assert.Equal("abc", config[0]["value1"]);
        Assert.True(config[1].Type == JTokenType.Object);
        Assert.Equal("def", config[1]["value2"]);

        // TestBaseline4
        Assert.Equal("TestBaseline4", baseline[3].Name);
        Assert.Null(baseline[3].Info.Synopsis.Text);
        Assert.NotNull(baseline[3].Spec.Override.Level);
        Assert.True(baseline[3].Spec.Override.Level!.TryGetValue("rule1", out var level));
        Assert.Equal(SeverityLevel.Warning, level);

        // TestBaseline5
        Assert.Equal("TestBaseline5", baseline[4].Name);
        Assert.Equal("github.com/microsoft/PSRule/v1", baseline[4].ApiVersion);
        Assert.True(baseline[4].Obsolete);
        Assert.Equal("This is an example obsolete baseline", baseline[4].Info.Synopsis.Text);

        // TestBaseline6
        Assert.Equal("TestBaseline6", baseline[5].Name);
        var labels = baseline[5].Spec.Rule.Labels;
        Assert.True(labels.Contains("framework.v1/control", ["*"]));
        Assert.True(labels.Contains("framework.v1/control", ["c-1"]));
        Assert.False(labels.Contains("framework.v1/control", ["c-3"]));
        Assert.False(labels.Contains("framework.v3/control", ["*"]));
    }

    [Theory]
    [InlineData(SourceType.Yaml, BaselineYamlFileName)]
    [InlineData(SourceType.Json, BaselineJsonFileName)]
    public void ReadBaselineInModule(SourceType type, string path)
    {
        var baseline = GetBaselines(GetSourceInModule(path, "TestModule", type));
        Assert.NotNull(baseline);
        Assert.Equal(6, baseline.Length);

        // TestBaseline1
        Assert.Equal("TestBaseline1", baseline[0].Name);
        Assert.Equal("github.com/microsoft/PSRule/v1", baseline[0].ApiVersion);
        Assert.Equal("value", baseline[0].Metadata.Annotations["key"]);
        Assert.False(baseline[0].Obsolete);
    }

    [Theory]
    [InlineData(BaselineYamlFileName)]
    [InlineData(BaselineJsonFileName)]
    public void FilterBaseline(string path)
    {
        var baseline = GetBaselines(GetSource(path));
        Assert.NotNull(baseline);

        var filter = new BaselineFilter(["TestBaseline5"]);
        var actual = baseline.FirstOrDefault(b => filter.Match(b));

        Assert.NotNull(actual);
        Assert.Equal("TestBaseline5", actual.Name);
    }

    [Fact]
    public void BaselineAsYaml()
    {
        var expected = GetBaselines(GetSource(BaselineYamlFileName));
        var s = YamlOutputWriter.ToYaml(expected);
        var d = new DeserializerBuilder().Build();
        var actual = d.Deserialize<dynamic>(s);

        // TestBaseline1
        Assert.Equal("github.com/microsoft/PSRule/v1", actual[0]["apiVersion"]);
        Assert.Equal("Baseline", actual[0]["kind"]);
        Assert.Equal("TestBaseline1", actual[0]["metadata"]["name"]);
        Assert.NotNull(actual[0]["spec"]);
        Assert.Equal("WithBaseline", actual[0]["spec"]["rule"]["include"][0]);
        Assert.Equal("value1", actual[0]["spec"]["configuration"]["key1"]);
        Assert.Equal("abc", actual[0]["spec"]["configuration"]["key2"][0]["value1"]);
        Assert.Equal("def", actual[0]["spec"]["configuration"]["key2"][1]["value2"]);

        // TestBaseline4
        Assert.Equal("github.com/microsoft/PSRule/v1", actual[3]["apiVersion"]);
        Assert.Equal("Baseline", actual[3]["kind"]);
        Assert.Equal("TestBaseline4", actual[3]["metadata"]["name"]);
        Assert.NotNull(actual[3]["spec"]);
        Assert.Equal("Warning", actual[3]["spec"]["override"]["level"]["rule1"]);
    }

    [Fact]
    public void BaselineAsJson()
    {
        var expected = new object[] { GetBaselines(GetSource(BaselineJsonFileName)) };
        var json = JsonOutputWriter.ToJson(expected, null);

        Assert.NotNull(json);
        var actual = (JArray)JsonConvert.DeserializeObject<dynamic>(json)!;

        // TestBaseline1
        Assert.Equal("github.com/microsoft/PSRule/v1", actual?[0]["apiVersion"]);
        Assert.Equal("Baseline", actual?[0]["kind"]);
        Assert.Equal("TestBaseline1", actual?[0]["metadata"]?["name"]);
        Assert.NotNull(actual?[0]["spec"]);
        Assert.Equal("WithBaseline", actual?[0]["spec"]?["rule"]?["include"]?[0]);
        Assert.Equal("value1", actual?[0]["spec"]?["configuration"]?["key1"]);
        Assert.Equal("abc", actual?[0]["spec"]?["configuration"]?["key2"]?[0]?["value1"]);
        Assert.Equal("def", actual?[0]["spec"]?["configuration"]?["key2"]?[1]?["value2"]);

        // TestBaseline4
        Assert.Equal("github.com/microsoft/PSRule/v1", actual?[3]["apiVersion"]);
        Assert.Equal("Baseline", actual?[3]["kind"]);
        Assert.Equal("TestBaseline4", actual?[3]["metadata"]?["name"]);
        Assert.NotNull(actual?[3]["spec"]);
        Assert.Equal("Warning", actual?[3]["spec"]?["override"]?["level"]?["rule1"]);
    }

    #region Helper methods

    private Baseline[] GetBaselines(Source[] sources)
    {
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources));
        context.Initialize(sources);
        context.Begin();
        var baseline = HostHelper.GetBaseline(sources, context).ToArray();
        return baseline;
    }

    private static Source[] GetSourceInModule(string path, string moduleName, SourceType type)
    {
        var file = new SourceFile(GetSourcePath(path), moduleName, type, null);
        var source = new Source(
            module: new Source.ModuleInfo(AppDomain.CurrentDomain.BaseDirectory, moduleName, "1.0.0", null, "4de0fd26-6aae-401f-a943-b49f082f141e", "Microsoft", null, []),
            file: [file],
            dependency: false
        );
        return [source];
    }

    #endregion Helper methods
}

#nullable restore
