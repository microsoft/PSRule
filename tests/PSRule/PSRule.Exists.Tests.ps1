#
# Unit tests for the Exists keyword
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

Describe 'PSRule -- Exists keyword' -Tag 'Exists' {

    Context 'Exists' {
        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
            Value = @{
                Value1 = 1
            }
        }

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'ExistsTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.RuleName | Should -Be 'ExistsTest'
        }
    }
}
