// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace PSRule
{
    public sealed class SemanticVersionTests
    {
        [Fact]
        public void Version()
        {
            Assert.True(Runtime.SemanticVersion.TryParseVersion("1.2.3-alpha.3+7223b39", out Runtime.SemanticVersion.Version actual1));
            Assert.Equal(1, actual1.Major);
            Assert.Equal(2, actual1.Minor);
            Assert.Equal(3, actual1.Patch);
            Assert.Equal("alpha.3", actual1.PreRelease.ToString());
            Assert.Equal("7223b39", actual1.Build);

            Assert.True(Runtime.SemanticVersion.TryParseVersion("v1.2.3-alpha.3+7223b39", out Runtime.SemanticVersion.Version actual2));
            Assert.Equal(1, actual2.Major);
            Assert.Equal(2, actual2.Minor);
            Assert.Equal(3, actual2.Patch);
            Assert.Equal("alpha.3", actual2.PreRelease.ToString());
            Assert.Equal("7223b39", actual2.Build);
        }

        [Fact]
        public void Constraint()
        {
            // Versions
            Assert.True(Runtime.SemanticVersion.TryParseVersion("1.2.3", out Runtime.SemanticVersion.Version version1));
            Assert.True(Runtime.SemanticVersion.TryParseVersion("1.2.3-alpha.3+7223b39", out Runtime.SemanticVersion.Version version2));
            Assert.True(Runtime.SemanticVersion.TryParseVersion("3.4.5-alpha.9", out Runtime.SemanticVersion.Version version3));
            Assert.True(Runtime.SemanticVersion.TryParseVersion("3.4.5", out Runtime.SemanticVersion.Version version4));
            Assert.False(Runtime.SemanticVersion.TryParseVersion("1.2.3-", out Runtime.SemanticVersion.Version _));
            Assert.True(Runtime.SemanticVersion.TryParseVersion("1.2.3-0", out Runtime.SemanticVersion.Version _));
            Assert.False(Runtime.SemanticVersion.TryParseVersion("1.2.3-0123", out Runtime.SemanticVersion.Version _));
            Assert.True(Runtime.SemanticVersion.TryParseVersion("1.2.3-0A", out Runtime.SemanticVersion.Version _));

            // Constraints
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("1.2.3", out Runtime.SemanticVersion.Constraint actual1));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("1.2.3-alpha.3", out Runtime.SemanticVersion.Constraint actual2));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">1.2.3-alpha.3", out Runtime.SemanticVersion.Constraint actual3));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">1.2.3-alpha.1", out Runtime.SemanticVersion.Constraint actual4));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<1.2.3-beta", out Runtime.SemanticVersion.Constraint actual5));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("^1.2.3-alpha", out Runtime.SemanticVersion.Constraint actual6));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<3.4.6", out Runtime.SemanticVersion.Constraint actual7));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("=v1.2.3", out Runtime.SemanticVersion.Constraint actual8));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">=v1.2.3", out Runtime.SemanticVersion.Constraint actual9));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">=v1.2.3-0", out Runtime.SemanticVersion.Constraint actual10));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<3.4.5", out Runtime.SemanticVersion.Constraint actual11));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<3.4.5-9999999999", out Runtime.SemanticVersion.Constraint actual12));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("^1.0.0", out Runtime.SemanticVersion.Constraint actual13));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<1.2.3-0", out Runtime.SemanticVersion.Constraint actual14));

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
        }
    }
}
