// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;

namespace PSRule.Pipeline;

/// <summary>
/// Tests for <see cref="InputPathBuilder"/>.
/// </summary>
public sealed class InputPathBuilderTests
{
    [Theory]
    [InlineData("./.github/*.yml", 1)]
    [InlineData("./.github/**/*.yaml", 9)]
    [InlineData("./.github/", 12)]
    [InlineData(".github/", 12)]
    [InlineData(".github", 12)]
    [InlineData("./*.json", 8)]
    public void Build_WithValidPathAdded_ShouldReturnFiles(string path, int expected)
    {
        var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, null);
        builder.Add(path);
        var actual = builder.Build();

        Assert.Equal(expected, actual.Length);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("./")]
    [InlineData("./src")]
    [InlineData("./src/")]
    [InlineData("src/")]
    public void Build_WithValidPathAdded_ShouldReturnManyFiles(string path)
    {
        var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, null);
        builder.Add(path);
        var actual = builder.Build();

        Assert.True(actual.Length > 100);
    }

    [Fact]
    public void Build_WithWorkingPathAdded_ShouldReturnFiles()
    {
        var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, null);
        builder.Add(GetWorkingPath());
        var actual = builder.Build();

        Assert.True(actual.Length > 100);
    }

    /// <summary>
    /// Test that an invalid path is handled correctly.
    /// Should not return any files, and should log an error.
    /// </summary>
    [Fact]
    public void Build_WithInvalidPathAdded_ShouldReturnEmpty()
    {
        var writer = new TestWriter(new Configuration.PSRuleOption());
        var builder = new InputPathBuilder(writer, GetWorkingPath(), "*", null, null);
        builder.Add("ZZ://not/path");
        var actual = builder.Build();

        Assert.Empty(actual);
        Assert.True(writer.Errors.Count(r => r.eventId.Name == "PSRule.ReadInputFailed") == 1);
    }

    [Fact]
    public void GetPathRequired()
    {
        var required = PathFilter.Create(GetWorkingPath(), ["README.md"]);
        var builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, required);
        builder.Add(".");
        var actual = builder.Build();
        Assert.True(actual.Length == 1);

        builder.Add(GetWorkingPath());
        actual = builder.Build();
        Assert.True(actual.Length == 1);

        required = PathFilter.Create(GetWorkingPath(), ["**"]);
        builder = new InputPathBuilder(null, GetWorkingPath(), "*", null, required);
        builder.Add(".");
        actual = builder.Build();
        Assert.True(actual.Length > 100);
    }

    #region Helper methods

    private static string GetWorkingPath()
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../.."));
    }

    #endregion Helper methods
}
