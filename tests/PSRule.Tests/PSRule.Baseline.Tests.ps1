# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

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
            $result.Length | Should -Be 5;
            $result[0].Name | Should -Be 'TestBaseline1';
            $result[3].Name | Should -Be 'TestBaseline4';;
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
                Id = '1'
            }
        )

        It 'With -Baseline' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestObjectType';
            $result[0].Field.kind | Should -Be 'TestObjectType';
        }

        It 'With obsolete' {
            # Not obsolete
            $Null = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1' -WarningVariable outWarn -WarningAction SilentlyContinue);
            $warnings = @($outWarn);
            $warnings.Length | Should -Be 0;

            # Obsolete
            $Null = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline5' -WarningVariable outWarn -WarningAction SilentlyContinue);
            $warnings = @($outWarn);
            $warnings.Length | Should -Be 1;
            $warnings[0] | Should -BeLike "*'TestBaseline5'*";
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
                'Binding.Field' = @{ kind = 'Kind' }
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M4.Rule1';
            $result[0].Outcome | Should -Be 'Fail';
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestObjectType';
            $result[0].Field.kind | Should -Be 'TestObjectType';
            $result[0].Field.uniqueIdentifer | Should -Be '1';
            $result[0].Field.AlternativeType | Should -Be 'TestObjectType';
            $result[1].RuleName | Should -Be 'M4.Rule2';
            $result[1].Outcome | Should -Be 'Pass';
            $result[1].TargetName | Should -Be 'TestObject1';
            $result[1].TargetType | Should -Be 'TestObjectType';
            $result[1].Field.AlternativeType | Should -Be 'TestObjectType';

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
            $result[0].Field.kind | Should -Be '1';
            $result[0].Field.uniqueIdentifer | Should -Be '1';

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
            $result[0].Field.AlternativeType | Should -Be 'TestObjectType';
            $result[1].RuleName | Should -Be 'M4.Rule3';
            $result[1].Outcome | Should -Be 'Pass';
            $result[1].Field.AlternativeType | Should -Be 'TestObjectType';

            # Module Config + Module + Workspace + Parameter + Explicit
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Name 'M4.Rule4' -Baseline 'Baseline3');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule4';
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].Field.AlternativeType | Should -Be 'TestObject1';
        }
    }

    Context 'Get-PSRule' {
        It 'With -Baseline' {
            $result = @(Get-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'WithBaseline';

            $result = @(Get-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline3');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[1].RuleName | Should -Be 'NotInBaseline';

            $result = @(Get-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline4');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[1].RuleName | Should -Be 'NotInBaseline';
        }
    }
}

#endregion Baseline
