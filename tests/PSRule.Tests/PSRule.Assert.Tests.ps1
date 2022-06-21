# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for PSRule $Assert
#

[CmdletBinding()]
param ()

BeforeAll {
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
}

#region PSRule variables

Describe 'PSRule assertions' -Tag 'Assert' {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFileAssert.Rule.ps1');
        $invokeParams = @{
            Path = $ruleFilePath
        }
    }

    Context '$Assert' {
        BeforeAll {
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
                    CompareDate = [DateTime]::Now.AddDays(3)
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
                    IsInteger = 1
                    IsBoolean = $True
                    IsArray = 1, 2, 3
                    IsDateTime = [DateTime]::Now.AddDays(4)
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
                    CompareDate = [DateTime]::Now
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
                    ParentPath = (Join-Path -Path $here -ChildPath 'notapath/template.json')
                    Lower = 'Test123'
                    Upper = 'Test123'
                    LetterLower = 'test123'
                    LetterUpper = 'TEST123'
                    IsInteger = '1'
                    IsBoolean = 'true'
                    IsArray = '123'
                    IsDateTime = ([DateTime]::Now.AddDays(4).ToString('o'))
                }
            )
        }

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

        It 'Create' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Create');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason[0] | Should -Be 'Reason 1';
            $result[1].Reason[1] | Should -Be 'Reason 2';
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
            $result[1].Reason | Should -Be 'Path Type: Does not exist.';
        }

        It 'Fail' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Fail');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Reason.Length | Should -Be 3;
            $result[0].Reason[2] | Should -Be 'Reason 5';
        }

        It 'AnyOf' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.AnyOf');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.Outcome | Should -BeIn 'Pass';
        }

        It 'AllOf' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.AllOf');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $False;
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
            $result[1].Reason | Should -BeLike "Path *: Does not exist.";
        }

        It 'Count' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Count');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason | Should -BeLike "Path CompareArray: The field 'CompareArray' has '0' items instead of '3'.";
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
            $result[1].Reason | Should -BeLike "Path *: The field '*' does not end with '*'.";
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
            $result[1].Reason | Should -Be "Path Path: Does not exist.";
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
            $result[1].Reason[0] | Should -Be "Path Path: Does not exist.";
            $result[1].Reason[1] | Should -BeLike "Path ParentPath: The file '*' does not exist.";
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
            $result[1].Reason.Length | Should -Be 4;
            $result[1].Reason | Should -BeLike "Path *: The value '*' was not > '*'.";
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
            $result[1].Reason.Length | Should -Be 4;
            $result[1].Reason | Should -BeLike "Path *: The value '*' was not >= '*'.";
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
            $result[1].Reason[0] | Should -Be "Path Type: Does not exist.";
            $result[1].Reason[1] | Should -Be "Path Not: Does not exist.";
            $result[1].Reason[2] | Should -Be "Path Type: Does not exist.";
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
            $result[1].Reason[0] | Should -Be "Path Type: Does not exist.";
            $result[1].Reason[1] | Should -Be "Path Type: Does not exist.";
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
            $result[1].Reason[0] | Should -BeLike "Path Type: Does not exist.";
            $result[1].Reason[1..3] | Should -BeLike "Path *: Is null or empty.";
            $result[1].Reason[4..6] | Should -BeLike "Path *: Is set to '*'.";
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
            $result[0].Reason | Should -BeIn 'Path $schema: Does not exist.';

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
            $result[0].Reason[0] | Should -Be "Path Name: The field value 'TestObject1' was not included in the set.";

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
            $result[1].Reason[0] | Should -BeLike "Path Lower: The value '*' does not contain only lowercase characters.";
            $result[1].Reason[1] | Should -BeLike "Path LetterLower: The value '*' does not contain only letters.";

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
            $result[1].Reason[0] | Should -BeLike "Path Upper: The value '*' does not contain only uppercase characters.";
            $result[1].Reason[1] | Should -BeLike "Path LetterUpper: The value '*' does not contain only letters.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'IsNumeric' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsNumeric');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason[0] | Should -BeLike "Path IsInteger: The value '1' is not numeric.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'IsInteger' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsInteger');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason[0] | Should -Be "Path IsInteger: The value '1' is not an integer.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'IsBoolean' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsBoolean');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason[0] | Should -Be "Path IsBoolean: The value 'true' is not a boolean.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'IsArray' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsArray');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason[0] | Should -Be "Path IsArray: The value '123' is not an array.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'IsString' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsString');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 1;
            $result[0].Reason[0] | Should -Be "Path IsInteger: The value '1' is not a string.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'IsDateTime' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.IsDateTime');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason[0] | Should -Be "Path IsDateTime: The value '$($testObject[1].IsDateTime)' is not a date.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'TypeOf' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.TypeOf');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 4;
            $result[1].Reason | Should -BeLike "Path *: The field value '*' of type * is not *.";

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
            $result[0].Reason.Length | Should -Be 4;
            $result[0].Reason | Should -BeLike "Path *: The value '*' was not < '*'.";
        }

        It 'LessOrEqual' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.LessOrEqual');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 4;
            $result[0].Reason | Should -BeLike "Path *: The value '*' was not <= '*'.";
        }

        It 'Match' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Match');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 2;
            $result[0].Reason[0] | Should -BeLike "Path *: The field value 'TestObject1' does not match the pattern '*'.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'NotHasField' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.NotHasField');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 1;
            $result[0].Reason[0] | Should -BeLike "Path OtherField: Exists.";

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
            $result[0].Reason[0] | Should -Be "Path Name: The field value 'TestObject1' was in the set.";

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
            $result[0].Reason[0] | Should -BeLike "Path Name: The field value 'TestObject1' matches the pattern '*'.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }

        It 'NotNull' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.NotNull');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 2;
            $result[1].Reason[0] | Should -BeLike "Path Type: Does not exist.";
            $result[1].Reason[1] | Should -BeLike "Path Value: The field value 'Value' is null.";
        }

        It 'NotWithinPath' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.NotWithinPath');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason.Length | Should -Be 1;
            $result[1].Reason[0] | Should -BeLike "Path ParentPath: The file '*' is within the path 'tests/PSRule.Tests/notapath/'.";
        }

        It 'Null' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.Null');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 2;
            $result[0].Reason[0] | Should -BeLike "Path Type: The field value 'Type' is not null.";
            $result[0].Reason[1] | Should -BeLike "Path Value: The field value 'Value' is not null.";
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
            $result[0].Reason | Should -BeLike "Path *: The field '*' is not empty.";
        }

        It 'SetOf' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.SetOf');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason[0] | Should -Be "Path InArray: The field 'InArray' did not contain 'Item3'.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
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
            $result[1].Reason | Should -BeLike "Path Version: The field '*' does not start with '*'.";
        }

        It 'Subset' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.Subset');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[1].IsSuccess() | Should -Be $False;
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].Reason[0] | Should -Be "Path Array: The field 'Array' did not contain 'Item2'.";

            # Positive case
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
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
            $result[1].Reason | Should -BeLike "Path Version: The version '*' does not match the constraint '*'.";
        }

        It 'WithinPath' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'Assert.WithinPath');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Negative case
            $result[0].IsSuccess() | Should -Be $False;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].Reason.Length | Should -Be 1;
            $result[0].Reason[0] | Should -BeLike "Path ParentPath: The file '*' is not within the path 'tests/PSRule.Tests/notapath/'.";

            # Positive case
            $result[1].IsSuccess() | Should -Be $True;
            $result[1].TargetName | Should -Be 'TestObject2';
        }
    }

    Context '$Assert extension' {
        BeforeAll {
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
        }

        It 'With Add-Member' {
            $result = @($testObject | Invoke-PSRule @invokeParams -Name 'Assert.AddMember');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.IsSuccess() | Should -BeIn $True;
        }
    }
}

#endregion PSRule variables
