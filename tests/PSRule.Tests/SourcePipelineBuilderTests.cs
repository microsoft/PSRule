// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule;

/// <summary>
/// Tests for <see cref="SourcePipelineBuilder"/>.
/// </summary>
public sealed class SourcePipelineBuilderTests : BaseTests
{
    [Theory]
    [InlineData("FromFile.Rule.ps1")]
    [InlineData("John's Documents/FromFileBaseline.Rule.ps1")]
    public void Directory_WithSingleFile_ShouldFindCount(string path)
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Single(sources[0].File);
        Assert.Equal(GetSourcePath(path), sources[0].File[0].Path);
    }

    [Theory]
    [InlineData("FromFile.Rule.yaml", 1)]
    public void Directory_WithSingleFileAndDisablePowerShell_ShouldFindCount(string path, int count)
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.DisablePowerShell;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(count, sources[0].File.Length);
    }

    [Theory]
    [InlineData("FromFile.Rule.ps1")]
    public void Directory_WithScriptFileAndDisablePowerShell_ShouldNotFindAny(string path)
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.DisablePowerShell;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Empty(sources);
    }

    [Theory]
    [InlineData("FromFile.Rule.yaml", 1)]
    public void Directory_WithSingleFileAndModuleOnly_ShouldFindCount(string path, int count)
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.ModuleOnly;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(count, sources[0].File.Length);
    }

    [Theory]
    [InlineData("FromFile.Rule.ps1")]
    public void Directory_WithScriptFileAndModuleOnly_ShouldNotFindAny(string path)
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.ModuleOnly;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Empty(sources);
    }

    [Theory]
    [InlineData("", 33)]
    [InlineData("John's Documents", 1)]
    public void Directory_WithDirectory_ShouldFindCount(string path, int count)
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(count, sources[0].File.Length);
    }

    [Theory]
    [InlineData("", 23)]
    public void Directory_WithDirectoryAndDisablePowerShell_ShouldFindCount(string path, int count)
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.DisablePowerShell;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(count, sources[0].File.Length);

        Array.ForEach(sources[0].File, file =>
        {
            Assert.False(file.Path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase));
            Assert.False(file.Path.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Theory]
    [InlineData("", 23)]
    public void Directory_WithDirectoryAndModuleOnly_ShouldFindCount(string path, int count)
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.ModuleOnly;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(path));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(count, sources[0].File.Length);
    }

    [Theory]
    [InlineData("TestModule7", "0.0.1")]
    [InlineData("TestModule8", "0.0.1-Alpha")]
    public void ModuleByName_WithNameAndVersion_ShouldFindModuleFiles(string name, string version)
    {
        var builder = new SourcePipelineBuilder(null, null, cachePath: GetSourcePath(""));
        builder.ModuleByName(name: name, version: version);
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(name, sources[0].Module.Name);
        Assert.Equal(version, sources[0].Module.FullVersion);
        Assert.NotEmpty(sources[0].File);
    }
}
