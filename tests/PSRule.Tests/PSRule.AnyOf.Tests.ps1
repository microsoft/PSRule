# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the AnyOf keyword
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

Describe 'PSRule -- AnyOf keyword' -Tag 'AnyOf' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    $invokeParams = @{
        Path = $ruleFilePath
    }

    Context 'AnyOf' {
        $testObject = @{
            Key = 'Value'
        }
        $result = $testObject | Invoke-PSRule @invokeParams -Name 'AnyOfTest', 'AnyOfTestNegative';

        It 'Should succeed on any positive conditions' {
            $filteredResult = $result | Where-Object { $_.RuleName -eq 'AnyOfTest' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.IsSuccess() | Should -Be $True;
        }

        It 'Should fail with all negative conditions' {
            $filteredResult = $result | Where-Object { $_.RuleName -eq 'AnyOfTestNegative' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.IsSuccess() | Should -Be $False;
        }
    }
}
