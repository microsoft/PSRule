// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule;

public sealed class JsonRulesTests : ContextBaseTests
{
    /// <summary>
    /// Test that a JSON-based rule can be parsed.
    /// </summary>
    [Fact]
    public void GetRule_FromCurrentDirectory_ShouldReturnRules()
    {
        var sources = GetSource("FromFile.Rule.jsonc");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources));
        context.Initialize(sources);
        context.Begin();

        // From current path
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Equal("JsonBasicRule", rule[0].Name);
        Assert.Equal(Environment.GetRootedPath(""), rule[0].Source.HelpPath);
        Assert.Equal(7, rule[0].Extent.Line);

        var block = HostHelper.GetRuleBlockGraph(context).GetAll();
        var actual = block.FirstOrDefault(b => b.Name == "JsonBasicRule");
        Assert.NotNull(actual.Info.Annotations);
        Assert.Equal("test123", actual.Info.Annotations["test_value"]);
        Assert.Equal("Basic JSON rule", actual.Info.DisplayName);
        Assert.Equal("This is a description of a basic rule.", actual.Info.Description);
        Assert.Equal("A JSON rule recommendation for testing.", actual.Info.Recommendation);
        Assert.Equal("https://aka.ms/ps-rule", actual.Info.GetOnlineHelpUrl());
    }

    [Fact]
    public void GetRule_WithRelativePath_ShouldReturnRules()
    {
        var sources = GetSource("../../../FromFile.Rule.jsonc");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources));
        context.Initialize(sources);
        context.Begin();

        // From relative path
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Equal("JsonBasicRule", rule[0].Name);
        Assert.Equal(Environment.GetRootedPath("../../.."), rule[0].Source.HelpPath);

        var hashtable = rule[0].Tag.ToHashtable();
        Assert.Equal("tag", hashtable["feature"]);

        var block = HostHelper.GetRuleBlockGraph(context).GetAll();
        var actual = block.FirstOrDefault(b => b.Name == "JsonBasicRule");
        Assert.NotNull(actual.Info.Annotations);
        Assert.Equal("test123", actual.Info.Annotations["test_value"]);
        Assert.Equal("Basic JSON rule", actual.Info.DisplayName);
        Assert.Equal("This is a description of a basic rule.", actual.Info.Description);
        Assert.Equal("A JSON rule recommendation for testing.", actual.Info.Recommendation);
        Assert.Equal("https://aka.ms/ps-rule", actual.Info.GetOnlineHelpUrl());
    }

    /// <summary>
    /// Test that a JSON-based rule with sub-selectors can be parsed.
    /// </summary>
    [Fact]
    public void ReadJsonSubSelectorRule()
    {
        var sources = GetSource("FromFileSubSelector.Rule.jsonc");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, optionBuilder: GetOptionBuilder()));
        context.Initialize(sources);
        context.Begin();

        // From current path
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Equal("JsonRuleWithPrecondition", rule[0].Name);
        Assert.Equal("JsonRuleWithSubselector", rule[1].Name);
        Assert.Equal("JsonRuleWithSubselectorReordered", rule[2].Name);
        Assert.Equal("JsonRuleWithQuantifier", rule[3].Name);

        context.Initialize(GetSource("FromFileSubSelector.Rule.yaml"));
        context.Begin();
        var subselector1 = GetRuleVisitor(context, "JsonRuleWithPrecondition");
        var subselector2 = GetRuleVisitor(context, "JsonRuleWithSubselector");
        var subselector3 = GetRuleVisitor(context, "JsonRuleWithSubselectorReordered");
        var subselector4 = GetRuleVisitor(context, "JsonRuleWithQuantifier");
        context.EnterLanguageScope(subselector1.Source);

        var actual1 = GetObject((name: "kind", value: "test"), (name: "resources", value: new string[] { "abc", "abc" }));
        var actual2 = GetObject((name: "resources", value: new string[] { "abc", "123", "abc" }));

        // JsonRuleWithPrecondition
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector1);
        Assert.True(subselector1.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector1);
        Assert.True(subselector1.Condition.If().Skipped());

        // JsonRuleWithSubselector
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector2);
        Assert.True(subselector2.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector2);
        Assert.False(subselector2.Condition.If().AllOf());

        // JsonRuleWithSubselectorReordered
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector3);
        Assert.True(subselector3.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector3);
        Assert.True(subselector3.Condition.If().AllOf());

        // JsonRuleWithQuantifier
        var fromFile = GetObjectAsTarget("ObjectFromFile3.json");
        actual1 = fromFile[0];
        actual2 = fromFile[1];
        var actual3 = fromFile[2];

        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector4);
        Assert.True(subselector4.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector4);
        Assert.False(subselector4.Condition.If().AllOf());

        context.EnterTargetObject(actual3);
        context.EnterRuleBlock(subselector4);
        Assert.True(subselector4.Condition.If().AllOf());
    }

    #region Helper methods

    private new static Source[] GetSource(string path)
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(path));
        return builder.Build();
    }

    private new static TargetObject GetObject(params (string name, object value)[] properties)
    {
        var result = new PSObject();
        for (var i = 0; properties != null && i < properties.Length; i++)
            result.Properties.Add(new PSNoteProperty(properties[i].name, properties[i].value));

        return new TargetObject(result);
    }

    private static TargetObject[] GetObjectAsTarget(string path)
    {
        return JsonConvert.DeserializeObject<object[]>(File.ReadAllText(path)).Select(o => new TargetObject(new PSObject(o))).ToArray();
    }

    private static RuleBlock GetRuleVisitor(LegacyRunspaceContext context, string name)
    {
        var block = HostHelper.GetRuleBlockGraph(context).GetAll();
        return block.FirstOrDefault(s => s.Name == name);
    }

    #endregion Helper methods
}
