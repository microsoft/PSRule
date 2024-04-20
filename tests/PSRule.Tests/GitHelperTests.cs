// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

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

        Assert.True(GitHelper.TryReadHead(out var actualHead, "../../../../../.git/"));
        Assert.Equal(expectedHead, NormalizeBranch(actualHead));
    }

    #region Helper methods

    private static string NormalizeBranch(string actualHead)
    {
        return actualHead.Replace("refs/heads/", "");
    }

    private static string GetGitOutput()
    {
        var bin = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "git" : "git.exe";
        var git = ExternalTool.Get(null, bin);
        git.WaitForExit("branch --show-current", out _);
        return git.GetOutput().Trim();
    }

    #endregion Helper methods
}
