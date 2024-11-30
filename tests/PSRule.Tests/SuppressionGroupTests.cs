// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using PSRule.Configuration;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

public sealed class SuppressionGroupTests : ContextBaseTests
{
    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void Import_WhenSuppressionGroupIsNotExpired_ShouldReturnMatchingVisitor(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new RunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionContext(), sources: sources, resourceCache: resourcesCache));
        context.Init(sources);
        context.Begin();

        var suppressionGroup = resourcesCache.OfType<SuppressionGroupV1>().ToArray();
        Assert.NotNull(suppressionGroup);
        Assert.Equal(4, suppressionGroup.Length);

        var actual = suppressionGroup[0];
        var visitor = context.Pipeline.SuppressionGroup.FirstOrDefault(g => g.Id == actual.Id);
        Assert.Equal("SuppressWithTargetName", actual.Name);
        Assert.Equal("Ignore test objects by name.", visitor.Info.Synopsis.Text);
        Assert.Null(actual.Spec.ExpiresOn);
        Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithTargetName"));

        actual = suppressionGroup[1];
        visitor = context.Pipeline.SuppressionGroup.FirstOrDefault(g => g.Id == actual.Id);
        Assert.Equal("SuppressWithTestType", actual.Name);
        Assert.Equal("Ignore test objects by type.", visitor.Info.Synopsis.Text);
        Assert.Null(actual.Spec.ExpiresOn);
        Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithTestType"));

        actual = suppressionGroup[2];
        visitor = context.Pipeline.SuppressionGroup.FirstOrDefault(g => g.Id == actual.Id);
        Assert.Equal("SuppressWithNonProdTag", actual.Name);
        Assert.Equal("Ignore objects with non-production tag.", visitor.Info.Synopsis.Text);
        Assert.Null(actual.Spec.ExpiresOn);
        Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithNonProdTag"));

        actual = suppressionGroup[3];
        visitor = context.Pipeline.SuppressionGroup.FirstOrDefault(g => g.Id == actual.Id);
        Assert.Equal("SuppressByScope", actual.Name);
        Assert.Equal("Suppress by scope.", actual.Info.Synopsis.Text);
    }

    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void Import_WhenSuppressionGroupIsExpired_ShouldReturnIssue(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new RunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionContext(), sources: sources, resourceCache: resourcesCache));
        context.Init(sources);
        context.Begin();

        var suppressionGroup = resourcesCache.OfType<SuppressionGroupV1>().Where(g => g.Id.Equals(".\\SuppressWithExpiry"));
        Assert.Empty(suppressionGroup);

        var issues = resourcesCache.Issues.Where(issue => issue.Resource.Id.Equals(".\\SuppressWithExpiry"));
        Assert.Single(issues);

        var actual = issues.FirstOrDefault().Resource as SuppressionGroupV1;
        Assert.Equal("SuppressWithExpiry", actual.Name);
        Assert.Equal("Suppress with expiry.", actual.Info.Synopsis.Text);
        Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z").ToUniversalTime(), actual.Spec.ExpiresOn);
    }

    //[Theory]
    //[InlineData("SuppressionGroups.Rule.yaml")]
    //[InlineData("SuppressionGroups.Rule.jsonc")]
    //public void EvaluateSuppressionGroup(string path)
    //{
    //    var context = new RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, GetOptionContext(), null), null);
    //    context.Init(GetSource(path));
    //    context.Begin();
    //    var suppressionGroup = HostHelper.GetSuppressionGroup(GetSource(path), context).ToArray();
    //    Assert.NotNull(suppressionGroup);

    //    var testObject = GetObject((name: "name", value: "TestObject1"));
    //    context.EnterTargetObject(new TargetObject(testObject, targetName: "TestObject1", scope: "/scope1"));

    //    var actual = suppressionGroup[0];
    //    var visitor = new SuppressionGroupVisitor(context, actual.Id, actual.Source, actual.Spec, actual.Info);
    //    Assert.True(visitor.TryMatch(testObject, out _));

    //    actual = suppressionGroup[4];
    //    visitor = new SuppressionGroupVisitor(context, actual.Id, actual.Source, actual.Spec, actual.Info);
    //    //Assert.True(visitor.TryMatch());
    //}

    #region Helper methods

    protected sealed override PSRuleOption GetOption()
    {
        var option = new PSRuleOption();
        option.Output.Culture = ["en-US", "en"];
        return option;
    }

    private OptionContextBuilder GetOptionContext()
    {
        return new OptionContextBuilder(GetOption());
    }

    #endregion Helper methods
}
