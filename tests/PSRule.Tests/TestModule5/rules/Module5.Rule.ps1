# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# A set of test rules in a module
#

# Synopsis: Test rule in TestModule5
Rule 'M5.Rule1' {
    $PSRule.TargetName -eq 'TestObject1'
    $PSRule.TargetType -eq 'TestObjectType'
    $Configuration.ruleConfig1 -eq 'Test'
}

# Synopsis: Test rule in TestModule5
Rule 'M5.Rule2' {
    $Configuration.ruleConfig1 -eq 'Test2'
}

Rule 'M5.Rule3' {
    $Configuration.ruleConfig2 -eq 'Test3'
}

# Synopsis: Test rule in TestModule5
Rule 'M5.Rule4' {
    $Configuration.ruleConfig3 -eq 'TestConfig3'
    $Configuration.ruleConfig2 -eq 'Test3'
}
