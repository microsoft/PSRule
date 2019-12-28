# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# An object described in a PowerShell data file for unit testing

@{
    targetName = 'TestObject1'
    spec = @{
        properties = @{
            value1 = 1
            kind = 'Test'
            array = @(
                @{
                    id = 1
                },
                @{
                    id = 2
                }
            )
            array2 = @(
                "1"
                "2"
                "3"
            )
        }
    }
}
