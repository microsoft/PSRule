// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Runtime;

namespace PSRule.Definitions.ModuleConfigs;

public sealed class ModuleConfigTests : ContextBaseTests
{
    [Theory]
    [InlineData("ModuleConfig.Rule.yaml")]
    [InlineData("ModuleConfig.Rule.jsonc")]
    public void Import_WithV1_ShouldReturnResource(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionBuilder(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();

        var moduleConfig = resourcesCache.OfType<ModuleConfigV1>().ToArray();
        Assert.NotNull(moduleConfig);
        Assert.Single(moduleConfig);

        var actual = moduleConfig[0];
        Assert.Equal("Configuration1", actual.Name);
    }

    [Theory]
    [InlineData("ModuleConfig.Rule.yaml")]
    [InlineData("ModuleConfig.Rule.jsonc")]
    public void Import_WithCapabilities_ShouldReturnResource(string path)
    {
        var sources = GetSource(path);
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        var context = new LegacyRunspaceContext(GetPipelineContext(option: GetOption(), optionBuilder: GetOptionBuilder(), sources: sources, resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();

        var moduleConfig = resourcesCache.OfType<ModuleConfigV2>().ToArray();
        Assert.NotNull(moduleConfig);
        Assert.Single(moduleConfig);

        var actual = moduleConfig[0];
        Assert.Equal("ConfigurationWithCapabilities", actual.Name);
    }
}
