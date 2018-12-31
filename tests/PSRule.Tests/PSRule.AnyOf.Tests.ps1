#
# Unit tests for the AnyOf keyword
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;
$here = (Resolve-Path $PSScriptRoot).Path;

Describe 'PSRule -- AnyOf keyword' -Tag 'AnyOf' {

    Context 'AnyOf' {
        $testObject = @{
            Key = 'Value'
        }

        It 'Should succeed on any positive conditions' {

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'AnyOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.RuleName | Should -Be 'AnyOfTest';
        }

        It 'Should fail with all negative conditions' {

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'AnyOfTestNegative';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.RuleName | Should -Be 'AnyOfTestNegative';
        }
    }
}
