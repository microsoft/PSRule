# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Pester unit test rules for error handling
#

# Synopsis: Should fail
Rule 'WithNonBoolean' {
    $True
    'false' # Not a boolean
}

Rule 'WithDependency1' -DependsOn 'WithDependency2' {
    $True;
}

Rule 'WithDependency2' -DependsOn 'WithDependency3' {
    $True;
}

Rule 'WithDependency3' -DependsOn 'WithDependency1' {
    $True;
}

Rule 'WithDependency4' -DependsOn 'WithDependency5' {
    $True;
}
