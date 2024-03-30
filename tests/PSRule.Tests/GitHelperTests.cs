// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule;

public sealed class GitHelperTests
{
    [Fact]
    public void Can_read_head()
    {
        var expectedHead = GetGitOutput().Trim();

        Assert.True(GitHelper.TryReadHead("../../../../../.git/", out var actualHead));
        Assert.Equal(expectedHead, NormalizeBranch(actualHead));
    }

    private static string NormalizeBranch(string actualHead)
    {
        var parts = actualHead.Split('/');
        return string.Join('/', parts, 2, parts.Length - 2);
    }

    private static string GetGitOutput()
    {
        var tool = ExternalTool.Get(null, GitHelper.GetGitBinary());
        tool.WaitForExit("rev-parse --abbrev-ref HEAD", out _);
        return tool.GetOutput();
    }
}
