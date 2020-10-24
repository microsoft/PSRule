# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# A set of benchmark rules for testing PSRule performance
#

# Synopsis: A rule for testing PSRule performance
Rule 'Assert.HasFieldValue' {
    $Assert.HasFieldValue($TargetObject, 'value', 'Microsoft.Compute/virtualMachines');
}
