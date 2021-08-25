# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for Baseline functionality
#

[CmdletBinding()]
param ()

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
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Badges;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

#region Baseline

Describe 'Badges' -Tag 'Badges' {
    $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileBadge.Rule.ps1';

    Context 'Generates badges' {
        $testObject = @(
            [PSCustomObject]@{
                Name = 'TestObject1'
            }
        )

        It 'Single' {
            $outputFile = Join-Path -Path $outputPath -ChildPath 'single.svg';
            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Convention Tests.Single -Name 'Tests.Badge.Pass';
            $text = ([xml](Get-Content -Path $outputFile -Raw)).svg.'aria-label';
            $text | Should -Be 'PSRule: Pass';

            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Convention Tests.Single;
            $text = ([xml](Get-Content -Path $outputFile -Raw)).svg.'aria-label';
            $text | Should -Be 'PSRule: Fail';
        }

        It 'Aggregate' {
            $outputFile = Join-Path -Path $outputPath -ChildPath 'aggregate.svg';
            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Convention Tests.Aggregate -Name 'Tests.Badge.Pass';
            $text = ([xml](Get-Content -Path $outputFile -Raw)).svg.'aria-label';
            $text | Should -Be 'PSRule: Pass';

            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Convention Tests.Aggregate;
            $text = ([xml](Get-Content -Path $outputFile -Raw)).svg.'aria-label';
            $text | Should -Be 'PSRule: Fail';
        }

        It 'Custom' {
            $outputFile = Join-Path -Path $outputPath -ChildPath 'custom.svg';
            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Convention Tests.CustomBadge -Name 'Tests.Badge.Fail';
            $text = ([xml](Get-Content -Path $outputFile -Raw)).svg.'aria-label';
            $text | Should -Be 'PSRule: OK';
        }
    }
}

#endregion Baseline
