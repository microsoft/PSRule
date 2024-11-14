// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

public sealed class ResourceValidatorTests : ContextBaseTests
{
    [Fact]
    public void ResourceName()
    {
        var writer = GetTestWriter();
        var sources = GetSource();
        var context = new RunspaceContext(GetPipelineContext(writer: writer, sources: sources));

        // Get good rules
        var rule = HostHelper.GetRule(sources, context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Empty(writer.Errors);

        // Get invalid rule names YAML
        rule = HostHelper.GetRule(GetSource("FromFileName.Rule.yaml"), context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.NotEmpty(writer.Errors);
        Assert.Equal("PSRule.Parse.InvalidResourceName", writer.Errors[0].FullyQualifiedErrorId);

        // Get invalid rule names JSON
        writer.Errors.Clear();
        rule = HostHelper.GetRule(GetSource("FromFileName.Rule.jsonc"), context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.NotEmpty(writer.Errors);
        Assert.Equal("PSRule.Parse.InvalidResourceName", writer.Errors[0].FullyQualifiedErrorId);
    }

    [Fact]
    public void IsNameValid()
    {
        Assert.True(ResourceValidator.IsNameValid("Local.Test.Rule"));
        Assert.True(ResourceValidator.IsNameValid("Local_Test_Rule"));
        Assert.True(ResourceValidator.IsNameValid("Local-Test-Rule"));
        Assert.True(ResourceValidator.IsNameValid("My rule 1"));
        Assert.True(ResourceValidator.IsNameValid("Ma règle 1"));
        Assert.True(ResourceValidator.IsNameValid("Ο κανόνας μου 1"));
        Assert.True(ResourceValidator.IsNameValid("私のルール1"));
        Assert.True(ResourceValidator.IsNameValid("我的规则 1"));
        Assert.False(ResourceValidator.IsNameValid("My rule '1'"));
        Assert.False(ResourceValidator.IsNameValid("My rule \"1\""));
        Assert.False(ResourceValidator.IsNameValid("My rule `1`"));
        Assert.False(ResourceValidator.IsNameValid("Test\0Rule"));
        Assert.False(ResourceValidator.IsNameValid("Test>Rule"));
        Assert.False(ResourceValidator.IsNameValid("Test|Rule"));
        Assert.False(ResourceValidator.IsNameValid("Test\nRule"));
        Assert.False(ResourceValidator.IsNameValid("Test\tRule"));
        Assert.False(ResourceValidator.IsNameValid("Test\\Rule"));
        Assert.False(ResourceValidator.IsNameValid("Test/Rule"));
        Assert.False(ResourceValidator.IsNameValid("Test Rule."));
        Assert.False(ResourceValidator.IsNameValid("Test Rule-"));
        Assert.False(ResourceValidator.IsNameValid("My"));
    }

    #region Helper methods

    private static new Source[] GetSource(string path = "FromFile.Rule.yaml")
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(path));
        return builder.Build();
    }

    #endregion Helper methods
}
