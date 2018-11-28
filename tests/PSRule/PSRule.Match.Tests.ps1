#
# Unit tests for the Match keyword
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

Describe 'PSRule -- Match keyword' -Tag 'Match' {

    Context 'Match' {
        $testObject = [PSCustomObject]@{
            PhoneNumber = '0400 000 000'
        }

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'MatchTest' -Verbose;
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.RuleName | Should -Be 'MatchTest'
        }
    }
}
