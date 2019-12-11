#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

#
# Unit tests for PSRule $Assert
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

#region PSRule variables

Describe 'PSRule assertions' -Tag 'Assert' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFileAssert.Rule.ps1');

    Context '$Assert' {
        $testObject = @(
            [PSCustomObject]@{
                Name = 'TestObject1'
                Type = 'TestType'
                Value = 'Value1'
                Array = 'Item1', 'Item2'
                String = 'Value'
                OtherField = 'Other'
                Int = 1
                Bool = $True
                Version = '2.0.0'
            }
            [PSCustomObject]@{
                Name = 'TestObject2'
                NotType = 'TestType'
                Value = $Null
                Array = @()
                String = ''
                Int = 2
                Bool = $False
                OtherBool = $False
                OtherInt = 2
                Version = '1.0.0'
            }
        )

        It 'Complete' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Complete');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -Be 'The field ''Type'' does not exist.';
        }

        It 'Contains' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Contains');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -BeLike "The field '*' does not exist.";
        }

        It 'EndsWith' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.EndsWith');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -BeLike "The field '*' does not end with '*'.";
        }

        It 'JsonSchema' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.JsonSchema');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason | Should -BeLike "Failed schema validation on `#*. *";
        }

        It 'HasField' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.HasField');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -BeLike "The field '*' does not exist.";
        }

        It 'HasFieldValue' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.HasFieldValue');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 7;
            $result[1].Reason[0] | Should -BeLike "The field '*' does not exist.";
            $result[1].Reason[1..3] | Should -BeLike "The value of '*' is null or empty.";
            $result[1].Reason[4..6] | Should -BeLike "The field '*' is set to '*'.";
        }

        It 'HasDefaultValue' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.HasDefaultValue');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 3;
            $result[1].Reason[0..2] | Should -BeLike "The field '*' is set to '*'.";
        }

        It 'NullOrEmpty' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.NullOrEmpty');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 4;
            $result[0].Reason | Should -BeLike "The field '*' is not empty.";
        }

        It 'StartsWith' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.StartsWith');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -BeLike "The field '*' does not start with '*'.";
        }

        It 'Version' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Version');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -BeLike "The version '*' does not match the constraint '*'.";
        }
    }

    Context '$Assert extension' {
        $testObject = @(
            [PSCustomObject]@{
                Name = 'TestObject1'
                Type = 'TestType'
            }
            [PSCustomObject]@{
                Name = 'TestObject2'
                NotType = 'TestType'
            }
        )

        It 'With Add-Member' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.AddMember');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.IsSuccess() | Should -BeIn $True;
        }
    }
}

#endregion PSRule variables
