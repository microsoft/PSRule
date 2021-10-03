# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the TypeOf keyword
#

[CmdletBinding()]
param (

)

BeforeAll {
    # Setup error handling
    $ErrorActionPreference = 'Stop';
    Set-StrictMode -Version latest;

    # Setup tests paths
    $rootPath = $PWD;

    Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;
    $here = (Resolve-Path $PSScriptRoot).Path;
}

Describe 'PSRule -- TypeOf keyword' -Tag 'TypeOf' {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    }

    Context 'TypeOf' {
        It 'With defaults' {
            # Test positive cases
            $hashTableObject = @{ Key = 'Value' }
            $hashTableObjectWithName1 = @{ Key = 'Value' }; $hashTableObjectWithName1.PSObject.TypeNames.Insert(0, 'AdditionalTypeName');
            $hashTableObjectWithName2 = @{ Key = 'Value' }; $hashTableObjectWithName2.PSObject.TypeNames.Add('AdditionalTypeName');
            $customObjectWithName = [PSCustomObject]@{ Key = 'Value' }; $customObjectWithName.PSObject.TypeNames.Add('PSRule.Test.OtherType');
            $testObject = @($hashTableObject, $hashTableObjectWithName1, $hashTableObjectWithName2, $customObjectWithName);

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'TypeOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.IsSuccess() | Should -BeIn $True;
            $result.RuleName | Should -BeIn 'TypeOfTest';
            $result[0].Reason | Should -BeNullOrEmpty;

            # Test negative cases
            $testObject = [PSCustomObject]@{
                Key = 'Value'
            }

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'TypeOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.RuleName | Should -Be 'TypeOfTest';
            $result.Reason | Should -BeLike "None of the type name(s) match: *";
        }

        It 'If pre-condition' {
            $testObject = @(
                [PSCustomObject]@{
                    PSTypeName = 'PSRule.Test.OtherType'
                    Name = 'TestObject1'
                }
                [PSCustomObject]@{
                    Name = 'TestObject2'
                }
            )
            $option = New-PSRuleOption -NotProcessedWarning $False
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'TypeOfCondition' -Outcome All -Option $option;

            # Test positive cases
            $filteredResult = $result | Where-Object { $_.TargetName -eq 'TestObject1' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.Outcome | Should -Be 'Pass';

            # Test negative cases
            $filteredResult = $result | Where-Object { $_.TargetName -eq 'TestObject2' };
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.Outcome | Should -Be 'None';
        }
    }
}
