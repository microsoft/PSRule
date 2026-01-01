// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// Tests for <see cref="OptionContext"/>.
/// </summary>
public sealed class OptionContextTests : BaseTests
{
    [Fact]
    public void Build()
    {
        // Create option context
        var builder = new OptionContextBuilder(GetOption());

        // Check empty scope
        var languageScope = new LanguageScope(".", null);
        languageScope.Configure(builder.Build(languageScope.Name));
        Assert.Equal(new string[] { "en-ZZ" }, languageScope.Culture);
    }

    [Fact]
    public void Order()
    {
        // Create option context
        var builder = new OptionContextBuilder(GetOption());

        var languageScope = new LanguageScope(".", null);
        languageScope.Configure(builder.Build(null));

        var ruleFilter = languageScope.GetFilter(ResourceKind.Rule) as RuleFilter;
        Assert.NotNull(ruleFilter);
        Assert.True(ruleFilter.IncludeLocal);

        // With explicit baseline
        builder = new OptionContextBuilder(GetOption());
        builder.Baseline(ScopeType.Explicit, "BaselineExplicit", null, GetBaseline(ruleInclude: ["abc"]), false);
        languageScope.Configure(builder.Build(languageScope.Name));
        ruleFilter = languageScope.GetFilter(ResourceKind.Rule) as RuleFilter;
        Assert.NotNull(ruleFilter);
        Assert.False(ruleFilter.IncludeLocal);

        // With include from parameters
        builder = new OptionContextBuilder(GetOption(), include: ["abc"]);
        languageScope.Configure(builder.Build(languageScope.Name));
        ruleFilter = languageScope.GetFilter(ResourceKind.Rule) as RuleFilter;
        Assert.NotNull(ruleFilter);
        Assert.False(ruleFilter.IncludeLocal);

        builder = new OptionContextBuilder(GetOption(ruleInclude: ["abc"]));
        languageScope.Configure(builder.Build(languageScope.Name));
        ruleFilter = languageScope.GetFilter(ResourceKind.Rule) as RuleFilter;
        Assert.NotNull(ruleFilter);
        Assert.True(ruleFilter.IncludeLocal);
    }

    /// <summary>
    /// Test that options from separate files can be combined.
    /// </summary>
    [Fact]
    public void Merge_multiple_options_from_file()
    {
        var builder = new OptionContextBuilder();

        builder.Workspace(GetOptionFromFile("PSRule.Tests2.yml"));
        builder.Workspace(GetOptionFromFile());
        builder.Workspace(GetOption());

        var context = builder.Build(null);

        // With workspace options ordered by first
        Assert.Equal(new[] { "ResourceName", "AlternateName" }, context.Binding.TargetName);
        Assert.Equal(new[] { "ResourceType", "kind" }, context.Binding.TargetType);
        Assert.Equal(new[] { "virtualMachine", "virtualNetwork" }, context.Input.TargetType);
        Assert.Equal(new[] { "en-CC", "en-DD" }, context.Output.Culture);
        Assert.True(context.Configuration.TryGetStringArray("option5", out var option5));
        Assert.Equal(new[] { "option5a", "option5b" }, option5);
        Assert.True(context.Configuration.TryGetString("option6", out var option6));
        Assert.Equal("value6", option6);

        // With module default baseline
        builder.Baseline(ScopeType.Module, "BaselineDefault", "Module1", GetBaseline(targetType: ["defaultType"], ruleInclude: ["defaultRule"]), false);
        context = builder.Build(null);

        Assert.Equal(new[] { "ResourceName", "AlternateName" }, context.Binding.TargetName);
        Assert.Equal(new[] { "ResourceType", "kind" }, context.Binding.TargetType);
        Assert.Equal(ResourceHelper.GetResourceIdReference(["rule1", "rule2"]), context.Rule.Include);

        context = builder.Build("Module1");

        Assert.Equal(new[] { "ResourceName", "AlternateName" }, context.Binding.TargetName);
        Assert.Equal(new[] { "ResourceType", "kind" }, context.Binding.TargetType);
        Assert.Equal(ResourceHelper.GetResourceIdReference(["rule1", "rule2"]), context.Rule.Include);

        // With explicit baseline
        builder.Baseline(ScopeType.Explicit, "BaselineExplicit", "Module1", GetBaseline(), false);
        context = builder.Build(null);

        Assert.Equal(new[] { "ResourceName", "AlternateName" }, context.Binding.TargetName);
        Assert.Equal(new[] { "ResourceType", "kind" }, context.Binding.TargetType);
        Assert.Equal(ResourceHelper.GetResourceIdReference(["rule1"]), context.Rule.Include);

        context = builder.Build("Module1");

        Assert.Equal(new[] { "ResourceName", "AlternateName" }, context.Binding.TargetName);
        Assert.Equal(new[] { "ResourceType", "kind" }, context.Binding.TargetType);
        Assert.Equal(ResourceHelper.GetResourceIdReference(["rule1"]), context.Rule.Include);
    }

    #region Helper methods

    private static PSRuleOption GetOption(string[]? culture = null, string[]? ruleInclude = null)
    {
        var option = new PSRuleOption();

        // Specify a culture otherwise it varies within CI.
        option.Output.Culture = culture ?? ["en-ZZ"];

        option.Rule.Include = ResourceHelper.GetResourceIdReference(ruleInclude);

        // Add a configuration option.
        option.Configuration.Add("option6", "value6");
        option.Configuration.Add("option5", "value5");
        return option;
    }

    private static PSRuleOption GetOptionFromFile(string file = "PSRule.Tests.yml")
    {
        return PSRuleOption.FromFileOrEmpty(GetSourcePath(file));
    }

    private static BaselineSpec GetBaseline(string[]? targetType = null, string[]? ruleInclude = null)
    {
        ruleInclude ??= ["rule1"];
        return new BaselineSpec
        {
            Rule = new RuleOption
            {
                Include = ResourceHelper.GetResourceIdReference(ruleInclude)
            }
        };
    }

    #endregion Helper methods
}
