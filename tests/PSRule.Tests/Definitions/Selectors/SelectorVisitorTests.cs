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
        var visitor = actual.ToSelectorVisitor();
        Assert.Equal("BasicSelector", actual.Name);
        Assert.NotNull(actual.Spec.If);
        Assert.False(visitor.If(context, testObject));

        actual = selector[4];
        visitor = actual.ToSelectorVisitor();
        Assert.Equal($"{type}AllOf", actual.Name);
        Assert.NotNull(actual.Spec.If);
        Assert.False(visitor.If(context, testObject));
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
    public void ExistsExpression_WithTrue(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}ExistsTrue", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "notValue", value: 3));
        var actual3 = GetTargetObject((name: "value", value: null));

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ExistsExpression_WithFalse(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}ExistsFalse", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "notValue", value: 3));
        var actual3 = GetTargetObject((name: "value", value: null));

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EqualsExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Equals", GetSource(path));
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

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EqualsExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameEquals", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EqualsExpression_WithType(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}TypeEquals", GetSource(path));
        var actual1 = new TargetObject(GetObject(), type: "CustomType1");
        var actual2 = new TargetObject(GetObject(), type: "CustomType2");

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotEqualsExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotEquals", GetSource(path));
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

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.True(visitor.If(context, actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotEqualsExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameNotEquals", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasValueExpression_WithTrue(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}HasValueTrue", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "notValue", value: 3));
        var actual3 = GetTargetObject((name: "value", value: null));

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasValueExpression_WithFalse(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}HasValueFalse", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "notValue", value: 3));
        var actual3 = GetTargetObject((name: "value", value: null));

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasValueExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameHasValue", GetSource(path));
        var actual1 = GetTargetObject(
            (name: "Name", value: "TargetObject1")
        );

        Assert.True(visitor.If(context, actual1));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void MatchExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Match", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: 0));
        var actual5 = GetTargetObject();

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void MatchExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameMatch", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void MatchExpression_WithCaseSensitive(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}MatchCaseSensitive", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "aBc"));

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotMatchExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotMatch", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: 0));

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotMatchExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameNotMatch", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.False(withName.If(context, actual1));
        Assert.True(withName.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotMatchExpression_WithCaseSensitive(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withCaseSensitivity = GetSelectorVisitor($"{type}NotMatchCaseSensitive", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "aBc"));

        Assert.False(withCaseSensitivity.If(context, actual1));
        Assert.True(withCaseSensitivity.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void WithinPathExpression(string type, string path)
    {
        var context = GetTestExpressionContext();

        // Source Case insensitive
        var sourceWithinPath = GetSelectorVisitor($"{type}SourceWithinPath", GetSource(path));

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

        Assert.True(sourceWithinPath.If(context, actualTO));

        // Source Case sensitive
        var sourceWithinPathCaseSensitive = GetSelectorVisitor($"{type}SourceWithinPathCaseSensitive", GetSource(path));

        Assert.False(sourceWithinPathCaseSensitive.If(context, actualTO));

        // Field Case insensitive
        var fieldWithinPath = GetSelectorVisitor($"{type}FieldWithinPath", GetSource(path));

        actual = new PSObject();
        actual.Properties.Add(new PSNoteProperty("FullName", "policy/policy.json"));

        actualTO = new TargetObject(actual);

        Assert.True(fieldWithinPath.If(context, actualTO));

        // Field Case sensitive
        var fieldWithinPathCaseSensitive = GetSelectorVisitor($"{type}FieldWithinPathCaseSensitive", GetSource(path));

        Assert.False(fieldWithinPathCaseSensitive.If(context, actualTO));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotWithinPathExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        // Source Case insensitive
        var notWithinPath = GetSelectorVisitor($"{type}SourceNotWithinPath", GetSource(path));

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

        Assert.False(notWithinPath.If(context, actualTO));

        // Source Case sensitive
        var notWithinPathCaseSensitive = GetSelectorVisitor($"{type}SourceNotWithinPathCaseSensitive", GetSource(path));

        Assert.True(notWithinPathCaseSensitive.If(context, actualTO));

        // Field Case insensitive
        var fieldNotWithinPath = GetSelectorVisitor($"{type}FieldNotWithinPath", GetSource(path));

        actual = new PSObject();
        actual.Properties.Add(new PSNoteProperty("FullName", "policy/policy.json"));

        actualTO = new TargetObject(actual);

        Assert.False(fieldNotWithinPath.If(context, actualTO));

        // Field Case sensitive
        var fieldNotWithinPathCaseSensitive = GetSelectorVisitor($"{type}FieldNotWithinPathCaseSensitive", GetSource(path));

        Assert.True(fieldNotWithinPathCaseSensitive.If(context, actualTO));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void InExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}In", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: new string[] { "Value1" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "Value2" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void InExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameIn", GetSource(path));
        var actual7 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.True(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotInExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotIn", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: new string[] { "Value1" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "Value2" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotInExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameNotIn", GetSource(path));
        var actual6 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual7 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.False(visitor.If(context, actual6));
        Assert.True(visitor.If(context, actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void SetOfExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}SetOf", GetSource(path));
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

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
        Assert.False(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void SubsetExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Subset", GetSource(path));
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

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
        Assert.False(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void CountExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Count", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: new string[] { "1", "2", "3" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "2", "1" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "1" }));
        var actual4 = GetTargetObject((name: "value", value: new int[] { 2, 3 }));
        var actual5 = GetTargetObject((name: "value", value: new int[] { 3 }));
        var actual6 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual7 = GetTargetObject((name: "value", value: null));
        var actual8 = GetTargetObject();

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotCountExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotCount", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: new string[] { "1", "2", "3" }));
        var actual2 = GetTargetObject((name: "value", value: new string[] { "2", "1" }));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "1" }));
        var actual4 = GetTargetObject((name: "value", value: new int[] { 2, 3 }));
        var actual5 = GetTargetObject((name: "value", value: new int[] { 3 }));
        var actual6 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual7 = GetTargetObject((name: "value", value: null));
        var actual8 = GetTargetObject();

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.True(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LessExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Less", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.True(visitor.If(context, actual6));
        Assert.True(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LessExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameLess", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LessOrEqualsExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}LessOrEquals", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.True(visitor.If(context, actual6));
        Assert.True(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LessOrEqualsExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameLessOrEquals", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void GreaterExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Greater", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void GreaterExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameGreater", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void GreaterOrEqualsExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}GreaterOrEquals", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: 3));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: new string[] { "Value3" }));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: 2));
        var actual7 = GetTargetObject((name: "value", value: -1));
        var actual8 = GetTargetObject((name: "valueStr", value: "0"));
        var actual9 = GetTargetObject((name: "valueStr", value: "-1"));

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void GreaterOrEqualsExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameGreaterOrEquals", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "ItemTwo")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "ItemThree")
        );

        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void StartsWithExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}StartsWith", GetSource(path));
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

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
        Assert.True(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void StartsWithExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameStartsWith", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "1TargetObject")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "2TargetObject")
        );

        Assert.True(withName.If(context, actual1));
        Assert.False(withName.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotStartsWithExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotStartsWith", GetSource(path));
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

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
        Assert.False(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EndsWithExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}EndsWith", GetSource(path));
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

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
        Assert.True(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EndsWithExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameEndsWith", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.True(withName.If(context, actual1));
        Assert.False(withName.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void EndsWithExpression_WithSource(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withSource = GetSelectorVisitor($"{type}EndsWithSource", GetSource(path));
        var source = new PSObject();
        source.Properties.Add(new PSNoteProperty("file", "deployments/path/template.json"));
        source.Properties.Add(new PSNoteProperty("line", 100));
        source.Properties.Add(new PSNoteProperty("position", 1000));
        source.Properties.Add(new PSNoteProperty("Type", "Template"));
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));
        var actual1 = GetTargetObject(
            (name: "Name", value: "TestObject1"),
            (name: "Value", value: 1),
            (name: "_PSRule", value: info)
        );

        Assert.True(withSource.If(context, actual1));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotEndsWithExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotEndsWith", GetSource(path));
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

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
        Assert.False(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ContainsExpression_WithField_ShouldReturnResult(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Contains", GetSource(path));
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

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
        Assert.True(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void ContainsExpression_WithName_ShouldReturnResult(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameContains", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "Target.1.Object")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: "Target.2.Object")
        );

        Assert.True(withName.If(context, actual1));
        Assert.False(withName.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotContainsExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotContains", GetSource(path));
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

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
        Assert.False(visitor.If(context, actual10));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void LikeExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}Like", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: "123"));
        var actual9 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: 123));

        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.True(visitor.If(context, actual8));
        Assert.True(visitor.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void NotLikeExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NotLike", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "efg"));
        var actual3 = GetTargetObject((name: "value", value: "hij"));
        var actual4 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "value", value: "EFG"));
        var actual8 = GetTargetObject((name: "value", value: "abc"), (name: "OtherValue", value: "123"));
        var actual9 = GetTargetObject((name: "value", value: "hij"), (name: "OtherValue", value: 123));

        Assert.False(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
        Assert.False(visitor.If(context, actual8));
        Assert.False(visitor.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsStringExpression_WithTrue(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}IsStringTrue", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual4 = GetTargetObject((name: "value", value: null));
        var actual5 = GetTargetObject();

        // isString: true
        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsStringExpression_WithFalse(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}IsStringFalse", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: Array.Empty<string>()));
        var actual4 = GetTargetObject((name: "value", value: null));
        var actual5 = GetTargetObject();

        // isString: false
        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsStringExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameIsString", GetSource(path));
        var actual1 = GetTargetObject(
           (name: "Name", value: "TargetObject1")
        );
        var actual2 = GetTargetObject(
           (name: "Name", value: 1)
        );

        Assert.True(withName.If(context, actual1));
        Assert.True(withName.If(context, actual2));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsArrayExpression_WithTrue(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}IsArrayTrue", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: new string[] { "abc" }));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: PSObject.AsPSObject(new int[] { 1 })));
        var actual4 = GetTargetObject((name: "value", value: null));
        var actual5 = GetTargetObject((name: "value", value: "abc"));
        var actual6 = GetTargetObject((name: "value", value: new int[] { 1 }));
        var actual7 = GetTargetObject();

        // isArray: true
        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.True(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsArrayExpression_WithFalse(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}IsArrayFalse", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: new string[] { "abc" }));
        var actual2 = GetTargetObject((name: "value", value: 4));
        var actual3 = GetTargetObject((name: "value", value: PSObject.AsPSObject(new int[] { 1 })));
        var actual4 = GetTargetObject((name: "value", value: null));
        var actual5 = GetTargetObject((name: "value", value: "abc"));
        var actual6 = GetTargetObject((name: "value", value: new int[] { 1 }));
        var actual7 = GetTargetObject();

        // isArray: false
        Assert.False(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsBooleanExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var actual1 = GetTargetObject((name: "value", value: true));
        var actual2 = GetTargetObject((name: "value", value: false));
        var actual3 = GetTargetObject((name: "value", value: "true"));
        var actual4 = GetTargetObject((name: "value", value: "false"));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: PSObject.AsPSObject(true)));
        var actual7 = GetTargetObject((name: "value", value: Array.Empty<bool>()));
        var actual8 = GetTargetObject();

        // Without conversion
        var isBooleanTrue = GetSelectorVisitor($"{type}IsBooleanTrue", GetSource(path));
        var isBooleanFalse = GetSelectorVisitor($"{type}IsBooleanFalse", GetSource(path));

        // isBoolean: true
        Assert.True(isBooleanTrue.If(context, actual1));
        Assert.True(isBooleanTrue.If(context, actual2));
        Assert.False(isBooleanTrue.If(context, actual3));
        Assert.False(isBooleanTrue.If(context, actual4));
        Assert.False(isBooleanTrue.If(context, actual5));
        Assert.True(isBooleanTrue.If(context, actual6));
        Assert.False(isBooleanTrue.If(context, actual7));
        Assert.False(isBooleanTrue.If(context, actual8));

        // isBoolean: false
        Assert.False(isBooleanFalse.If(context, actual1));
        Assert.False(isBooleanFalse.If(context, actual2));
        Assert.True(isBooleanFalse.If(context, actual3));
        Assert.True(isBooleanFalse.If(context, actual4));
        Assert.True(isBooleanFalse.If(context, actual5));
        Assert.False(isBooleanFalse.If(context, actual6));
        Assert.True(isBooleanFalse.If(context, actual7));
        Assert.False(isBooleanFalse.If(context, actual8));

        // With conversion
        var isBooleanConvertTrue = GetSelectorVisitor($"{type}IsBooleanTrueWithConversion", GetSource(path));
        var isBooleanConvertFalse = GetSelectorVisitor($"{type}IsBooleanFalseWithConversion", GetSource(path));

        // isBoolean: true
        Assert.True(isBooleanConvertTrue.If(context, actual1));
        Assert.True(isBooleanConvertTrue.If(context, actual2));
        Assert.True(isBooleanConvertTrue.If(context, actual3));
        Assert.True(isBooleanConvertTrue.If(context, actual4));
        Assert.False(isBooleanConvertTrue.If(context, actual5));
        Assert.True(isBooleanConvertTrue.If(context, actual6));
        Assert.False(isBooleanConvertTrue.If(context, actual7));
        Assert.False(isBooleanConvertTrue.If(context, actual8));

        // isBoolean: false
        Assert.False(isBooleanConvertFalse.If(context, actual1));
        Assert.False(isBooleanConvertFalse.If(context, actual2));
        Assert.False(isBooleanConvertFalse.If(context, actual3));
        Assert.False(isBooleanConvertFalse.If(context, actual4));
        Assert.True(isBooleanConvertFalse.If(context, actual5));
        Assert.False(isBooleanConvertFalse.If(context, actual6));
        Assert.True(isBooleanConvertFalse.If(context, actual7));
        Assert.False(isBooleanConvertFalse.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsDateTimeExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
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
        var isDateTimeTrue = GetSelectorVisitor($"{type}IsDateTimeTrue", GetSource(path));
        var isDateTimeFalse = GetSelectorVisitor($"{type}IsDateTimeFalse", GetSource(path));

        // isDateTime: true
        Assert.True(isDateTimeTrue.If(context, actual1));
        Assert.False(isDateTimeTrue.If(context, actual2));
        Assert.False(isDateTimeTrue.If(context, actual3));
        Assert.True(isDateTimeTrue.If(context, actual4));
        Assert.False(isDateTimeTrue.If(context, actual5));
        Assert.True(isDateTimeTrue.If(context, actual6));
        Assert.False(isDateTimeTrue.If(context, actual7));
        Assert.False(isDateTimeTrue.If(context, actual8));
        Assert.False(isDateTimeTrue.If(context, actual9));

        // isDateTime: false
        Assert.False(isDateTimeFalse.If(context, actual1));
        Assert.True(isDateTimeFalse.If(context, actual2));
        Assert.True(isDateTimeFalse.If(context, actual3));
        Assert.False(isDateTimeFalse.If(context, actual4));
        Assert.True(isDateTimeFalse.If(context, actual5));
        Assert.False(isDateTimeFalse.If(context, actual6));
        Assert.True(isDateTimeFalse.If(context, actual7));
        Assert.True(isDateTimeFalse.If(context, actual8));
        Assert.False(isDateTimeFalse.If(context, actual9));

        // With conversion
        var isDateTimeConvertTrue = GetSelectorVisitor($"{type}IsDateTimeTrueWithConversion", GetSource(path));
        var isDateTimeConvertFalse = GetSelectorVisitor($"{type}IsDateTimeFalseWithConversion", GetSource(path));

        // isDateTime: true
        Assert.True(isDateTimeConvertTrue.If(context, actual1));
        Assert.True(isDateTimeConvertTrue.If(context, actual2));
        Assert.True(isDateTimeConvertTrue.If(context, actual3));
        Assert.True(isDateTimeConvertTrue.If(context, actual4));
        Assert.False(isDateTimeConvertTrue.If(context, actual5));
        Assert.True(isDateTimeConvertTrue.If(context, actual6));
        Assert.True(isDateTimeConvertTrue.If(context, actual7));
        Assert.False(isDateTimeConvertTrue.If(context, actual8));
        Assert.False(isDateTimeConvertTrue.If(context, actual9));

        // isDateTime: false
        Assert.False(isDateTimeConvertFalse.If(context, actual1));
        Assert.False(isDateTimeConvertFalse.If(context, actual2));
        Assert.False(isDateTimeConvertFalse.If(context, actual3));
        Assert.False(isDateTimeConvertFalse.If(context, actual4));
        Assert.True(isDateTimeConvertFalse.If(context, actual5));
        Assert.False(isDateTimeConvertFalse.If(context, actual6));
        Assert.False(isDateTimeConvertFalse.If(context, actual7));
        Assert.True(isDateTimeConvertFalse.If(context, actual8));
        Assert.False(isDateTimeConvertFalse.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsIntegerExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var actual1 = GetTargetObject((name: "value", value: 123));
        var actual2 = GetTargetObject((name: "value", value: 1.0f));
        var actual3 = GetTargetObject((name: "value", value: long.MaxValue));
        var actual4 = GetTargetObject((name: "value", value: "123"));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject((name: "value", value: PSObject.AsPSObject(123)));
        var actual7 = GetTargetObject((name: "value", value: byte.MaxValue));
        var actual8 = GetTargetObject();

        // Without conversion
        var isIntegerTrue = GetSelectorVisitor($"{type}IsIntegerTrue", GetSource(path));
        var isIntegerFalse = GetSelectorVisitor($"{type}IsIntegerFalse", GetSource(path));

        // isInteger: true
        Assert.True(isIntegerTrue.If(context, actual1));
        Assert.False(isIntegerTrue.If(context, actual2));
        Assert.True(isIntegerTrue.If(context, actual3));
        Assert.False(isIntegerTrue.If(context, actual4));
        Assert.False(isIntegerTrue.If(context, actual5));
        Assert.True(isIntegerTrue.If(context, actual6));
        Assert.True(isIntegerTrue.If(context, actual7));
        Assert.False(isIntegerTrue.If(context, actual8));

        // isInteger: false
        Assert.False(isIntegerFalse.If(context, actual1));
        Assert.True(isIntegerFalse.If(context, actual2));
        Assert.False(isIntegerFalse.If(context, actual3));
        Assert.True(isIntegerFalse.If(context, actual4));
        Assert.True(isIntegerFalse.If(context, actual5));
        Assert.False(isIntegerFalse.If(context, actual6));
        Assert.False(isIntegerFalse.If(context, actual7));
        Assert.False(isIntegerFalse.If(context, actual8));

        // With conversion
        var isIntegerConvertTrue = GetSelectorVisitor($"{type}IsIntegerTrueWithConversion", GetSource(path));
        var isIntegerConvertFalse = GetSelectorVisitor($"{type}IsIntegerFalseWithConversion", GetSource(path));

        // isInteger: true
        Assert.True(isIntegerConvertTrue.If(context, actual1));
        Assert.False(isIntegerConvertTrue.If(context, actual2));
        Assert.True(isIntegerConvertTrue.If(context, actual3));
        Assert.True(isIntegerConvertTrue.If(context, actual4));
        Assert.False(isIntegerConvertTrue.If(context, actual5));
        Assert.True(isIntegerConvertTrue.If(context, actual6));
        Assert.True(isIntegerConvertTrue.If(context, actual7));
        Assert.False(isIntegerConvertTrue.If(context, actual8));

        // isInteger: false
        Assert.False(isIntegerConvertFalse.If(context, actual1));
        Assert.True(isIntegerConvertFalse.If(context, actual2));
        Assert.False(isIntegerConvertFalse.If(context, actual3));
        Assert.False(isIntegerConvertFalse.If(context, actual4));
        Assert.True(isIntegerConvertFalse.If(context, actual5));
        Assert.False(isIntegerConvertFalse.If(context, actual6));
        Assert.False(isIntegerConvertFalse.If(context, actual7));
        Assert.False(isIntegerConvertFalse.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsNumericExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
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
        var isNumericTrue = GetSelectorVisitor($"{type}IsNumericTrue", GetSource(path));
        var isNumericFalse = GetSelectorVisitor($"{type}IsNumericFalse", GetSource(path));

        // isNumeric: true
        Assert.True(isNumericTrue.If(context, actual1));
        Assert.True(isNumericTrue.If(context, actual2));
        Assert.True(isNumericTrue.If(context, actual3));
        Assert.False(isNumericTrue.If(context, actual4));
        Assert.False(isNumericTrue.If(context, actual5));
        Assert.True(isNumericTrue.If(context, actual6));
        Assert.True(isNumericTrue.If(context, actual7));
        Assert.True(isNumericTrue.If(context, actual8));
        Assert.False(isNumericTrue.If(context, actual9));

        // isNumeric: false
        Assert.False(isNumericFalse.If(context, actual1));
        Assert.False(isNumericFalse.If(context, actual2));
        Assert.False(isNumericFalse.If(context, actual3));
        Assert.True(isNumericFalse.If(context, actual4));
        Assert.True(isNumericFalse.If(context, actual5));
        Assert.False(isNumericFalse.If(context, actual6));
        Assert.False(isNumericFalse.If(context, actual7));
        Assert.False(isNumericFalse.If(context, actual8));
        Assert.False(isNumericFalse.If(context, actual9));

        // With conversion
        var isNumericConvertTrue = GetSelectorVisitor($"{type}IsNumericTrueWithConversion", GetSource(path));
        var isNumericConvertFalse = GetSelectorVisitor($"{type}IsNumericFalseWithConversion", GetSource(path));

        // isNumeric: true
        Assert.True(isNumericConvertTrue.If(context, actual1));
        Assert.True(isNumericConvertTrue.If(context, actual2));
        Assert.True(isNumericConvertTrue.If(context, actual3));
        Assert.True(isNumericConvertTrue.If(context, actual4));
        Assert.False(isNumericConvertTrue.If(context, actual5));
        Assert.True(isNumericConvertTrue.If(context, actual6));
        Assert.True(isNumericConvertTrue.If(context, actual7));
        Assert.True(isNumericConvertTrue.If(context, actual8));
        Assert.False(isNumericConvertTrue.If(context, actual9));

        // isNumeric: false
        Assert.False(isNumericConvertFalse.If(context, actual1));
        Assert.False(isNumericConvertFalse.If(context, actual2));
        Assert.False(isNumericConvertFalse.If(context, actual3));
        Assert.False(isNumericConvertFalse.If(context, actual4));
        Assert.True(isNumericConvertFalse.If(context, actual5));
        Assert.False(isNumericConvertFalse.If(context, actual6));
        Assert.False(isNumericConvertFalse.If(context, actual7));
        Assert.False(isNumericConvertFalse.If(context, actual8));
        Assert.False(isNumericConvertFalse.If(context, actual9));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsLowerExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var isLowerTrue = GetSelectorVisitor($"{type}IsLowerTrue", GetSource(path));
        var isLowerFalse = GetSelectorVisitor($"{type}IsLowerFalse", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "abc"));
        var actual2 = GetTargetObject((name: "value", value: "aBc"));
        var actual3 = GetTargetObject((name: "value", value: "a-b-c"));
        var actual4 = GetTargetObject((name: "value", value: 4));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();

        // isLower: true
        Assert.True(isLowerTrue.If(context, actual1));
        Assert.False(isLowerTrue.If(context, actual2));
        Assert.True(isLowerTrue.If(context, actual3));
        Assert.False(isLowerTrue.If(context, actual4));
        Assert.False(isLowerTrue.If(context, actual5));
        Assert.False(isLowerTrue.If(context, actual6));

        // isLower: false
        Assert.False(isLowerFalse.If(context, actual1));
        Assert.True(isLowerFalse.If(context, actual2));
        Assert.False(isLowerFalse.If(context, actual3));
        Assert.True(isLowerFalse.If(context, actual4));
        Assert.True(isLowerFalse.If(context, actual5));
        Assert.False(isLowerFalse.If(context, actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsLowerExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameIsLower", GetSource(path));
        var actual7 = GetTargetObject(
           (name: "Name", value: "targetobject1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.True(withName.If(context, actual7));
        Assert.False(withName.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsUpperExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var isUpperTrue = GetSelectorVisitor($"{type}IsUpperTrue", GetSource(path));
        var isUpperFalse = GetSelectorVisitor($"{type}IsUpperFalse", GetSource(path));
        var actual1 = GetTargetObject((name: "value", value: "ABC"));
        var actual2 = GetTargetObject((name: "value", value: "aBc"));
        var actual3 = GetTargetObject((name: "value", value: "A-B-C"));
        var actual4 = GetTargetObject((name: "value", value: 4));
        var actual5 = GetTargetObject((name: "value", value: null));
        var actual6 = GetTargetObject();

        // isUpper: true
        Assert.True(isUpperTrue.If(context, actual1));
        Assert.False(isUpperTrue.If(context, actual2));
        Assert.True(isUpperTrue.If(context, actual3));
        Assert.False(isUpperTrue.If(context, actual4));
        Assert.False(isUpperTrue.If(context, actual5));
        Assert.False(isUpperTrue.If(context, actual6));

        // isUpper: false
        Assert.False(isUpperFalse.If(context, actual1));
        Assert.True(isUpperFalse.If(context, actual2));
        Assert.False(isUpperFalse.If(context, actual3));
        Assert.True(isUpperFalse.If(context, actual4));
        Assert.True(isUpperFalse.If(context, actual5));
        Assert.False(isUpperFalse.If(context, actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void IsUpperExpression_WithName(string type, string path)
    {
        var context = GetTestExpressionContext();
        var withName = GetSelectorVisitor($"{type}NameIsUpper", GetSource(path));
        var actual7 = GetTargetObject(
           (name: "Name", value: "TARGETOBJECT1")
        );
        var actual8 = GetTargetObject(
           (name: "Name", value: "TargetObject2")
        );

        Assert.True(withName.If(context, actual7));
        Assert.False(withName.If(context, actual8));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasSchemaExpression(string type, string path)
    {
        var context = GetTestExpressionContext();
        var hasSchema = GetSelectorVisitor($"{type}HasSchema", GetSource(path));
        var actual1 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#"));
        var actual2 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json"));
        var actual3 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: "http://schema.management.azure.com/schemas/2019-04-01/DeploymentParameters.json#"));
        var actual4 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: null));
        var actual5 = GetTargetObject((name: "key", value: "value"), (name: "$schema", value: ""));
        var actual6 = GetTargetObject();

        Assert.True(hasSchema.If(context, actual1));
        Assert.True(hasSchema.If(context, actual2));
        Assert.False(hasSchema.If(context, actual3));
        Assert.False(hasSchema.If(context, actual4));
        Assert.False(hasSchema.If(context, actual5));
        Assert.False(hasSchema.If(context, actual6));

        hasSchema = GetSelectorVisitor($"{type}HasSchemaIgnoreScheme", GetSource(path));
        Assert.True(hasSchema.If(context, actual1));
        Assert.True(hasSchema.If(context, actual2));
        Assert.True(hasSchema.If(context, actual3));
        Assert.False(hasSchema.If(context, actual4));
        Assert.False(hasSchema.If(context, actual5));
        Assert.False(hasSchema.If(context, actual6));

        hasSchema = GetSelectorVisitor($"{type}HasSchemaCaseSensitive", GetSource(path));
        Assert.True(hasSchema.If(context, actual1));
        Assert.True(hasSchema.If(context, actual2));
        Assert.False(hasSchema.If(context, actual3));
        Assert.False(hasSchema.If(context, actual4));
        Assert.False(hasSchema.If(context, actual5));
        Assert.False(hasSchema.If(context, actual6));

        hasSchema = GetSelectorVisitor($"{type}HasAnySchema", GetSource(path));
        Assert.True(hasSchema.If(context, actual1));
        Assert.True(hasSchema.If(context, actual2));
        Assert.True(hasSchema.If(context, actual3));
        Assert.False(hasSchema.If(context, actual4));
        Assert.False(hasSchema.If(context, actual5));
        Assert.False(hasSchema.If(context, actual6));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Version(string type, string path)
    {
        var context = GetTestExpressionContext();
        var actual1 = GetTargetObject((name: "version", value: "1.2.3"));
        var actual2 = GetTargetObject((name: "version", value: "0.2.3"));
        var actual3 = GetTargetObject((name: "version", value: "2.2.3"));
        var actual4 = GetTargetObject((name: "version", value: "1.1.3"));
        var actual5 = GetTargetObject((name: "version", value: "1.3.3-preview.1"));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "version", value: "a.b.c"));

        var visitor = GetSelectorVisitor($"{type}Version", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));

        visitor = GetSelectorVisitor($"{type}VersionWithPrerelease", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.False(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));

        visitor = GetSelectorVisitor($"{type}VersionAnyVersion", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));

        visitor = GetSelectorVisitor($"{type}VersionAnyStableVersion", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void APIVersion(string type, string path)
    {
        var context = GetTestExpressionContext();
        var actual1 = GetTargetObject((name: "dateVersion", value: "2015-10-01"));
        var actual2 = GetTargetObject((name: "dateVersion", value: "2014-01-01"));
        var actual3 = GetTargetObject((name: "dateVersion", value: "2022-01-01"));
        var actual4 = GetTargetObject((name: "dateVersion", value: "2015-10-01-preview"));
        var actual5 = GetTargetObject((name: "dateVersion", value: "2022-01-01-preview"));
        var actual6 = GetTargetObject();
        var actual7 = GetTargetObject((name: "dateVersion", value: "a-b-c"));

        var visitor = GetSelectorVisitor($"{type}APIVersion", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));

        visitor = GetSelectorVisitor($"{type}APIVersionWithPrerelease", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.False(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));

        visitor = GetSelectorVisitor($"{type}APIVersionAnyVersion", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.True(visitor.If(context, actual4));
        Assert.True(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));

        visitor = GetSelectorVisitor($"{type}APIVersionAnyStableVersion", GetSource(path));
        Assert.True(visitor.If(context, actual1));
        Assert.True(visitor.If(context, actual2));
        Assert.True(visitor.If(context, actual3));
        Assert.False(visitor.If(context, actual4));
        Assert.False(visitor.If(context, actual5));
        Assert.False(visitor.If(context, actual6));
        Assert.False(visitor.If(context, actual7));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void HasDefault(string type, string path)
    {
        var context = GetTestExpressionContext();
        var actual1 = GetTargetObject((name: "integerValue", value: 100), (name: "boolValue", value: true), (name: "stringValue", value: "testValue"));
        var actual2 = GetTargetObject((name: "integerValue", value: 1));
        var actual3 = GetTargetObject((name: "boolValue", value: false));
        var actual4 = GetTargetObject((name: "stringValue", value: "TestValue"));
        var actual5 = GetTargetObject();
        var actual6 = GetTargetObject((name: "integerValue", value: new JValue(100)));
        var actual7 = GetTargetObject((name: "boolValue", value: new JValue(true)));
        var actual8 = GetTargetObject((name: "stringValue", value: new JValue("testValue")));

        var hasDefault = GetSelectorVisitor($"{type}HasDefault", GetSource(path));
        Assert.True(hasDefault.If(context, actual1));
        Assert.False(hasDefault.If(context, actual2));
        Assert.False(hasDefault.If(context, actual3));
        Assert.False(hasDefault.If(context, actual4));
        Assert.True(hasDefault.If(context, actual5));
        Assert.True(hasDefault.If(context, actual6));
        Assert.True(hasDefault.If(context, actual7));
        Assert.True(hasDefault.If(context, actual8));
    }

    #endregion Conditions

    #region Operators

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void AllOf(string type, string path)
    {
        var context = GetTestExpressionContext();
        var allOf = GetSelectorVisitor($"{type}AllOf", GetSource(path));
        var actual1 = GetTargetObject((name: "Name", value: "Name1"));
        var actual2 = GetTargetObject((name: "AlternateName", value: "Name2"));
        var actual3 = GetTargetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
        var actual4 = GetTargetObject((name: "OtherName", value: "Name3"));

        Assert.False(allOf.If(context, actual1));
        Assert.False(allOf.If(context, actual2));
        Assert.True(allOf.If(context, actual3));
        Assert.False(allOf.If(context, actual4));

        // With quantifier
        allOf = GetSelectorVisitor($"{type}AllOfWithQuantifier", GetSource(path));
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

        Assert.True(allOf.If(context, actual1));
        Assert.True(allOf.If(context, actual2));
        Assert.False(allOf.If(context, actual3));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void AnyOf(string type, string path)
    {
        var context = GetTestExpressionContext();
        var allOf = GetSelectorVisitor($"{type}AnyOf", GetSource(path));
        var actual1 = GetTargetObject((name: "Name", value: "Name1"));
        var actual2 = GetTargetObject((name: "AlternateName", value: "Name2"));
        var actual3 = GetTargetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
        var actual4 = GetTargetObject((name: "OtherName", value: "Name3"));

        Assert.True(allOf.If(context, actual1));
        Assert.True(allOf.If(context, actual2));
        Assert.True(allOf.If(context, actual3));
        Assert.False(allOf.If(context, actual4));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Not(string type, string path)
    {
        var context = GetTestExpressionContext();
        var allOf = GetSelectorVisitor($"{type}Not", GetSource(path));
        var actual1 = GetTargetObject((name: "Name", value: "Name1"));
        var actual2 = GetTargetObject((name: "AlternateName", value: "Name2"));
        var actual3 = GetTargetObject((name: "Name", value: "Name1"), (name: "AlternateName", value: "Name2"));
        var actual4 = GetTargetObject((name: "OtherName", value: "Name3"));

        Assert.False(allOf.If(context, actual1));
        Assert.False(allOf.If(context, actual2));
        Assert.False(allOf.If(context, actual3));
        Assert.True(allOf.If(context, actual4));
    }

    #endregion Operators

    #region Properties

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Type(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}TypeEquals", GetSource(path));
        var actual = new TargetObject(GetObject(), type: "CustomType1");

        Assert.True(visitor.If(context, actual));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void TypePrecondition(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}TypePrecondition", GetSource(path));
        var actual = new TargetObject(GetObject(), type: "CustomType1");

        // context.EnterLanguageScope(equals.Source);
        Assert.True(visitor.If(context, actual));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Name(string type, string path)
    {
        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}NameEquals", GetSource(path));
        var actual = GetTargetObject(
            (name: "Name", value: "TargetObject1")
        );

        Assert.True(visitor.If(context, actual));
    }

    [Theory]
    [InlineData("Yaml", SelectorYamlFileName)]
    [InlineData("Json", SelectorJsonFileName)]
    public void Scope(string type, string path)
    {
        var testObject = GetObject(
            (name: "Name", value: "TargetObject1")
        );

        var context = GetTestExpressionContext();
        var visitor = GetSelectorVisitor($"{type}ScopeEquals", GetSource(path));

        var actual = new TargetObject(testObject, scope: ["/scope1"]);
        Assert.True(visitor.If(context, actual));

        actual = new TargetObject(testObject, scope: ["/scope2"]);
        Assert.False(visitor.If(context, actual));

        actual = new TargetObject(testObject);
        Assert.False(visitor.If(context, actual));

        visitor = GetSelectorVisitor($"{type}ScopeStartsWith", GetSource(path));

        actual = new TargetObject(testObject, scope: ["/scope1/"]);
        Assert.True(visitor.If(context, actual));

        actual = new TargetObject(testObject, scope: ["/scope2/"]);
        Assert.True(visitor.If(context, actual));

        actual = new TargetObject(testObject, scope: ["/scope2"]);
        Assert.False(visitor.If(context, actual));

        actual = new TargetObject(testObject);
        Assert.False(visitor.If(context, actual));

        visitor = GetSelectorVisitor($"{type}ScopeHasValueFalse", GetSource(path));

        actual = new TargetObject(testObject, scope: ["/scope1"]);
        Assert.False(visitor.If(context, actual));

        actual = new TargetObject(testObject);
        Assert.True(visitor.If(context, actual));

        visitor = GetSelectorVisitor($"{type}ScopeHasValueTrue", GetSource(path));

        actual = new TargetObject(testObject, scope: ["/scope1"]);
        Assert.True(visitor.If(context, actual));

        actual = new TargetObject(testObject);
        Assert.False(visitor.If(context, actual));
    }

    #endregion Properties

    #region Functions

    [Theory]
    [InlineData("Yaml", FunctionsYamlFileName)]
    [InlineData("Json", FunctionsJsonFileName)]
    public void WithFunction(string type, string path)
    {
        var context = GetTestExpressionContext();
        var example1 = GetSelectorVisitor($"{type}.Fn.Example1", GetSource(path));
        var example2 = GetSelectorVisitor($"{type}.Fn.Example2", GetSource(path));
        var example3 = GetSelectorVisitor($"{type}.Fn.Example3", GetSource(path));
        var example4 = GetSelectorVisitor($"{type}.Fn.Example4", GetSource(path));
        var example5 = GetSelectorVisitor($"{type}.Fn.Example5", GetSource(path));
        var example6 = GetSelectorVisitor($"{type}.Fn.Example6", GetSource(path));
        var actual1 = GetTargetObject(
            (name: "Name", value: "TestObject1")
        );

        Assert.True(example1.If(context, actual1));
        Assert.True(example2.If(context, actual1));
        Assert.True(example3.If(context, actual1));
        Assert.True(example4.If(context, actual1));
        Assert.True(example5.If(context, actual1));
        Assert.True(example6.If(context, actual1));
    }

    [Theory]
    [InlineData("Yaml", FunctionsYamlFileName)]
    [InlineData("Json", FunctionsJsonFileName)]
    public void WithFunctionSpecific(string type, string path)
    {
        var context = GetTestExpressionContext();
        var example1 = GetSelectorVisitor($"{type}.Fn.Replace", GetSource(path));
        var example2 = GetSelectorVisitor($"{type}.Fn.Trim", GetSource(path));
        var example3 = GetSelectorVisitor($"{type}.Fn.First", GetSource(path));
        var example4 = GetSelectorVisitor($"{type}.Fn.Last", GetSource(path));
        var example5 = GetSelectorVisitor($"{type}.Fn.Split", GetSource(path));
        var example6 = GetSelectorVisitor($"{type}.Fn.PadLeft", GetSource(path));
        var example7 = GetSelectorVisitor($"{type}.Fn.PadRight", GetSource(path));
        var actual1 = GetTargetObject(
            (name: "Name", value: "TestObject1")
        );

        Assert.True(example1.If(context, actual1));
        Assert.True(example2.If(context, actual1));
        Assert.True(example3.If(context, actual1));
        Assert.True(example4.If(context, actual1));
        Assert.True(example5.If(context, actual1));
        Assert.True(example6.If(context, actual1));
        Assert.True(example7.If(context, actual1));
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

    private SelectorVisitor GetSelectorVisitor(string name, Source[] sources)
    {
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var optionBuilder = new OptionContextBuilder(option: GetOption(), bindTargetName: PipelineHookActions.BindTargetName, bindTargetType: PipelineHookActions.BindTargetType, bindField: PipelineHookActions.BindField);
        // context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), sources: sources, optionBuilder: optionBuilder, resourceCache: resourcesCache));
        // context.Initialize(sources);
        // context.Begin();
        var selector = resourcesCache.OfType<ISelector>().FirstOrDefault(s => s.Id.Name == name);
        // context.EnterLanguageScope(selector.Source);
        return selector.ToSelectorVisitor();
    }

    #endregion Helper methods
}
