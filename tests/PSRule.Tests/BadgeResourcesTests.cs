// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Badges;

namespace PSRule;

public sealed class BadgeResourcesTests
{
    [Fact]
    public void TestResources()
    {
        var width = BadgeResources.Measure("PSRule");
        Assert.True(width > 0);
    }
}
