# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

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
    $invokeParams = @{
        Path = $ruleFilePath
    }

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
                CompareNumeric = 3
                CompareArray = 1, 2, 3
                CompareString = 'abc'
                InArray = @(
                    'Item1'
                    'Item3'
                    'Item4'
                )
                Path = $PSCommandPath
                ParentPath = $here
                Lower = 'test123'
                Upper = 'TEST123'
                LetterLower = 'test'
                LetterUpper = 'TEST'
            }
            [PSCustomObject]@{
                '$schema' = "http://json-schema.org/draft-07/schema`#"
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
                CompareNumeric = 0
                CompareArray = @()
                CompareString = ''
                InArray = @(
                    'item1'
                    'item2'
                    'item3'
                )
                InArrayPSObject = [PSObject[]]@(
                    'item1'
                    'item2'
                    'item3'
                )
                ParentPath = (Join-Path -Path $here -ChildPath 'notapath')
                Lower = 'Test123'
                Upper = 'Test123'
                LetterLower = 'test123'
                LetterUpper = 'TEST123'
            }
        )

        It 'In pre-conditions' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Precondition' -Outcome All -WarningAction SilentlyContinue);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].Outcome | Should -Be 'None';
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'With self field' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Self' -Outcome All -WarningAction SilentlyContinue);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].Outcome | Should -Be 'Fail';
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'Complete' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Complete');
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

        It 'Fail' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Fail');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Reason.Length | Should -Be 3;
            $result[0].Reason[2] | Should -Be 'Reason 5';
        }

        It 'Contains' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Contains');
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
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.EndsWith');
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

        It 'FileHeader' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.FileHeader');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -Be "The field 'Path' does not exist.";
        }

        It 'FilePath' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.FilePath');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason[0] | Should -Be "The field 'Path' does not exist.";
            $result[1].Reason[1] | Should -BeLike "The file '*' does not exist.";
        }

        It 'Greater' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Greater');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 3;
            $result[1].Reason | Should -BeLike "The value '*' was not > '*'.";
        }

        It 'GreaterOrEqual' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.GreaterOrEqual');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 3;
            $result[1].Reason | Should -BeLike "The value '0' was not >= '*'.";
        }

        It 'HasField' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.HasField');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 3;
            $result[1].Reason | Should -BeLike "The field '*' does not exist.";
        }

        It 'HasFields' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.HasFields');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason | Should -BeLike "The field '*' does not exist.";
        }

        It 'HasFieldValue' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.HasFieldValue');
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
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.HasDefaultValue');
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

        It 'HasJsonSchema' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.HasJsonSchema');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 3;
            $result[0].Reason | Should -BeIn 'The field ''$schema'' does not exist.';

            # Negative case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'JsonSchema' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.JsonSchema');
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

        It 'In' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.In');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 5;
            $result[0].Reason[0] | Should -Be "The field value 'TestObject1' was not included in the set.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'IsLower' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsLower');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason[0] | Should -BeLike "The value '*' does not contain only lowercase characters.";
            $result[1].Reason[1] | Should -BeLike "The value '*' does not contain only letters.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'IsUpper' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsUpper');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason[0] | Should -BeLike "The value '*' does not contain only uppercase characters.";
            $result[1].Reason[1] | Should -BeLike "The value '*' does not contain only letters.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'Less' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Less');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 3;
            $result[0].Reason | Should -BeLike "The value '*' was not < '*'.";
        }

        It 'LessOrEqual' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.LessOrEqual');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';

            # Positive case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 3;
            $result[0].Reason | Should -BeLike "The value '*' was not <= '*'.";
        }

        It 'Match' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Match');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 2;
            $result[0].Reason[0] | Should -BeLike "The field value 'TestObject1' does not match the pattern '*'.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'NotIn' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.NotIn');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 3;
            $result[0].Reason[0] | Should -Be "The field value 'TestObject1' was in the set.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'NotMatch' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.NotMatch');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 2;
            $result[0].Reason[0] | Should -BeLike "The field value 'TestObject1' matches the pattern '*'.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'NullOrEmpty' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.NullOrEmpty');
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
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.StartsWith');
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
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Version');
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
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.AddMember');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.IsSuccess() | Should -BeIn $True;
        }
    }
}

#endregion PSRule variables
