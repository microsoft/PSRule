# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for Baseline functionality
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
    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Common;
    Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
    $Null = New-Item -Path $outputPath -ItemType Directory -Force;
}

#region Get-PSRuleBaseline

Describe 'Get-PSRuleBaseline' -Tag 'Baseline','Get-PSRuleBaseline' {
    BeforeAll {
        $baselineFilePath = Join-Path -Path $here -ChildPath 'Baseline.Rule.yaml';
    }

    Context 'Read baseline' {
        It 'With defaults' {
            $result = @(Get-PSRuleBaseline -Path $baselineFilePath);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 5;
            $result[0].Name | Should -Be 'TestBaseline1';
            $result[0].Module | Should -BeNullOrEmpty;
            $result[3].Name | Should -Be 'TestBaseline4';
        }

        It 'With -Name' {
            $result = @(Get-PSRuleBaseline -Path $baselineFilePath -Name TestBaseline1);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].Name | Should -Be 'TestBaseline1';
        }

        It 'With -Module' {
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule4';
            $Null = Import-Module $testModuleSourcePath;

            $result = @(Get-PSRuleBaseline -Module 'TestModule4');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;
            $result.Name | Should -BeIn 'Module4', 'Baseline2', 'Baseline3';
            $result.Module | Should -BeIn 'TestModule4';

            # Filter by name
            $result = @(Get-PSRuleBaseline -Module 'TestModule4' -Name 'Baseline2');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result.Name | Should -BeIn 'Baseline2';
            $result.Module | Should -BeIn 'TestModule4';
        }
    }

    Context 'Using -OutputFormat Yaml' {
        BeforeAll {
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule4';
            $Null = Import-Module $testModuleSourcePath;
        }
        It '<baseline>' -TestCases @(
            @{Baseline = 'Module4'; ExpectedYaml = @"
`# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  annotations: {}
  name: Module4
  tags: {}
spec:
  binding:
    field:
      kind:
      - Id
      uniqueIdentifer:
      - Id
      - AlternateName
    targetName:
    - AlternateName
    targetType:
    - Kind
  configuration:
    ruleConfig1: Test
  rule:
    include:
    - M4.Rule1
"@}
@{Baseline = 'Baseline2'; ExpectedYaml = @"
`# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  annotations: {}
  name: Baseline2
  tags: {}
spec:
  binding:
    targetName:
    - AlternateName
    targetType:
    - Kind
  configuration:
    ruleConfig2: Test3
  rule:
    include:
    - M4.Rule1
"@}
@{Baseline = 'Baseline3'; ExpectedYaml = @"
`# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  annotations: {}
  name: Baseline3
  tags: {}
spec:
  binding:
    field:
      AlternativeType:
      - AlternateName
    targetName:
    - AlternateName
    targetType:
    - Kind
  configuration:
    ruleConfig2: Test3
  rule:
    include:
    - M4.Rule1
"@}) {
    param($Baseline, $ExpectedYaml)
    $result = @(Get-PSRuleBaseline -Module 'TestModule4' -OutputFormat Yaml -Name $Baseline);
    $result | Should -Not -BeNullOrEmpty;
    $result | Should -MatchExactly $ExpectedYaml;
}
        It 'All Baselines' {
            $result = @(Get-PSRuleBaseline -Module 'TestModule4' -OutputFormat Yaml);
            $result | Should -Not -BeNullOrEmpty;

            $expectedYaml = @"
`# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  annotations: {}
  name: Module4
  tags: {}
spec:
  binding:
    field:
      kind:
      - Id
      uniqueIdentifer:
      - Id
      - AlternateName
    targetName:
    - AlternateName
    targetType:
    - Kind
  configuration:
    ruleConfig1: Test
  rule:
    include:
    - M4.Rule1
---
`# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  annotations: {}
  name: Baseline2
  tags: {}
spec:
  binding:
    targetName:
    - AlternateName
    targetType:
    - Kind
  configuration:
    ruleConfig2: Test3
  rule:
    include:
    - M4.Rule1
---
`# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  annotations: {}
  name: Baseline3
  tags: {}
spec:
  binding:
    field:
      AlternativeType:
      - AlternateName
    targetName:
    - AlternateName
    targetType:
    - Kind
  configuration:
    ruleConfig2: Test3
  rule:
    include:
    - M4.Rule1
"@
            $result | Should -MatchExactly $expectedYaml;
        }
    }
}

#endregion Get-PSRuleBaseline

#region Baseline

Describe 'Baseline' -Tag 'Baseline' {
    BeforeAll {
        $baselineFilePath = Join-Path -Path $here -ChildPath 'Baseline.Rule.yaml';
        $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileBaseline.Rule.ps1';
    }

    Context 'Invoke-PSRule' {
        BeforeAll {
            $testObject = @(
                [PSCustomObject]@{
                    AlternateName = 'TestObject1'
                    Kind = 'TestObjectType'
                    Id = '1'
                }
            )
        }

        It 'With -Baseline' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestObjectType';
            $result[0].Field.kind | Should -Be 'TestObjectType';
        }

        It 'With obsolete' {
            # Not obsolete
            $Null = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1' -WarningVariable outWarn -WarningAction SilentlyContinue);
            $warnings = @($outWarn);
            $warnings.Length | Should -Be 1;
            $warnings | Should -BeLike "The * resource * does not have an apiVersion set.*";

            # Obsolete
            $Null = @($testObject | Invoke-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline5' -WarningVariable outWarn -WarningAction SilentlyContinue);
            $warnings = @($outWarn | Where-Object {
                $_ -notlike "The * resource * does not have an apiVersion set.*"
            });
            $warnings.Length | Should -Be 1;
            $warnings[0] | Should -BeLike "*'TestBaseline5'*";
        }

        It 'With -Module' {
            $Null = Import-Module (Join-Path $here -ChildPath 'TestModule4') -Force;

            # Module
            $result = @($testObject | Invoke-PSRule -Module TestModule4);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule1';
            $result[0].Outcome | Should -Be 'Pass';

            # Module + Workspace
            $option = @{
                'Configuration.ruleConfig1' = 'Test2'
                'Rule.Include' = @('M4.Rule1', 'M4.Rule2')
                'Binding.Field' = @{ kind = 'Kind' }
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M4.Rule1';
            $result[0].Outcome | Should -Be 'Fail';
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestObjectType';
            $result[0].Field.kind | Should -Be 'TestObjectType';
            $result[0].Field.uniqueIdentifer | Should -Be '1';
            $result[0].Field.AlternativeType | Should -Be 'TestObjectType';
            $result[1].RuleName | Should -Be 'M4.Rule2';
            $result[1].Outcome | Should -Be 'Pass';
            $result[1].TargetName | Should -Be 'TestObject1';
            $result[1].TargetType | Should -Be 'TestObjectType';
            $result[1].Field.AlternativeType | Should -Be 'TestObjectType';

            # Module + Workspace + Parameter
            $option = @{
                'Configuration.ruleConfig1' = 'Test2'
                'Rule.Include' = @('M4.Rule1', 'M4.Rule2')
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option -Name 'M4.Rule2');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule2';
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].Field.kind | Should -Be '1';
            $result[0].Field.uniqueIdentifer | Should -Be '1';

            # Module + Workspace + Parameter + Explicit
            $option = @{
                'Configuration.ruleConfig1' = 'Test2'
                'Rule.Include' = @('M4.Rule1', 'M4.Rule2')
            }
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Option $option -Name 'M4.Rule2', 'M4.Rule3' -Baseline 'Baseline2');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M4.Rule2';
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].Field.AlternativeType | Should -Be 'TestObjectType';
            $result[1].RuleName | Should -Be 'M4.Rule3';
            $result[1].Outcome | Should -Be 'Pass';
            $result[1].Field.AlternativeType | Should -Be 'TestObjectType';

            # Module Config + Module + Workspace + Parameter + Explicit
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Name 'M4.Rule4' -Baseline 'Baseline3');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule4';
            $result[0].Outcome | Should -Be 'Pass';
            $result[0].Field.AlternativeType | Should -Be 'TestObject1';

            # Explict with default
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Path $ruleFilePath -Baseline 'Module4');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'M4.Rule1';

            # Explict with local scope
            $result = @($testObject | Invoke-PSRule -Module TestModule4 -Path $ruleFilePath -Baseline 'Module4' -Option @{
                'Rule.IncludeLocal' = $True
            });
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;
            $result.RuleName | Should -BeIn 'M4.Rule1', 'WithBaseline', 'NotInBaseline';
        }
    }

    Context 'Get-PSRule' {
        It 'With -Baseline' {
            $result = @(Get-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].RuleName | Should -Be 'WithBaseline';

            $result = @(Get-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline3');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[1].RuleName | Should -Be 'NotInBaseline';

            $result = @(Get-PSRule -Path $ruleFilePath,$baselineFilePath -Baseline 'TestBaseline4');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'WithBaseline';
            $result[1].RuleName | Should -Be 'NotInBaseline';
        }
    }
}

#endregion Baseline
