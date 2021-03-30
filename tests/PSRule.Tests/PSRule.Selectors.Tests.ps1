# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for selectors
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
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Selectors;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSRule -- Selectors' -Tag 'Selectors' {
    $rulePath = Join-Path -Path $here -ChildPath 'FromFileWithSelectors.Rule.ps1';
    $selectorPath = Join-Path -Path $here -ChildPath 'Selectors.Rule.yaml';

    Context 'With selector' {
        $invokeParams = @{
            Path = $rulePath, $selectorPath
        }
        It 'From file' {
            $testObjects = @(
                [PSCustomObject]@{
                    Name = 'TargetObject1'
                    Value = 'value1'
                }
                [PSCustomObject]@{
                    Name = 'TargetObject2'
                    Value = 'value1'
                    CustomValue = 'Value2'
                }
            )

            $result = @($testObjects | Invoke-PSRule @invokeParams -Name 'WithSelectorTrue', 'WithSelectorFalse');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].RuleName | Should -Be 'WithSelectorTrue';
            $result[1].Outcome | Should -Be 'Fail';
            $result[1].RuleName | Should -Be 'WithSelectorFalse';
        }

        It 'From module' {
            $testObjects = @(
                [PSCustomObject]@{
                    Name = 'TargetObject1'
                    Value = 'value1'
                }
                [PSCustomObject]@{
                    Name = 'TargetObject2'
                    Value = 'value1'
                    CustomValue = 'Value2'
                }
            )

            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule2';
            $Null = Import-Module $testModuleSourcePath;
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule3';
            $Null = Import-Module $testModuleSourcePath;

            $result = @($testObjects | Invoke-PSRule -Name 'RemoteSelector' -Module 'TestModule3' -WarningAction SilentlyContinue);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].TargetName | Should -Be 'TargetObject2';
        }
    }
}
