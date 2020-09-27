# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the AllOf keyword
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

if ($Env:SYSTEM_DEBUG -eq 'true') {
    $VerbosePreference = 'Continue';
}

# Setup tests paths
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;
$here = (Resolve-Path $PSScriptRoot).Path;

Describe 'PSRule -- AllOf keyword' -Tag 'AllOf' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    $invokeParams = @{
        Path = $ruleFilePath
    }

    Context 'AllOf' {
        $testObject = @{
            Key = 'Value'
        }
        $result = $testObject | Invoke-PSRule @invokeParams -Name 'AllOfTest','AllOfTestNegative';

        It 'Should succeed on all positive conditions' {
            $filteredResult = $result | Where-Object { $_.RuleName -eq 'AllOfTest' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.IsSuccess() | Should -Be $True;
        }

        It 'Should fail on any negative conditions' {
            $filteredResult = $result | Where-Object { $_.RuleName -eq 'AllOfTestNegative' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.IsSuccess() | Should -Be $False;
        }
    }
}
