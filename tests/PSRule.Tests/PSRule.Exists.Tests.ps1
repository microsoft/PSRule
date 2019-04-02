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
            Properties = $Null
        }

        $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ keyword = 'Exists' };

        It 'Return pass' {
            $filteredResult = $result | Where-Object { $_.RuleName -eq 'ExistsTest' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.IsSuccess() | Should -Be $True;
        }

        It 'Return fail' {
            $filteredResult = $result | Where-Object { $_.RuleName -eq 'ExistsTestNegative' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.IsSuccess() | Should -Be $False;
        }
    }
}
