#
# Unit tests for the Recommend keyword
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

Describe 'PSRule -- Recommend keyword' -Tag 'Recommend' {
    Context 'Recommend' {
        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
            Value = @{
                Value1 = 1
            }
        }

        It 'Sets result properties' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = @($testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name 'HintTest', 'RecommendTest' -Outcome All -WarningVariable outWarning);
            $warningMessages = @($outWarning);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'HintTest', 'RecommendTest';
            $result.Recommendation | Should -BeIn 'This is a recommendation';
            $warningMessages.Length | Should -Be 0;
        }

        It 'Uses comment metadata' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = @($testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name 'TestWithDescription', 'TestWithSynopsis' -Outcome All);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'TestWithDescription', 'TestWithSynopsis';
            $result.Recommendation | Should -BeIn 'Test for Recommend keyword';
        }

        It 'Uses documentation' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = @($testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name 'RecommendTest2' -Culture en-ZZ -Outcome All);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result.RuleName | Should -BeIn 'RecommendTest2';
            $result.Recommendation | Should -BeIn 'This is a recommendation from documentation.';
        }
    }
}
