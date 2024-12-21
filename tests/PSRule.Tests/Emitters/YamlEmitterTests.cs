// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="YamlEmitter"/>.
/// </summary>
public sealed class YamlEmitterTests : EmitterTests
{
    /// <summary>
    /// Test that the <see cref="YamlEmitter"/> accepts files and strings.
    /// </summary>
    [Fact]
    public void Accepts_WhenValidType_ShouldReturnTrue()
    {
        var context = new TestEmitterContext();
        var emitter = new YamlEmitter(NullLogger<YamlEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Accepts(context, typeof(InternalFileInfo)));
        Assert.True(emitter.Accepts(context, typeof(string)));
        Assert.False(emitter.Accepts(null, null));
        Assert.False(emitter.Accepts(context, typeof(object)));
    }

    [Fact]
    public void Visit_WhenValidFile_ShouldVisitDefaultTypes()
    {
        var context = new TestEmitterContext();
        var emitter = new YamlEmitter(NullLogger<YamlEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile.yaml")));
        Assert.True(emitter.Visit(context, GetFileInfo("PSRule.Tests.yml")));
        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.json")));
        Assert.False(emitter.Visit(context, new InternalFileInfo("", "")));
        Assert.False(emitter.Visit(null, null));

        // Path doesn't work with YAML files yet.
        Assert.Null(context.Items[0].Path);

        Assert.Equal(3, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenFormatOptionIsSet_ShouldOnlyVisitSpecifiedTypes()
    {
        var context = new TestEmitterContext();
        var emitter = new YamlEmitter(NullLogger<YamlEmitter>.Instance, GetEmitterConfiguration(format: [("yaml", [".yml"])]));

        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.yaml")));
        Assert.True(emitter.Visit(context, GetFileInfo("PSRule.Tests.yml")));
        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.json")));
        Assert.False(emitter.Visit(context, new InternalFileInfo("", "")));

        Assert.Single(context.Items);
    }

    [Fact]
    public void Visit_WhenValidFileContainsArray_ShouldEmitItems()
    {
        var context = new TestEmitterContext();
        var emitter = new YamlEmitter(NullLogger<YamlEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile3.yaml")));

        Assert.Equal(2, context.Items.Count);
    }

    [Fact]
    public void Visit_WhenEmptyFile_ShouldNotEmitItems()
    {
        var context = new TestEmitterContext();
        var emitter = new YamlEmitter(NullLogger<YamlEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Visit(context, GetFileInfo("FromFileEmpty.Rule.yaml")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenString_ShouldEmitItems()
    {
        var context = new TestEmitterContext(format: Options.InputFormat.Yaml);
        var emitter = new YamlEmitter(NullLogger<YamlEmitter>.Instance, EmptyEmitterConfiguration.Instance);

        // With format.
        Assert.True(emitter.Visit(context, ReadFileAsString("ObjectFromFile.yaml")));
        Assert.Equal(2, context.Items.Count);

        // Without format.
        context = new TestEmitterContext(format: Options.InputFormat.None);
        Assert.False(emitter.Visit(context, ReadFileAsString("ObjectFromFile.yaml")));
    }
}
