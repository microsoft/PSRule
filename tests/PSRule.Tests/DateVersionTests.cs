// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule;

/// <summary>
/// Tests for date version comparison.
/// </summary>
public sealed class DateVersionTests
{
    /// <summary>
    /// Test parsing of versions.
    /// </summary>
    [Fact]
    public void Version()
    {
        Assert.True(DateVersion.TryParseVersion("2015-10-01", out var actual));
        Assert.Equal(2015, actual.Year);
        Assert.Equal(10, actual.Month);
        Assert.Equal(1, actual.Day);
        Assert.Equal(string.Empty, actual.Prerelease.Value);

        Assert.True(DateVersion.TryParseVersion("2015-1-01-prerelease", out actual));
        Assert.Equal(2015, actual.Year);
        Assert.Equal(1, actual.Month);
        Assert.Equal(1, actual.Day);
        Assert.Equal("prerelease", actual.Prerelease.Value);
    }

    /// <summary>
    /// Test ordering of versions by comparison.
    /// </summary>
    [Fact]
    public void VersionOrder()
    {
        Assert.True(DateVersion.TryParseVersion("2015-10-01", out var actual1));
        Assert.True(DateVersion.TryParseVersion("2015-10-01-prerelease", out var actual2));
        Assert.True(DateVersion.TryParseVersion("2022-03-01", out var actual3));
        Assert.True(DateVersion.TryParseVersion("2022-01-03", out var actual4));

        Assert.True(actual1.CompareTo(actual1) == 0);
        Assert.True(actual1.CompareTo(actual2) > 0);
        Assert.True(actual1.CompareTo(actual3) < 0);
        Assert.True(actual1.CompareTo(actual4) < 0);
        Assert.True(actual2.CompareTo(actual2) == 0);
        Assert.True(actual2.CompareTo(actual1) < 0);
        Assert.True(actual2.CompareTo(actual3) < 0);
        Assert.True(actual2.CompareTo(actual4) < 0);
        Assert.True(actual3.CompareTo(actual4) > 0);
        Assert.True(actual1.CompareTo(actual4) < 0);
    }

