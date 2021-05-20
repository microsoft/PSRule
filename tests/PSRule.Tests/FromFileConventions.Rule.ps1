# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Conventions for pesters tests
#

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention1' {
    Write-Debug -Message 'Process convention1'
    $PSRule.Data.count += 1;
} -Begin {
    Write-Debug -Message 'Begin convention1'
    $PSRule.Data.count += 1;
    $PSRule.Data['Order'] += 'Convention1|'
} -End {
    Write-Debug -Message 'End convention1'
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention2' {
    Write-Debug -Message 'Process convention2'
    $PSRule.Data.count *= 10;
} -Begin {
    Write-Debug -Message 'Begin convention2'
    $PSRule.Data.count *= 10;
} -End {
    Write-Debug -Message 'End convention2'
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention3' -If { $Assert.HasFieldValue($TargetObject, 'IfTest', 1) } {
    Write-Debug 'Convention3::Process'
    $PSRule.Data.count += 1000;
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention.Expansion' -If { $TargetObject.Name -eq 'TestObject1' } -Begin {
    $newObject = [PSCustomObject]@{
        Name = 'TestObject2'
    };
    $PSRule.Import($newObject);
    $PSRule.Import(@($newObject, $newObject));
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention.WithException' -Begin {
    throw 'Some exception';
}

# Synopsis: A rule for testing conventions
Rule 'ConventionTest' {
    $Assert.HasFieldValue($PSRule.Data, 'count', 1);
}
