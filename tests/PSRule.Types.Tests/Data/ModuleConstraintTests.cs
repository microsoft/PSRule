// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// Tests for <see cref="ModuleConstraint"/>.
/// </summary>
public sealed class ModuleConstraintTests
{
    [Theory]
    [InlineData("1.0.0")]
    [InlineData("0.1.0+build.1")]
    public void Any_WhenIncludePrereleaseIsFalse_ShouldAcceptStableVersions(string version)
    {
        var constraint = ModuleConstraint.Any("test", includePrerelease: false);
        Assert.True(SemanticVersion.TryParseVersion(version, out var actualVersion));
        Assert.True(constraint.Accepts(actualVersion));
    }

    [Theory]
    [InlineData("1.0.0-preview")]
    [InlineData("0.1.0-alpha.1+build.1")]
    public void Any_WhenIncludePrereleaseIsFalse_ShouldNotAcceptPrereleaseVersions(string version)
    {
        var constraint = ModuleConstraint.Any("test", includePrerelease: false);
        Assert.True(SemanticVersion.TryParseVersion(version, out var actualVersion));
        Assert.False(constraint.Accepts(actualVersion));
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("0.1.0+build.1")]
    [InlineData("1.0.0-preview")]
    [InlineData("0.1.0-alpha.1+build.1")]
    public void Any_WhenIncludePrereleaseIsTrue_ShouldAcceptStableOrPrereleaseVersions(string version)
    {
        var constraint = ModuleConstraint.Any("test", includePrerelease: true);
        Assert.True(SemanticVersion.TryParseVersion(version, out var actualVersion));
        Assert.True(constraint.Accepts(actualVersion));
    }
}
