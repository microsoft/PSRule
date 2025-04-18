// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// Tests for <see cref="SuppressionGroupVisitor"/>.
/// </summary>
public sealed class SuppressionGroupVisitorTests : ContextBaseTests
{
    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void TryMatch_WithType_ShouldMatchTestType(string path)
    {
        var suppressionGroup = GetSuppressionGroupVisitor("SuppressWithTestType", GetSource(path), out var context);
        var testObject = new TargetObject(GetObject((name: "value", value: 3)), "TestName", "TestType");
        context.EnterTargetObject(testObject);
        context.EnterLanguageScope(suppressionGroup.Source);

        Assert.True(suppressionGroup.TryMatch(ResourceId.Parse("FromFile3"), testObject, out _));
    }

    [Theory]
    [InlineData("SuppressionGroups.Rule.yaml")]
    [InlineData("SuppressionGroups.Rule.jsonc")]
    public void TryMatch_WithTypePrecondition_ShouldMatchTestType(string path)
    {
        var suppressionGroup = GetSuppressionGroupVisitor("SuppressWithTestTypePrecondition", GetSource(path), out var context);
        var testObject = new TargetObject(GetObject((name: "value", value: 3)), "TestName", "TestType");
        context.EnterTargetObject(testObject);
        context.EnterLanguageScope(suppressionGroup.Source);

        Assert.True(suppressionGroup.TryMatch(ResourceId.Parse("FromFile3"), testObject, out _));
    }

    #region Helper methods

    protected sealed override PSRuleOption GetOption()
    {
        var option = new PSRuleOption();
        option.Output.Culture = ["en-US", "en"];
        option.Binding.PreferTargetInfo = true;
        return option;
    }

    private SuppressionGroupVisitor GetSuppressionGroupVisitor(string name, Source[] sources, out LegacyRunspaceContext context)
    {
        var optionBuilder = new OptionContextBuilder(option: GetOption(), bindTargetName: PipelineHookActions.BindTargetName, bindTargetType: PipelineHookActions.BindTargetType, bindField: PipelineHookActions.BindField);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), sources: sources, optionBuilder: optionBuilder, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();
        var suppressionGroup = resourcesCache.OfType<ISuppressionGroup>().Where(g => g.Id.Name == name).FirstOrDefault();
        context.EnterLanguageScope(suppressionGroup.Source);
        return suppressionGroup.ToSuppressionGroupVisitor(context);
    }

    #endregion Helper methods
}
