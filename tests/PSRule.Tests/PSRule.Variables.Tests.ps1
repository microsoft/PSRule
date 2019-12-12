# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for PSRule variables
#

# Notes:
# $Assert variable is included in PSRule.Assert.Tests.ps1

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

#region PSRule variables

Describe 'PSRule variables' -Tag 'Variables' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'PowerShell automatic variables' {
        $testObject = [PSCustomObject]@{
            Name = 'VariableTest'
            Type = 'TestType'
            PSScriptRoot = $PSScriptRoot
            PWD = $PWD
            PSCommandPath = $ruleFilePath
            RuleTest = 'WithRuleVariable'
        }
        $testObject.PSObject.TypeNames.Insert(0, $testObject.Type);

        It '$PSRule' {
            $option = New-PSRuleOption -BindingField @{ kind = 'Type' };
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'VariableContextVariable';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
        }

        It '$Rule' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithRuleVariable';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
        }

        It '$LocalizedData' {
            $invokeParams = @{
                Path = $ruleFilePath
                Name = 'WithLocalizedData'
                Culture = 'en-ZZ'
                WarningAction = 'SilentlyContinue'
            }
            $result = $testObject | Invoke-PSRule @invokeParams -WarningVariable outWarning;
            $messages = @($outwarning);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
            $messages[0] | Should -Be 'LocalizedMessage for en-ZZ. Format=TestType.';
        }

        It '$PSScriptRoot' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithPSScriptRoot';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
        }

        It '$PWD' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithPWD';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
        }

        It '$PSCommandPath' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithPSCommandPath';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
        }
    }
}

#endregion PSRule variables
