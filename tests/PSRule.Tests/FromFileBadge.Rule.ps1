# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Rules and conventions for testing badges
#

# Synopsis: A convention that generates a badge for a single result.
Export-PSRuleConvention 'Tests.Single' -End {
    foreach ($result in $PSRule.Output) {
        $PSRule.Badges.Create($result).ToFile('out/tests/PSRule.Tests/Badges/single.svg');
    }
}

# Synopsis: A convention that generates a badge for an aggregate result.
Export-PSRuleConvention 'Tests.Aggregate' -End {
    $PSRule.Badges.Create($PSRule.Output).ToFile('out/tests/PSRule.Tests/Badges/aggregate.svg');
}

# Synopsis: A convention that generates a custom badge.
Export-PSRuleConvention 'Tests.CustomBadge' -End {
    $PSRule.Badges.Create('PSRule', [PSRule.Badges.BadgeType]::Success, 'OK').ToFile('out/tests/PSRule.Tests/Badges/custom.svg');
}

# Synopsis: A rule that always passes.
Rule 'Tests.Badge.Pass' {
    $Assert.Pass();
}

# Synopsis: A rule that always fails.
Rule 'Tests.Badge.Fail' {
    $Assert.Fail();
}
