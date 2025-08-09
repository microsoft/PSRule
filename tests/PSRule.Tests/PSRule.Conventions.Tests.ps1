# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for conventions
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
    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Conventions;
    Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
    $Null = New-Item -Path $outputPath -ItemType Directory -Force;
}

Describe 'PSRule -- Conventions' -Tag 'Convention' {
    BeforeAll {
        $rulePath = Join-Path -Path $here -ChildPath 'FromFileConventions.Rule.ps1';
    }

    Context 'With -Convention' {
        BeforeAll {
            $invokeParams = @{
                Path = $rulePath
                InputObject = [PSCustomObject]@{
                    Name = 'TestObject1'
                    IfTest = 0
                }
            }
        }

        It 'Uses convention' {
            # Without init convention
            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Convention 'Convention1');
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Outcome | Should -Be 'Fail';
            $result[0].Data.count | Should -Be 2;

            # Single convention + init convention
            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Convention 'Convention1', 'Convention.Init');
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].Data.count | Should -Be 2;

            # Multiple conventions
            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Convention 'Convention1','Convention2','Convention3');
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Outcome | Should -Be 'Fail';
            $result[0].Data.count | Should -Be 110;

            # With -If condition
            $invokeParams.InputObject.IfTest = 1;
            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Convention 'Convention1','Convention2','Convention3');
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Outcome | Should -Be 'Fail';
            $result[0].Data.count | Should -Be 1110;
        }

        It 'From module' {
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule4';
            $Null = Import-Module $testModuleSourcePath;

            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Module 'TestModule4' -Convention 'M4.Convention1');
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Data.M4 | Should -Be 1;
            $result[0].Data.M4C2 | Should -Be 1;

            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Module 'TestModule4' -Convention 'TestModule4\M4.Convention1');
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Data.M4 | Should -Be 1;
            $result[0].Data.M4C2 | Should -Be 1;
        }

        It 'Processes conventions in order' {
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule4';
            $Null = Import-Module $testModuleSourcePath;

            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Module 'TestModule4' -Convention 'Convention1' -Option @{
                'Convention.Include' = 'TestModule4\M4.Convention1'
            });
            $result | Should -Not -BeNullOrEmpty;
            $result[0].Data.Order | Should -Be 'Convention1|M4.Convention1|M4.Convention2|';
        }

        It 'Expands input object' {
            $result = @(Invoke-PSRule @invokeParams -Name 'ConventionTest' -Convention 'Convention.Expansion');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 5;
            $result[0].TargetName | Should -Be 'TestObject3';
            $result[0].TargetType | Should -Be 'ExpandCustomObject';
            $result[1].TargetName | Should -Be 'TestObject1';
            $result[1].TargetType | Should -Be 'System.Management.Automation.PSCustomObject';
            $result[2].TargetName | Should -Be 'TestObject2';
            $result[2].TargetType | Should -Be 'System.Management.Automation.PSCustomObject';
        }

        It 'Handles nested exceptions' {
            $Null = Invoke-PSRule @invokeParams -Name 'ConventionTest' -Convention 'Convention.WithException' -ErrorVariable errors -ErrorAction SilentlyContinue;
            $errors | Should -Not -BeNullOrEmpty;
            $errors.Exception.Message | Should -Be 'Some exception';
        }
    }
}
