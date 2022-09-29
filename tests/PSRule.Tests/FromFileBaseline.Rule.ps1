# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Rules for baseline unit testing
#

# Synopsis: Test for baseline
Rule 'WithBaseline' -Tag @{ category = 'group2'; severity = 'high' } {
    $PSRule.TargetName -eq 'TestObject1'
    $PSRule.TargetType -eq 'TestObjectType'
    $PSRule.Field.kind -eq 'TestObjectType'
} -Labels @{ 'framework.v1/control' = @('c-2') }

# Synopsis: Test for baseline
Rule 'NotInBaseline' -Tag @{ category = 'group2'; severity = 'low' } {
    $False;
} -Labels @{ 'framework.v1/control' = @('c-3') }
