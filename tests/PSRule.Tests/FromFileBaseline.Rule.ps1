#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

#
# Rules for baseline unit testing
#

# Synopsis: Test for baseline
Rule 'WithBaseline' {
    $Rule.TargetName -eq 'TestObject1'
    $Rule.TargetType -eq 'TestObjectType'
}

# Synopsis: Test for baseline
Rule 'NotInBaseline' {
    $False;
}
