# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# A set of test rules in a module
#

# Synopsis: Test rule in TestModule3
Rule 'M3.Rule1' -DependsOn 'TestModule2\M2.Rule1', 'OtherRule' {
    $True
}

# Synopsis: Test rule in TestModule3
Rule 'OtherRule' -Tag @{ module = "TestModule3"} {
    $True
}

# Synopsis: Test selector from TestModule2
Rule 'RemoteSelector' -With 'TestModule2\M2.CustomValueSelector' {
    $True
}
