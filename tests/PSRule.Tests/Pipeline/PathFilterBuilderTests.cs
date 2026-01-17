// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PSRule.Pipeline;

/// <summary>
/// Tests for <see cref="PathFilterBuilder"/>.
/// </summary>
public sealed class PathFilterBuilderTests
{
    [Fact]
    public void Build()
    {
        var builder = PathFilterBuilder.Create(GetWorkingPath(), ["out/"], false, false);
        var actual1 = builder.Build();
        Assert.False(actual1.Match("out/not.file"));
        Assert.True(actual1.Match(".git/HEAD"));
        Assert.True(actual1.Match(".gitignore"));
        Assert.True(actual1.Match(".github/CODEOWNERS"));
        Assert.True(actual1.Match(".github/dependabot.yml"));

        builder = PathFilterBuilder.Create(GetWorkingPath(), ["out/"], true, false);
        var actual2 = builder.Build();
        Assert.False(actual2.Match("out/not.file"));
        Assert.False(actual2.Match(".git/HEAD"));
        Assert.True(actual2.Match(".gitignore"));
        Assert.True(actual2.Match(".github/CODEOWNERS"));
        Assert.True(actual2.Match(".github/dependabot.yml"));

        builder = PathFilterBuilder.Create(GetWorkingPath(), ["out/"], true, true);
        var actual3 = builder.Build();
        Assert.False(actual3.Match("out/not.file"));
        Assert.False(actual3.Match(".git/HEAD"));
        Assert.False(actual3.Match(".gitignore"));
        Assert.False(actual3.Match(".github/CODEOWNERS"));
        Assert.True(actual3.Match(".github/dependabot.yml"));
    }

    [Theory]
    [InlineData("deployments/prod/deploy.bicep")]
    [InlineData("modules/network/module.tests.bicep")]
    public void Build_WhenExcludeAllFilesExceptDeepPaths_ShouldMatch(string path)
    {
        var expressions = new string[]
        {
            "**",
            "!deployments/**/deploy.bicep",
            "!modules/**/*.tests.bicep"
        };

        var filter = PathFilterBuilder.Create(GetWorkingPath(), expressions, ignoreGitPath: false, ignoreRepositoryCommon: false).Build();
        Assert.True(filter.Match(path));
    }

    #region Helper methods

    private static string GetWorkingPath()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    #endregion Helper methods
}
