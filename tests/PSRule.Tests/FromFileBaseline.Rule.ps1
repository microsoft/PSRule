#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

#
# Rules for baseline unit testing
#

# Synopsis: Test for baseline
Rule 'WithBaseline' -Tag @{ category = 'group2'; severity = 'high' } {
    $Rule.TargetName -eq 'TestObject1'
    $Rule.TargetType -eq 'TestObjectType'
    $PSRule.Field.kind -eq 'TestObjectType'
}

# Synopsis: Test for baseline
Rule 'NotInBaseline' -Tag @{ category = 'group2'; severity = 'low' } {
    $False;
}
