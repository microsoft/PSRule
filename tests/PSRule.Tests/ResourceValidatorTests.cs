// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

public sealed class ResourceValidatorTests : ContextBaseTests
{
    [Theory]
    [InlineData("FromFile.Rule.yaml")]
    public void GetRule_WithValidResourceName_ShouldNotReturnError(string path)
    {
        var writer = GetTestWriter();
        var sources = GetSource(path);
        var context = new LegacyRunspaceContext(GetPipelineContext(writer: writer, sources: sources));

        // Get good rules
        var rule = HostHelper.GetRule(context, includeDependencies: false);
        Assert.NotNull(rule);
        Assert.Empty(writer.Errors);
    }

    [Theory]
    [InlineData("FromFileName.Rule.yaml")]
    [InlineData("FromFileName.Rule.jsonc")]
    public void GetRule_WithInvalidResourceName_ShouldReturnError(string path)
    {
        var writer = GetTestWriter();
        var sources = GetSource(path);
        var context = new LegacyRunspaceContext(GetPipelineContext(writer: writer, sources: sources));

        // Get invalid rule names.
        var rule = HostHelper.GetRule(context, includeDependencies: false);
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

    private static new Source[] GetSource(string path)
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(path));
        return builder.Build();
    }

    #endregion Helper methods
}
