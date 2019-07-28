#
# A set of test rules in a module
#

# Synopsis: Test rule in TestModule2
Rule 'M2.Rule1' {
    $True
}

# Synopsis: Test rule in TestModule2
Rule 'OtherRule' -Tag @{ module = "TestModule2"} {
    $True
}
