// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        var collection = new EmitterBuilder().Build(context);

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
}
