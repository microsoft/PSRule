// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using PSRule.Configuration;
using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule;

/// <summary>
/// Tests for <see cref="SourcePipelineBuilder"/>.
/// </summary>
public sealed class SourcePipelineBuilderTests
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
        Assert.Equal(23, sources[0].File.Length);
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
        Assert.Equal(18, sources[0].File.Length);
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
        Assert.Equal(18, sources[0].File.Length);
    }

    #region Helper methods

    private static string GetSourcePath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    }

    private static PSRuleOption GetOption()
    {
        return new PSRuleOption();
    }

    #endregion Helper methods
}
