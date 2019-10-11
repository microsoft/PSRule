#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

#
# A set of test rules in a module
#

# Synopsis: Test rule in TestModule2
Rule 'M2.Rule2' -DependsOn 'M2.Rule1', 'OtherRule' {
    $True
}
