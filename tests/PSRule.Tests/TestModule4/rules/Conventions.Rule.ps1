# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# A set of test conventions in a module
#

Export-PSRuleConvention 'M4.Convention1' {
    Write-Debug 'M4.Convention1::Process'
    $PSRule.Data.M4 += 1;
} -Begin {
    $PSRule.Data['Order'] += 'M4.Convention1|'
}

Export-PSRuleConvention 'M4.Convention2' {
    Write-Debug 'M4.Convention2::Process'
    $PSRule.Data.M4C2 += 1;
} -Begin {
    $PSRule.Data['Order'] += 'M4.Convention2|'
}
