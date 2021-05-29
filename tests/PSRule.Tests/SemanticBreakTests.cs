// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace PSRule
{
    public sealed class SemanticBreakTests
    {
        [Fact]
        public void BreakShort()
        {
            var original = "Consider using Hybrid Use Benefit for eligible workloads.";
            var actual = original.SplitSemantic();
            Assert.Single(actual);
            Assert.Equal(original, actual[0]);
        }

        [Fact]
        public void BreakLong()
        {
            var original = "Use a minimum of Standard for production container registries. Basic container registries are only recommended for non-production deployments. Consider upgrading ACR to Premium and enabling geo-replication between Azure regions to provide an in region registry to complement high availability or disaster recovery for container environments.";
            var actual = original.SplitSemantic();
            Assert.Equal("Use a minimum of Standard for production container registries. Basic container", actual[0]);
            Assert.Equal("registries are only recommended for non-production deployments. Consider", actual[1]);
            Assert.Equal("upgrading ACR to Premium and enabling geo-replication between Azure regions to", actual[2]);
            Assert.Equal("provide an in region registry to complement high availability or disaster", actual[3]);
            Assert.Equal("recovery for container environments.", actual[4]);
            Assert.Equal(5, actual.Length);
        }

        [Fact]
        public void BreakDash()
        {
            var original = "Consider configuring NSGs rules to block outbound management traffic from non-management hosts.";
            var actual = original.SplitSemantic();
            Assert.Equal("Consider configuring NSGs rules to block outbound management traffic from", actual[0]);
            Assert.Equal("non-management hosts.", actual[1]);
            Assert.Equal(2, actual.Length);
        }

        [Fact]
        public void BreakNewLine()
        {
            var original = "Use a minimum of Standard for production container registries. Basic container registries are only recommended for non-production deployments.\r\nConsider upgrading ACR to Premium and enabling geo-replication between Azure regions to provide an in region registry to complement high availability or disaster recovery for container environments.";
            var actual = original.SplitSemantic();
            Assert.Equal("Use a minimum of Standard for production container registries. Basic container", actual[0]);
            Assert.Equal("registries are only recommended for non-production deployments.", actual[1]);
            Assert.Equal("Consider upgrading ACR to Premium and enabling geo-replication between Azure", actual[2]);
            Assert.Equal("regions to provide an in region registry to complement high availability or", actual[3]);
            Assert.Equal("disaster recovery for container environments.", actual[4]);
            Assert.Equal(5, actual.Length);
        }
    }
}
