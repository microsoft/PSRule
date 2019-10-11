#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

#
# Unit tests for Baseline functionality
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

if ($Env:SYSTEM_DEBUG -eq 'true') {
    $VerbosePreference = 'Continue';
}

# Setup tests paths
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;

$here = (Resolve-Path $PSScriptRoot).Path;
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Common;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

#region Get-PSRuleBaseline

Describe 'Get-PSRuleBaseline' -Tag 'Baseline','Get-PSRuleBaseline' {
    $baselineFilePath = Join-Path -Path $here -ChildPath 'Baseline.Rule.yaml';

    Context 'Read baseline' {
        It 'With defaults' {
            $result = @(Get-PSRuleBaseline -Path $baselineFilePath);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Name | Should -Be 'TestBaseline1';
            $result[1].Name | Should -Be 'TestBaseline2';
        }

        It 'With -Name' {
            $result = @(Get-PSRuleBaseline -Path $baselineFilePath -Name TestBaseline1);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].Name | Should -Be 'TestBaseline1';
        }
    }
}

#endregion Get-PSRuleBaseline

#region Baseline

Describe 'Baseline' -Tag 'Baseline' {
    $baselineFilePath = Join-Path -Path $here -ChildPath 'Baseline.Rule.yaml';
    $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileBaseline.Rule.ps1';

    Context 'Invoke-PSRule' {
        $testObject = @(
            [PSCustomObject]@{
                AlternateName = 'TestObject1'
                Kind = 'TestObjectType'
            }
        )
        $result = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1');

        It 'With -Baseline' {
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[0].Outcome | Should -Be 'Pass';
        }

        It 'With -Module' {
            $Null = Import-Module (Join-Path $here -ChildPath 'TestModule4') -Force;

            # Module
            $result = @($testObject | Invoke-PSRule -Module TestModule4);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule1';
            $result[0].Outcome | Should -Be 'Pass';

            # Module + Workspace
            $option = @{
                'Configuration.ruleConfig1' = 'Test2'
                'Rule.Include' = @('M4.Rule1', 'M4.Rule2')
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M4.Rule1';
            $result[0].Outcome | Should -Be 'Fail';
            $result[1].RuleName | Should -Be 'M4.Rule2';
            $result[1].Outcome | Should -Be 'Pass';

            # Module + Workspace + Parameter
            $option = @{
                'Configuration.ruleConfig1' = 'Test2'
                'Rule.Include' = @('M4.Rule1', 'M4.Rule2')
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option -Name 'M4.Rule2');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule2';
            $result[0].Outcome | Should -Be 'Pass';

            # Module + Workspace + Parameter + Explicit
            $option = @{
                'Configuration.ruleConfig1' = 'Test2'
                'Rule.Include' = @('M4.Rule1', 'M4.Rule2')
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option -Name 'M4.Rule2', 'M4.Rule3' -Baseline 'Baseline2');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M4.Rule2';
            $result[0].Outcome | Should -Be 'Pass';
            $result[1].RuleName | Should -Be 'M4.Rule3';
            $result[1].Outcome | Should -Be 'Pass';
        }
    }
}

#endregion Baseline
