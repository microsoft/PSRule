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
    Context 'Match' {
        It 'Evaluates regex' {
            # Test positive cases
            $goodObjects = @(
                [PSCustomObject]@{ PhoneNumber = '0400 000 000' }
                [PSCustomObject]@{ PhoneNumber = '000' }
                @{ PhoneNumber = '000' }
            )
            $result = $goodObjects | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'MatchTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result.Outcome | Should -BeIn 'Pass';
            $result.RuleName | Should -BeIn 'MatchTest';

            # Test negative cases
            $badObjects = @(
                [PSCustomObject]@{ PhoneNo = '0400 000 000' }
                [PSCustomObject]@{ PhoneNumber = '0 000 000' }
                [PSCustomObject]@{ PhoneNumber = '100' }
                @{ PhoneNumber = '100' }
            )
            $result = $badObjects | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'MatchTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.Outcome | Should -BeIn 'Fail';
            $result.RuleName | Should -BeIn 'MatchTest';
        }

        It 'Evaluates regex with case sensitivity' {
            # Test positive cases
            $goodObjects = @(
                [PSCustomObject]@{ Title = 'Mr' }
                [PSCustomObject]@{ tiTle = 'Miss' }
                @{ Title = 'Miss' }
            )
            $result = $goodObjects | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'MatchTestCaseSensitive';
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
            $result = $badObjects | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'MatchTestCaseSensitive';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result.Outcome | Should -BeIn 'Fail';
            $result.RuleName | Should -BeIn 'MatchTestCaseSensitive';
        }
    }
}
