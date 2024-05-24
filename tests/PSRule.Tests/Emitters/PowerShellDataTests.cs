// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline.Emitters;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="PowerShellDataEmitter"/>.
/// </summary>
public sealed class PowerShellDataTests : BaseTests
{
    [Fact]
    public void Accepts_WhenValidType_ShouldReturnTrue()
    {
        var context = new TestEmitterContext();
        var emitter = new PowerShellDataEmitter();

        Assert.True(emitter.Accepts(context, typeof(InternalFileInfo)));
        Assert.True(emitter.Accepts(context, typeof(string)));
        Assert.False(emitter.Accepts(null, null));
        Assert.False(emitter.Accepts(context, typeof(object)));
    }

    [Fact]
    public void Visit_WhenValidFile_ShouldEmitItems()
    {
        var context = new TestEmitterContext();
        var emitter = new PowerShellDataEmitter();

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile.psd1")));
        Assert.False(emitter.Visit(context, GetFileInfo("ObjectFromFile.yaml")));
        Assert.False(emitter.Visit(context, new InternalFileInfo("", "")));
        Assert.False(emitter.Visit(null, null));

        Assert.Single(context.Items);
    }

    [Fact]
    public void Visit_WhenEmptyFile_ShouldNotEmitItems()
    {
        var context = new TestEmitterContext();
        var emitter = new PowerShellDataEmitter();

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFileEmpty.psd1")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenString_ShouldEmitItems()
    {
        var context = new TestEmitterContext(format: Options.InputFormat.PowerShellData);
        var emitter = new PowerShellDataEmitter();

        // With format.
        Assert.True(emitter.Visit(context, ReadFileAsString("ObjectFromFile.psd1")));
        Assert.Single(context.Items);

        // Without format.
        context = new TestEmitterContext(format: Options.InputFormat.None);
        Assert.False(emitter.Visit(context, ReadFileAsString("ObjectFromFile.psd1")));
    }
}
