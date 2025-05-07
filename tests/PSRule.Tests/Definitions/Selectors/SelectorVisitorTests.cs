// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions.Selectors;

/// <summary>
/// Tests for <see cref="SelectorVisitor"/>.
/// </summary>
public sealed class SelectorVisitorTests : ContextBaseTests
{
    private const string SelectorYamlFileName = "Selectors.Rule.yaml";
    private const string SelectorJsonFileName = "Selectors.Rule.jsonc";
    private const string FunctionsYamlFileName = "Functions.Rule.yaml";
    private const string FunctionsJsonFileName = "Functions.Rule.jsonc";

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ReadSelectorV1(string type, string path)
    {
        var testObject = GetTargetObject((name: "value", value: 3));
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();
        var selector = resourcesCache.OfType<SelectorV1>().ToArray();
        Assert.NotNull(selector);
        Assert.Equal(104, selector.Length);

        var actual = selector[0];
        var visitor = actual.ToSelectorVisitor(context);
        Assert.Equal("BasicSelector", actual.Name);
        Assert.NotNull(actual.Spec.If);
        Assert.False(visitor.Match(testObject));

        actual = selector[4];
        visitor = actual.ToSelectorVisitor(context);
        Assert.Equal($"{type}AllOf", actual.Name);
        Assert.NotNull(actual.Spec.If);
        Assert.False(visitor.Match(testObject));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ReadSelectorV2(string type, string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();
        var selector = resourcesCache.OfType<SelectorV2>().ToArray();
        Assert.NotNull(selector);
        Assert.Single(selector);

        var actual = selector[0];
        Assert.Equal($"{type}TypePrecondition", actual.Name);
        Assert.NotNull(actual.Spec.If);
    }

    #region Conditions

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ExistsExpression(string type, string path)
    {
        var existsTrue = GetSelectorVisitor($"{type}ExistsTrue", GetSource(path), out _);
        var existsFalse = GetSelectorVisitor($"{type}ExistsFalse", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "notValue", value: 3));
        var actual3 = GetTargetObject((name: "value", value: null));

        Assert.True(existsTrue.Match(actual1));
        Assert.False(existsTrue.Match(actual2));
        Assert.True(existsTrue.Match(actual3));

        Assert.False(existsFalse.Match(actual1));
        Assert.True(existsFalse.Match(actual2));
        Assert.False(existsFalse.Match(actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EqualsExpression(string type, string path)
    {
        var equals = GetSelectorVisitor($"{type}Equals", GetSource(path), out _);
        var actual1 = GetTargetObject(
            (name: "ValueString", value: "abc"),
            (name: "ValueInt", value: 123),
            (name: "ValueBool", value: true),
            (name: "ValueEnum", value: TestEnumValue.All)
        );
        var actual2 = GetTargetObject(
            (name: "ValueString", value: "efg"),
            (name: "ValueInt", value: 123),
            (name: "ValueBool", value: true)
        );
        var actual3 = GetTargetObject(
            (name: "ValueString", value: "abc"),
            (name: "ValueInt", value: 456),
            (name: "ValueBool", value: true)
        );
        var actual4 = GetTargetObject(
            (name: "ValueString", value: "abc"),
            (name: "ValueInt", value: 123),
            (name: "ValueBool", value: false)
        );
        var actual5 = GetTargetObject(
            (name: "ValueString", value: "abc"),
            (name: "ValueInt", value: 123),
            (name: "ValueBool", value: true),
            (name: "ValueEnum", value: TestEnumValue.None)
        );
        var actual6 = GetTargetObject(
            (name: "ValueString", value: "ABC"),
            (name: "ValueInt", value: 123),
            (name: "ValueBool", value: true)
        );

        Assert.True(equals.Match(actual1));
        Assert.False(equals.Match(actual2));
        Assert.False(equals.Match(actual3));
        Assert.False(equals.Match(actual4));
        Assert.False(equals.Match(actual5));
        Assert.False(equals.Match(actual6));

        // With name
        var withName = GetSelectorVisitor($"{type}NameEquals", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual1);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));

        // With type
        var withType = GetSelectorVisitor($"{type}TypeEquals", GetSource(path), out context);
        var actual7 = new TargetObject(GetObject(), type: "CustomType1");
        var actual8 = new TargetObject(GetObject(), type: "CustomType2");

        context.EnterTargetObject(actual7);
        Assert.True(withType.Match(actual7));

        context.EnterTargetObject(actual8);
        Assert.False(withType.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotEqualsExpression(string type, string path)
    {
        var notEquals = GetSelectorVisitor($"{type}NotEquals", GetSource(path), out _);
        var actual1 = GetTargetObject(
            (name: "ValueString", value: "efg"),
            (name: "ValueInt", value: 456),
            (name: "ValueBool", value: false),
            (name: "ValueEnum", value: TestEnumValue.None)
        );
        var actual2 = GetTargetObject(
            (name: "ValueString", value: "abc"),
            (name: "ValueInt", value: 456),
            (name: "ValueBool", value: false)
        );
        var actual3 = GetTargetObject(
            (name: "ValueString", value: "efg"),
            (name: "ValueInt", value: 123),
            (name: "ValueBool", value: false)
        );
        var actual4 = GetTargetObject(
            (name: "ValueString", value: "efg"),
            (name: "ValueInt", value: 456),
            (name: "ValueBool", value: true)
        );
        var actual5 = GetTargetObject(
            (name: "ValueString", value: "efg"),
            (name: "ValueInt", value: 456),
            (name: "ValueBool", value: false),
            (name: "ValueEnum", value: TestEnumValue.All)
        );
        var actual6 = GetTargetObject(
            (name: "ValueString", value: "ABC"),
            (name: "ValueInt", value: 456),
            (name: "ValueBool", value: false)
        );

        Assert.True(notEquals.Match(actual1));
        Assert.False(notEquals.Match(actual2));
        Assert.False(notEquals.Match(actual3));
        Assert.False(notEquals.Match(actual4));
        Assert.False(notEquals.Match(actual5));
        Assert.True(notEquals.Match(actual6));

        // With name
        var withName = GetSelectorVisitor($"{type}NameNotEquals", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual1);
        Assert.False(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.True(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasValueExpression(string type, string path)
    {
        var hasValueTrue = GetSelectorVisitor($"{type}HasValueTrue", GetSource(path), out _);
        var hasValueFalse = GetSelectorVisitor($"{type}HasValueFalse", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "notValue", value: 3));
        var actual3 = GetTargetObject((name: "value", value: null));

        Assert.True(hasValueTrue.Match(actual1));
        Assert.False(hasValueTrue.Match(actual2));
        Assert.False(hasValueTrue.Match(actual3));

        Assert.False(hasValueFalse.Match(actual1));
        Assert.True(hasValueFalse.Match(actual2));
        Assert.True(hasValueFalse.Match(actual3));

        // With name
        var withName = GetSelectorVisitor($"{type}NameHasValue", GetSource(path), out var context);
        var actual4 = GetTargetObject();

        context.EnterTargetObject(actual4);
        Assert.True(withName.Match(actual4));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void MatchExpression(string type, string path)
    {
        var match = GetSelectorVisitor($"{type}Match", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: 0));
        var actual5 = GetTargetObject();

        Assert.True(match.Match(actual1));
        Assert.True(match.Match(actual2));
        Assert.False(match.Match(actual3));
        Assert.False(match.Match(actual4));
        Assert.False(match.Match(actual5));

        // With name
        var withName = GetSelectorVisitor($"{type}NameMatch", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual1);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));

        // With case-sensitivity
        var withCaseSensitivity = GetSelectorVisitor($"{type}MatchCaseSensitive", GetSource(path), out _);
        actual1 = GetTargetObject((name: "value", value: "abc"));
        actual2 = GetTargetObject((name: "value", value: "aBc"));

        Assert.True(withCaseSensitivity.Match(actual1));
        Assert.False(withCaseSensitivity.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotMatchExpression(string type, string path)
    {
        var notMatch = GetSelectorVisitor($"{type}NotMatch", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: 0));

        Assert.False(notMatch.Match(actual1));
        Assert.False(notMatch.Match(actual2));
        Assert.True(notMatch.Match(actual3));
        Assert.True(notMatch.Match(actual4));

        // With name
        var withName = GetSelectorVisitor($"{type}NameNotMatch", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual1);
        Assert.False(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.True(withName.Match(actual2));

        // With case-sensitivity
        var withCaseSensitivity = GetSelectorVisitor($"{type}NotMatchCaseSensitive", GetSource(path), out _);
        actual1 = GetTargetObject((name: "value", value: "abc"));
        actual2 = GetTargetObject((name: "value", value: "aBc"));

        Assert.False(withCaseSensitivity.Match(actual1));
        Assert.True(withCaseSensitivity.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void WithinPathExpression(string type, string path)
    {
        // Source Case insensitive
        var sourceWithinPath = GetSelectorVisitor($"{type}SourceWithinPath", GetSource(path), out var context);

        var source = new PSObject();
        source.Properties.Add(new PSNoteProperty("file", "deployments/path/template.json"));
        source.Properties.Add(new PSNoteProperty("line", 100));
        source.Properties.Add(new PSNoteProperty("position", 1000));
        source.Properties.Add(new PSNoteProperty("Type", "Template"));
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));

        var actual = new PSObject();
        actual.Properties.Add(new PSNoteProperty("Name", "TestObject1"));
        actual.Properties.Add(new PSNoteProperty("Value", 1));
        actual.Properties.Add(new PSNoteProperty("_PSRule", info));

        var actualTO = new TargetObject(actual);
        context.EnterTargetObject(actualTO);

        Assert.True(sourceWithinPath.Match(actualTO));

        // Source Case sensitive
        var sourceWithinPathCaseSensitive = GetSelectorVisitor($"{type}SourceWithinPathCaseSensitive", GetSource(path), out context);

        context.EnterTargetObject(actualTO);

        Assert.False(sourceWithinPathCaseSensitive.Match(actualTO));

        // Field Case insensitive
        var fieldWithinPath = GetSelectorVisitor($"{type}FieldWithinPath", GetSource(path), out context);

        actual = new PSObject();
        actual.Properties.Add(new PSNoteProperty("FullName", "policy/policy.json"));

        actualTO = new TargetObject(actual);
        context.EnterTargetObject(actualTO);

        Assert.True(fieldWithinPath.Match(actualTO));

        // Field Case sensitive
        var fieldWithinPathCaseSensitive = GetSelectorVisitor($"{type}FieldWithinPathCaseSensitive", GetSource(path), out context);

        context.EnterTargetObject(actualTO);

        Assert.False(fieldWithinPathCaseSensitive.Match(actualTO));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotWithinPathExpression(string type, string path)
    {
        // Source Case insensitive
        var notWithinPath = GetSelectorVisitor($"{type}SourceNotWithinPath", GetSource(path), out var context);

        var source = new PSObject();
        source.Properties.Add(new PSNoteProperty("file", "deployments/path/template.json"));
        source.Properties.Add(new PSNoteProperty("line", 100));
        source.Properties.Add(new PSNoteProperty("position", 1000));
        source.Properties.Add(new PSNoteProperty("Type", "Template"));
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));
        var actual = new PSObject();
        actual.Properties.Add(new PSNoteProperty("Name", "TestObject1"));
        actual.Properties.Add(new PSNoteProperty("Value", 1));
        actual.Properties.Add(new PSNoteProperty("_PSRule", info));

        var actualTO = new TargetObject(actual);
        context.EnterTargetObject(actualTO);

        Assert.False(notWithinPath.Match(actualTO));

        // Source Case sensitive
        var notWithinPathCaseSensitive = GetSelectorVisitor($"{type}SourceNotWithinPathCaseSensitive", GetSource(path), out context);

        context.EnterTargetObject(actualTO);

        Assert.True(notWithinPathCaseSensitive.Match(actualTO));

        // Field Case insensitive
        var fieldNotWithinPath = GetSelectorVisitor($"{type}FieldNotWithinPath", GetSource(path), out context);

        actual = new PSObject();
        actual.Properties.Add(new PSNoteProperty("FullName", "policy/policy.json"));

        actualTO = new TargetObject(actual);
        context.EnterTargetObject(actualTO);

        Assert.False(fieldNotWithinPath.Match(actualTO));

        // Field Case sensitive
        var fieldNotWithinPathCaseSensitive = GetSelectorVisitor($"{type}FieldNotWithinPathCaseSensitive", GetSource(path), out context);

        context.EnterTargetObject(actualTO);

        Assert.True(fieldNotWithinPathCaseSensitive.Match(actualTO));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void InExpression(string type, string path)
    {
        var @in = GetSelectorVisitor($"{type}In", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "Value1" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "Value2" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();

        Assert.True(@in.Match(actual1));
        Assert.True(@in.Match(actual2));
        Assert.False(@in.Match(actual3));
        Assert.False(@in.Match(actual4));
        Assert.False(@in.Match(actual5));
        Assert.False(@in.Match(actual6));

        // With name
        var withName = GetSelectorVisitor($"{type}NameIn", GetSource(path), out var context);
        var actual7 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual7);
        Assert.True(withName.Match(actual7));

        context.EnterTargetObject(actual8);
        Assert.False(withName.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotInExpression(string type, string path)
    {
        var notIn = GetSelectorVisitor($"{type}NotIn", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "Value1" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "Value2" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));

        Assert.False(notIn.Match(actual1));
        Assert.False(notIn.Match(actual2));
        Assert.True(notIn.Match(actual3));
        Assert.True(notIn.Match(actual4));
        Assert.True(notIn.Match(actual5));

        // With name
        var withName = GetSelectorVisitor($"{type}NameNotIn", GetSource(path), out var context);
        var actual6 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual7 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual6);
        Assert.False(withName.Match(actual6));

        context.EnterTargetObject(actual7);
        Assert.True(withName.Match(actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void SetOfExpression(string type, string path)
    {
        var setOf = GetSelectorVisitor($"{type}SetOf", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-apiserver", "kube-scheduler" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "cluster-autoscaler" }));
        var actual4 = GetTargetObject((name: "value", value: new string[] { "kube-apiserver", "kube-scheduler" }));
        var actual5 = GetTargetObject((name: "value", value: new string[] { "kube-scheduler" }));
        var actual6 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual7 = GetTargetObject((name: "value", value: null));
        var actual8 = GetTargetObject();
        var actual9 = GetTargetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler", "kube-apiserver" }));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-APIserver" }));

        Assert.False(setOf.Match(actual1));
        Assert.True(setOf.Match(actual2));
        Assert.False(setOf.Match(actual3));
        Assert.False(setOf.Match(actual4));
        Assert.False(setOf.Match(actual5));
        Assert.False(setOf.Match(actual6));
        Assert.False(setOf.Match(actual7));
        Assert.False(setOf.Match(actual8));
        Assert.False(setOf.Match(actual9));
        Assert.False(setOf.Match(actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void SubsetExpression(string type, string path)
    {
        var subset = GetSelectorVisitor($"{type}Subset", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-apiserver", "kube-scheduler" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "cluster-autoscaler" }));
        var actual4 = GetTargetObject((name: "value", value: new string[] { "kube-apiserver", "kube-scheduler" }));
        var actual5 = GetTargetObject((name: "value", value: new string[] { "kube-scheduler" }));
        var actual6 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual7 = GetTargetObject((name: "value", value: null));
        var actual8 = GetTargetObject();
        var actual9 = GetTargetObject((name: "value", value: new string[] { "kube-apiserver", "cluster-autoscaler", "kube-apiserver" }));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "cluster-autoscaler", "kube-APIserver" }));

        Assert.True(subset.Match(actual1));
        Assert.True(subset.Match(actual2));
        Assert.False(subset.Match(actual3));
        Assert.False(subset.Match(actual4));
        Assert.False(subset.Match(actual5));
        Assert.False(subset.Match(actual6));
        Assert.False(subset.Match(actual7));
        Assert.False(subset.Match(actual8));
        Assert.False(subset.Match(actual9));
        Assert.False(subset.Match(actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void CountExpression(string type, string path)
    {
        var count = GetSelectorVisitor($"{type}Count", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "1", "2", "3" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "2", "1" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "1" }));
        var actual4 = GetTargetObject((name: "value", value: new int[] { 2, 3 }));
        var actual5 = GetTargetObject((name: "value", value: new int[] { 3 }));
        var actual6 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual7 = GetTargetObject((name: "value", value: null));
        var actual8 = GetTargetObject();

        Assert.False(count.Match(actual1));
        Assert.True(count.Match(actual2));
        Assert.False(count.Match(actual3));
        Assert.True(count.Match(actual4));
        Assert.False(count.Match(actual5));
        Assert.False(count.Match(actual6));
        Assert.False(count.Match(actual7));
        Assert.False(count.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotCountExpression(string type, string path)
    {
        var count = GetSelectorVisitor($"{type}NotCount", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "1", "2", "3" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "2", "1" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "1" }));
        var actual4 = GetTargetObject((name: "value", value: new int[] { 2, 3 }));
        var actual5 = GetTargetObject((name: "value", value: new int[] { 3 }));
        var actual6 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual7 = GetTargetObject((name: "value", value: null));
        var actual8 = GetTargetObject();

        Assert.True(count.Match(actual1));
        Assert.False(count.Match(actual2));
        Assert.True(count.Match(actual3));
        Assert.False(count.Match(actual4));
        Assert.True(count.Match(actual5));
        Assert.True(count.Match(actual6));
        Assert.False(count.Match(actual7));
        Assert.False(count.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LessExpression(string type, string path)
    {
        var less = GetSelectorVisitor($"{type}Less", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.False(less.Match(actual1));
        Assert.False(less.Match(actual2));
        Assert.True(less.Match(actual3));
        Assert.True(less.Match(actual4));
        Assert.True(less.Match(actual5));
        Assert.True(less.Match(actual6));
        Assert.True(less.Match(actual7));
        Assert.False(less.Match(actual8));
        Assert.True(less.Match(actual9));

        // With name
        var withName = GetSelectorVisitor($"{type}NameLess", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        context.EnterTargetObject(actual1);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LessOrEqualsExpression(string type, string path)
    {
        var lessOrEquals = GetSelectorVisitor($"{type}LessOrEquals", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.True(lessOrEquals.Match(actual1));
        Assert.False(lessOrEquals.Match(actual2));
        Assert.True(lessOrEquals.Match(actual3));
        Assert.True(lessOrEquals.Match(actual4));
        Assert.True(lessOrEquals.Match(actual5));
        Assert.True(lessOrEquals.Match(actual6));
        Assert.True(lessOrEquals.Match(actual7));
        Assert.True(lessOrEquals.Match(actual8));
        Assert.True(lessOrEquals.Match(actual9));

        // With name
        var withName = GetSelectorVisitor($"{type}NameLessOrEquals", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        context.EnterTargetObject(actual1);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void GreaterExpression(string type, string path)
    {
        var greater = GetSelectorVisitor($"{type}Greater", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.False(greater.Match(actual1));
        Assert.True(greater.Match(actual2));
        Assert.False(greater.Match(actual3));
        Assert.False(greater.Match(actual4));
        Assert.False(greater.Match(actual5));
        Assert.False(greater.Match(actual6));
        Assert.False(greater.Match(actual7));
        Assert.True(greater.Match(actual8));
        Assert.False(greater.Match(actual9));

        // With name
        var withName = GetSelectorVisitor($"{type}NameGreater", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        context.EnterTargetObject(actual1);
        Assert.False(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.True(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void GreaterOrEqualsExpression(string type, string path)
    {
        var greaterOrEquals = GetSelectorVisitor($"{type}GreaterOrEquals", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.True(greaterOrEquals.Match(actual1));
        Assert.True(greaterOrEquals.Match(actual2));
        Assert.False(greaterOrEquals.Match(actual3));
        Assert.False(greaterOrEquals.Match(actual4));
        Assert.False(greaterOrEquals.Match(actual5));
        Assert.False(greaterOrEquals.Match(actual6));
        Assert.False(greaterOrEquals.Match(actual7));
        Assert.True(greaterOrEquals.Match(actual8));
        Assert.True(greaterOrEquals.Match(actual9));

        // With name
        var withName = GetSelectorVisitor($"{type}NameGreaterOrEquals", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        context.EnterTargetObject(actual1);
        Assert.False(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.True(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void StartsWithExpression(string type, string path)
    {
        var startsWith = GetSelectorVisitor($"{type}StartsWith", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.All));
        var actual9 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.None));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "hij", "abc" }));

        Assert.True(startsWith.Match(actual1));
        Assert.True(startsWith.Match(actual2));
        Assert.False(startsWith.Match(actual3));
        Assert.False(startsWith.Match(actual4));
        Assert.False(startsWith.Match(actual5));
        Assert.False(startsWith.Match(actual6));
        Assert.False(startsWith.Match(actual7));
        Assert.True(startsWith.Match(actual8));
        Assert.False(startsWith.Match(actual9));
        Assert.True(startsWith.Match(actual10));

        // With name
        var withName = GetSelectorVisitor($"{type}NameStartsWith", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "1TargetObject")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "2TargetObject")
        );

        context.EnterTargetObject(actual1);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotStartsWithExpression(string type, string path)
    {
        var notStartsWith = GetSelectorVisitor($"{type}NotStartsWith", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.All));
        var actual9 = GetTargetObject((name: "value", value: "hij"), (name: "OtherValue", value: TestEnumValue.None));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "hij", "abc" }));

        Assert.False(notStartsWith.Match(actual1));
        Assert.False(notStartsWith.Match(actual2));
        Assert.True(notStartsWith.Match(actual3));
        Assert.True(notStartsWith.Match(actual4));
        Assert.True(notStartsWith.Match(actual5));
        Assert.False(notStartsWith.Match(actual6));
        Assert.False(notStartsWith.Match(actual7));
        Assert.False(notStartsWith.Match(actual8));
        Assert.True(notStartsWith.Match(actual9));
        Assert.False(notStartsWith.Match(actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EndsWithExpression(string type, string path)
    {
        var endsWith = GetSelectorVisitor($"{type}EndsWith", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.All));
        var actual9 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.None));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "hij", "abc" }));

        Assert.True(endsWith.Match(actual1));
        Assert.True(endsWith.Match(actual2));
        Assert.False(endsWith.Match(actual3));
        Assert.False(endsWith.Match(actual4));
        Assert.False(endsWith.Match(actual5));
        Assert.False(endsWith.Match(actual6));
        Assert.False(endsWith.Match(actual7));
        Assert.True(endsWith.Match(actual8));
        Assert.False(endsWith.Match(actual9));
        Assert.True(endsWith.Match(actual10));

        // With name
        var withName = GetSelectorVisitor($"{type}NameEndsWith", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual1);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));

        // With source
        var withSource = GetSelectorVisitor($"{type}EndsWithSource", GetSource(path), out context);
        var source = new PSObject();
        source.Properties.Add(new PSNoteProperty("file", "deployments/path/template.json"));
        source.Properties.Add(new PSNoteProperty("line", 100));
        source.Properties.Add(new PSNoteProperty("position", 1000));
        source.Properties.Add(new PSNoteProperty("Type", "Template"));
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));
        actual1 = GetTargetObject(
            (name: "Name", value: "TestObject1"),
            (name: "Value", value: 1),
            (name: "_PSRule", value: info)
        );

        context.EnterTargetObject(actual1);
        Assert.True(withSource.Match(actual1));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotEndsWithExpression(string type, string path)
    {
        var notEndsWith = GetSelectorVisitor($"{type}NotEndsWith", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.All));
        var actual9 = GetTargetObject((name: "value", value: "hij"), (name: "OtherValue", value: TestEnumValue.None));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "hij", "abc" }));

        Assert.False(notEndsWith.Match(actual1));
        Assert.False(notEndsWith.Match(actual2));
        Assert.True(notEndsWith.Match(actual3));
        Assert.True(notEndsWith.Match(actual4));
        Assert.True(notEndsWith.Match(actual5));
        Assert.False(notEndsWith.Match(actual6));
        Assert.False(notEndsWith.Match(actual7));
        Assert.False(notEndsWith.Match(actual8));
        Assert.True(notEndsWith.Match(actual9));
        Assert.False(notEndsWith.Match(actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ContainsExpression(string type, string path)
    {
        var contains = GetSelectorVisitor($"{type}Contains", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "bcd"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "BCD"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.All));
        var actual9 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.None));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "hij", "abc" }));

        Assert.True(contains.Match(actual1));
        Assert.True(contains.Match(actual2));
        Assert.False(contains.Match(actual3));
        Assert.False(contains.Match(actual4));
        Assert.False(contains.Match(actual5));
        Assert.False(contains.Match(actual6));
        Assert.False(contains.Match(actual7));
        Assert.True(contains.Match(actual8));
        Assert.False(contains.Match(actual9));
        Assert.True(contains.Match(actual10));

        // With name
        var withName = GetSelectorVisitor($"{type}NameContains", GetSource(path), out var context);
        actual1 = GetTargetObject(
           (name: "Name", value: "Target.1.Object")
        );
        actual2 = GetTargetObject(
           (name: "Name", value: "Target.2.Object")
        );

        context.EnterTargetObject(actual1);
        context.EnterLanguageScope(withName.Source);
        Assert.True(withName.Match(actual1));

        context.EnterTargetObject(actual2);
        Assert.False(withName.Match(actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotContainsExpression(string type, string path)
    {
        var notContains = GetSelectorVisitor($"{type}NotContains", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "bcd"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "BCD"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: TestEnumValue.All));
        var actual9 = GetTargetObject((name: "value", value: "hij"), (name: "OtherValue", value: TestEnumValue.None));
        var actual10 = GetTargetObject((name: "value", value: new string[] { "hij", "abc" }));

        Assert.False(notContains.Match(actual1));
        Assert.False(notContains.Match(actual2));
        Assert.True(notContains.Match(actual3));
        Assert.True(notContains.Match(actual4));
        Assert.True(notContains.Match(actual5));
        Assert.False(notContains.Match(actual6));
        Assert.False(notContains.Match(actual7));
        Assert.False(notContains.Match(actual8));
        Assert.True(notContains.Match(actual9));
        Assert.False(notContains.Match(actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LikeExpression(string type, string path)
    {
        var like = GetSelectorVisitor($"{type}Like", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: "123"));
        var actual9 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: 123));

        Assert.True(like.Match(actual1));
        Assert.True(like.Match(actual2));
        Assert.False(like.Match(actual3));
        Assert.False(like.Match(actual4));
        Assert.False(like.Match(actual5));
        Assert.False(like.Match(actual6));
        Assert.False(like.Match(actual7));
        Assert.True(like.Match(actual8));
        Assert.True(like.Match(actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotLikeExpression(string type, string path)
    {
        var notLike = GetSelectorVisitor($"{type}NotLike", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: "123"));
        var actual9 = GetTargetObject((name: "value", value: "hij"), (name: "OtherValue", value: 123));

        Assert.False(notLike.Match(actual1));
        Assert.False(notLike.Match(actual2));
        Assert.True(notLike.Match(actual3));
        Assert.True(notLike.Match(actual4));
        Assert.True(notLike.Match(actual5));
        Assert.False(notLike.Match(actual6));
        Assert.False(notLike.Match(actual7));
        Assert.False(notLike.Match(actual8));
        Assert.False(notLike.Match(actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsStringExpression(string type, string path)
    {
        var isStringTrue = GetSelectorVisitor($"{type}IsStringTrue", GetSource(path), out _);
        var isStringFalse = GetSelectorVisitor($"{type}IsStringFalse", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual4 = GetTargetObject((name: "value", value: null));
        var actual5 = GetTargetObject();

        // isString: true
        Assert.True(isStringTrue.Match(actual1));
        Assert.False(isStringTrue.Match(actual2));
        Assert.False(isStringTrue.Match(actual3));
        Assert.False(isStringTrue.Match(actual4));
        Assert.False(isStringTrue.Match(actual5));

        // isString: false
        Assert.False(isStringFalse.Match(actual1));
        Assert.True(isStringFalse.Match(actual2));
        Assert.True(isStringFalse.Match(actual3));
        Assert.True(isStringFalse.Match(actual4));
        Assert.False(isStringFalse.Match(actual5));

        // With name
        var withName = GetSelectorVisitor($"{type}NameIsString", GetSource(path), out var context);
        var actual7 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: 1)
        );

        context.EnterTargetObject(actual7);
        Assert.True(withName.Match(actual7));

        context.EnterTargetObject(actual8);
        Assert.True(withName.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsArrayExpression(string type, string path)
    {
        var isArrayTrue = GetSelectorVisitor($"{type}IsArrayTrue", GetSource(path), out _);
        var isArrayFalse = GetSelectorVisitor($"{type}IsArrayFalse", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: new string[] { "abc" }));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: PSObject.AsPSObject(new int[] { 1 })));
        var actual4 = GetTargetObject((name: "value", value: null));
        var actual5 = GetTargetObject((name: "value", value: "abc"));
        var actual6 = GetTargetObject((name: "value", value: new int[] { 1 }));
        var actual7 = GetTargetObject();

        // isArray: true
        Assert.True(isArrayTrue.Match(actual1));
        Assert.False(isArrayTrue.Match(actual2));
        Assert.True(isArrayTrue.Match(actual3));
        Assert.False(isArrayTrue.Match(actual4));
        Assert.False(isArrayTrue.Match(actual5));
        Assert.True(isArrayTrue.Match(actual6));
        Assert.False(isArrayFalse.Match(actual7));

        // isArray: false
        Assert.False(isArrayFalse.Match(actual1));
        Assert.True(isArrayFalse.Match(actual2));
        Assert.False(isArrayFalse.Match(actual3));
        Assert.True(isArrayFalse.Match(actual4));
        Assert.True(isArrayFalse.Match(actual5));
        Assert.False(isArrayFalse.Match(actual6));
        Assert.False(isArrayFalse.Match(actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsBooleanExpression(string type, string path)
    {
        var actual1 = GetTargetObject((name: "value", value: true));
        var actual2 = GetTargetObject((name: "value", value: false));
        var actual3 = GetTargetObject((name: "value", value: "true"));
        var actual4 = GetTargetObject((name: "value", value: "false"));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: PSObject.AsPSObject(true)));
        var actual7 = GetTargetObject((name: "value", value: Array.Empty<bool>()));
        var actual8 = GetTargetObject();

        // Without conversion
        var isBooleanTrue = GetSelectorVisitor($"{type}IsBooleanTrue", GetSource(path), out _);
        var isBooleanFalse = GetSelectorVisitor($"{type}IsBooleanFalse", GetSource(path), out _);

        // isBoolean: true
        Assert.True(isBooleanTrue.Match(actual1));
        Assert.True(isBooleanTrue.Match(actual2));
        Assert.False(isBooleanTrue.Match(actual3));
        Assert.False(isBooleanTrue.Match(actual4));
        Assert.False(isBooleanTrue.Match(actual5));
        Assert.True(isBooleanTrue.Match(actual6));
        Assert.False(isBooleanTrue.Match(actual7));
        Assert.False(isBooleanTrue.Match(actual8));

        // isBoolean: false
        Assert.False(isBooleanFalse.Match(actual1));
        Assert.False(isBooleanFalse.Match(actual2));
        Assert.True(isBooleanFalse.Match(actual3));
        Assert.True(isBooleanFalse.Match(actual4));
        Assert.True(isBooleanFalse.Match(actual5));
        Assert.False(isBooleanFalse.Match(actual6));
        Assert.True(isBooleanFalse.Match(actual7));
        Assert.False(isBooleanFalse.Match(actual8));

        // With conversion
        var isBooleanConvertTrue = GetSelectorVisitor($"{type}IsBooleanTrueWithConversion", GetSource(path), out _);
        var isBooleanConvertFalse = GetSelectorVisitor($"{type}IsBooleanFalseWithConversion", GetSource(path), out _);

        // isBoolean: true
        Assert.True(isBooleanConvertTrue.Match(actual1));
        Assert.True(isBooleanConvertTrue.Match(actual2));
        Assert.True(isBooleanConvertTrue.Match(actual3));
        Assert.True(isBooleanConvertTrue.Match(actual4));
        Assert.False(isBooleanConvertTrue.Match(actual5));
        Assert.True(isBooleanConvertTrue.Match(actual6));
        Assert.False(isBooleanConvertTrue.Match(actual7));
        Assert.False(isBooleanConvertTrue.Match(actual8));

        // isBoolean: false
        Assert.False(isBooleanConvertFalse.Match(actual1));
        Assert.False(isBooleanConvertFalse.Match(actual2));
        Assert.False(isBooleanConvertFalse.Match(actual3));
        Assert.False(isBooleanConvertFalse.Match(actual4));
        Assert.True(isBooleanConvertFalse.Match(actual5));
        Assert.False(isBooleanConvertFalse.Match(actual6));
        Assert.True(isBooleanConvertFalse.Match(actual7));
        Assert.False(isBooleanConvertFalse.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsDateTimeExpression(string type, string path)
    {
        var actual1 = GetTargetObject((name: "value", value: DateTime.Now));
        var actual2 = GetTargetObject((name: "value", value: 1));
        var actual3 = GetTargetObject((name: "value", value: "2021-04-03T15:00:00.00+10:00"));
        var actual4 = GetTargetObject((name: "value", value: new JValue(DateTime.Now)));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: PSObject.AsPSObject(DateTime.Now)));
        var actual7 = GetTargetObject((name: "value", value: new JValue("2021-04-03T15:00:00.00+10:00")));
        var actual8 = GetTargetObject((name: "value", value: long.MaxValue));
        var actual9 = GetTargetObject();

        // Without conversion
        var isDateTimeTrue = GetSelectorVisitor($"{type}IsDateTimeTrue", GetSource(path), out _);
        var isDateTimeFalse = GetSelectorVisitor($"{type}IsDateTimeFalse", GetSource(path), out _);

        // isDateTime: true
        Assert.True(isDateTimeTrue.Match(actual1));
        Assert.False(isDateTimeTrue.Match(actual2));
        Assert.False(isDateTimeTrue.Match(actual3));
        Assert.True(isDateTimeTrue.Match(actual4));
        Assert.False(isDateTimeTrue.Match(actual5));
        Assert.True(isDateTimeTrue.Match(actual6));
        Assert.False(isDateTimeTrue.Match(actual7));
        Assert.False(isDateTimeTrue.Match(actual8));
        Assert.False(isDateTimeTrue.Match(actual9));

        // isDateTime: false
        Assert.False(isDateTimeFalse.Match(actual1));
        Assert.True(isDateTimeFalse.Match(actual2));
        Assert.True(isDateTimeFalse.Match(actual3));
        Assert.False(isDateTimeFalse.Match(actual4));
        Assert.True(isDateTimeFalse.Match(actual5));
        Assert.False(isDateTimeFalse.Match(actual6));
        Assert.True(isDateTimeFalse.Match(actual7));
        Assert.True(isDateTimeFalse.Match(actual8));
        Assert.False(isDateTimeFalse.Match(actual9));

        // With conversion
        var isDateTimeConvertTrue = GetSelectorVisitor($"{type}IsDateTimeTrueWithConversion", GetSource(path), out _);
        var isDateTimeConvertFalse = GetSelectorVisitor($"{type}IsDateTimeFalseWithConversion", GetSource(path), out _);

        // isDateTime: true
        Assert.True(isDateTimeConvertTrue.Match(actual1));
        Assert.True(isDateTimeConvertTrue.Match(actual2));
        Assert.True(isDateTimeConvertTrue.Match(actual3));
        Assert.True(isDateTimeConvertTrue.Match(actual4));
        Assert.False(isDateTimeConvertTrue.Match(actual5));
        Assert.True(isDateTimeConvertTrue.Match(actual6));
        Assert.True(isDateTimeConvertTrue.Match(actual7));
        Assert.False(isDateTimeConvertTrue.Match(actual8));
        Assert.False(isDateTimeConvertTrue.Match(actual9));

        // isDateTime: false
        Assert.False(isDateTimeConvertFalse.Match(actual1));
        Assert.False(isDateTimeConvertFalse.Match(actual2));
        Assert.False(isDateTimeConvertFalse.Match(actual3));
        Assert.False(isDateTimeConvertFalse.Match(actual4));
        Assert.True(isDateTimeConvertFalse.Match(actual5));
        Assert.False(isDateTimeConvertFalse.Match(actual6));
        Assert.False(isDateTimeConvertFalse.Match(actual7));
        Assert.True(isDateTimeConvertFalse.Match(actual8));
        Assert.False(isDateTimeConvertFalse.Match(actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsIntegerExpression(string type, string path)
    {
        var actual1 = GetTargetObject((name: "value", value: 123));
        var actual2 = GetTargetObject((name: "value", value: 1.0f));
        var actual3 = GetTargetObject((name: "value", value: long.MaxValue));
        var actual4 = GetTargetObject((name: "value", value: "123"));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: PSObject.AsPSObject(123)));
        var actual7 = GetTargetObject((name: "value", value: byte.MaxValue));
        var actual8 = GetTargetObject();

        // Without conversion
        var isIntegerTrue = GetSelectorVisitor($"{type}IsIntegerTrue", GetSource(path), out _);
        var isIntegerFalse = GetSelectorVisitor($"{type}IsIntegerFalse", GetSource(path), out _);

        // isInteger: true
        Assert.True(isIntegerTrue.Match(actual1));
        Assert.False(isIntegerTrue.Match(actual2));
        Assert.True(isIntegerTrue.Match(actual3));
        Assert.False(isIntegerTrue.Match(actual4));
        Assert.False(isIntegerTrue.Match(actual5));
        Assert.True(isIntegerTrue.Match(actual6));
        Assert.True(isIntegerTrue.Match(actual7));
        Assert.False(isIntegerTrue.Match(actual8));

        // isInteger: false
        Assert.False(isIntegerFalse.Match(actual1));
        Assert.True(isIntegerFalse.Match(actual2));
        Assert.False(isIntegerFalse.Match(actual3));
        Assert.True(isIntegerFalse.Match(actual4));
        Assert.True(isIntegerFalse.Match(actual5));
        Assert.False(isIntegerFalse.Match(actual6));
        Assert.False(isIntegerFalse.Match(actual7));
        Assert.False(isIntegerFalse.Match(actual8));

        // With conversion
        var isIntegerConvertTrue = GetSelectorVisitor($"{type}IsIntegerTrueWithConversion", GetSource(path), out _);
        var isIntegerConvertFalse = GetSelectorVisitor($"{type}IsIntegerFalseWithConversion", GetSource(path), out _);

        // isInteger: true
        Assert.True(isIntegerConvertTrue.Match(actual1));
        Assert.False(isIntegerConvertTrue.Match(actual2));
        Assert.True(isIntegerConvertTrue.Match(actual3));
        Assert.True(isIntegerConvertTrue.Match(actual4));
        Assert.False(isIntegerConvertTrue.Match(actual5));
        Assert.True(isIntegerConvertTrue.Match(actual6));
        Assert.True(isIntegerConvertTrue.Match(actual7));
        Assert.False(isIntegerConvertTrue.Match(actual8));

        // isInteger: false
        Assert.False(isIntegerConvertFalse.Match(actual1));
        Assert.True(isIntegerConvertFalse.Match(actual2));
        Assert.False(isIntegerConvertFalse.Match(actual3));
        Assert.False(isIntegerConvertFalse.Match(actual4));
        Assert.True(isIntegerConvertFalse.Match(actual5));
        Assert.False(isIntegerConvertFalse.Match(actual6));
        Assert.False(isIntegerConvertFalse.Match(actual7));
        Assert.False(isIntegerConvertFalse.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsNumericExpression(string type, string path)
    {
        var actual1 = GetTargetObject((name: "value", value: 123));
        var actual2 = GetTargetObject((name: "value", value: 1.0f));
        var actual3 = GetTargetObject((name: "value", value: long.MaxValue));
        var actual4 = GetTargetObject((name: "value", value: "123"));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: PSObject.AsPSObject(123)));
        var actual7 = GetTargetObject((name: "value", value: byte.MaxValue));
        var actual8 = GetTargetObject((name: "value", value: double.MaxValue));
        var actual9 = GetTargetObject();

        // Without conversion
        var isNumericTrue = GetSelectorVisitor($"{type}IsNumericTrue", GetSource(path), out _);
        var isNumericFalse = GetSelectorVisitor($"{type}IsNumericFalse", GetSource(path), out _);

        // isNumeric: true
        Assert.True(isNumericTrue.Match(actual1));
        Assert.True(isNumericTrue.Match(actual2));
        Assert.True(isNumericTrue.Match(actual3));
        Assert.False(isNumericTrue.Match(actual4));
        Assert.False(isNumericTrue.Match(actual5));
        Assert.True(isNumericTrue.Match(actual6));
        Assert.True(isNumericTrue.Match(actual7));
        Assert.True(isNumericTrue.Match(actual8));
        Assert.False(isNumericTrue.Match(actual9));

        // isNumeric: false
        Assert.False(isNumericFalse.Match(actual1));
        Assert.False(isNumericFalse.Match(actual2));
        Assert.False(isNumericFalse.Match(actual3));
        Assert.True(isNumericFalse.Match(actual4));
        Assert.True(isNumericFalse.Match(actual5));
        Assert.False(isNumericFalse.Match(actual6));
        Assert.False(isNumericFalse.Match(actual7));
        Assert.False(isNumericFalse.Match(actual8));
        Assert.False(isNumericFalse.Match(actual9));

        // With conversion
        var isNumericConvertTrue = GetSelectorVisitor($"{type}IsNumericTrueWithConversion", GetSource(path), out _);
        var isNumericConvertFalse = GetSelectorVisitor($"{type}IsNumericFalseWithConversion", GetSource(path), out _);

        // isNumeric: true
        Assert.True(isNumericConvertTrue.Match(actual1));
        Assert.True(isNumericConvertTrue.Match(actual2));
        Assert.True(isNumericConvertTrue.Match(actual3));
        Assert.True(isNumericConvertTrue.Match(actual4));
        Assert.False(isNumericConvertTrue.Match(actual5));
        Assert.True(isNumericConvertTrue.Match(actual6));
        Assert.True(isNumericConvertTrue.Match(actual7));
        Assert.True(isNumericConvertTrue.Match(actual8));
        Assert.False(isNumericConvertTrue.Match(actual9));

        // isNumeric: false
        Assert.False(isNumericConvertFalse.Match(actual1));
        Assert.False(isNumericConvertFalse.Match(actual2));
        Assert.False(isNumericConvertFalse.Match(actual3));
        Assert.False(isNumericConvertFalse.Match(actual4));
        Assert.True(isNumericConvertFalse.Match(actual5));
        Assert.False(isNumericConvertFalse.Match(actual6));
        Assert.False(isNumericConvertFalse.Match(actual7));
        Assert.False(isNumericConvertFalse.Match(actual8));
        Assert.False(isNumericConvertFalse.Match(actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsLowerExpression(string type, string path)
    {
        var isLowerTrue = GetSelectorVisitor($"{type}IsLowerTrue", GetSource(path), out _);
        var isLowerFalse = GetSelectorVisitor($"{type}IsLowerFalse", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "aBc"));
        var actual3 = GetTargetObject((name: "value", value: "a-b-c"));
        var actual4 = GetTargetObject((name: "value", value: 4));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();

        // isLower: true
        Assert.True(isLowerTrue.Match(actual1));
        Assert.False(isLowerTrue.Match(actual2));
        Assert.True(isLowerTrue.Match(actual3));
        Assert.False(isLowerTrue.Match(actual4));
        Assert.False(isLowerTrue.Match(actual5));
        Assert.False(isLowerTrue.Match(actual6));

        // isLower: false
        Assert.False(isLowerFalse.Match(actual1));
        Assert.True(isLowerFalse.Match(actual2));
        Assert.False(isLowerFalse.Match(actual3));
        Assert.True(isLowerFalse.Match(actual4));
        Assert.True(isLowerFalse.Match(actual5));
        Assert.False(isLowerTrue.Match(actual6));

        // With name
        var withName = GetSelectorVisitor($"{type}NameIsLower", GetSource(path), out var context);
        var actual7 = GetTargetObject(
           (name: "Name", value: "targetobject1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual7);
        Assert.True(withName.Match(actual7));

        context.EnterTargetObject(actual8);
        Assert.False(withName.Match(actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsUpperExpression(string type, string path)
    {
        var isUpperTrue = GetSelectorVisitor($"{type}IsUpperTrue", GetSource(path), out _);
        var isUpperFalse = GetSelectorVisitor($"{type}IsUpperFalse", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "value", value: "ABC"));
        var actual2 = GetTargetObject((name: "value", value: "aBc"));
        var actual3 = GetTargetObject((name: "value", value: "A-B-C"));
        var actual4 = GetTargetObject((name: "value", value: 4));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();

        // isUpper: true
        Assert.True(isUpperTrue.Match(actual1));
        Assert.False(isUpperTrue.Match(actual2));
        Assert.True(isUpperTrue.Match(actual3));
        Assert.False(isUpperTrue.Match(actual4));
        Assert.False(isUpperTrue.Match(actual5));
        Assert.False(isUpperTrue.Match(actual6));

        // isUpper: false
        Assert.False(isUpperFalse.Match(actual1));
        Assert.True(isUpperFalse.Match(actual2));
        Assert.False(isUpperFalse.Match(actual3));
        Assert.True(isUpperFalse.Match(actual4));
        Assert.True(isUpperFalse.Match(actual5));
        Assert.False(isUpperFalse.Match(actual6));

        // With name
        var withName = GetSelectorVisitor($"{type}NameIsUpper", GetSource(path), out var context);
        var actual7 = GetTargetObject(
           (name: "Name", value: "TARGETOBJECT1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        context.EnterTargetObject(actual7);
        Assert.True(withName.Match(actual7));

        context.EnterTargetObject(actual8);
        Assert.False(withName.Match(actual8));
    }


    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasSchemaExpression(string type, string path)
    {
        var hasSchema = GetSelectorVisitor($"{type}HasSchema", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#"));
        var actual2 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json"));
        var actual3 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: "http://schema.management.azure.com/schemas/2019-04-01/DeploymentParameters.json#"));
        var actual4 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: null));
        var actual5 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: ""));
        var actual6 = GetTargetObject();

        Assert.True(hasSchema.Match(actual1));
        Assert.True(hasSchema.Match(actual2));
        Assert.False(hasSchema.Match(actual3));
        Assert.False(hasSchema.Match(actual4));
        Assert.False(hasSchema.Match(actual5));
        Assert.False(hasSchema.Match(actual6));

        hasSchema = GetSelectorVisitor($"{type}HasSchemaIgnoreScheme", GetSource(path), out _);
        Assert.True(hasSchema.Match(actual1));
        Assert.True(hasSchema.Match(actual2));
        Assert.True(hasSchema.Match(actual3));
        Assert.False(hasSchema.Match(actual4));
        Assert.False(hasSchema.Match(actual5));
        Assert.False(hasSchema.Match(actual6));

        hasSchema = GetSelectorVisitor($"{type}HasSchemaCaseSensitive", GetSource(path), out _);
        Assert.True(hasSchema.Match(actual1));
        Assert.True(hasSchema.Match(actual2));
        Assert.False(hasSchema.Match(actual3));
        Assert.False(hasSchema.Match(actual4));
        Assert.False(hasSchema.Match(actual5));
        Assert.False(hasSchema.Match(actual6));

        hasSchema = GetSelectorVisitor($"{type}HasAnySchema", GetSource(path), out _);
        Assert.True(hasSchema.Match(actual1));
        Assert.True(hasSchema.Match(actual2));
        Assert.True(hasSchema.Match(actual3));
        Assert.False(hasSchema.Match(actual4));
        Assert.False(hasSchema.Match(actual5));
        Assert.False(hasSchema.Match(actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Version(string type, string path)
    {
        var actual1 = GetTargetObject((name: "version", value: "1.2.3"));
        var actual2 = GetTargetObject((name: "version", value: "0.2.3"));
        var actual3 = GetTargetObject((name: "version", value: "2.2.3"));
        var actual4 = GetTargetObject((name: "version", value: "1.1.3"));
        var actual5 = GetTargetObject((name: "version", value: "1.3.3-preview.1"));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "version", value: "a.b.c"));

        var version = GetSelectorVisitor($"{type}Version", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.False(version.Match(actual2));
        Assert.False(version.Match(actual3));
        Assert.False(version.Match(actual4));
        Assert.False(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));

        version = GetSelectorVisitor($"{type}VersionWithPrerelease", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.False(version.Match(actual2));
        Assert.False(version.Match(actual3));
        Assert.False(version.Match(actual4));
        Assert.True(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));

        version = GetSelectorVisitor($"{type}VersionAnyVersion", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.True(version.Match(actual2));
        Assert.True(version.Match(actual3));
        Assert.True(version.Match(actual4));
        Assert.True(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));

        version = GetSelectorVisitor($"{type}VersionAnyStableVersion", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.True(version.Match(actual2));
        Assert.True(version.Match(actual3));
        Assert.True(version.Match(actual4));
        Assert.False(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void APIVersion(string type, string path)
    {
        var actual1 = GetTargetObject((name: "dateVersion", value: "2015-10-01"));
        var actual2 = GetTargetObject((name: "dateVersion", value: "2014-01-01"));
        var actual3 = GetTargetObject((name: "dateVersion", value: "2022-01-01"));
        var actual4 = GetTargetObject((name: "dateVersion", value: "2015-10-01-preview"));
        var actual5 = GetTargetObject((name: "dateVersion", value: "2022-01-01-preview"));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "dateVersion", value: "a-b-c"));

        var version = GetSelectorVisitor($"{type}APIVersion", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.False(version.Match(actual2));
        Assert.True(version.Match(actual3));
        Assert.False(version.Match(actual4));
        Assert.False(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));

        version = GetSelectorVisitor($"{type}APIVersionWithPrerelease", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.False(version.Match(actual2));
        Assert.True(version.Match(actual3));
        Assert.False(version.Match(actual4));
        Assert.True(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));

        version = GetSelectorVisitor($"{type}APIVersionAnyVersion", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.True(version.Match(actual2));
        Assert.True(version.Match(actual3));
        Assert.True(version.Match(actual4));
        Assert.True(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));

        version = GetSelectorVisitor($"{type}APIVersionAnyStableVersion", GetSource(path), out _);
        Assert.True(version.Match(actual1));
        Assert.True(version.Match(actual2));
        Assert.True(version.Match(actual3));
        Assert.False(version.Match(actual4));
        Assert.False(version.Match(actual5));
        Assert.False(version.Match(actual6));
        Assert.False(version.Match(actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasDefault(string type, string path)
    {
        var actual1 = GetTargetObject((name: "integerValue", value: 100), (name: "boolValue", value: true), (name: "stringValue", value: "testValue"));
        var actual2 = GetTargetObject((name: "integerValue", value: 1));
        var actual3 = GetTargetObject((name: "boolValue", value: false));
        var actual4 = GetTargetObject((name: "stringValue", value: "TestValue"));
        var actual5 = GetTargetObject();
        var actual6 = GetTargetObject((name: "integerValue", value: new JValue(100)));
        var actual7 = GetTargetObject((name: "boolValue", value: new JValue(true)));
        var actual8 = GetTargetObject((name: "stringValue", value: new JValue("testValue")));

        var hasDefault = GetSelectorVisitor($"{type}HasDefault", GetSource(path), out _);
        Assert.True(hasDefault.Match(actual1));
        Assert.False(hasDefault.Match(actual2));
        Assert.False(hasDefault.Match(actual3));
        Assert.False(hasDefault.Match(actual4));
        Assert.True(hasDefault.Match(actual5));
        Assert.True(hasDefault.Match(actual6));
        Assert.True(hasDefault.Match(actual7));
        Assert.True(hasDefault.Match(actual8));
    }

    #endregion Conditions

    #region Operators

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void AllOf(string type, string path)
    {
        var allOf = GetSelectorVisitor($"{type}AllOf", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "Name", value: "Name1"));
        var actual2 = GetTargetObject((name: "AlternateName", value: "Name2"));
        var actual3 = GetTargetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
        var actual4 = GetTargetObject((name: "OtherName", value: "Name3"));

        Assert.False(allOf.Match(actual1));
        Assert.False(allOf.Match(actual2));
        Assert.True(allOf.Match(actual3));
        Assert.False(allOf.Match(actual4));

        // With quantifier
        allOf = GetSelectorVisitor($"{type}AllOfWithQuantifier", GetSource(path), out _);
        actual1 = GetTargetObject((name: "Name", value: "TargetObject1"), (name: "properties", value: GetObject((name: "logs", value: new object[]
        {
            GetObject((name: "name", value: "log1"))
        }))));
        actual2 = GetTargetObject((name: "Name", value: "TargetObject1"), (name: "properties", value: GetObject((name: "logs", value: new object[]
        {
            GetObject((name: "name", value: "log1")),
            GetObject((name: "name", value: "log2"))
        }))));
        actual3 = GetTargetObject((name: "Name", value: "TargetObject1"), (name: "properties", value: GetObject((name: "logs", value: Array.Empty<object>()))));

        Assert.True(allOf.Match(actual1));
        Assert.True(allOf.Match(actual2));
        Assert.False(allOf.Match(actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void AnyOf(string type, string path)
    {
        var allOf = GetSelectorVisitor($"{type}AnyOf", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "Name", value: "Name1"));
        var actual2 = GetTargetObject((name: "AlternateName", value: "Name2"));
        var actual3 = GetTargetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
        var actual4 = GetTargetObject((name: "OtherName", value: "Name3"));

        Assert.True(allOf.Match(actual1));
        Assert.True(allOf.Match(actual2));
        Assert.True(allOf.Match(actual3));
        Assert.False(allOf.Match(actual4));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Not(string type, string path)
    {
        var allOf = GetSelectorVisitor($"{type}Not", GetSource(path), out _);
        var actual1 = GetTargetObject((name: "Name", value: "Name1"));
        var actual2 = GetTargetObject((name: "AlternateName", value: "Name2"));
        var actual3 = GetTargetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
        var actual4 = GetTargetObject((name: "OtherName", value: "Name3"));

        Assert.False(allOf.Match(actual1));
        Assert.False(allOf.Match(actual2));
        Assert.False(allOf.Match(actual3));
        Assert.True(allOf.Match(actual4));
    }

    #endregion Operators

    #region Properties

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Type(string type, string path)
    {
        var equals = GetSelectorVisitor($"{type}TypeEquals", GetSource(path), out var context);
        var actual = new TargetObject(GetObject(), type: "CustomType1");

        context.EnterTargetObject(actual);

        Assert.True(equals.Match(actual));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void TypePrecondition(string type, string path)
    {
        var equals = GetSelectorVisitor($"{type}TypePrecondition", GetSource(path), out var context);
        var actual = new TargetObject(GetObject(), type: "CustomType1");

        context.EnterTargetObject(actual);
        context.EnterLanguageScope(equals.Source);

        Assert.True(equals.Match(actual));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Name(string type, string path)
    {
        var equals = GetSelectorVisitor($"{type}NameEquals", GetSource(path), out var context);
        var actual = GetTargetObject(
            (name: "Name", value: "TargetObject1")
        );

        context.EnterTargetObject(actual);

        Assert.True(equals.Match(actual));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Scope(string type, string path)
    {
        var testObject = GetObject(
            (name: "Name", value: "TargetObject1")
        );

        var equals = GetSelectorVisitor($"{type}ScopeEquals", GetSource(path), out var context);

        var actual = new TargetObject(testObject, scope: ["/scope1"]);
        context.EnterTargetObject(actual);
        Assert.True(equals.Match(actual));

        actual = new TargetObject(testObject, scope: ["/scope2"]);
        context.EnterTargetObject(actual);
        Assert.False(equals.Match(actual));

        actual = new TargetObject(testObject);
        context.EnterTargetObject(actual);
        Assert.False(equals.Match(actual));

        var startsWith = GetSelectorVisitor($"{type}ScopeStartsWith", GetSource(path), out context);

        actual = new TargetObject(testObject, scope: ["/scope1/"]);
        context.EnterTargetObject(actual);
        Assert.True(startsWith.Match(actual));

        actual = new TargetObject(testObject, scope: ["/scope2/"]);
        context.EnterTargetObject(actual);
        Assert.True(startsWith.Match(actual));

        actual = new TargetObject(testObject, scope: ["/scope2"]);
        context.EnterTargetObject(actual);
        Assert.False(startsWith.Match(actual));

        actual = new TargetObject(testObject);
        context.EnterTargetObject(actual);
        Assert.False(startsWith.Match(actual));

        var hasValueFalse = GetSelectorVisitor($"{type}ScopeHasValueFalse", GetSource(path), out context);

        actual = new TargetObject(testObject, scope: ["/scope1"]);
        context.EnterTargetObject(actual);
        Assert.False(hasValueFalse.Match(actual));

        actual = new TargetObject(testObject);
        context.EnterTargetObject(actual);
        Assert.True(hasValueFalse.Match(actual));

        var hasValueTrue = GetSelectorVisitor($"{type}ScopeHasValueTrue", GetSource(path), out context);

        actual = new TargetObject(testObject, scope: ["/scope1"]);
        context.EnterTargetObject(actual);
        Assert.True(hasValueTrue.Match(actual));

        actual = new TargetObject(testObject);
        context.EnterTargetObject(actual);
        Assert.False(hasValueTrue.Match(actual));
    }

    #endregion Properties

    #region Functions

    [Theory]
    [InlineData("Yaml", FunctionsYamlFileName)]
    [InlineData("Json", FunctionsJsonFileName)]
    public void WithFunction(string type, string path)
    {
        var example1 = GetSelectorVisitor($"{type}.Fn.Example1", GetSource(path), out _);
        var example2 = GetSelectorVisitor($"{type}.Fn.Example2", GetSource(path), out _);
        var example3 = GetSelectorVisitor($"{type}.Fn.Example3", GetSource(path), out _);
        var example4 = GetSelectorVisitor($"{type}.Fn.Example4", GetSource(path), out _);
        var example5 = GetSelectorVisitor($"{type}.Fn.Example5", GetSource(path), out _);
        var example6 = GetSelectorVisitor($"{type}.Fn.Example6", GetSource(path), out _);
        var actual1 = GetTargetObject(
            (name: "Name", value: "TestObject1")
        );

        Assert.True(example1.Match(actual1));
        Assert.True(example2.Match(actual1));
        Assert.True(example3.Match(actual1));
        Assert.True(example4.Match(actual1));
        Assert.True(example5.Match(actual1));
        Assert.True(example6.Match(actual1));
    }

    [Theory]
    [InlineData("Yaml", FunctionsYamlFileName)]
    [InlineData("Json", FunctionsJsonFileName)]
    public void WithFunctionSpecific(string type, string path)
    {
        var example1 = GetSelectorVisitor($"{type}.Fn.Replace", GetSource(path), out _);
        var example2 = GetSelectorVisitor($"{type}.Fn.Trim", GetSource(path), out _);
        var example3 = GetSelectorVisitor($"{type}.Fn.First", GetSource(path), out _);
        var example4 = GetSelectorVisitor($"{type}.Fn.Last", GetSource(path), out _);
        var example5 = GetSelectorVisitor($"{type}.Fn.Split", GetSource(path), out _);
        var example6 = GetSelectorVisitor($"{type}.Fn.PadLeft", GetSource(path), out _);
        var example7 = GetSelectorVisitor($"{type}.Fn.PadRight", GetSource(path), out _);
        var actual1 = GetTargetObject(
            (name: "Name", value: "TestObject1")
        );

        Assert.True(example1.Match(actual1));
        Assert.True(example2.Match(actual1));
        Assert.True(example3.Match(actual1));
        Assert.True(example4.Match(actual1));
        Assert.True(example5.Match(actual1));
        Assert.True(example6.Match(actual1));
        Assert.True(example7.Match(actual1));
    }

    #endregion Functions

    #region Helper methods

    protected sealed override PSRuleOption GetOption()
    {
        var option = new PSRuleOption();
        option.Configuration["ConfigArray"] = new string[] { "1", "2", "3", "4", "5" };
        option.Binding.PreferTargetInfo = true;
        return option;
    }

    private SelectorVisitor GetSelectorVisitor(string name, Source[] sources, out LegacyRunspaceContext context)
    {
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var optionBuilder = new OptionContextBuilder(option: GetOption(), bindTargetName: PipelineHookActions.BindTargetName, bindTargetType: PipelineHookActions.BindTargetType, bindField: PipelineHookActions.BindField);
        context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), sources: sources, optionBuilder: optionBuilder, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();
        var selector = resourcesCache.OfType<ISelector>().FirstOrDefault(s => s.Id.Name == name);
        context.EnterLanguageScope(selector.Source);
        return selector.ToSelectorVisitor(context);
    }

    #endregion Helper methods
}
