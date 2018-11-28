#
# Unit tests for the AllOf keyword
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

Describe 'PSRule -- AllOf keyword' -Tag 'AllOf' {

    Context 'AllOf' {
        $testObject = @{
            Key = 'Value'
        }

        It 'Should succeed on all positive conditions' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'AllOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.RuleName | Should -Be 'AllOfTest'
        }

        It 'Should fail on any negative conditions' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'AllOfTestNegative';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $False;
            $result.RuleName | Should -Be 'AllOfTestNegative'
        }
    }
}
