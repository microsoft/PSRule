// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// Unit tests for <see cref="EmitterBuilder"/>.
/// </summary>
public sealed class EmitterBuilderTests : ContextBaseTests
{
    /// <summary>
    /// Test that a collection contains the default emitters.
    /// </summary>
    [Fact]
    public void Build_WhenNull_ShouldAddDefaultEmitters()
    {
        // Check for default emitters.
        var option = GetOption();
        var collection = new EmitterBuilder(formatOption: option.Format).Build(new TestEmitterContext());
        Assert.Equal(4, collection.Count);
    }

    /// <summary>
    /// That that interfaces from the service collection are correctly injected into the constructor.
    /// </summary>
    [Fact]
    public void Build_WhenEmitterSupportsConfiguration_ShouldInjectConfigurationInstance()
    {
        var option = GetOption();
        option.Format.Add("test", new FormatType { Type = [".t"] });
        option.Configuration["custom_flag"] = true;
        var optionContextBuilder = GetOptionBuilder(option);

        var languageScopeSet = GetLanguageScopeSet(optionContextBuilder: optionContextBuilder);
        var builder = new EmitterBuilder(languageScopeSet, option.Format, allowAlwaysEnabled: true);
        builder.AddEmitter<TestEmitter>(ResourceHelper.STANDALONE_SCOPE_NAME);

        var collection = builder.Build(new TestEmitterContext());
        var actual = collection.Emitters.OfType<TestEmitter>().FirstOrDefault();

        Assert.NotNull(actual);
        Assert.NotNull(actual.Configuration);
        Assert.Equal([".t"], actual.Configuration.GetFormatTypes("test"));
        Assert.True(actual.Configuration.IsEnabled("custom_flag"));
        Assert.False(actual.Configuration.IsEnabled("not_set"));
        Assert.Equal("default", actual.Configuration.GetValueOrDefault("not_set", "default"));
    }

    /// <summary>
    /// Tests that any emitters that have been registered as services in the language scope are added to the collection.
    /// </summary>
    [Fact]
    public void Build_WhenLanguageScopeIncludedEmitter_ShouldAddCustomEmitter()
    {
        var builder = new LanguageScopeSetBuilder();
        builder.CreateModuleScope("test", null);
        var languageScopeSet = builder.Build();
        if (languageScopeSet.TryScope("test", out var scope))
        {
            scope.ConfigureServices(c => c.AddService<IEmitter, CustomEmitter>());
        }

        var collection = new EmitterBuilder(languageScopeSet, allowAlwaysEnabled: true).Build(new TestEmitterContext());

        Assert.NotNull(collection);
        Assert.NotNull(collection.Emitters.FirstOrDefault(i => i is CustomEmitter));
    }

    #region Helper methods

    protected override PSRuleOption GetOption()
    {
        return new PSRuleOption()
        {
            Format = new FormatOption
            {
                ["yaml"] = new FormatType
                {
                    Enabled = true,
                },
                ["json"] = new FormatType
                {
                    Enabled = true,
                },
                ["markdown"] = new FormatType
                {
                    Enabled = true,
                },
                ["powershell_data"] = new FormatType
                {
                    Enabled = true,
                },
            }
        };
    }

    #endregion Helper methods
}
