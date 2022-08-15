// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using Xunit;

namespace PSRule
{
    public sealed class SemanticVersionTests
    {
        /// <summary>
        /// Test parsing of versions.
        /// </summary>
        [Fact]
        public void Version()
        {
            Assert.True(SemanticVersion.TryParseVersion("1.2.3-alpha.3+7223b39", out var actual1));
            Assert.Equal(1, actual1.Major);
            Assert.Equal(2, actual1.Minor);
            Assert.Equal(3, actual1.Patch);
            Assert.Equal("alpha.3", actual1.Prerelease.Value);
            Assert.Equal("7223b39", actual1.Build);

            Assert.True(SemanticVersion.TryParseVersion("v1.2.3-alpha.3", out var actual2));
            Assert.Equal(1, actual2.Major);
            Assert.Equal(2, actual2.Minor);
            Assert.Equal(3, actual2.Patch);
            Assert.Equal("alpha.3", actual2.Prerelease.Value);

            Assert.True(SemanticVersion.TryParseVersion("v1.2.3+7223b39", out var actual3));
            Assert.Equal(1, actual3.Major);
            Assert.Equal(2, actual3.Minor);
            Assert.Equal(3, actual3.Patch);
            Assert.Equal("7223b39", actual3.Build);
        }

        /// <summary>
        /// Test ordering of versions by comparison.
        /// </summary>
        [Fact]
        public void VersionOrder()
        {
            SemanticVersion.TryParseVersion("1.0.0", out var actual1);
            SemanticVersion.TryParseVersion("1.2.0", out var actual2);
            SemanticVersion.TryParseVersion("10.0.0", out var actual3);
            SemanticVersion.TryParseVersion("1.0.2", out var actual4);

            Assert.True(actual1.CompareTo(actual1) == 0);
            Assert.True(actual1.CompareTo(actual2) < 0);
            Assert.True(actual1.CompareTo(actual3) < 0);
            Assert.True(actual1.CompareTo(actual4) < 0);
            Assert.True(actual2.CompareTo(actual2) == 0);
            Assert.True(actual2.CompareTo(actual1) > 0);
            Assert.True(actual2.CompareTo(actual3) < 0);
            Assert.True(actual2.CompareTo(actual4) > 0);
        }

        /// <summary>
        /// Test parsing of constraints.
        /// </summary>
        [Fact]
        public void Constraint()
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
            Assert.True(actual1.Equals(version1));
            Assert.False(actual2.Equals(version1));
            Assert.True(actual3.Equals(version1));
            Assert.True(actual4.Equals(version1));
            Assert.False(actual5.Equals(version1));
            Assert.True(actual6.Equals(version1));
            Assert.True(actual7.Equals(version1));
            Assert.True(actual8.Equals(version1));
            Assert.True(actual9.Equals(version1));
            Assert.True(actual10.Equals(version1));
            Assert.True(actual11.Equals(version1));
            Assert.True(actual12.Equals(version1));
            Assert.True(actual13.Equals(version1));
            Assert.False(actual14.Equals(version1));
            Assert.True(actual15.Equals(version1));
            Assert.True(actual16.Equals(version1));
            Assert.True(actual17.Equals(version1));
            Assert.True(actual18.Equals(version1));
            Assert.True(actual19.Equals(version1));
            Assert.True(actual20.Equals(version1));
            Assert.True(actual21.Equals(version1));

            // Version2 - 1.2.3-alpha.3+7223b39
            Assert.False(actual1.Equals(version2));
            Assert.True(actual2.Equals(version2));
            Assert.False(actual3.Equals(version2));
            Assert.True(actual4.Equals(version2));
            Assert.True(actual5.Equals(version2));
            Assert.True(actual6.Equals(version2));
            Assert.False(actual7.Equals(version2));
            Assert.False(actual8.Equals(version2));
            Assert.False(actual9.Equals(version2));
            Assert.True(actual10.Equals(version2));
            Assert.False(actual11.Equals(version2));
            Assert.False(actual12.Equals(version2));
            Assert.False(actual13.Equals(version2));
            Assert.False(actual14.Equals(version2));
            Assert.False(actual15.Equals(version2));
            Assert.False(actual16.Equals(version2));
            Assert.False(actual17.Equals(version2));
            Assert.False(actual18.Equals(version2));
            Assert.True(actual19.Equals(version2));
            Assert.False(actual20.Equals(version2));
            Assert.True(actual21.Equals(version2));

            // Version3 - 3.4.5-alpha.9
            Assert.False(actual1.Equals(version3));
            Assert.False(actual2.Equals(version3));
            Assert.False(actual3.Equals(version3));
            Assert.False(actual4.Equals(version3));
            Assert.False(actual5.Equals(version3));
            Assert.False(actual6.Equals(version3));
            Assert.False(actual7.Equals(version3));
            Assert.False(actual8.Equals(version3));
            Assert.False(actual9.Equals(version3));
            Assert.False(actual10.Equals(version3));
            Assert.False(actual11.Equals(version3));
            Assert.False(actual12.Equals(version3));
            Assert.False(actual13.Equals(version3));
            Assert.False(actual14.Equals(version3));
            Assert.False(actual15.Equals(version3));
            Assert.True(actual16.Equals(version3));
            Assert.False(actual17.Equals(version3));
            Assert.True(actual18.Equals(version3));
            Assert.False(actual19.Equals(version3));
            Assert.True(actual20.Equals(version3));
            Assert.False(actual21.Equals(version3));

            // Version4 - 3.4.5
            Assert.False(actual1.Equals(version4));
            Assert.False(actual2.Equals(version4));
            Assert.True(actual3.Equals(version4));
            Assert.True(actual4.Equals(version4));
            Assert.False(actual5.Equals(version4));
            Assert.False(actual6.Equals(version4));
            Assert.True(actual7.Equals(version4));
            Assert.False(actual8.Equals(version4));
            Assert.True(actual9.Equals(version4));
            Assert.True(actual10.Equals(version4));
            Assert.False(actual11.Equals(version4));
            Assert.False(actual12.Equals(version4));
            Assert.False(actual13.Equals(version4));
            Assert.False(actual14.Equals(version4));
            Assert.True(actual15.Equals(version4));
            Assert.True(actual16.Equals(version4));
            Assert.True(actual17.Equals(version4));
            Assert.True(actual18.Equals(version4));
            Assert.False(actual19.Equals(version4));
            Assert.True(actual20.Equals(version4));
            Assert.False(actual21.Equals(version4));
        }

        /// <summary>
        /// Test parsing and order of pre-releases.
        /// </summary>
        [Fact]
        public void Prerelease()
        {
            var actual1 = new SemanticVersion.PR(null);
            var actual2 = new SemanticVersion.PR("alpha");
            var actual3 = new SemanticVersion.PR("alpha.1");
            var actual4 = new SemanticVersion.PR("alpha.beta");
            var actual5 = new SemanticVersion.PR("beta");
            var actual6 = new SemanticVersion.PR("beta.2");
            var actual7 = new SemanticVersion.PR("beta.11");
            var actual8 = new SemanticVersion.PR("rc.1");

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
        }
    }
}
