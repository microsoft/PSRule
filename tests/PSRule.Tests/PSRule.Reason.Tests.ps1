#
# Unit tests for the Reason keyword
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

Describe 'PSRule -- Reason keyword' -Tag 'Reason' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'Reason' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = @{
                Value1 = 1
            }
        }

        It 'Sets reason' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name 'ReasonTest','ReasonTest2','ReasonTest3' -Outcome All);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;
            $result.RuleName | Should -BeIn 'ReasonTest', 'ReasonTest2', 'ReasonTest3';
            $result[0].Reason | Should -BeIn 'This is a reason for TestObject1';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason | Should -BeIn 'This is a reason 1', 'This is a reason 2';
            $result[2].Reason | Should -BeNullOrEmpty;
        }
    }
}
