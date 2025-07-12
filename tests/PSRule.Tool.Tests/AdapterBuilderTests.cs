// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool.Adapters;

public sealed class AdapterBuilderTests
{
    [Fact]
    public void TryAdapter_WithGitHubActionsFlag_ReturnsTrue()
    {
        // Test that the --in-github-actions flag is detected
        var args = new string[] { "run", "--in-github-actions" };

        Assert.True(AdapterBuilder.TryAdapter(args, out var execute));
        Assert.NotNull(execute);
    }

    [Fact]
    public void TryAdapter_WithoutGitHubActionsFlag_ReturnsFalse()
    {
        // Test that without the flag, the adapter is not used
        var args = new string[] { "run" };

        Assert.False(AdapterBuilder.TryAdapter(args, out var execute));
        Assert.Null(execute);
    }

    [Fact]
    public void TryAdapter_WithAzurePipelinesFlag_ReturnsTrue()
    {
        // Test that the --in-azure-pipelines flag is detected
        var args = new string[] { "run", "--in-azure-pipelines" };

        Assert.True(AdapterBuilder.TryAdapter(args, out var execute));
        Assert.NotNull(execute);
    }

    [Fact]
    public void TryAdapter_WithoutAzurePipelinesFlag_ReturnsFalse()
    {
        // Test that without the flag, the adapter is not used
        var args = new string[] { "run" };

        Assert.False(AdapterBuilder.TryAdapter(args, out var execute));
        Assert.Null(execute);
    }
}
