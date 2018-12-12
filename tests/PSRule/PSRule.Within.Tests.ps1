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

        It 'Matches list' {
            $testObject = @(
                [PSCustomObject]@{ Title = 'mr' },
                [PSCustomObject]@{ Title = 'unknown' }
            )

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'WithinTest' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleId | Should -BeIn 'WithinTest'
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $False;
        }

        It 'Matches list with case sensitivity' {
            $testObject = @(
                [PSCustomObject]@{ Title = 'mr' },
                [PSCustomObject]@{ Title = 'Mr' }
            )

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'WithinTestCaseSensitive';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleId | Should -BeIn 'WithinTestCaseSensitive'
            $result[0].IsSuccess() | Should -Be $False;
            $result[1].IsSuccess() | Should -Be $True;
        }
    }
}
