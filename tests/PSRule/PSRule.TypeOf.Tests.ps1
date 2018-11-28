#
# Unit tests for the TypeOf keyword
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

Describe 'PSRule -- TypeOf keyword' -Tag 'TypeOf' {

    Context 'TypeOf' {
        $testObject = @{
            Key = 'Value'
        }

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'TypeOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.RuleName | Should -Be 'TypeOfTest'
        }
    }
}
