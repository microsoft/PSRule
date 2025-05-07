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

public sealed class YamlRulesTests : ContextBaseTests
{
    /// <summary>
    /// Test that a YAML-based rule can be parsed.
    /// </summary>
    [Fact]
    public void GetRule_FromCurrentDirectory_ShouldReturnRules()
    {
        var sources = GetSource("FromFile.Rule.yaml");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources));
        context.Initialize(sources);
        context.Begin();

        // From current path
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Equal("YamlBasicRule", rule[0].Name);
        Assert.Equal(Environment.GetRootedPath(""), rule[0].Source.HelpPath);
        Assert.Equal(10, rule[0].Extent.Line);

        var block = HostHelper.GetRuleBlockGraph(context).GetAll();
        var actual = block.FirstOrDefault(b => b.Name == "YamlBasicRule");
        Assert.NotNull(actual.Info.Annotations);
        Assert.Equal("test123", actual.Info.Annotations["test_value"]);
        Assert.Equal("Basic YAML rule", actual.Info.DisplayName);
        Assert.Equal("This is a description of a basic rule.", actual.Info.Description);
        Assert.Equal("A YAML rule recommendation for testing.", actual.Info.Recommendation);
        Assert.Equal("https://aka.ms/ps-rule", actual.Info.GetOnlineHelpUrl());
    }

    [Fact]
    public void GetRule_WithRelativePath_ShouldReturnRules()
    {
        var sources = GetSource("../../../FromFile.Rule.yaml");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources));
        context.Initialize(sources);
        context.Begin();

        // From relative path
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Equal("YamlBasicRule", rule[0].Name);
        Assert.Equal(Environment.GetRootedPath("../../.."), rule[0].Source.HelpPath);

        var hashtable = rule[0].Tag.ToHashtable();
        Assert.Equal("tag", hashtable["feature"]);

        var block = HostHelper.GetRuleBlockGraph(context).GetAll();
        var actual = block.FirstOrDefault(b => b.Name == "YamlBasicRule");
        Assert.NotNull(actual.Info.Annotations);
        Assert.Equal("test123", actual.Info.Annotations["test_value"]);
        Assert.Equal("Basic YAML rule", actual.Info.DisplayName);
        Assert.Equal("This is a description of a basic rule.", actual.Info.Description);
        Assert.Equal("A YAML rule recommendation for testing.", actual.Info.Recommendation);
        Assert.Equal("https://aka.ms/ps-rule", actual.Info.GetOnlineHelpUrl());
    }

    /// <summary>
    /// Test that a YAML-based rule with sub-selectors can be parsed.
    /// </summary>
    [Fact]
    public void ReadYamlSubSelectorRule()
    {
        var sources = GetSource("FromFileSubSelector.Rule.yaml");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, optionBuilder: GetOptionBuilder()));
        context.Initialize(sources);
        context.Begin();

        // From current path
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Equal("YamlRuleWithPrecondition", rule[0].Name);
        Assert.Equal("YamlRuleWithSubselector", rule[1].Name);
        Assert.Equal("YamlRuleWithSubselectorReordered", rule[2].Name);
        Assert.Equal("YamlRuleWithQuantifier", rule[3].Name);

        context.Initialize(sources);
        context.Begin();
        var subselector1 = GetRuleVisitor(context, "YamlRuleWithPrecondition");
        var subselector2 = GetRuleVisitor(context, "YamlRuleWithSubselector");
        var subselector3 = GetRuleVisitor(context, "YamlRuleWithSubselectorReordered");
        var subselector4 = GetRuleVisitor(context, "YamlRuleWithQuantifier");
        context.EnterLanguageScope(subselector1.Source);

        var actual1 = GetObject((name: "kind", value: "test"), (name: "resources", value: new string[] { "abc", "abc" }));
        var actual2 = GetObject((name: "resources", value: new string[] { "abc", "123", "abc" }));

        // YamlRuleWithPrecondition
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector1);
        Assert.True(subselector1.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector1);
        Assert.True(subselector1.Condition.If().Skipped());

        // YamlRuleWithSubselector
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector2);
        Assert.True(subselector2.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector2);
        Assert.False(subselector2.Condition.If().AllOf());

        // YamlRuleWithSubselectorReordered
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(subselector3);
        Assert.True(subselector3.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(subselector3);
        Assert.True(subselector3.Condition.If().AllOf());

        // YamlRuleWithQuantifier
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

    [Fact]
    public void EvaluateYamlRule()
    {
        var sources = GetSource("FromFile.Rule.yaml");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, optionBuilder: GetOptionBuilder()));
        context.Initialize(sources);
        context.Begin();

        var yamlTrue = GetRuleVisitor(context, "RuleYamlTrue");
        var yamlFalse = GetRuleVisitor(context, "RuleYamlFalse");
        var customType = GetRuleVisitor(context, "RuleYamlWithCustomType");
        var withSelector = GetRuleVisitor(context, "RuleYamlWithSelector");
        context.EnterLanguageScope(yamlTrue.Source);

        var actual1 = GetObject((name: "value", value: 3));
        var actual2 = GetObject((name: "notValue", value: 3));
        var actual3 = GetObject((name: "value", value: 4));

        if (actual3.Value is PSObject pso)
            pso.TypeNames.Insert(0, "CustomType");

        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(yamlTrue);
        Assert.True(yamlTrue.Condition.If().AllOf());
        context.EnterRuleBlock(yamlFalse);
        Assert.False(yamlFalse.Condition.If().AllOf());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(yamlTrue);
        Assert.False(yamlTrue.Condition.If().AllOf());
        context.EnterRuleBlock(yamlFalse);
        Assert.False(yamlFalse.Condition.If().AllOf());

        context.EnterTargetObject(actual3);
        context.EnterRuleBlock(yamlTrue);
        Assert.False(yamlTrue.Condition.If().AllOf());
        context.EnterRuleBlock(yamlFalse);
        Assert.True(yamlFalse.Condition.If().AllOf());

        // With type pre-condition
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(customType);
        Assert.Null(customType.Condition.If());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(customType);
        Assert.Null(customType.Condition.If());

        context.EnterTargetObject(actual3);
        context.EnterRuleBlock(customType);
        Assert.NotNull(customType.Condition.If());

        // With selector pre-condition
        context.EnterTargetObject(actual1);
        context.EnterRuleBlock(withSelector);
        Assert.Null(withSelector.Condition.If());

        context.EnterTargetObject(actual2);
        context.EnterRuleBlock(withSelector);
        Assert.NotNull(withSelector.Condition.If());

        context.EnterTargetObject(actual3);
        context.EnterRuleBlock(withSelector);
        Assert.Null(withSelector.Condition.If());
    }

    [Fact]
    public void RuleWithObjectPath()
    {
        var sources = GetSource("FromFile.Rule.yaml");
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, optionBuilder: GetOptionBuilder()));
        context.Initialize(sources);
        context.Begin();

        var yamlObjectPath = GetRuleVisitor(context, "YamlObjectPath");
        context.EnterLanguageScope(yamlObjectPath.Source);

        var actual = GetObject(GetSourcePath("ObjectFromFile3.json"));

        context.EnterTargetObject(new TargetObject(new PSObject(actual[0])));
        context.EnterRuleBlock(yamlObjectPath);
        Assert.True(yamlObjectPath.Condition.If().AllOf());

        context.EnterTargetObject(new TargetObject(new PSObject(actual[1])));
        context.EnterRuleBlock(yamlObjectPath);
        Assert.False(yamlObjectPath.Condition.If().AllOf());

        context.EnterTargetObject(new TargetObject(new PSObject(actual[2])));
        context.EnterRuleBlock(yamlObjectPath);
        Assert.True(yamlObjectPath.Condition.If().AllOf());
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

    private static object[] GetObject(string path)
    {
        return JsonConvert.DeserializeObject<object[]>(File.ReadAllText(path));
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
