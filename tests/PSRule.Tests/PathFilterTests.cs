// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Pipeline;

namespace PSRule;

public sealed class PathFilterTests
{
    [Fact]
    public void Match()
    {
        // Single expression match
        var filter = PathFilter.Create(GetWorkingPath(), "**/Resources.Parameters*.json");
        Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
        Assert.True(filter.Match("tests/example2/Resources.Parameters2.json"));
        Assert.False(filter.Match("tests/example2/Resources.Parameters2.txt"));
        filter = PathFilter.Create(GetWorkingPath(), "**/example2/Resources.Parameters*.json");
        Assert.False(filter.Match("tests/example/Resources.Parameters.json"));
        Assert.True(filter.Match("tests/example2/Resources.Parameters2.json"));
        filter = PathFilter.Create(GetWorkingPath(), "**/example?/Resources.Parameters*.json");
        Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
        Assert.True(filter.Match("tests/example2/Resources.Parameters2.json"));
        Assert.False(filter.Match("tests/example2/Resources.Parameters2.txt"));
        filter = PathFilter.Create(GetWorkingPath(), "tests/example?/Resources.Parameters.json");
        Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
        Assert.True(filter.Match("tests/example2/Resources.Parameters.json"));
        filter = PathFilter.Create(GetWorkingPath(), "tests/example/Resources.Parameters*.json");
        Assert.True(filter.Match("tests/example/Resources.Parameters.json"));
        Assert.True(filter.Match("tests/example/Resources.Parameters2.json"));

        // Multi-expression match
        var expressions = new string[]
        {
            "out/",
            "**/bin/",
            "**/obj/",
            "",
            "# Add reports/bin",
            "!reports/bin/",
            "**/ObjectFromFile*.json",
            "!**/ObjectFromFile.json"
        };
        filter = PathFilter.Create(GetWorkingPath(), expressions, matchResult: true);
        Assert.True(filter.Match("out/example.parameters.json"));
        Assert.False(filter.Match("outer/example.parameters.json"));
        Assert.False(filter.Match("reports/out/example.parameters.json"));
        Assert.True(filter.Match("src/bin/pwsh.exe"));
        Assert.True(filter.Match("src/obj/example.parameters.json"));
        Assert.False(filter.Match("reports/bin/other.json"));
        Assert.False(filter.Match("ObjectFromFile.json"));
        Assert.True(filter.Match("ObjectFromFileSingle.json"));

        // Exclude
        filter = PathFilter.Create(GetWorkingPath(), expressions, matchResult: false);
        Assert.False(filter.Match("out/example.parameters.json"));
        Assert.True(filter.Match("outer/example.parameters.json"));
        Assert.True(filter.Match("reports/out/example.parameters.json"));
        Assert.False(filter.Match("src/bin/pwsh.exe"));
        Assert.False(filter.Match("src/obj/example.parameters.json"));
        Assert.True(filter.Match("reports/bin/other.json"));
    }

    [Theory]
    [InlineData("out/file.cs")]
    [InlineData("out/otherfile.cs")]
    [InlineData("otherpath/file.cs")]
    public void Match_WhenPathMatchesExclusionCompletely_ShouldMatch(string path)
    {
        var expressions = new string[]
        {
            "**",
            "!*.cs",
            "!**/*.cs"
        };

        var filter = PathFilter.Create(GetWorkingPath(), expressions, matchResult: false);
        Assert.True(filter.Match(path));
    }

    [Theory]
    [InlineData("out/file.csproj")]
    [InlineData("out/file.c")]
    public void Match_WhenPathMatchesPrefixOnlyExclusion_ShouldNotMatch(string path)
    {
        var expressions = new string[]
        {
            "**",
            "!*.cs",
            "!**/*.cs"
        };

        var filter = PathFilter.Create(GetWorkingPath(), expressions, matchResult: false);
        Assert.False(filter.Match(path));
    }

    [Theory]
    [InlineData("src/bin/pwsh.exe")]
    [InlineData("out/bin/pwsh.exe")]
    [InlineData("src/bin/debug/pwsh.exe")]
    public void Match_WhenPathMatchesParentDirectoryExclusion_ShouldMatch(string path)
    {
        var expressions = new string[]
        {
            "**",
            "!**/bin/",
        };

        var filter = PathFilter.Create(GetWorkingPath(), expressions, matchResult: false);
        Assert.True(filter.Match(path));
    }

    [Fact]
    public void Builder()
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

    #region Helper methods

    private static string GetWorkingPath()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    #endregion Helper methods
}
