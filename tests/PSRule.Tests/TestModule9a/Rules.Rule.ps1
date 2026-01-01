# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Test scoped configuration.
Rule 'M9ATestConfig' {
    Write-Verbose "Writing configuration: $($Configuration | ConvertTo-Json -Depth 5)"

    $Configuration.ConfigA -eq 'ValueA'
}

# Synopsis: Test scoped configuration.
Rule 'M9ATestConfig2' {
    $Null -eq $Configuration.ConfigB
}
