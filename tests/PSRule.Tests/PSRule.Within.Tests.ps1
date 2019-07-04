#
# Unit tests for the Within keyword
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

Describe 'PSRule -- Within keyword' -Tag 'Within' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'Within' {
        It 'With defaults' {
            $testObject = @(
                [PSCustomObject]@{ Title = 'mr' }
                [PSCustomObject]@{ Title = 'unknown' }
                @{ Title = 'mr' }
                @{ Value = @{ Title = 'Mr' } }
                @{ NotTitle = 'a different property' }
            )

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithinTest' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            $result.RuleName | Should -BeIn 'WithinTest';
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $False;
            $result[2].IsSuccess() | Should -Be $True;
            $result[3].IsSuccess() | Should -Be $True;
            $result[4].IsSuccess() | Should -Be $False;

            # Check non-string types
            $testObject = @(
                [PSCustomObject]@{ BooleanValue = $True; IntValue = 1; NullValue = $Null; }
                [PSCustomObject]@{ BooleanValue = $False; IntValue = 100; NullValue = $Null; }
                ([PSCustomObject]@{ BooleanValue = $True; IntValue = 1; NullValue = $Null; } | ConvertTo-Json | ConvertFrom-Json)
                [PSCustomObject]@{ BooleanValue = $Null; IntValue = $Null; NullValue = 'NotNull'; }
            )

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithinTypes' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.RuleName | Should -BeIn 'WithinTypes';
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $False;
            $result[2].IsSuccess() | Should -Be $True;
            $result[3].IsSuccess() | Should -Be $False;
        }

        It 'With -CaseSensitive' {
            $testObject = @(
                [PSCustomObject]@{ Title = 'mr' }
                [PSCustomObject]@{ Title = 'Mr' }
                @{ Title = 'Mr' }
            )

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithinTestCaseSensitive';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result.RuleName | Should -BeIn 'WithinTestCaseSensitive';
            $result[0].IsSuccess() | Should -Be $False;
            $result[1].IsSuccess() | Should -Be $True;
            $result[2].IsSuccess() | Should -Be $True;
        }

        It 'With -Not' {
            $testObject = @(
                [PSCustomObject]@{ Title = 'Miss' }
                [PSCustomObject]@{ Title = 'Mr' }
            )

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithinNot';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleName | Should -BeIn 'WithinNot';
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $False;
        }
    }
}
