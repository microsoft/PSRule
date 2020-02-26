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
            Assert.Equal("alpha.3", actual1.PreRelease);
            Assert.Equal("7223b39", actual1.Build);

            Assert.True(Runtime.SemanticVersion.TryParseVersion("v1.2.3-alpha.3+7223b39", out Runtime.SemanticVersion.Version actual2));
            Assert.Equal(1, actual2.Major);
            Assert.Equal(2, actual2.Minor);
            Assert.Equal(3, actual2.Patch);
            Assert.Equal("alpha.3", actual2.PreRelease);
            Assert.Equal("7223b39", actual2.Build);
        }

        [Fact]
        public void Constraint()
        {
            Runtime.SemanticVersion.TryParseVersion("1.2.3", out Runtime.SemanticVersion.Version version1);
            Runtime.SemanticVersion.TryParseVersion("1.2.3-alpha.3+7223b39", out Runtime.SemanticVersion.Version version2);
            Runtime.SemanticVersion.TryParseVersion("3.4.5-alpha.9", out Runtime.SemanticVersion.Version version3);
            Runtime.SemanticVersion.TryParseVersion("3.4.5", out Runtime.SemanticVersion.Version version4);

            Assert.True(Runtime.SemanticVersion.TryParseConstraint("1.2.3", out Runtime.SemanticVersion.Constraint actual1));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("1.2.3-alpha.3", out Runtime.SemanticVersion.Constraint actual2));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">1.2.3-alpha.3", out Runtime.SemanticVersion.Constraint actual3));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">1.2.3-alpha.1", out Runtime.SemanticVersion.Constraint actual4));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<1.2.3-beta", out Runtime.SemanticVersion.Constraint actual5));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("^1.2.3-alpha", out Runtime.SemanticVersion.Constraint actual6));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("<3.4.6", out Runtime.SemanticVersion.Constraint actual7));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint("=v1.2.3", out Runtime.SemanticVersion.Constraint actual8));
            Assert.True(Runtime.SemanticVersion.TryParseConstraint(">=v1.2.3", out Runtime.SemanticVersion.Constraint actual9));

            Assert.True(actual1.Equals(version1));
            Assert.False(actual2.Equals(version1));
            Assert.True(actual3.Equals(version1));
            Assert.True(actual4.Equals(version1));
            Assert.False(actual5.Equals(version1));
            Assert.True(actual6.Equals(version1));
            Assert.True(actual7.Equals(version1));
            Assert.True(actual8.Equals(version1));
            Assert.True(actual9.Equals(version1));

            Assert.False(actual1.Equals(version2));
            Assert.True(actual2.Equals(version2));
            Assert.False(actual3.Equals(version2));
            Assert.True(actual4.Equals(version2));
            Assert.True(actual5.Equals(version2));
            Assert.True(actual6.Equals(version2));
            Assert.False(actual7.Equals(version2));
            Assert.False(actual8.Equals(version2));
            Assert.False(actual9.Equals(version2));

            Assert.False(actual1.Equals(version3));
            Assert.False(actual2.Equals(version3));
            Assert.False(actual3.Equals(version3));
            Assert.False(actual4.Equals(version3));
            Assert.False(actual5.Equals(version3));
            Assert.False(actual6.Equals(version3));
            Assert.False(actual7.Equals(version3));
            Assert.False(actual8.Equals(version3));
            Assert.False(actual9.Equals(version3));

            Assert.False(actual1.Equals(version4));
            Assert.False(actual2.Equals(version4));
            Assert.True(actual3.Equals(version4));
            Assert.True(actual4.Equals(version4));
            Assert.False(actual5.Equals(version4));
            Assert.False(actual6.Equals(version4));
            Assert.True(actual7.Equals(version4));
            Assert.False(actual8.Equals(version4));
            Assert.True(actual9.Equals(version4));
        }
    }
}
