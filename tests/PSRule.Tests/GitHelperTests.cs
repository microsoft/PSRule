// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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

        Assert.True(GitHelper.TryReadHead(out var actualHead, GetGitPath()));
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
        var branch = git.GetOutput().Trim();
        if (string.IsNullOrEmpty(branch))
        {
            git.WaitForExit("rev-parse HEAD", out _);
            branch = git.GetOutput().Trim();
        }
        return branch;
    }

    private static string GetGitPath()
    {
        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../");
    }

    #endregion Helper methods
}
