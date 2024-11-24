// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline.Emitters;
using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="JsonEmitter"/>.
/// </summary>
public sealed class JsonEmitterTests : EmitterTests
{
    /// <summary>
    /// Test that the <see cref="JsonEmitter"/> accepts files.
    /// </summary>
    [Fact]
    public void Accepts_WhenValidType_ShouldReturnTrue()
    {
        var context = new TestEmitterContext();
        var emitter = new JsonEmitter(NullLogger<JsonEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Accepts(context, typeof(InternalFileInfo)));
        Assert.True(emitter.Accepts(context, typeof(string)));
        Assert.False(emitter.Accepts(null, null));
        Assert.False(emitter.Accepts(context, typeof(object)));
    }

    [Fact]
    public void Visit_WhenValidFile_ShouldVisitDefaultTypes()
    {
        var context = new TestEmitterContext();
        var emitter = new JsonEmitter(NullLogger<JsonEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile.json")));
        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFileSingle.jsonc")));
        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile.sarif")));
        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.yaml")));
        Assert.False(emitter.Visit(context, new InternalFileInfo("", "")));
        Assert.False(emitter.Visit(null, null));

        // Check path is set by _PSRule.
        Assert.Equal("master.items[0]", context.Items[0].Path);

        // Check path is JSON element.
        Assert.Equal("[1]", context.Items[1].Path);

        // Check path is root element.
        Assert.Equal("", context.Items[2].Path);
        Assert.Equal("", context.Items[3].Path);

        Assert.Equal(4, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenFormatOptionIsSet_ShouldOnlyVisitSpecifiedTypes()
    {
        var context = new TestEmitterContext();
        var emitter = new JsonEmitter(NullLogger<JsonEmitter>.Instance, GetEmitterConfiguration(format: [("json", [".sarif"])]));

        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.json")));
        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFileSingle.jsonc")));
        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile.sarif")));
        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.yaml")));
        Assert.False(emitter.Visit(context, new InternalFileInfo("", "")));

        Assert.Single(context.Items);
    }

    [Fact]
    public void Visit_WhenEmptyFile_ShouldNotEmitItems()
    {
        var context = new TestEmitterContext();
        var emitter = new JsonEmitter(NullLogger<JsonEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Visit(context, GetFileInfo("FromFileEmpty.Rule.jsonc")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenString_ShouldEmitItems()
    {
        var context = new TestEmitterContext(format: Options.InputFormat.Json);
        var emitter = new JsonEmitter(NullLogger<JsonEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        // With format.
        Assert.True(emitter.Visit(context, ReadFileAsString("ObjectFromFile.json")));
        Assert.Equal(2, context.Items.Count);

        // Without format.
        context = new TestEmitterContext(format: Options.InputFormat.None);
        Assert.False(emitter.Visit(context, ReadFileAsString("ObjectFromFile.json")));
    }
}
