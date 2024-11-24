// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using PSRule.Definitions;
using PSRule.Pipeline.Emitters;

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
        var collection = new EmitterBuilder().Build(new TestEmitterContext());
        Assert.Equal(4, collection.Count);
    }

    /// <summary>
    /// That that interfaces from the service collection are correctly injected into the constructor.
    /// </summary>
    [Fact]
    public void Build_WhenEmitterSupportsConfiguration_ShouldInjectConfigurationInstance()
    {
        var option = GetOption();
        option.Format.Add("test", new Options.FormatType { Type = [".t"] });

        var languageScopeSet = GetLanguageScopeSet(option: option);
        var builder = new EmitterBuilder(languageScopeSet, option.Format);
        builder.AddEmitter<TestEmitter>(ResourceHelper.STANDALONE_SCOPE_NAME);

        var collection = builder.Build(new TestEmitterContext());
        var actual = collection.Emitters.OfType<TestEmitter>().FirstOrDefault();

        Assert.NotNull(actual);
        Assert.NotNull(actual.Configuration);
        Assert.Equal([".t"], actual.Configuration.GetFormatTypes("test"));
    }
}
