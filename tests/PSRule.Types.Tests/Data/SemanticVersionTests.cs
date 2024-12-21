// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// Tests for semantic version comparison.
/// </summary>
public sealed class SemanticVersionTests
{
    /// <summary>
    /// Test parsing of versions.
    /// </summary>
    [Fact]
    public void SemanticVersion_WithTryParseVersion_ShouldParseSuccessfully()
    {
        Assert.True(SemanticVersion.TryParseVersion("1.2.3-alpha.3+7223b39", out var actual1));
        Assert.Equal(1, actual1!.Major);
        Assert.Equal(2, actual1.Minor);
        Assert.Equal(3, actual1.Patch);
        Assert.Equal("alpha.3", actual1.Prerelease.Value);
        Assert.Equal("7223b39", actual1.Build);

        Assert.True(SemanticVersion.TryParseVersion("v1.2.3-alpha.3", out var actual2));
        Assert.Equal(1, actual2!.Major);
        Assert.Equal(2, actual2.Minor);
        Assert.Equal(3, actual2.Patch);
        Assert.Equal("alpha.3", actual2.Prerelease.Value);

        Assert.True(SemanticVersion.TryParseVersion("v1.2.3+7223b39", out var actual3));
        Assert.Equal(1, actual3!.Major);
        Assert.Equal(2, actual3.Minor);
        Assert.Equal(3, actual3.Patch);
        Assert.Equal("7223b39", actual3.Build);
    }

    /// <summary>
    /// Test ordering of versions by comparison.
    /// </summary>
    [Fact]
    public void SemanticVersion_WithCompareTo_ShouldCrossCompare()
    {
        Assert.True(SemanticVersion.TryParseVersion("1.0.0", out var actual1));
        Assert.True(SemanticVersion.TryParseVersion("1.2.0", out var actual2));
        Assert.True(SemanticVersion.TryParseVersion("10.0.0", out var actual3));
        Assert.True(SemanticVersion.TryParseVersion("1.0.2", out var actual4));

        Assert.Equal(0, actual1!.CompareTo(actual1));
        Assert.True(actual1.CompareTo(actual2) < 0);
        Assert.True(actual1.CompareTo(actual3) < 0);
        Assert.True(actual1.CompareTo(actual4) < 0);
        Assert.Equal(0, actual2!.CompareTo(actual2));
        Assert.True(actual2.CompareTo(actual1) > 0);
        Assert.True(actual2.CompareTo(actual3) < 0);
        Assert.True(actual2.CompareTo(actual4) > 0);
    }

