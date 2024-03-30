// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule;

/// <summary>
/// Unit tests for <see cref="GitHelper"/>.
/// </summary>
public sealed class GitHelperTests
{
    [Fact]
    public void TryReadHead_WhenValidPath_ShouldReturnGitHead()
    {
        var expectedHead = GetGitOutput();

        Assert.True(GitHelper.TryReadHead("../../../../../.git/", out var actualHead));
        Assert.Equal(expectedHead, NormalizeBranch(actualHead));
    }

    #region Helper methods

    private static string NormalizeBranch(string actualHead)
    {
        return actualHead.Replace("refs/heads/", "");
    }

    private static string GetGitOutput()
    {
        var tool = ExternalTool.Get(null, GitHelper.GetGitBinary());
        tool.WaitForExit("rev-parse --abbrev-ref HEAD", out _);
        return tool.GetOutput().Trim();
    }

    #endregion Helper methods
}
