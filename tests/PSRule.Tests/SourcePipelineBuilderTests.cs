// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule;

/// <summary>
/// Tests for <see cref="SourcePipelineBuilder"/>.
/// </summary>
public sealed class SourcePipelineBuilderTests : BaseTests
{
    [Fact]
    public void Add_single_file()
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Single(sources[0].File);
    }

    [Fact]
    public void Add_directory()
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(""));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(27, sources[0].File.Length);
    }

    [Fact]
    public void Add_script_file_with_disable_powershell()
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.DisablePowerShell;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
        var sources = builder.Build();

        Assert.Empty(sources);
    }

    [Fact]
    public void Add_script_file_with_module_only()
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.ModuleOnly;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
        var sources = builder.Build();

        Assert.Empty(sources);
    }

    [Fact]
    public void Add_yaml_file_with_disable_powershell()
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.DisablePowerShell;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath("FromFile.Rule.yaml"));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Single(sources[0].File);
    }

    [Fact]
    public void Add_yaml_file_with_module_only()
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.ModuleOnly;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath("FromFile.Rule.yaml"));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Single(sources[0].File);
    }

    [Fact]
    public void Add_directory_with_disable_powershell()
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.DisablePowerShell;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(""));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(21, sources[0].File.Length);
    }

    [Fact]
    public void Add_directory_with_module_only()
    {
        var option = GetOption();
        option.Execution.RestrictScriptSource = RestrictScriptSource.ModuleOnly;
        var builder = new SourcePipelineBuilder(null, option);
        builder.Directory(GetSourcePath(""));
        var sources = builder.Build();

        Assert.Single(sources);
        Assert.Equal(21, sources[0].File.Length);
    }

    [Theory]
    [InlineData("TestModule7", "0.0.1")]
    [InlineData("TestModule8", "0.0.1-Alpha")]
    public void ModuleByName_WithNameAndVersion_ShouldAddModuleFiles(string name, string version)
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
