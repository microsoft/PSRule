#
# Unit tests for the Within keyword
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

Describe 'PSRule -- Within keyword' -Tag 'Within' {

    Context 'Within' {
        $testObject = [PSCustomObject]@{
            Title = 'Mr'
        }

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'WithinTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.RuleName | Should -Be 'WithinTest'
        }
    }
}
