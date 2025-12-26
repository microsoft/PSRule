// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;

namespace PSRule;

/// <summary>
/// Tests for <see cref="PSRuleOption"/>.
/// </summary>
public sealed class PSRuleOptionTests : ContextBaseTests
{
    [Fact]
    public void GetRootedBasePath()
    {
        var pwd = Directory.GetCurrentDirectory();
        var basePwd = $"{pwd}{Path.DirectorySeparatorChar}";
        Assert.Equal(basePwd, Environment.GetRootedBasePath(null));
        Assert.Equal(basePwd, Environment.GetRootedBasePath(pwd));
        Assert.Equal(pwd, Environment.GetRootedPath(null));
        Assert.Equal(pwd, Environment.GetRootedPath(pwd));
    }

    [Fact]
    public void Configuration()
    {
        var option = new PSRuleOption();
        option.Configuration.Add("key1", "value1");

        dynamic configuration = GetConfigurationHelper(option);
        Assert.Equal("value1", configuration.key1);
    }

    [Fact]
    public void GetStringValues()
    {
        var option = new PSRuleOption();
        option.Configuration.Add("key1", "123");
        option.Configuration.Add("key2", new string[] { "123" });
        option.Configuration.Add("key3", new object[] { "123", 456 });

        var configuration = GetConfigurationHelper(option);
        Assert.Equal(new string[] { "123" }, configuration.GetStringValues("key1"));
        Assert.Equal(new string[] { "123" }, configuration.GetStringValues("key2"));
        Assert.Equal(new string[] { "123", "456" }, configuration.GetStringValues("key3"));
    }

    [Fact]
    public void GetStringValuesFromYaml()
    {
        var option = GetOption();
        var actual = option.Configuration["option5"] as Array;
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        Assert.IsType<PSObject>(actual.GetValue(0));
        var pso = actual.GetValue(0) as PSObject;
        Assert.Equal("option5a", pso.BaseObject);

        var configuration = GetConfigurationHelper(option);
        Assert.Equal(new string[] { "option5a", "option5b" }, configuration.GetStringValues("option5"));
    }

    [Fact]
    public void GetObjectArrayFromYaml()
    {
        var option = GetOption();
        var actual = option.Configuration["option4"] as Array;
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        Assert.IsType<PSObject>(actual.GetValue(0));
        var pso = actual.GetValue(0) as PSObject;
        Assert.Equal("East US", pso.PropertyValue<string>("location"));
    }

    [Fact]
    public void GetBaselineGroupFromYaml()
    {
        var option = GetOption();
        var actual = option.Baseline.Group;
        Assert.NotNull(actual);
        Assert.Single(actual);
        Assert.True(actual.TryGetValue("latest", out var latest));
        Assert.Equal(new string[] { ".\\TestBaseline1" }, latest);
    }

    [Fact]
    public void FromFile_WhenOverrideIsDefined_ShouldDeserializeLevel()
    {
        var option = GetOption();
        var actual = option.Override.Level;
        Assert.True(actual.TryGetValue("rule1", out var level));
        Assert.Equal(SeverityLevel.Information, level);

        Assert.True(actual.TryGetValue("Group.*", out level));
        Assert.Equal(SeverityLevel.Error, level);
    }

    [Theory]
    [InlineData("PSRule.Tests2.yml")]
    [InlineData("PSRule.Tests17.yml")]
    public void FromFile_WhenOverrideIsPartiallyDefined_ShouldDeserializeWithoutError(string path)
    {
        var option = GetOption(GetSourcePath(path));
        var actual = option.Override.Level;
        Assert.Null(actual);
    }

    #region Helper methods

    private Runtime.Configuration GetConfigurationHelper(PSRuleOption option)
    {
        var optionBuilder = new OptionContextBuilder(option);
        var pipeline = GetPipelineContext(option: option, optionBuilder: optionBuilder);
        var context = new Runtime.LegacyRunspaceContext(pipeline);
        context.Initialize(null);
        context.Begin();
        context.EnterLanguageScope(GetSource()[0].File[0]);
        return new Runtime.Configuration(pipeline.RunspaceContext);
    }

    private static Source[] GetSource()
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
        return builder.Build();
    }

    protected sealed override PSRuleOption GetOption()
    {
        return GetOption(GetSourcePath("PSRule.Tests.yml"));
    }

    #endregion Helper methods
}
