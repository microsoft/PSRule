// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Options;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="EmitterCollection"/>.
/// </summary>
public sealed class EmitterCollectionTests : BaseTests
{
    [Fact]
    public void Visit_WhenValidFile_ShouldEmitItems()
    {
        var context = new TestEmitterContext();
        var collection = new EmitterBuilder(formatOption: GetOption().Format).Build(context);

        Assert.True(collection.Visit(GetFileInfo("ObjectFromFile.yaml")));
        Assert.Equal(2, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenString_ShouldEmitItems()
    {
        var context = new TestEmitterContext(stringFormat: "yaml");
        var collection = new EmitterBuilder(formatOption: GetOption().Format).Build(context);

        Assert.True(collection.Visit(ReadFileAsString("ObjectFromFile.yaml")));
        Assert.Equal(2, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenNotValidFile_ShouldNotItems()
    {
        var context = new TestEmitterContext();
        var collection = new EmitterBuilder(formatOption: GetOption().Format).Build(context);

        Assert.False(collection.Visit(GetFileInfo("not-a-file.yaml")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenNoEmitterMatches_ShouldNotItems()
    {
        var context = new TestEmitterContext();
        var collection = new EmitterBuilder(formatOption: GetOption().Format).Build(context);

        Assert.False(collection.Visit(GetFileInfo("jenkinsfile")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenMultipleMatches_ShouldProcessAll()
    {
        var processed1 = false;
        var context = new TestEmitterContext();
        var builder = new EmitterBuilder(formatOption: GetOption().Format, allowAlwaysEnabled: true);

        builder.AddEmitter(ResourceHelper.STANDALONE_SCOPE_NAME, new TestEmitter
        (
            types: [".yaml"],
            visitFile: (context, a) =>
            {
                processed1 = true;
                return true;
            }
        ));

        var collection = builder.Build(context);

        Assert.True(collection.Visit(GetFileInfo("ObjectFromFile.yaml")));
        Assert.True(processed1);
        Assert.Equal(2, context.Items.Count);
    }

    #region Helper methods

    protected override PSRuleOption GetOption()
    {
        return new PSRuleOption()
        {
            Format = new FormatOption
            {
                ["yaml"] = new FormatType
                {
                    Enabled = true,
                },
                ["json"] = new FormatType
                {
                    Enabled = true,
                },
                ["markdown"] = new FormatType
                {
                    Enabled = true,
                },
                ["powershell_data"] = new FormatType
                {
                    Enabled = true,
                },
            }
        };
    }

    #endregion Helper methods
}