    /// <summary>
    /// Test parsing of constraints.
    /// </summary>
    [Fact]
    public void SemanticVersion_WithTryParseConstraint_ShouldAcceptMatchingVersions()
    {
        // Versions
        Assert.True(SemanticVersion.TryParseVersion("1.2.3", out var version1));
        Assert.True(SemanticVersion.TryParseVersion("1.2.3-alpha.3+7223b39", out var version2));
        Assert.True(SemanticVersion.TryParseVersion("3.4.5-alpha.9", out var version3));
        Assert.True(SemanticVersion.TryParseVersion("3.4.5", out var version4));
        Assert.False(SemanticVersion.TryParseVersion("1.2.3-", out var _));
        Assert.True(SemanticVersion.TryParseVersion("1.2.3-0", out var _));
        Assert.False(SemanticVersion.TryParseVersion("1.2.3-0123", out var _));
        Assert.True(SemanticVersion.TryParseVersion("1.2.3-0A", out var _));

        // Constraints
        Assert.True(SemanticVersion.TryParseConstraint("1.2.3", out var actual1));
        Assert.True(SemanticVersion.TryParseConstraint("1.2.3-alpha.3", out var actual2));
        Assert.True(SemanticVersion.TryParseConstraint(">1.2.3-alpha.3", out var actual3));
        Assert.True(SemanticVersion.TryParseConstraint(">1.2.3-alpha.1", out var actual4));
        Assert.True(SemanticVersion.TryParseConstraint("<1.2.3-beta", out var actual5));
        Assert.True(SemanticVersion.TryParseConstraint("^1.2.3-alpha", out var actual6));
        Assert.True(SemanticVersion.TryParseConstraint("<3.4.6", out var actual7));
        Assert.True(SemanticVersion.TryParseConstraint("=v1.2.3", out var actual8));
        Assert.True(SemanticVersion.TryParseConstraint(">=v1.2.3", out var actual9));
        Assert.True(SemanticVersion.TryParseConstraint(">=v1.2.3-0", out var actual10));
        Assert.True(SemanticVersion.TryParseConstraint("<3.4.5", out var actual11));
        Assert.True(SemanticVersion.TryParseConstraint("<3.4.5-9999999999", out var actual12));
        Assert.True(SemanticVersion.TryParseConstraint("^1.0.0", out var actual13));
        Assert.True(SemanticVersion.TryParseConstraint("<1.2.3-0", out var actual14));
        Assert.True(SemanticVersion.TryParseConstraint("1.2.3|| >=3.4.5-0 3.4.5", out var actual15));
        Assert.True(SemanticVersion.TryParseConstraint("1.2.3 ||>=3.4.5-0 || 3.4.5", out var actual16));
        Assert.True(SemanticVersion.TryParseConstraint("1.2.3||3.4.5", out var actual17));
        Assert.True(SemanticVersion.TryParseConstraint(">=1.2.3", out var actual18, includePrerelease: true));
        Assert.True(SemanticVersion.TryParseConstraint("<=3.4.5-0", out var actual19, includePrerelease: true));
        Assert.True(SemanticVersion.TryParseConstraint("@pre >=1.2.3", out var actual20));
        Assert.True(SemanticVersion.TryParseConstraint("@prerelease <=3.4.5-0", out var actual21));

        // Version1 - 1.2.3
        Assert.True(actual1.Accepts(version1));
        Assert.False(actual2.Accepts(version1));
        Assert.True(actual3.Accepts(version1));
        Assert.True(actual4.Accepts(version1));
        Assert.False(actual5.Accepts(version1));
        Assert.True(actual6.Accepts(version1));
        Assert.True(actual7.Accepts(version1));
        Assert.True(actual8.Accepts(version1));
        Assert.True(actual9.Accepts(version1));
        Assert.True(actual10.Accepts(version1));
        Assert.True(actual11.Accepts(version1));
        Assert.True(actual12.Accepts(version1));
        Assert.True(actual13.Accepts(version1));
        Assert.False(actual14.Accepts(version1));
        Assert.True(actual15.Accepts(version1));
        Assert.True(actual16.Accepts(version1));
        Assert.True(actual17.Accepts(version1));
        Assert.True(actual18.Accepts(version1));
        Assert.True(actual19.Accepts(version1));
        Assert.True(actual20.Accepts(version1));
        Assert.True(actual21.Accepts(version1));

        // Version2 - 1.2.3-alpha.3+7223b39
        Assert.False(actual1.Accepts(version2));
        Assert.True(actual2.Accepts(version2));
        Assert.False(actual3.Accepts(version2));
        Assert.True(actual4.Accepts(version2));
        Assert.True(actual5.Accepts(version2));
        Assert.True(actual6.Accepts(version2));
        Assert.False(actual7.Accepts(version2));
        Assert.False(actual8.Accepts(version2));
        Assert.False(actual9.Accepts(version2));
        Assert.True(actual10.Accepts(version2));
        Assert.False(actual11.Accepts(version2));
        Assert.False(actual12.Accepts(version2));
        Assert.False(actual13.Accepts(version2));
        Assert.False(actual14.Accepts(version2));
        Assert.False(actual15.Accepts(version2));
        Assert.False(actual16.Accepts(version2));
        Assert.False(actual17.Accepts(version2));
        Assert.False(actual18.Accepts(version2));
        Assert.True(actual19.Accepts(version2));
        Assert.False(actual20.Accepts(version2));
        Assert.True(actual21.Accepts(version2));

        // Version3 - 3.4.5-alpha.9
        Assert.False(actual1.Accepts(version3));
        Assert.False(actual2.Accepts(version3));
        Assert.False(actual3.Accepts(version3));
        Assert.False(actual4.Accepts(version3));
        Assert.False(actual5.Accepts(version3));
        Assert.False(actual6.Accepts(version3));
        Assert.False(actual7.Accepts(version3));
        Assert.False(actual8.Accepts(version3));
        Assert.False(actual9.Accepts(version3));
        Assert.False(actual10.Accepts(version3));
        Assert.False(actual11.Accepts(version3));
        Assert.False(actual12.Accepts(version3));
        Assert.False(actual13.Accepts(version3));
        Assert.False(actual14.Accepts(version3));
        Assert.False(actual15.Accepts(version3));
        Assert.True(actual16.Accepts(version3));
        Assert.False(actual17.Accepts(version3));
        Assert.True(actual18.Accepts(version3));
        Assert.False(actual19.Accepts(version3));
        Assert.True(actual20.Accepts(version3));
        Assert.False(actual21.Accepts(version3));

        // Version4 - 3.4.5
        Assert.False(actual1.Accepts(version4));
        Assert.False(actual2.Accepts(version4));
        Assert.True(actual3.Accepts(version4));
        Assert.True(actual4.Accepts(version4));
        Assert.False(actual5.Accepts(version4));
        Assert.False(actual6.Accepts(version4));
        Assert.True(actual7.Accepts(version4));
        Assert.False(actual8.Accepts(version4));
        Assert.True(actual9.Accepts(version4));
        Assert.True(actual10.Accepts(version4));
        Assert.False(actual11.Accepts(version4));
        Assert.False(actual12.Accepts(version4));
        Assert.False(actual13.Accepts(version4));
        Assert.False(actual14.Accepts(version4));
        Assert.True(actual15.Accepts(version4));
        Assert.True(actual16.Accepts(version4));
        Assert.True(actual17.Accepts(version4));
        Assert.True(actual18.Accepts(version4));
        Assert.False(actual19.Accepts(version4));
        Assert.True(actual20.Accepts(version4));
        Assert.False(actual21.Accepts(version4));
    }

