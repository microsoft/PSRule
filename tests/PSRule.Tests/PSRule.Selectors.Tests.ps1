# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for selectors
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
    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Selectors;
    Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
    $Null = New-Item -Path $outputPath -ItemType Directory -Force;
}

Describe 'PSRule -- Selectors' -Tag 'Selectors' {
    BeforeAll {
        $rulePath = Join-Path -Path $here -ChildPath 'FromFileWithSelectors.Rule.ps1';
        $yamlSelectorPath = Join-Path -Path $here -ChildPath 'Selectors.Rule.yaml';
        $jsonSelectorPath = Join-Path -Path $here -ChildPath 'Selectors.Rule.jsonc';
    }

    Context 'With selector' {
        BeforeAll {
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
        }

        It 'From Yaml file' {
            $invokeParams = @{
                Path = $rulePath, $yamlSelectorPath
            }

            $result = @($testObjects | Invoke-PSRule @invokeParams -Name 'WithSelectorTrue', 'WithSelectorFalse1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].RuleName | Should -Be 'WithSelectorTrue';
            $result[1].Outcome | Should -Be 'Fail';
            $result[1].RuleName | Should -Be 'WithSelectorFalse1';
        }

        It 'From Json file' {
            $invokeParams = @{
                Path = $rulePath, $jsonSelectorPath
            }

            $result = @($testObjects | Invoke-PSRule @invokeParams -Name 'WithSelectorTrue', 'WithSelectorFalse2');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].RuleName | Should -Be 'WithSelectorTrue';
            $result[1].Outcome | Should -Be 'Fail';
            $result[1].RuleName | Should -Be 'WithSelectorFalse2';
        }

        It 'From module' {
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
