// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

public sealed class ResourceIdEqualityComparerTests
{
    [Theory]
    [InlineData("scope\\name", "scope\\name")]
    [InlineData("scope\\name", "name")]
    [InlineData("name", "scope\\name")]
    [InlineData(".\\name", ".\\name")]
    [InlineData(".\\name", "name")]
    [InlineData("name", ".\\name")]
    [InlineData("name", "name")]
    public void IdEquals_WithSimilarIDs_ShouldReturnTrue(string x, string y)
    {
        Assert.True(ResourceIdEqualityComparer.IdEquals(x, y));
    }

    [Theory]
    [InlineData("scope\\name", "scope\\other")]
    [InlineData("scope\\other", "scope\\name")]
    [InlineData("scope\\name", "other\\name")]
    [InlineData("other\\name", "scope\\name")]
    [InlineData("name", "other")]
    [InlineData(".\\name", ".\\other")]
    public void IdEquals_WithDifferentIDs_ShouldReturnFalse(string x, string y)
    {
        Assert.False(ResourceIdEqualityComparer.IdEquals(x, y));
    }
}