    /// <summary>
    /// Test parsing and order of pre-releases.
    /// </summary>
    [Fact]
    public void SemanticVersion_WithPrerelease_ShouldCrossCompare()
    {
        var actual1 = new SemanticVersion.PR(null);
        var actual2 = new SemanticVersion.PR("alpha");
        var actual3 = new SemanticVersion.PR("alpha.1");
        var actual4 = new SemanticVersion.PR("alpha.beta");
        var actual5 = new SemanticVersion.PR("beta");
        var actual6 = new SemanticVersion.PR("beta.2");
        var actual7 = new SemanticVersion.PR("beta.11");
        var actual8 = new SemanticVersion.PR("rc.1");

        Assert.Equal(0, actual1.CompareTo(actual1));
        Assert.True(actual1.CompareTo(actual2) > 0);
        Assert.True(actual1.CompareTo(actual6) > 0);
        Assert.True(actual2.CompareTo(actual3) < 0);
        Assert.True(actual3.CompareTo(actual4) < 0);
        Assert.True(actual4.CompareTo(actual5) < 0);
        Assert.True(actual5.CompareTo(actual6) < 0);
        Assert.True(actual6.CompareTo(actual7) < 0);
        Assert.True(actual7.CompareTo(actual8) < 0);
        Assert.True(actual8.CompareTo(actual1) < 0);
        Assert.True(actual8.CompareTo(actual2) > 0);
    }

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-alpha.3+7223b39")]
    [InlineData("3.4.5-alpha.9")]
    [InlineData("3.4.5+7223b39")]
    public void SemanticVersion_WithToString_ShouldReturnString(string version)
    {
        Assert.True(SemanticVersion.TryParseVersion(version, out var actual));
        Assert.Equal(version, actual!.ToString());
    }

    /// <summary>
    /// Test <see cref="SemanticVersion"/> represented as a string with <c>ToString()</c> formats the string correctly.
    /// </summary>
    [Theory]
    [InlineData("1", "1.0.0")]
    [InlineData("1.2", "1.2.0")]
    [InlineData("v1", "1.0.0")]
    [InlineData("v1.2", "1.2.0")]
    [InlineData("v1.2.3", "1.2.3")]
    public void SemanticVersion_WithPartialVersion_ShouldReturnCompletedString(string version, string expected)
    {
        Assert.True(SemanticVersion.TryParseVersion(version, out var actual));
        Assert.Equal(expected, actual!.ToString());
    }

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-alpha.3+7223b39")]
    [InlineData("3.4.5-alpha.9")]
    [InlineData("3.4.5+7223b39")]
    public void SemanticVersion_WithToShortString_ShouldReturnString(string version)
    {
        Assert.True(SemanticVersion.TryParseVersion(version, out var actual));
        Assert.Equal(string.Join(".", actual!.Major, actual.Minor, actual.Patch), actual!.ToShortString());
    }

    [Theory]
    [InlineData("1.2.3", "=1.2.3")]
    [InlineData("=1.2.3", "=1.2.3")]
    [InlineData(">=1.2.3", ">=1.2.3")]
    [InlineData("1.2.3 ||>=3.4.5-0 || 3.4.5", "=1.2.3 || >=3.4.5-0 || =3.4.5")]
    public void SemanticVersion_WithTryParseConstraint_ShouldReturnString(string constraint, string expected)
    {
        Assert.True(SemanticVersion.TryParseConstraint(constraint, out var actual));
        Assert.Equal(expected, actual!.ToString());
    }
}
