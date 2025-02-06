// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="PowerShellDataEmitter"/>.
/// </summary>
public sealed class PowerShellDataTests : EmitterTests
{
    [Fact]
    public void Accepts_WhenValidType_ShouldReturnTrue()
    {
        var context = new TestEmitterContext();
        var emitter = new PowerShellDataEmitter(EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Accepts(context, typeof(InternalFileInfo)));
        Assert.True(emitter.Accepts(context, typeof(string)));
        Assert.False(emitter.Accepts(null, null));
        Assert.False(emitter.Accepts(context, typeof(object)));
    }

    [Fact]
    public void Visit_WhenValidFile_ShouldVisitDefaultTypes()
    {
        var context = new TestEmitterContext();
        var emitter = new PowerShellDataEmitter(EmptyEmitterConfiguration.Instance);

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
        var emitter = new PowerShellDataEmitter(EmptyEmitterConfiguration.Instance);

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFileEmpty.psd1")));
        Assert.Empty(context.Items);
    }

    [Fact]
    public void Visit_WhenReplacementConfigured_ShouldReplaceTokens()
    {
        var context = new TestEmitterContext();
        var emitter = new PowerShellDataEmitter(GetEmitterConfiguration(format: [("powershell_data", default, default, [new KeyValuePair<string, string>("kind = 'Test'", "kind = 'ReplacementTest'")])]));

        Assert.True(emitter.Visit(context, GetFileInfo("ObjectFromFile.psd1")));

        var item = (context.Items[0].Value as PSObject).BaseObject as Hashtable;
        var spec = item["spec"] as Hashtable;
        var properties = spec["properties"] as Hashtable;
        Assert.Equal("ReplacementTest", properties["kind"]);
    }

    [Fact]
    public void Visit_WhenString_ShouldEmitItems()
    {
        var context = new TestEmitterContext(stringFormat: "powershell_data");
        var emitter = new PowerShellDataEmitter(EmptyEmitterConfiguration.Instance);

        // With format.
        Assert.True(emitter.Visit(context, ReadFileAsString("ObjectFromFile.psd1")));
        Assert.Single(context.Items);

        // Without format.
        context = new TestEmitterContext();
        Assert.False(emitter.Visit(context, ReadFileAsString("ObjectFromFile.psd1")));
    }
}
