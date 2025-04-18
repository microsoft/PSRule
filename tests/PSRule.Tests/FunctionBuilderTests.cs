// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Definitions.Selectors;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

public sealed class FunctionBuilderTests : ContextBaseTests
{
    private const string FunctionYamlFileName = "Functions.Rule.yaml";
    private const string FunctionJsonFileName = "Functions.Rule.jsonc";

    [Theory]
    [InlineData("Yaml", FunctionYamlFileName)]
    [InlineData("Json", FunctionJsonFileName)]
    public void Build(string type, string path)
    {
        Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example1", GetSource(path), out _));
        Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example2", GetSource(path), out _));
        Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example3", GetSource(path), out _));
        Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example4", GetSource(path), out _));
        Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example5", GetSource(path), out _));
        Assert.NotNull(GetSelectorVisitor($"{type}.Fn.Example6", GetSource(path), out _));
    }

    #region Helper methods

    private SelectorVisitor GetSelectorVisitor(string name, Source[] sources, out LegacyRunspaceContext context)
    {
        var resourcesCache = GetResourceCache(option: GetOption(), sources: sources);
        context = new LegacyRunspaceContext(GetPipelineContext(resourceCache: resourcesCache));
        context.Initialize(sources);
        context.Begin();
        var selector = resourcesCache.OfType<SelectorV1>().FirstOrDefault(s => s.Id.Name == name);
        return selector.ToSelectorVisitor(context);
    }

    #endregion Helper methods
}
