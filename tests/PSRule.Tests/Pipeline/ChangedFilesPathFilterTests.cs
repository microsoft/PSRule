// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace PSRule.Pipeline;

/// <summary>
/// Unit tests for <see cref="ChangedFilesPathFilter"/>.
/// </summary>
public sealed class ChangedFilesPathFilterTests : BaseTests
{
    [Theory]
    [InlineData("file1.txt")]
    [InlineData("./file1.txt")]
    [InlineData("file2.txt")]
    public void Match_WhenChangedFile_ShouldMatch(string path)
    {
        var basePath = GetSourcePath(string.Empty);
        var changedFiles = new[] { "file1.txt", "file2.txt" };
        var filter = new ChangedFilesPathFilter(new AlwaysTruePathFilter(), basePath, changedFiles);

        Assert.True(filter.Match(path));
        Assert.True(filter.Match(Path.Combine(basePath, path)));
    }

    [Theory]
    [InlineData("file3.txt")]
    [InlineData("./file3.txt")]
    [InlineData("sub/file3.txt")]
    public void Match_WhenUnchangedFile_ShouldNotMatch(string path)
    {
        var basePath = GetSourcePath(string.Empty);
        var changedFiles = new[] { "file1.txt", "file2.txt" };
        var filter = new ChangedFilesPathFilter(new AlwaysTruePathFilter(), basePath, changedFiles);

        Assert.False(filter.Match(path));
        Assert.False(filter.Match(Path.Combine(basePath, path)));
    }
}
