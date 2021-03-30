# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Rule 'WithSelectorTrue' -With 'BasicSelector' {
    $True
}

Rule 'WithSelectorFalse' -With 'YamlCustomValueIn' {
    $False
}

