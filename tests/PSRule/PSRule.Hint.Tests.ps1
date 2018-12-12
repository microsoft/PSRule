#
# Unit tests for the Hint keyword
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

Describe 'PSRule -- Hint keyword' -Tag 'Hint' {

    Context 'Hint' {
        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
            Value = @{
                Value1 = 1
            }
        }

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'HintTest' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.RuleId | Should -Be 'HintTest';
            $result.TargetName | Should -Be 'HintTarget';
            $result.Message | SHould -Be 'This is a message';
        }
    }
}
