#
# Unit tests for the Match keyword
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

Describe 'PSRule -- Match keyword' -Tag 'Match' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'Match' {
        It 'With defaults' {
            # Test positive cases
            $goodObjects = @(
                [PSCustomObject]@{ PhoneNumber = '0400 000 000' }
                [PSCustomObject]@{ PhoneNumber = '000' }
                @{ PhoneNumber = '000' }
                @{ Value = @{ PhoneNumber = '0400 000 000' }}
            )
            $result = $goodObjects | Invoke-PSRule -Path $ruleFilePath -Name 'MatchTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.Outcome | Should -BeIn 'Pass';
            $result.RuleName | Should -BeIn 'MatchTest';

            # Test negative cases
            $badObjects = @(
                [PSCustomObject]@{ PhoneNo = '0400 000 000' }
                [PSCustomObject]@{ PhoneNumber = '0 000 000' }
                [PSCustomObject]@{ PhoneNumber = '100' }
                @{ PhoneNumber = '100' }
            )
            $result = $badObjects | Invoke-PSRule -Path $ruleFilePath -Name 'MatchTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.Outcome | Should -BeIn 'Fail';
            $result.RuleName | Should -BeIn 'MatchTest';
        }

        It 'With -CaseSensitive' {
            # Test positive cases
            $goodObjects = @(
                [PSCustomObject]@{ Title = 'Mr' }
                [PSCustomObject]@{ tiTle = 'Miss' }
                @{ Title = 'Miss' }
            )
            $result = $goodObjects | Invoke-PSRule -Path $ruleFilePath -Name 'MatchTestCaseSensitive';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result.Outcome | Should -BeIn 'Pass';
            $result.RuleName | Should -BeIn 'MatchTestCaseSensitive';

            # Test negative cases
            $badObjects = @(
                [PSCustomObject]@{ Title = 'MR' }
                [PSCustomObject]@{ tiTle = 'miss' }
                @{ Title = 'miss' }
            )
            $result = $badObjects | Invoke-PSRule -Path $ruleFilePath -Name 'MatchTestCaseSensitive';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result.Outcome | Should -BeIn 'Fail';
            $result.RuleName | Should -BeIn 'MatchTestCaseSensitive';
        }

        It 'With -Not' {
            $testObject = @(
                [PSCustomObject]@{ Title = 'Miss' }
                [PSCustomObject]@{ Title = 'Mr' }
            )

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'MatchNot';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleName | Should -BeIn 'MatchNot';
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $False;
        }
    }
}
