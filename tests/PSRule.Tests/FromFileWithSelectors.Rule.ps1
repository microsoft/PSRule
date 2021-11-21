# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Rule 'WithSelectorTrue' -With 'BasicSelector' {
    $True
}

Rule 'WithSelectorFalse1' -With 'YamlCustomValueIn' {
    $False
}

Rule 'WithSelectorFalse2' -With 'JsonCustomValueIn' {
    $False
}