    /// <summary>
    /// Test parsing of constraints.
    /// </summary>
    [Fact]
    public void Constraint()
    {
        // Versions
        Assert.True(DateVersion.TryParseVersion("2015-10-01", out var version1));
        Assert.True(DateVersion.TryParseVersion("2015-10-01-alpha.9", out var version2));
        Assert.True(DateVersion.TryParseVersion("2022-03-01", out var version3));
        Assert.False(DateVersion.TryParseVersion("2022-03-01-", out var _));
        Assert.True(DateVersion.TryParseVersion("2022-03-01-0", out var _));

        // Constraints
        Assert.True(DateVersion.TryParseConstraint("2015-10-01", out var actual1));
        Assert.True(DateVersion.TryParseConstraint("2015-10-01-alpha.3", out var actual2));
        Assert.True(DateVersion.TryParseConstraint(">2015-10-01-alpha.3", out var actual3));
        Assert.True(DateVersion.TryParseConstraint(">2015-10-01-alpha.1", out var actual4));
        Assert.True(DateVersion.TryParseConstraint("<2015-10-01-beta", out var actual5));
        Assert.True(DateVersion.TryParseConstraint("<2022-03-01", out var actual7));
        Assert.True(DateVersion.TryParseConstraint("=2015-10-01", out var actual8));
        Assert.True(DateVersion.TryParseConstraint(">=2015-10-01", out var actual9));
        Assert.True(DateVersion.TryParseConstraint(">=2015-10-01-0", out var actual10));
        Assert.True(DateVersion.TryParseConstraint("<2022-03-01", out var actual11));
        Assert.True(DateVersion.TryParseConstraint("<2022-03-01-9999999999", out var actual12));
        Assert.True(DateVersion.TryParseConstraint("<2015-10-01-0", out var actual14));
        Assert.True(DateVersion.TryParseConstraint("2015-10-01|| >=2022-03-01-0 2022-03-01", out var actual15));
        Assert.True(DateVersion.TryParseConstraint("2015-10-01 ||>=2022-03-01-0 || 2022-03-01", out var actual16));
        Assert.True(DateVersion.TryParseConstraint("2015-10-01||2022-03-01", out var actual17));
        Assert.True(DateVersion.TryParseConstraint(">=2015-09-01", out var actual18, includePrerelease: true));
        Assert.True(DateVersion.TryParseConstraint("<=2022-03-01-0", out var actual19, includePrerelease: true));
        Assert.True(DateVersion.TryParseConstraint("@pre >=2015-09-01", out var actual20));
        Assert.True(DateVersion.TryParseConstraint("@prerelease <=2022-03-01-0", out var actual21));

        // Version1 - 2015-10-01
        Assert.True(actual1.Accepts(version1));
        Assert.False(actual2.Accepts(version1));
        Assert.True(actual3.Accepts(version1));
        Assert.True(actual4.Accepts(version1));
        Assert.False(actual5.Accepts(version1));
        Assert.True(actual7.Accepts(version1));
        Assert.True(actual8.Accepts(version1));
        Assert.True(actual9.Accepts(version1));
        Assert.True(actual10.Accepts(version1));
        Assert.True(actual11.Accepts(version1));
        Assert.True(actual12.Accepts(version1));
        Assert.False(actual14.Accepts(version1));
        Assert.True(actual15.Accepts(version1));
        Assert.True(actual16.Accepts(version1));
        Assert.True(actual17.Accepts(version1));
        Assert.True(actual18.Accepts(version1));
        Assert.True(actual19.Accepts(version1));
        Assert.True(actual20.Accepts(version1));
        Assert.True(actual21.Accepts(version1));

        // Version3 - 2015-10-01-alpha.9
        Assert.False(actual1.Accepts(version2));
        Assert.False(actual2.Accepts(version2));
        Assert.True(actual3.Accepts(version2));
        Assert.True(actual4.Accepts(version2));
        Assert.True(actual5.Accepts(version2));
        Assert.False(actual7.Accepts(version2));
        Assert.False(actual8.Accepts(version2));
        Assert.False(actual9.Accepts(version2));
        Assert.True(actual10.Accepts(version2));
        Assert.False(actual11.Accepts(version2));
        Assert.False(actual12.Accepts(version2));
        Assert.False(actual14.Accepts(version2));
        Assert.False(actual15.Accepts(version2));
        Assert.False(actual16.Accepts(version2));
        Assert.False(actual17.Accepts(version2));
        Assert.True(actual18.Accepts(version2));
        Assert.True(actual19.Accepts(version2));
        Assert.True(actual20.Accepts(version2));
        Assert.True(actual21.Accepts(version2));

        // Version4 - 2022-03-01
        Assert.False(actual1.Accepts(version3));
        Assert.False(actual2.Accepts(version3));
        Assert.True(actual3.Accepts(version3));
        Assert.True(actual4.Accepts(version3));
        Assert.False(actual5.Accepts(version3));
        Assert.False(actual7.Accepts(version3));
        Assert.False(actual8.Accepts(version3));
        Assert.True(actual9.Accepts(version3));
        Assert.True(actual10.Accepts(version3));
        Assert.False(actual11.Accepts(version3));
        Assert.False(actual12.Accepts(version3));
        Assert.False(actual14.Accepts(version3));
        Assert.True(actual15.Accepts(version3));
        Assert.True(actual16.Accepts(version3));
        Assert.True(actual17.Accepts(version3));
        Assert.True(actual18.Accepts(version3));
        Assert.False(actual19.Accepts(version3));
        Assert.True(actual20.Accepts(version3));
        Assert.False(actual21.Accepts(version3));
    }

    /// <summary>
    /// Test parsing and order of pre-releases.
    /// </summary>
    [Fact]
    public void Prerelease()
    {
        var actual1 = new DateVersion.PR(null);
        var actual2 = new DateVersion.PR("alpha");
        var actual3 = new DateVersion.PR("alpha.1");
        var actual4 = new DateVersion.PR("alpha.beta");
        var actual5 = new DateVersion.PR("beta");
        var actual6 = new DateVersion.PR("beta.2");
        var actual7 = new DateVersion.PR("beta.11");
        var actual8 = new DateVersion.PR("rc.1");
        var actual9 = new DateVersion.PR("alpha.9");

        Assert.True(actual1.CompareTo(actual1) == 0);
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
        Assert.True(actual9.CompareTo(actual3) > 0);
    }
}
