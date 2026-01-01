// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Runtime;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// A test class for <see cref="RunCollectionBuilder"/>.
/// </summary>
public sealed class RunCollectionBuilderTests : ContextBaseTests
{
    /// <summary>
    /// A test that builds a run based on the default set of rules.
    /// </summary>
    [Fact]
    public void Build_WithDefault_ShouldSelectAllRules()
    {
        var sources = GetSourceAsModule(".", "TestModule7", "0.0.1");
        var resourceCache = GetResourceCache(sources: sources);
        var languageScopeSet = GetLanguageScopeSet(sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, resourceCache: resourceCache, languageScope: languageScopeSet));

        var builder = new RunCollectionBuilder(resourceCache, null, null, languageScopeSet, "TestInstance");
        var runs = builder.WithDefaultRun(context).Build();

        Assert.Single(runs);
        Assert.Equal(1, runs.RuleCount);
    }

    /// <summary>
    /// An baseline should only select rules within its own scope by default unless a fully qualified name is used.
    /// </summary>
    [Fact]
    public void Build_WithDefaultMultipleModule_ShouldSelectRulesFromEachBaseline()
    {
        var sources = GetSourceAsModule(".", "TestModule9a", "0.0.1").Concat(GetSourceAsModule(".", "TestModule9b", "0.0.1")).ToArray();
        var resourceCache = GetResourceCache(sources: sources);
        var languageScopeSet = GetLanguageScopeSet(sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, resourceCache: resourceCache, languageScope: languageScopeSet));

        var builder = new RunCollectionBuilder(resourceCache, null, null, GetLanguageScopeSet(sources), "TestInstance");
        var runs = builder.WithDefaultRun(context).Build();

        Assert.Equal(4, runs.RuleCount);
    }

    /// <summary>
    /// A test that builds a set of rules for a baseline.
    /// </summary>
    [Fact]
    public void Build_WithBaseline_ShouldSelectScopedRules()
    {
        var sources = GetSourceAsModule(".", "TestModule7", "0.0.1");
        var resourceCache = GetResourceCache(sources: sources);

        var builder = new RunCollectionBuilder(resourceCache, null, null, GetLanguageScopeSet(sources), "TestInstance");
        var runs = builder.WithBaselines(["Module7"]).Build();

        Assert.Single(runs);
        Assert.Equal(1, runs.RuleCount);
    }

    [Fact]
    public void Build_WithBaseline_ShouldUseBaselineConfiguration()
    {
        var sources = GetSourceAsModule(".", "TestModule7", "0.0.1");
        var resourceCache = GetResourceCache(sources: sources);

        var builder = new RunCollectionBuilder(resourceCache, null, null, GetLanguageScopeSet(sources), "TestInstance");
        var runs = builder.WithBaselines(["Module7"]).Build();

        var actual = runs.FirstOrDefault();
        Assert.NotNull(actual);

        Assert.True(actual.TryConfigurationValue("key1", out var value1));
        Assert.Equal("baselineConfig", value1);

        Assert.True(actual.TryConfigurationValue("key2", out var value2));
        Assert.Equal("moduleConfig", value2);
    }

    [Fact]
    public void Build_WithModuleConfig_ShouldDefaultToModuleConfiguration()
    {
        var sources = GetSourceAsModule(".", "TestModule7", "0.0.1");
        var resourceCache = GetResourceCache(sources: sources);
        var languageScopeSet = GetLanguageScopeSet(sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(sources: sources, resourceCache: resourceCache, languageScope: languageScopeSet));

        var builder = new RunCollectionBuilder(resourceCache, null, null, GetLanguageScopeSet(sources), "TestInstance");
        var runs = builder.WithDefaultRun(context).Build();

        var actual = runs.FirstOrDefault(r => r.Scope == "TestModule7");
        Assert.NotNull(actual);

        Assert.True(actual.TryConfigurationValue("key1", out var value1));
        Assert.Equal("baselineConfig", value1);

        Assert.True(actual.TryConfigurationValue("key2", out var value2));
        Assert.Equal("moduleConfig", value2);
    }

    /// <summary>
    /// Local configuration options should always win when contested.
    /// </summary>
    [Fact]
    public void Build_WithLocalOptions_ShouldUseOptionConfigurationValues()
    {
        var sources = GetSourceAsModule(".", "TestModule7", "0.0.1");
        var resourceCache = GetResourceCache(sources: sources);
        var option = GetOption();
        option.Configuration["key1"] = "localConfig";
        option.Configuration["key3"] = "localConfig";

        var builder = new RunCollectionBuilder(resourceCache, null, option, GetLanguageScopeSet(sources), "TestInstance");
        var runs = builder.WithBaselines(["Module7"]).Build();

        var actual = runs.FirstOrDefault();
        Assert.NotNull(actual);

        Assert.True(actual.TryConfigurationValue("key1", out var value1));
        Assert.Equal("localConfig", value1);

        Assert.True(actual.TryConfigurationValue("key2", out var value2));
        Assert.Equal("moduleConfig", value2);

        Assert.True(actual.TryConfigurationValue("key3", out var value3));
        Assert.Equal("localConfig", value3);
    }

    /// <summary>
    /// Local configuration options should be applied to the workspace run when set.
    /// </summary>
    [Fact]
    public void Build_WithLocalOptions_ShouldUseOptionsForWorkspaceRun()
    {
        var option = GetOption();
        option.Configuration["key1"] = "localConfig";

        var sources = GetSource("John's Documents/");
        var resourceCache = GetResourceCache(sources: sources);
        var languageScopeSet = GetLanguageScopeSet(sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: option, sources: sources, resourceCache: resourceCache, languageScope: languageScopeSet));

        var builder = new RunCollectionBuilder(resourceCache, null, option, GetLanguageScopeSet(sources), "TestInstance");
        var runs = builder.WithDefaultRun(context).Build();

        var actual = runs.FirstOrDefault();
        Assert.NotNull(actual);

        actual.TryConfigurationValue("key1", out var value1);
        Assert.Equal("localConfig", value1);
    }
}
