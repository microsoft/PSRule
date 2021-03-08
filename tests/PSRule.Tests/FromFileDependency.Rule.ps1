# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Tests for dependencies
Rule 'Rule.1' {
    $TargetObject
}

# Synopsis: Tests for dependencies
Rule 'Rule.2' -DependsOn 'Rule.1' {
    $TargetObject
}

# Synopsis: Tests for dependencies
Rule 'Rule.3' -DependsOn 'Rule.2' {
    $TargetObject
}

# Synopsis: Tests for dependencies
Rule 'Rule.4' -DependsOn 'Rule.1' {
    !$TargetObject
}

# Synopsis: Tests for dependencies
Rule 'Rule.5' -DependsOn 'Rule.4' {
    !$TargetObject
}
