# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Test scoped configuration.
Rule 'M9BTestConfig' {
    $Configuration.ConfigB -eq 'ValueB'
}

# Synopsis: Test scoped configuration.
Rule 'M9BTestConfig2' {
    $Null -eq $Configuration.ConfigA
}
