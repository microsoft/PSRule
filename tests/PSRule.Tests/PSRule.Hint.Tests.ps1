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

        It 'Sets result properties' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name 'HintTest' -Outcome All -WarningVariable outWarning -WarningAction SilentlyContinue;
            $warningMessages = @($outWarning);
            $result | Should -Not -BeNullOrEmpty;
            $result.RuleName | Should -Be 'HintTest';
            $result.Message | Should -Be 'This is a message';
            $warningMessages.Length | Should -Be 1;
            $warningMessages | Should -Be "Hint parameter -TargetName is obsolete and has been replaced with TargetName binding. See about_PSRule_Options for details on how to set Binding.TargetName.";
        }

        It 'Uses description' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name 'HintTestWithDescription' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.RuleName | Should -Be 'HintTestWithDescription';
            $result.Message | Should -Be 'Test for Hint keyword';
        }
    }
}
