// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace PSRule
{
    public sealed class BadgeResourcesTests
    {
        [Fact]
        public void TestResources()
        {
            var width = PSRule.Badges.BadgeResources.Measure("PSRule");

        }
    }
}
