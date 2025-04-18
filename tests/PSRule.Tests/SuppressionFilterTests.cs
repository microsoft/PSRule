// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule;

public sealed class SuppressionFilterTests : ContextBaseTests
{
    [Fact]
    public void Match()
    {
        var option = GetOption();
        var sources = GetSource();
        var context = new LegacyRunspaceContext(GetPipelineContext(option: option, sources: sources));
        context.Initialize(sources);
        context.Begin();
        var rules = HostHelper.GetRule(context, includeDependencies: false);
        var resourceIndex = new ResourceIndex(rules);
        var filter = new SuppressionFilter(context, option.Suppression, resourceIndex);

        Assert.True(filter.Match(new ResourceId(".", "YAML.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject1"));
        Assert.False(filter.Match(new ResourceId(".", "JSON.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject1"));
        Assert.True(filter.Match(new ResourceId(".", "PS.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject1"));

        Assert.True(filter.Match(new ResourceId(".", "YAML.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject2"));
        Assert.True(filter.Match(new ResourceId(".", "JSON.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject2"));
        Assert.False(filter.Match(new ResourceId(".", "PS.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject2"));

        Assert.False(filter.Match(new ResourceId(".", "YAML.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject3"));
        Assert.True(filter.Match(new ResourceId(".", "JSON.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject3"));
        Assert.True(filter.Match(new ResourceId(".", "PS.RuleWithAlias1", ResourceIdKind.Unknown), "TestObject3"));
    }

    #region Helper methods

    private static Source[] GetSource()
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath("FromFileAlias.Rule.yaml"));
        builder.Directory(GetSourcePath("FromFileAlias.Rule.jsonc"));
        builder.Directory(GetSourcePath("FromFileAlias.Rule.ps1"));
        return builder.Build();
    }

    protected override PSRuleOption GetOption()
    {
        return PSRuleOption.FromFileOrEmpty("PSRule.Tests14.yml");
    }

    #endregion Helper methods
}
