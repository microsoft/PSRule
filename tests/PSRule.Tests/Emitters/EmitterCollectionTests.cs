// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline.Emitters;

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
        var collection = new EmitterBuilder(default).Build(context);

        Assert.True(collection.Visit(GetFileInfo("ObjectFromFile.yaml")));
        Assert.Equal(2, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenString_ShouldEmitItems()
    {
        var context = new TestEmitterContext(format: Options.InputFormat.Yaml);
        var collection = new EmitterBuilder().Build(context);

        Assert.True(collection.Visit(ReadFileAsString("ObjectFromFile.yaml")));
        Assert.Equal(2, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenNotValidFile_ShouldNotItems()
    {
        var context = new TestEmitterContext();
        var collection = new EmitterBuilder().Build(context);

        Assert.False(collection.Visit(GetFileInfo("not-a-file.yaml")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenNoEmitterMatches_ShouldNotItems()
    {
        var context = new TestEmitterContext();
        var collection = new EmitterBuilder().Build(context);

        Assert.False(collection.Visit(GetFileInfo("jenkinsfile")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenMultipleMatches_ShouldProcessAll()
    {
        var processed1 = false;
        var context = new TestEmitterContext();
        var builder = new EmitterBuilder();

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
}
