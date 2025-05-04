// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// Tests for <see cref="SuppressionGroupV1"/>.
/// </summary>
public sealed class SuppressionGroupTests : ContextBaseTests
{
    /// <summary>
    /// Test that suppression groups V1 can be imported.
    /// </summary>
    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void Import_WhenSuppressionGroupIsNotExpired_ShouldReturnMatchingVisitor(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionContext(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
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

    /// <summary>
    /// Test that suppression groups V2 can be imported.
    /// </summary>
    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void Import_WhenSuppressionGroupV2HasTypePrecondition_ShouldReturnMatchingVisitor(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionContext(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();

        var suppressionGroup = resourcesCache.OfType<SuppressionGroupV2>().ToArray();
        Assert.NotNull(suppressionGroup);
        Assert.Single(suppressionGroup);

        var actual = suppressionGroup[0];
        var visitor = context.Pipeline.SuppressionGroup.FirstOrDefault(g => g.Id == actual.Id);
        Assert.Equal("SuppressWithTestTypePrecondition", actual.Name);
        Assert.Equal("Ignore test objects by type precondition.", visitor.Info.Synopsis.Text);
        Assert.Null(actual.Spec.ExpiresOn);
        Assert.Equal("TestType", actual.Spec.Type[0]);
        Assert.Contains(context.Pipeline.SuppressionGroup, g => g.Id.Equals(".\\SuppressWithTestTypePrecondition"));
    }

    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void Import_WhenSuppressionGroupIsExpired_ShouldReturnIssue(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionContext(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();

        var resourceId = ".\\SuppressWithExpiry";

        var suppressionGroup = resourcesCache.OfType<SuppressionGroupV1>().Where(g => g.Id.Equals(resourceId));
        Assert.Empty(suppressionGroup);

        var issues = resourcesCache.Issues.Where(issue => issue.ResourceId.Equals(resourceId));
        Assert.Single(issues);
    }

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
