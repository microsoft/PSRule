# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for core PSRule cmdlets
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

#region Invoke-PSRule

Describe 'Invoke-PSRule' -Tag 'Invoke-PSRule','Common' {
    BeforeAll {
        $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1';
        $yamlFilePath = Join-Path -Path $here -ChildPath 'FromFile.Rule.yaml';
        $jsonFilePath = Join-Path -Path $here -ChildPath 'FromFile.Rule.jsonc';
        $emptyOptionsFilePath = Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml';
    }

    Context 'With defaults' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
                '_PSRule' = [PSCustomObject]@{
                    source = @(
                        [PSCustomObject]@{
                            file = 'source.json'
                            Line = 100
                            Position = 1000
                            type = "Origin"
                        }
                    )
                }
            }
            $testObject.PSObject.TypeNames.Insert(0, 'TestType');
        }

        It 'Returns passed' {
            [PSRule.Environment]::UseCurrentCulture('en-ZZ');
            try {
                $result = $testObject | Invoke-PSRule -Option $emptyOptionsFilePath -Path $ruleFilePath -Name 'FromFile1';
                $result | Should -Not -BeNullOrEmpty;
                $result.IsSuccess() | Should -Be $True;
                $result.TargetName | Should -Be 'TestObject1';
                $result.Info.Annotations.culture | Should -Be 'en-ZZ';
                $result.Recommendation | Should -Be 'This is a recommendation.';
                $result.Source | Should -Not -BeNullOrEmpty;
                $result.Source[0].File | Should -Be 'source.json';
                $result.Source[0].Line | Should -Be 100;
                $result.Source[0].Position | Should -Be 1000;
                Assert-VerifiableMock;
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }
        }

        It 'Returns failure' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile2' -ErrorAction Stop;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.TargetName | Should -Be 'TestObject1';
            $result.GetReasonViewString() | Should -Be 'Returned a `false`.'

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile5' -ErrorAction Stop;
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Fail';
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Returns error' {
            $withLoggingRulePath = (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1');
            $result = $testObject | Invoke-PSRule -Path $withLoggingRulePath -Name 'WithError' -ErrorAction SilentlyContinue -WarningAction SilentlyContinue;
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Error';
            $result.TargetName | Should -Be 'TestObject1';
            $result.Error.ErrorId | Should -BeLike '*,WithError';
        }

        It 'Returns inconclusive' {
            $option = @{ 'Execution.RuleInconclusive' = 'Ignore' };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile3' -Outcome All -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.OutcomeReason | Should -Be 'Inconclusive';
        }

        It 'Returns rule timing' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'WithSleep';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.Time | Should -BeGreaterThan 0;
        }

        It 'Propagates PowerShell logging' {
            $withLoggingRulePath = (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1');
            $loggingParams = @{
                Path = $withLoggingRulePath
                Name = 'WithWarning','WithError','WithInformation','WithVerbose'
                InformationVariable = 'outInformation'
                WarningVariable = 'outWarnings'
                ErrorVariable = 'outErrors'
                WarningAction = 'SilentlyContinue'
                ErrorAction = 'SilentlyContinue'
                InformationAction = 'SilentlyContinue'
            }

            $outVerbose = $testObject | Invoke-PSRule @loggingParams -Verbose 4>&1 | Where-Object {
                $_ -is [System.Management.Automation.VerboseRecord] -and
                $_.Message -like "* verbose message"
            };

            # Warnings, errors, information, verbose
            $warningMessages = $outWarnings.ToArray();
            $warningMessages.Length | Should -Be 2;
            $warningMessages[0] | Should -Be 'Script warning message';
            $warningMessages[1] | Should -Be 'Rule warning message';

            # Errors
            $outErrors | Should -BeLike '*Rule error message*';
            $outErrors.FullyQualifiedErrorId | Should -BeLike '*,WithError,Invoke-PSRule';

            # Information
            $informationMessages = $outInformation.ToArray();
            $informationMessages.Length | Should -Be 2;
            $informationMessages[0] | Should -Be 'Script information message';
            $informationMessages[1] | Should -Be 'Rule information message';

            # Verbose
            $outVerbose.Length | Should -Be 2;
            $outVerbose[0] | Should -Be 'Script verbose message';
            $outVerbose[1] | Should -Be 'Rule verbose message';
        }

        It 'Propagates PowerShell exceptions' {
            $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFileWithException.Rule.ps1') -ErrorVariable outErrors -ErrorAction SilentlyContinue -WarningAction SilentlyContinue;
            $outErrors | Should -Be 'You cannot call a method on a null-valued expression.', 'PSR0016: Could not find a matching rule. Please check that Path, Name, and Tag parameters are correct. See https://aka.ms/ps-rule/troubleshooting';
        }

        It 'Processes rule tags' {
            # Ensure that rules can be selected by tag and that tags are mapped back to the rule results using Yaml file path
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath, $yamlFilePath -Tag @{ feature = 'tag' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 6;
            $result.Tag.feature | Should -BeIn 'tag';

            # Ensure that rules can be selected by tag and that tags are mapped back to the rule results using Json file path
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath, $jsonFilePath -Tag @{ feature = 'tag' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 6;
            $result.Tag.feature | Should -BeIn 'tag';

            # Ensure that tag selection is and'ed together, requiring all tags to be selected
            # Tag values, will be matched without case sensitivity
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ feature = 'tag'; severity = 'critical'; };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result.Tag.feature | Should -BeIn 'tag';

            # Using a * wildcard in tag filter, matches rules with the tag regardless of value 
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ feature = 'tag'; severity = '*'; };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.Tag.feature | Should -BeIn 'tag';
            $result.Tag.severity | Should -BeIn 'critical', 'information';
        }

        It 'Processes rule script preconditions' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ category = 'precondition-if' } -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionTrue' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionFalse' }).Outcome | Should -Be 'None';
        }

        It 'Processes rule type preconditions' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ category = 'precondition-type' } -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithTypeTrue' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithTypeFalse' }).Outcome | Should -Be 'None';
        }

        It 'Processes rule dependencies' {
            # Same file
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name WithDependency1 -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency5' }).Outcome | Should -Be 'Fail';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency4' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency3' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency2' }).Outcome | Should -Be 'None';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency1' }).Outcome | Should -Be 'None';

            # Multiple objects
            $dependsRuleFilePath = Join-Path -Path $here -ChildPath 'FromFileDependency.Rule.ps1';
            $result = @($True, $False, $True | Invoke-PSRule -Path $dependsRuleFilePath -Outcome All);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 15;
            # True
            $result[0..2].Outcome | Should -BeIn Pass;
            $result[0..2].OutcomeReason | Should -BeIn Processed;
            $result[3].Outcome | Should -BeIn Fail;
            $result[3].OutcomeReason | Should -BeIn Processed;
            $result[4].Outcome | Should -BeIn None;
            $result[4].OutcomeReason | Should -BeIn DependencyFail;
            # False
            $result[5].Outcome | Should -BeIn Fail;
            $result[5].OutcomeReason | Should -BeIn Processed;
            $result[6..9].Outcome | Should -BeIn None;
            $result[6..9].OutcomeReason | Should -BeIn DependencyFail;
            # True
            $result[10..12].Outcome | Should -BeIn Pass;
            $result[10..12].OutcomeReason | Should -BeIn Processed;
            $result[13].Outcome | Should -BeIn Fail;
            $result[13].OutcomeReason | Should -BeIn Processed;
            $result[14].Outcome | Should -BeIn None;
            $result[14].OutcomeReason | Should -BeIn DependencyFail;

            # Same module, cross file
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule2';
            $Null = Import-Module $testModuleSourcePath;
            $result = $testObject | Invoke-PSRule -Module 'TestModule2' -Name 'M2.Rule2' -Outcome Pass;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'M2.Rule1' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'M2.Rule2' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleId -eq 'TestModule2\OtherRule' }).Outcome | Should -Be 'Pass';

            # Cross module
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule3';
            $Null = Import-Module $testModuleSourcePath;
            $result = $testObject | Invoke-PSRule -Module 'TestModule3' -Name 'M3.Rule1' -Outcome Pass;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'M2.Rule1' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'M3.Rule1' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleId -eq 'TestModule3\OtherRule' }).Outcome | Should -Be 'Pass';

            # Cross module - only required rules
            $result = $testObject | Invoke-PSRule -Module 'TestModule3' -Outcome Pass;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
        }

        It 'Suppresses rules' {
            $testObject = @(
                [PSCustomObject]@{
                    Name = "TestObject1"
                }
                [PSCustomObject]@{
                    Name = "TestObject2"
                }
            )

            $option = New-PSRuleOption -SuppressTargetName @{ FromFile1 = 'TestObject1'; FromFile2 = 'testobject1'; };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name 'FromFile1', 'FromFile2' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            ($result | Where-Object { $_.TargetName -eq 'TestObject1' }).OutcomeReason | Should -BeIn 'Suppressed';
            ($result | Where-Object { $_.TargetName -eq 'TestObject2' }).OutcomeReason | Should -BeIn 'Processed';

            # With aliases
            $aliasRuleFilePath = @(
                (Join-Path -Path $here -ChildPath 'FromFileAlias.Rule.jsonc')
                (Join-Path -Path $here -ChildPath 'FromFileAlias.Rule.ps1')
            )
            $option = New-PSRuleOption -SuppressTargetName @{ 'JSON.AlternativeName' = 'TestObject1'; 'PSRZZ.0003' = 'testobject2'; 'PS.AlternativeName' = 'TestObject1' };
            $result = $testObject | Invoke-PSRule -Path $aliasRuleFilePath -Option $option -Name 'JSON.AlternativeName','PS.RuleWithAlias1' -Outcome All;
            ($result | Where-Object { $_.TargetName -eq 'TestObject1' }).OutcomeReason | Should -BeIn 'Suppressed';
            $result.Count | Should -Be 4;
        }

        It 'Processes configuration' {
            $option = New-PSRuleOption -BaselineConfiguration @{ Value1 = 1 };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name WithConfiguration;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
        }

        It 'Returns failure reason' {
            $testObject = @(
                [PSCustomObject]@{
                    Name = "TestObject1"
                }
                [PSCustomObject]@{
                    Name = "TestObject2"
                }
            )

            # With Yaml path
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath, $yamlFilePath -Tag @{ test = 'Reason' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result[0].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject1''.';
            $result[1].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject1''.';
            $result[2].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject2''.';
            $result[3].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject2''.';

            # With Json path
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath, $jsonFilePath -Tag @{ test = 'Reason' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result[0].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject1''.';
            $result[1].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject1''.';
            $result[2].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject2''.';
            $result[3].Reason | Should -BeExactly 'Path Name: Is set to ''TestObject2''.';
        }

        It 'Returns severity level' {
            $testObject = @(
                [PSCustomObject]@{
                    Name = "TestObject1"
                }
                [PSCustomObject]@{
                    Name = "TestObject2"
                }
            )

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath, $yamlFilePath, $jsonFilePath -Tag @{ test = 'Level' } -Outcome Fail;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 9;

            # Error
            $filteredResult = @($result | Where-Object {
                $_.Level -eq 'Error'
            })
            $filteredResult.Count | Should -Be 3;
            $filteredResult.RuleName | Should -BeIn 'PS1RuleErrorLevel', 'YamlRuleErrorLevel', 'JsonRuleErrorLevel'

            # Warning
            $filteredResult = @($result | Where-Object {
                $_.Level -eq 'Warning'
            })
            $filteredResult.Count | Should -Be 3;
            $filteredResult.RuleName | Should -BeIn 'PS1RuleWarningLevel', 'YamlRuleWarningLevel', 'JsonRuleWarningLevel'

            # Information
            $filteredResult = @($result | Where-Object {
                $_.Level -eq 'Information'
            })
            $filteredResult.Count | Should -Be 3;
            $filteredResult.RuleName | Should -BeIn 'PS1RuleInfoLevel', 'YamlRuleInfoLevel', 'JsonRuleInfoLevel'
        }
    }

    Context 'Using -Path' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
            }
        }

        It 'Returns error with bad path' {
            $notFilePath = Join-Path -Path $here -ChildPath 'NotAFile.ps1';
            { $testObject | Invoke-PSRule -Path $notFilePath } | Should -Throw -ExceptionType System.IO.FileNotFoundException;
        }

        It 'Returns warning with empty path' {
            $emptyPath = Join-Path -Path $outputPath -ChildPath 'empty';
            if (!(Test-Path -Path $emptyPath)) {
                $Null = New-Item -Path $emptyPath -ItemType Directory -Force;
            }
            $Null = $testObject | Invoke-PSRule -Path $emptyPath -ErrorVariable outErrors -ErrorAction SilentlyContinue;
            $errorMessages = @($outErrors);
            $errorMessages.Length | Should -Be 1;
            $errorMessages[0] | Should -BeOfType [System.Management.Automation.ErrorRecord];
            $errorMessages[0].Exception.Message | Should -Be 'PSR0015: No valid sources were found. Please check your working path and configured options. See https://aka.ms/ps-rule/troubleshooting';
        }
    }

    Context 'Using -As' {
        BeforeAll {
            $testObject = @(
                [PSCustomObject]@{
                    Name = "TestObject1"
                    Value = 1
                }
                [PSCustomObject]@{
                    Name = "TestObject2"
                    Value = 2
                }
            );
        }

        It 'Returns detail' {
            $option = Join-Path -Path $here -ChildPath 'PSRule.Tests5.yml';
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ category = 'group1' } -As Detail -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
        }

        It 'Returns summary' {
            [PSRule.Environment]::UseCurrentCulture('en-ZZ');
            try {
                $option = Join-Path -Path $here -ChildPath 'PSRule.Tests5.yml';
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ category = 'group1' } -As Summary -Outcome All -Option $option;
                $result | Should -Not -BeNullOrEmpty;
                $result.Count | Should -Be 4;
                $result | Should -BeOfType PSRule.Rules.RuleSummaryRecord;
                $result.RuleName | Should -BeIn 'FromFile1', 'FromFile2', 'FromFile3', 'FromFile4'
                $result.Tag.category | Should -BeIn 'group1';
                ($result | Where-Object { $_.RuleName -eq 'FromFile1'}).Outcome | Should -Be 'Pass';
                ($result | Where-Object { $_.RuleName -eq 'FromFile1'}).Pass | Should -Be 2;
                ($result | Where-Object { $_.RuleName -eq 'FromFile1'}).Info.Annotations.culture | Should -Be 'en-ZZ';
                ($result | Where-Object { $_.RuleName -eq 'FromFile2'}).Outcome | Should -Be 'Fail';
                ($result | Where-Object { $_.RuleName -eq 'FromFile2'}).Fail | Should -Be 2;
                ($result | Where-Object { $_.RuleName -eq 'FromFile4'}).Outcome | Should -Be 'None';
                ($result | Where-Object { $_.RuleName -eq 'FromFile4'}).Pass | Should -Be 0;
                ($result | Where-Object { $_.RuleName -eq 'FromFile4'}).Fail | Should -Be 0;
                Assert-VerifiableMock;
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }
        }

        It 'Returns filtered summary' {
            $option = Join-Path -Path $here -ChildPath 'PSRule.Tests5.yml';
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ category = 'group1' } -As Summary -Outcome Fail -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleSummaryRecord;
            $result.RuleName | Should -BeIn 'FromFile2', 'FromFile3';
            $result.Tag.category | Should -BeIn 'group1';
        }
    }

    Context 'Using -InputStringFormat' {
        It 'Yaml String' {
            $yaml = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $yaml -InputStringFormat Yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'Yaml FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -InputStringFormat Yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'Json String' {
            $json = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.json') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $json -InputStringFormat Json);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'Json FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.json') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -InputStringFormat Json);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'Markdown String' {
            $markdown = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.md') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $markdown -InputStringFormat Markdown);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'Markdown FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.md') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -InputStringFormat Markdown);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'PowerShellData String' {
            $data = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.psd1') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $data -InputStringFormat powershell_data);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -BeIn $True;
        }

        It 'PowerShellData FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.psd1') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -InputStringFormat powershell_data);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -BeIn $True;
        }
    }

    Context 'Using -OutputFormat' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
            }
            $testObject.PSObject.TypeNames.Insert(0, 'TestType');
        }

        It 'Yaml' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result -cmatch 'ruleName: FromFile1' | Should -Be $True;
            $result -cmatch 'outcome: Pass' | Should -Be $True;
            $result -cmatch 'targetName: TestObject1' | Should -Be $True;
            $result -cmatch 'targetType: TestType' | Should -Be $True;
            $result | Should -Match 'tag:(\r|\n){1,2}\s{2,}(test: Test1|category: group1)';
            $result | Should -Not -Match 'targetObject:';
        }

        It 'Json' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Json);
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result -cmatch '"ruleName":"FromFile1"' | Should -Be $True;
            $result -cmatch '"outcome":"Pass"' | Should -Be $True;
            $result -cmatch '"targetName":"TestObject1"' | Should -Be $True;
            $result -cmatch '"targetType":"TestType"' | Should -Be $True;
            $result | Should -Not -Match '"targetObject":';
        }

        It 'NUnit3' {
            $invokeParams = @{
                Path = $ruleFilePath
                OutputFormat = 'NUnit3'
            }
            $result = $testObject | Invoke-PSRule @invokeParams -Name 'WithPreconditionFalse' -WarningAction SilentlyContinue | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;

            $result = $testObject | Invoke-PSRule @invokeParams -Name 'FromFile1','FromFile2','FromFile3' -Option @{ 'Output.Style' = 'AzurePipelines' } -WarningAction SilentlyContinue | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match 'Returned a \\`false\\`\.'

            # Check XML schema

            $schemas = New-Object -TypeName System.Xml.Schema.XmlSchemaSet;
            $schemas.CompilationSettings.EnableUpaCheck = $false

            try {
                $stream = (Get-Item -Path (Join-Path -Path $here -ChildPath 'NUnit.Schema.xsd')).OpenRead();
                $schema = [Xml.Schema.XmlSchema]::Read($stream, $Null);
                $Null = $schemas.Add($schema);
            }
            finally {
                $stream.Close();
            }
            $schemas.Compile();

            $resultXml = [XML]$result;
            $resultXml.Schemas = $schemas;
            { $resultXml.Validate($Null) } | Should -Not -Throw;

            # Success
            $filteredResult = $resultXml.SelectNodes('/test-results/test-suite/results/test-case') | Where-Object {
                $_.name -like '* FromFile1'
            }
            $filteredResult.success | Should -Be 'True';
            $filteredResult.executed | Should -Be 'True';

            # Failure
            $filteredResult = $resultXml.SelectNodes('/test-results/test-suite/results/test-case') | Where-Object {
                $_.name -like '* FromFile2'
            }
            $filteredResult.success | Should -Be 'False';
            $filteredResult.executed | Should -Be 'True';
            $filteredResult.failure | Should -Not -BeNullOrEmpty;

            # Inconclusive
            $filteredResult = $resultXml.SelectNodes('/test-results/test-suite/results/test-case') | Where-Object {
                $_.name -like '* FromFile3'
            }
            $filteredResult.success | Should -Be 'False';
            $filteredResult.executed | Should -Be 'True';
            $filteredResult.failure | Should -Not -BeNullOrEmpty;
        }

        It 'Csv' {
            $option = @{
                Path = $ruleFilePath
                OutputFormat = 'Csv'
                Name = 'FromFile1', 'FromFile3', 'WithCsv'
                WarningAction = 'SilentlyContinue'
                Culture = 'en-ZZ'
            }

            # Detail
            $result = $testObject | Invoke-PSRule @option | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $resultCsv = @($result | ConvertFrom-Csv);
            $resultCsv.Length | Should -Be 3;
            $resultCsv.RuleName | Should -BeIn 'FromFile1', 'FromFile3', 'WithCsv';
            $resultCsv[0].Outcome | Should -Be 'Pass';
            $resultCsv[1].Outcome | Should -Be 'Fail';
            $resultCsv[1].Synopsis | Should -Be 'Test rule 3';
            $resultCsv[2].RuleName | Should -Be 'WithCsv';
            $resultCsv[2].Synopsis | Should -Be 'This is "a" synopsis.';
            ($resultCsv[2].Recommendation -replace "`r`n", "`n") | Should -Be "This is an extended recommendation. - That includes line breaks - And lists";

            # Summary
            $result = $testObject | Invoke-PSRule @option -As Summary | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $resultCsv = @($result | ConvertFrom-Csv);
            $resultCsv.Length | Should -Be 3;
            $resultCsv.RuleName | Should -BeIn 'FromFile1', 'FromFile3', 'WithCsv';
            $resultCsv[0].Outcome | Should -Be 'Pass';
            $resultCsv[0].Pass | Should -Be '1';
            $resultCsv[0].Fail | Should -Be '0';
            $resultCsv[1].Outcome | Should -Be 'Fail';
            $resultCsv[1].Pass | Should -Be '0';
            $resultCsv[1].Fail | Should -Be '1';
            $resultCsv[2].RuleName | Should -Be 'WithCsv';
            $resultCsv[2].Synopsis | Should -Be 'This is "a" synopsis.';
            ($resultCsv[2].Recommendation -replace "`r`n", "`n") | Should -Be "This is an extended recommendation. - That includes line breaks - And lists";
        }

        It 'Sarif' {
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Sarif -Option @{ 'Output.SarifProblemsOnly' = $False });
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result = $result | ConvertFrom-Json;
            $result.version | Should -Be '2.1.0';
            $result.runs[0].tool.driver.name | Should -Be 'PSRule';
            $result.runs[0].results | Should -HaveCount 1;
        }
    }

    Context 'Using -OutputPath' {
        It 'Json' {
            $testInputPath = (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml');
            $testOutputPath = (Join-Path -Path $outputPath -ChildPath 'newPath/outputPath.results.json');
            $testOptions = @{
                Path = $ruleFilePath
                Name = 'WithFormat'
                InputPath = $testInputPath
                Formats = 'yaml'
                Option = (New-PSRuleOption -OutputEncoding UTF7)
            }
            $Null = Invoke-PSRule @testOptions -OutputFormat Json -OutputPath $testOutputPath;
            $result = @((Get-Content -Path $testOutputPath -Encoding utf7 -Raw -WarningAction SilentlyContinue | ConvertFrom-Json));
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'WithFormat';
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }

        It 'Sarif' {
            $testInputPath = (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml');
            $testOutputPath = (Join-Path -Path $outputPath -ChildPath 'newPath/outputPath.results.sarif');
            $testOptions = @{
                Path = $ruleFilePath
                Name = 'WithFormat'
                InputPath = $testInputPath
                Formats = 'yaml'
                Option = (New-PSRuleOption -OutputSarifProblemsOnly $False)
            }
            $Null = Invoke-PSRule @testOptions -OutputFormat Sarif -OutputPath $testOutputPath;
            $result = Get-Content -Path $testOutputPath -Encoding UTF8 -Raw -WarningAction SilentlyContinue | ConvertFrom-Json;
            $result.version | Should -Be '2.1.0';
            $result.runs[0].tool.driver.name | Should -Be 'PSRule';
            $result.runs[0].results | Should -HaveCount 2;
        }
    }

    Context 'With -OutputFormat Json and JsonIndent output option' {
        BeforeDiscovery {
            # Redefining $here since the one above is not visible in discovery phase
            $here = (Resolve-Path $PSScriptRoot).Path;

            $testCases = @(
                @{
                    Title = '0 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 0}
                    YamlPath = (Join-Path -Path $here -ChildPath 'PSRule.Tests9.yml')
                    ExpectedJson = '"outcomeReason":"Processed","ruleName":"FromFile1"'
                }
                @{
                    Title = '1 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 1}
                    YamlPath = (Join-Path -Path $here -ChildPath 'PSRule.Tests10.yml')
                    ExpectedJson = "`"outcomeReason`": `"Processed`",$([Environment]::Newline)  `"ruleName`": `"FromFile1`""
                }
                @{
                    Title = '2 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 2}
                    YamlPath = (Join-Path -Pat $here -ChildPath 'PSRule.Tests11.yml')
                    ExpectedJson = "`"outcomeReason`": `"Processed`",$([Environment]::Newline)    `"ruleName`": `"FromFile1`""
                }
                @{
                    Title = '3 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 3}
                    YamlPath = (Join-Path -Pat $here -ChildPath 'PSRule.Tests12.yml')
                    ExpectedJson = "`"outcomeReason`": `"Processed`",$([Environment]::Newline)      `"ruleName`": `"FromFile1`""
                }
                @{
                    Title = '4 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 4}
                    YamlPath = (Join-Path -Pat $here -ChildPath 'PSRule.Tests13.yml')
                    ExpectedJson = "`"outcomeReason`": `"Processed`",$([Environment]::Newline)        `"ruleName`": `"FromFile1`""
                }
            )
        }

        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
            }
        }

        Context 'Using Hashtable option' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedJson)
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $OptionHashtable;
                $result | Should -MatchExactly $ExpectedJson;
            }
        }

        Context 'Using New-PSRuleOption -Option' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedJson)
                $option = New-PSRuleOption -Option $OptionHashtable;
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $option;
                $result | Should -MatchExactly $ExpectedJson;
            }
        }

        Context 'Using New-PSRuleOption -OutputJsonIndent' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedJson)
                $option = New-PSRuleOption -OutputJsonIndent $OptionHashtable['Output.JsonIndent'];
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $option;
                $result | Should -MatchExactly $ExpectedJson;
            }
        }

        Context 'Using New-PSRuleOption with YAML config' {
            It '<title>' -TestCases $testCases {
                param($Title, $YamlPath, $ExpectedJson)
                $option = New-PSRuleOption -Option $YamlPath;
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $option;
                $result | Should -MatchExactly $ExpectedJson;
            }
        }

        Context 'Using environment variable' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedJson)
                try {
                    $env:PSRULE_OUTPUT_JSONINDENT = $OptionHashtable['Output.JsonIndent'];
                    $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json';
                    $result | Should -MatchExactly $ExpectedJson;
                }
                finally {
                    Remove-Item 'env:PSRULE_OUTPUT_JSONINDENT' -Force;
                }
            }
        }

        Context 'Normalizie range' {
            It 'Normalize to 0 when indentation is less than 0' {
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option @{'Output.JsonIndent' = -1};
                $result | Should -MatchExactly '"outcomeReason":"Processed","ruleName":"FromFile1"';
            }

            It 'Normalize to 4 when indentation is more than 4' {
                $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option @{'Output.JsonIndent' = 5};
                $result | Should -MatchExactly "`"outcomeReason`": `"Processed`",$([Environment]::Newline)        `"ruleName`": `"FromFile1`"";
            }
        }
    }

    Context 'Using -InputPath' {
        It 'Yaml' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml')
                (Join-Path -Path $here -ChildPath 'ObjectFromFile2.yaml')
            )

            # Single file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles[0] -Formats yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';

            # Multiple files, check that there are no duplicates
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles -Formats yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 3;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2', 'TestObject3';
        }

        It 'Json' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'ObjectFromFile.json')
                (Join-Path -Path $here -ChildPath 'ObjectFromFile2.json')
            )

            # Single file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles[0] -Formats json);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
            $result[0].Source[0].File | Should -Be 'some-file.json';
            $result[0].Source[0].Line | Should -Be 1;
            $result[1].Source[0].File.Split([char[]]@('\', '/'))[-1] | Should -Be 'ObjectFromFile.json';
            $result[1].Source[0].Line | Should -Be 70;

            # Multiple file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles -Formats json);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 4;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2','TestObject3', 'TestObject4';
        }

        # It 'File' {
        #     $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFileFormat' -InputPath $rootPath -Format File);
        #     $result.Length | Should -BeGreaterThan 100;
        #     $result.Length | Should -BeLessOrEqual 1000;

        #     # No ignored path
        #     $filteredResult = @($result | Where-Object { $_.Data.FullName.Replace('\', '/') -like '*/out/*' });
        #     $filteredResult.Length | Should -Be 0;

        #     # Contains nested
        #     $filteredResult = @($result | Where-Object { $_.Data.FullName.Replace('\', '/') -like '*/Assert.cs' });
        #     $filteredResult.Length | Should -Be 1;

        #     # Success only
        #     $filteredResult = @($result | Where-Object { $_.Outcome -ne 'Pass' });
        #     $filteredResult | Should -BeNullOrEmpty;

        #     # Dockerfile
        #     $filteredResult = @($result | Where-Object { $_.Data.FullName.Replace('\', '/') -like '*/Dockerfile' });
        #     $filteredResult[0].Data.TargetType | Should -Be 'Dockerfile';
        # }

        It 'Globbing processes paths' {
            # Wildcards capture both files
            $inputFiles = Join-Path -Path $here -ChildPath 'ObjectFromFile*.yaml';
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -InputPath $inputFiles -Formats yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 5;
        }

        It 'Uses source paths' {
            $inputFiles = @(
                (Join-Path -Path $rootPath -ChildPath 'ps-rule.yaml')
                (Join-Path -Path $here -ChildPath 'ObjectFromFile2.yaml')
            )
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'SourceTest' -InputPath $inputFiles -Formats yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Passing
            $filteredResult = @($result | Where-Object { $_.Outcome -eq 'Pass' });
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].Source[0].File | Should -Be $inputFiles[1];

            # Failing
            $filteredResult = @($result | Where-Object { $_.Outcome -eq 'Fail' });
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].Source[0].File | Should -Be $inputFiles[0];
        }

        It 'Returns error with bad path' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'not-a-file1.yaml')
                (Join-Path -Path $here -ChildPath 'not-a-file2.yaml')
            )

            # Single file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles[0] -ErrorVariable outErrors -ErrorAction SilentlyContinue -Option @{
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            });
            $result | Should -BeNullOrEmpty;
            $records = @($outErrors);
            $records | Should -Not -BeNullOrEmpty;
            $records.CategoryInfo.Category | Should -BeIn 'ObjectNotFound';
            $records.Length | Should -Be 1;

            # Multiple files
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles -ErrorVariable outErrors -ErrorAction SilentlyContinue -Option @{
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            });
            $result | Should -BeNullOrEmpty;
            $records = @($outErrors);
            $records | Should -Not -BeNullOrEmpty;
            $records.CategoryInfo.Category | Should -BeIn 'ObjectNotFound';
            $records.Length | Should -Be 2;
        }
    }

    Context 'Using -ObjectPath' {
        It 'Processes nested objects' {
            $yaml = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromNestedFile.yaml') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $yaml -InputStringFormat yaml -ObjectPath items);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }
    }

    Context 'Using -TargetType' {
        It 'Filters target object' {
            $testObject = [PSCustomObject]@{
                PSTypeName = 'TestType'
                Name = 'TestObject1'
                Value = 1
            }
            $invokeParams = @{
                Path = $ruleFilePath
                Name = 'FromFile1'
            }

            # Include
            $result = @(Invoke-PSRule @invokeParams -InputObject $testObject -TargetType 'TestType');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;

            # Exclude
            $result = @(Invoke-PSRule @invokeParams -InputObject $testObject -TargetType 'NotTestType');
            $result | Should -BeNullOrEmpty;

            $testObject = @(
                [PSCustomObject]@{
                    PSTypeName = 'TestType'
                    Name = 'TestObject1'
                    Value = 1
                }
                [PSCustomObject]@{
                    PSTypeName = 'NotTestType'
                    Name = 'TestObject2'
                    Value = 2
                }
            )

            # Multiple objects
            $result = @($testObject | Invoke-PSRule @invokeParams -TargetType 'TestType' -Outcome All);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestType';
            $result[0].Outcome | Should -Be 'Pass';
            $result[1].Outcome | Should -Be 'None';

            # Mutliple types
            $result = @($testObject | Invoke-PSRule @invokeParams -TargetType 'TestType','NotTestType');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestType';
            $result[0].Outcome | Should -Be 'Pass';
            $result[1].TargetName | Should -Be 'TestObject2';
            $result[1].TargetType | Should -Be 'NotTestType';
            $result[1].Outcome | Should -Be 'Pass';
        }
    }

    Context 'With constrained language' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
            }

            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }
        }

        It 'Checks if DeviceGuard is enabled' {
            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{
                'Execution.Mode' = 'ConstrainedLanguage';
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            };
            { $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }

    Context 'With downstream issues' {
        It 'Uses issues' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'ObjectFromFile.json')
            )
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'IssueGetTest' -InputPath $inputFiles -Formats json);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Pass
            $filteredResult = @($result | Where-Object { $_.Outcome -eq 'Pass' });
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult.Length | Should -Be 2;
        }

        It 'Assert with issues' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'ObjectFromFile.json')
            )
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'IssueReportTest' -InputPath $inputFiles -Formats json);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;

            # Pass
            $filteredResult = @($result | Where-Object { $_.Outcome -eq 'Pass' });
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].TargetName | Should -Be 'TestObject2';

            # Fail
            $filteredResult = @($result | Where-Object { $_.Outcome -eq 'Fail' });
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].TargetName | Should -Be 'TestObject1';
        }
    }

    Context 'Logging' {
        It 'RuleFail' {
            $testObject = [PSCustomObject]@{
                Name = 'LoggingTest'
            }

            # Warning
            $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Warning'};
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile2' -WarningVariable outWarning -WarningAction SilentlyContinue;
            $messages = @($outwarning);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Fail';
            $messages | Should -Not -BeNullOrEmpty;
            $messages | Should -Be "[FAIL] -- FromFile2:: Reported for 'LoggingTest'"

            # Error
            $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error'};
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile2' -ErrorVariable outError -ErrorAction SilentlyContinue;
            $messages = @($outError);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Fail';
            $messages | Should -Not -BeNullOrEmpty;
            $messages.Exception.Message | Should -Be "[FAIL] -- FromFile2:: Reported for 'LoggingTest'"

            # Information
            $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Information'};
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile2' -InformationVariable outInformation;
            $messages = @($outInformation);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Fail';
            $messages | Should -Not -BeNullOrEmpty;
            $messages | Should -Be "[FAIL] -- FromFile2:: Reported for 'LoggingTest'"
        }

        It 'RulePass' {
            $testObject = [PSCustomObject]@{
                Name = 'LoggingTest'
            }

            # Warning
            $option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Warning'};
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1' -WarningVariable outWarning -WarningAction SilentlyContinue;
            $messages = @($outwarning);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Pass';
            $messages | Should -Not -BeNullOrEmpty;
            $messages | Should -Be "[PASS] -- FromFile1:: Reported for 'LoggingTest'"

            # Error
            $option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Error'};
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1' -ErrorVariable outError -ErrorAction SilentlyContinue;
            $messages = @($outError);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Pass';
            $messages | Should -Not -BeNullOrEmpty;
            $messages.Exception.Message | Should -Be "[PASS] -- FromFile1:: Reported for 'LoggingTest'"

            # Information
            $option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Information'};
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1' -InformationVariable outInformation;
            $messages = @($outInformation);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Pass';
            $messages | Should -Not -BeNullOrEmpty;
            $messages | Should -Be "[PASS] -- FromFile1:: Reported for 'LoggingTest'"
        }

        It 'LimitDebug' {
            $withLoggingRulePath = (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1');
            $loggingParams = @{
                Path = $withLoggingRulePath
                Name = 'WithDebug', 'WithDebug2'
                WarningAction = 'SilentlyContinue'
                ErrorAction = 'SilentlyContinue'
                InformationAction = 'SilentlyContinue'
            }
            $option = New-PSRuleOption -LoggingLimitDebug 'WithDebug2', '[Discovery.Rule]';
            $testObject = [PSCustomObject]@{
                Name = 'LoggingTest'
            }

            $outDebug = @()
            $originalDebugPreference = $DebugPreference;

            try {
                $Global:DebugPreference = [System.Management.Automation.ActionPreference]::Continue;
                $outDebug += ($testObject | Invoke-PSRule @loggingParams -Option $option 5>&1 | Where-Object {
                    $_ -like "* debug message*"
                });
            }
            finally {
                $Global:DebugPreference = $originalDebugPreference;
            }

            # Debug
            $outDebug.Length | Should -Be 2;
            $outDebug[0] | Should -Be 'Script debug message';
            $outDebug[1] | Should -Be 'Rule debug message 2';
        }

        It 'LimitVerbose' {
            $withLoggingRulePath = (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1');
            $loggingParams = @{
                Path = $withLoggingRulePath
                Name = 'WithVerbose', 'WithVerbose2'
                WarningAction = 'SilentlyContinue'
                ErrorAction = 'SilentlyContinue'
                InformationAction = 'SilentlyContinue'
            }
            $option = New-PSRuleOption -LoggingLimitVerbose 'WithVerbose2', '[Discovery.Rule]';
            $testObject = [PSCustomObject]@{
                Name = 'LoggingTest'
            }

            $outVerbose = @($testObject | Invoke-PSRule @loggingParams -Option $option -Verbose 4>&1 | Where-Object {
                $_ -is [System.Management.Automation.VerboseRecord] -and
                $_.Message -like "* verbose message*"
            });

            # Verbose
            $outVerbose.Length | Should -Be 2;
            $outVerbose[0] | Should -Be 'Script verbose message';
            $outVerbose[1] | Should -Be 'Rule verbose message 2';
        }
    }

    Context 'Suppression output warnings' {
        BeforeAll {
            $testObject = @(
                [PSCustomObject]@{
                    Name = "TestObject1"
                }
                [PSCustomObject]@{
                    Name = "TestObject2"
                }
            )
        }

        Context 'Detail' {
            It 'Show Warnings' {
                $option = New-PSRuleOption -SuppressTargetName @{ FromFile1 = 'TestObject1'; FromFile2 = 'TestObject1'; } -ExecutionRuleSuppressed Warn -OutputAs Detail -ExecutionInvariantCulture 'Ignore';

                $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name 'FromFile1', 'FromFile2' -WarningVariable outWarnings -WarningAction SilentlyContinue;
    
                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 2;
    
                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "Rule '.\FromFile1' was suppressed for 'TestObject1'.";
                $warningMessages[1] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[1].Message | Should -BeExactly "Rule '.\FromFile2' was suppressed for 'TestObject1'.";
            }

            It 'No warnings' {
                $option = New-PSRuleOption -SuppressTargetName @{ FromFile1 = 'TestObject1'; FromFile2 = 'TestObject1'; } -ExecutionRuleSuppressed Ignore -OutputAs Detail -ExecutionInvariantCulture 'Ignore';

                $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name 'FromFile1', 'FromFile2' -WarningVariable outWarnings -WarningAction SilentlyContinue;
    
                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 0;
            }
        }

        Context 'Summary' {
            It 'Show warnings' {
                $option = New-PSRuleOption -SuppressTargetName @{ FromFile1 = 'TestObject1'; FromFile2 = 'TestObject1'; } -ExecutionRuleSuppressed Warn -OutputAs Summary -ExecutionInvariantCulture 'Ignore';

                $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name 'FromFile1', 'FromFile2' -WarningVariable outWarnings -WarningAction SilentlyContinue;
    
                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 1;
                
                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "2 rule/s were suppressed for 'TestObject1'.";
            }

            It 'No warnings' {
                $option = New-PSRuleOption -SuppressTargetName @{ FromFile1 = 'TestObject1'; FromFile2 = 'TestObject1'; } -ExecutionRuleSuppressed Ignore -OutputAs Summary -ExecutionInvariantCulture 'Ignore';

                $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name 'FromFile1', 'FromFile2' -WarningVariable outWarnings -WarningAction SilentlyContinue;
    
                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 0;
            }
        }
    }

    Context 'Suppression Group output warnings' {
        BeforeAll {
            $testObject = @(
                [PSCustomObject]@{
                    Name = "TestObject1"
                    Tags = @{
                        Env = 'dev'
                    }
                }
                [PSCustomObject]@{
                    Name = "TestObject2"
                    Tags = @{
                        Env = 'test'
                    }
                }
            )

            $testObject[0].PSObject.TypeNames.Insert(0, 'TestType');
            $testObject[1].PSObject.TypeNames.Insert(0, 'TestType');

            $suppressionGroupPath = Join-Path -Path $here -ChildPath 'SuppressionGroups.Rule.yaml';
            $suppressionGroupPath2 = Join-Path -Path $here -ChildPath 'SuppressionGroups2.Rule.yaml';

            $invokeParams = @{
                Path = $ruleFilePath, $suppressionGroupPath
            }

            $invokeParams2 = @{
                Path = $ruleFilePath, $suppressionGroupPath2
            }
        }

        Context 'Detail' {
            It 'Show warnings' {
                $option = New-PSRuleOption -ExecutionRuleSuppressed Warn -OutputAs Detail -ExecutionInvariantCulture Ignore -OutputCulture 'en-US';

                $Null = $testObject | Invoke-PSRule @invokeParams -Option $option -Name 'FromFile1', 'FromFile2', 'WithTag2' -WarningVariable outWarnings -WarningAction SilentlyContinue;

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 7;

                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "Suppression group '.\SuppressWithExpiry' has expired and will be ignored.";
                $warningMessages[1] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[1].Message | Should -BeExactly "Rule '.\FromFile1' was suppressed by suppression group '.\SuppressWithTargetName' for 'TestObject1'. Ignore test objects by name.";
                $warningMessages[2] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[2].Message | Should -BeExactly "Rule '.\FromFile2' was suppressed by suppression group '.\SuppressWithTargetName' for 'TestObject1'. Ignore test objects by name.";
                $warningMessages[3] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[3].Message | Should -BeExactly "Rule '.\WithTag2' was suppressed by suppression group '.\SuppressWithNonProdTag' for 'TestObject1'. Ignore objects with non-production tag.";
                $warningMessages[4] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[4].Message | Should -BeExactly "Rule '.\FromFile1' was suppressed by suppression group '.\SuppressWithTargetName' for 'TestObject2'. Ignore test objects by name.";
                $warningMessages[5] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[5].Message | Should -BeExactly "Rule '.\FromFile2' was suppressed by suppression group '.\SuppressWithTargetName' for 'TestObject2'. Ignore test objects by name.";
                $warningMessages[6] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[6].Message | Should -BeExactly "Rule '.\WithTag2' was suppressed by suppression group '.\SuppressWithNonProdTag' for 'TestObject2'. Ignore objects with non-production tag.";
            }

            It 'Show warnings for all rules when rule property is null or empty' {
                $option = New-PSRuleOption -ExecutionRuleSuppressed Warn -OutputAs Detail -ExecutionInvariantCulture Ignore -SuppressionGroupExpired Ignore;

                $Null = $testObject | Invoke-PSRule @invokeParams2 -Option $option -WarningVariable outWarnings -WarningAction SilentlyContinue;

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 154;

                $warningMessages | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages.Message | Should -MatchExactly "Rule '.\\[a-zA-Z0-9]+' was suppressed by suppression group '.\\SuppressWithTargetNameAnd(Null|Empty)Rule' for 'TestObject[1-2]'. Ignore test objects for all \((null|empty)\) rules."
            }

            It 'No warnings' {
                $option = New-PSRuleOption -ExecutionRuleSuppressed Ignore -OutputAs Detail -ExecutionInvariantCulture Ignore -SuppressionGroupExpired Ignore;

                $Null = $testObject | Invoke-PSRule @invokeParams -Option $option -Name 'FromFile1', 'FromFile2', 'WithTag2' -WarningVariable outWarnings -WarningAction SilentlyContinue;

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 0;
            }
        }

        Context 'Summary' {
            It 'Show warnings' {
                $option = New-PSRuleOption -ExecutionRuleSuppressed Warn -OutputAs Summary -ExecutionInvariantCulture Ignore -SuppressionGroupExpired Ignore -OutputCulture 'en-US';

                $Null = $testObject | Invoke-PSRule @invokeParams -Option $option -Name 'FromFile3', 'FromFile5', 'WithTag3' -WarningVariable outWarnings -WarningAction SilentlyContinue;

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 4;

                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "2 rule/s were suppressed by suppression group '.\SuppressWithTestType' for 'TestObject1'. Ignore test objects by type.";
                $warningMessages[1] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[1].Message | Should -BeExactly "1 rule/s were suppressed by suppression group '.\SuppressWithNonProdTag' for 'TestObject1'. Ignore objects with non-production tag.";
                $warningMessages[2] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[2].Message | Should -BeExactly "2 rule/s were suppressed by suppression group '.\SuppressWithTestType' for 'TestObject2'. Ignore test objects by type.";
                $warningMessages[3] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[3].Message | Should -BeExactly "1 rule/s were suppressed by suppression group '.\SuppressWithNonProdTag' for 'TestObject2'. Ignore objects with non-production tag.";
            }

            It 'Show warnings for all rules when rule property is null or empty' {
                $option = New-PSRuleOption -ExecutionRuleSuppressed Warn -OutputAs Summary -ExecutionInvariantCulture Ignore;

                $Null = $testObject | Invoke-PSRule @invokeParams2 -Option $option -WarningVariable outWarnings -WarningAction SilentlyContinue;

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 2;

                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "77 rule/s were suppressed by suppression group '.\SuppressWithTargetNameAndNullRule' for 'TestObject1'. Ignore test objects for all (null) rules."
                $warningMessages[1] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[1].Message | Should -BeExactly "77 rule/s were suppressed by suppression group '.\SuppressWithTargetNameAndEmptyRule' for 'TestObject2'. Ignore test objects for all (empty) rules."
            }

            It 'No warnings' {
                $option = New-PSRuleOption -ExecutionRuleSuppressed Ignore -OutputAs Summary -ExecutionInvariantCulture Ignore -SuppressionGroupExpired Ignore;

                $Null = $testObject | Invoke-PSRule @invokeParams -Option $option -Name 'FromFile3', 'FromFile5', 'WithTag3' -WarningVariable outWarnings -WarningAction SilentlyContinue;

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 0;
            }
        }
    }
}

#endregion Invoke-PSRule

#region Test-PSRuleTarget

Describe 'Test-PSRuleTarget' -Tag 'Test-PSRuleTarget','Common' {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    }

    Context 'With defaults' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
            }
        }

        It 'Returns boolean' {
            # Check passing rule
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $True;

            # Check result with one failing rule
            $option = @{ 'Execution.RuleInconclusive' = 'Ignore' };
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'FromFile1', 'FromFile2', 'FromFile3' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $False;
        }

        It 'Returns warnings on inconclusive' {
            # Check result with an inconclusive rule
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'FromFile3' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $False;
            $outWarnings | Should -BeLike "Inconclusive result reported for *";
        }

        It 'Returns warnings on no rules' {
            # Check result with no matching rules
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'NotARule' -ErrorVariable outErrors -ErrorAction SilentlyContinue;
            $result | Should -BeNullOrEmpty;
            $outErrors | Should -Be 'PSR0016: Could not find a matching rule. Please check that Path, Name, and Tag parameters are correct. See https://aka.ms/ps-rule/troubleshooting';

            # Json
            $jsonRuleFilePath = Join-Path -Path $here -ChildPath 'FromFileEmpty.Rule.jsonc'
            $result = $testObject | Invoke-PSRule -Path $jsonRuleFilePath -ErrorVariable outErrors -ErrorAction SilentlyContinue;
            $result | Should -BeNullOrEmpty;
            $outErrors | Should -Be 'PSR0016: Could not find a matching rule. Please check that Path, Name, and Tag parameters are correct. See https://aka.ms/ps-rule/troubleshooting';

            # Yaml
            $yamlRuleFilePath = Join-Path -Path $here -ChildPath 'FromFileEmpty.Rule.yaml'
            $result = $testObject | Invoke-PSRule -Path $yamlRuleFilePath -ErrorVariable outErrors -ErrorAction SilentlyContinue;
            $result | Should -BeNullOrEmpty;
            $outErrors | Should -Be 'PSR0016: Could not find a matching rule. Please check that Path, Name, and Tag parameters are correct. See https://aka.ms/ps-rule/troubleshooting';
        }

        It 'Returns warning with empty path' {
            $emptyPath = Join-Path -Path $outputPath -ChildPath 'empty';
            if (!(Test-Path -Path $emptyPath)) {
                $Null = New-Item -Path $emptyPath -ItemType Directory -Force;
            }
            $Null = $testObject | Test-PSRuleTarget -Path $emptyPath -ErrorVariable outErrors -ErrorAction SilentlyContinue;
            $errorMessages = @($outErrors);
            $errorMessages.Length | Should -Be 1;
            $errorMessages[0] | Should -BeOfType [System.Management.Automation.ErrorRecord];
            $errorMessages[0].Exception.Message | Should -Be 'PSR0015: No valid sources were found. Please check your working path and configured options. See https://aka.ms/ps-rule/troubleshooting';
        }

        It 'Returns warning when not processed' {
            # Default outcome
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'WithPreconditionFalse' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $result | Should -BeNullOrEmpty;
            $outWarnings | Should -Be 'Target object ''TestObject1'' has not been processed because no matching rules were found.';

            # Outcome All
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'WithPreconditionFalse' -WarningVariable outWarnings -WarningAction SilentlyContinue -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $True;
            $outWarnings | Should -Be 'Target object ''TestObject1'' has not been processed because no matching rules were found.';
        }
    }

    Context 'Using -TargetType' {
        It 'Filters target object' {
            $testObject = [PSCustomObject]@{
                PSTypeName = 'TestType'
                Name = 'TestObject1'
                Value = 1
            }
            $testParams = @{
                Path = $ruleFilePath
                Name = 'FromFile1'
            }

            # Include
            $result = @(Test-PSRuleTarget @testParams -InputObject $testObject -TargetType 'TestType');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;

            # Exclude
            $result = @(Test-PSRuleTarget @testParams -InputObject $testObject -TargetType 'NotTestType');
            $result | Should -BeNullOrEmpty;

            $testObject = @(
                [PSCustomObject]@{
                    PSTypeName = 'TestType'
                    Name = 'TestObject1'
                    Value = 1
                }
                [PSCustomObject]@{
                    PSTypeName = 'NotTestType'
                    Name = 'TestObject2'
                    Value = 2
                }
            )

            # Multiple objects
            $result = @($testObject | Test-PSRuleTarget @testParams -TargetType 'TestType');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0] | Should -Be $True;

            # Mutliple types
            $result = @($testObject | Test-PSRuleTarget @testParams -TargetType 'TestType','NotTestType');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0] | Should -Be $True;
            $result[1] | Should -Be $True;
        }
    }

    Context 'With constrained language' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
            }

            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }
        }

        It 'Checks if DeviceGuard is enabled' {
            $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{
                'Execution.Mode' = 'ConstrainedLanguage';
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            };
            { $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }
}

#endregion Test-PSRuleTarget

#region Get-PSRuleTarget

Describe 'Get-PSRuleTarget' -Tag 'Get-PSRuleTarget','Common' {
    Context 'With defaults' {
        It 'Yaml' {
            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $rootPath -ChildPath 'GitVersion.yml') -Formats yaml);
            $result.Length | Should -Be 1;
            $result[0].increment | Should -Be 'Inherit';

            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml') -Formats yaml);
            $result.Length | Should -Be 1;
            $result[0].input.StringFormat | Should -Be 'Yaml';
        }

        It 'Json' {
            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $here -ChildPath 'ObjectFromFileSingle.json') -Formats json);
            $result.Length | Should -Be 1;
            $result[0].TargetName | Should -Be 'TestObject1';

            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $here -ChildPath 'ObjectFromFileSingle.jsonc') -Formats json);
            $result.Length | Should -Be 1;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'None' {
            $result = @(Get-PSRuleTarget -InputPath '**/HEAD');
            $result.Length | Should -Be 0;

            # $result = @(Get-PSRuleTarget -InputPath '**/HEAD' -Option @{ 'Input.IgnoreGitPath' = $False });
            # $result.Length | Should -BeGreaterThan 0;
        }
    }

    # Context 'With -Format' {
    #     It 'File' {
    #         $result = @(Get-PSRuleTarget -InputPath $rootPath -Format File);
    #         $result.Length | Should -BeGreaterThan 100;
    #         $result.Length | Should -BeLessOrEqual 1000;

    #         # No ignored path
    #         $filteredResult = @($result | Where-Object { $_.FullName.Replace('\', '/') -like '*/out/*' });
    #         $filteredResult.Length | Should -Be 0;

    #         # Contains nested
    #         $filteredResult = @($result | Where-Object { $_.FullName.Replace('\', '/') -like '*/Assert.cs' });
    #         $filteredResult.Length | Should -Be 1;
    #     }
    # }
}

#endregion Get-PSRuleTarget

#region Assert-PSRule

Describe 'Assert-PSRule' -Tag 'Assert-PSRule','Common' {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    }

    Context 'With defaults' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
            }
        }

        It 'Returns output' {
            # Check single
            $result = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'FromFile1' -Style Plain 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match "\[PASS\] FromFile1";

            # Check multiple
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Execution.RuleInconclusive' = 'Ignore'; 'Output.Style' = 'Plain' }
                Name = 'FromFile1', 'FromFile2', 'FromFile3'
                ErrorVariable = 'errorOut'
            }
            $result = $testObject | Assert-PSRule @assertParams -ErrorAction SilentlyContinue 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match "\[PASS\] FromFile1";
            $result | Should -Match "\[FAIL\] FromFile2";
            $result | Should -Match "\[FAIL\] FromFile3";
            $errorOut | Should -Not -BeNullOrEmpty;
        }
    }

    Context 'With -OutputPath' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
            }
        }

        It 'Returns output' {
            $testOutputPath = (Join-Path -Path $outputPath -ChildPath 'newPath/assert.results.json');
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Execution.RuleInconclusive' = 'Ignore'; 'Output.Style' = 'Plain'; 'Binding.Field' = @{ extra = 'Name'} }
                Name = 'FromFile1', 'FromFile2', 'FromFile3'
                ErrorVariable = 'errorOut'
                OutputFormat = 'Json'
                OutputPath = $testOutputPath
            }
            $result = $testObject | Assert-PSRule @assertParams -ErrorAction SilentlyContinue 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match "\[PASS\] FromFile1";
            $result | Should -Match "\[FAIL\] FromFile2";
            $result | Should -Match "\[FAIL\] FromFile3";
            $errorOut | Should -Not -BeNullOrEmpty;
            Test-Path -Path $testOutputPath | Should -Be $True;
        }

        It 'With -WarningAction' {
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Execution.RuleInconclusive' = 'Ignore'; 'Output.Style' = 'Plain' }
                Name = 'WithWarning'
            }
            $result = $testObject | Assert-PSRule @assertParams 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match "\[WARN\] This is a warning";

            $result = $testObject | Assert-PSRule @assertParams -WarningAction SilentlyContinue 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Not -Match "\[WARN\] This is a warning";
        }

        It 'Writes output to file' {
            $testOutputPath = (Join-Path -Path $outputPath -ChildPath 'newPath/assert.results2.json');
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Execution.RuleInconclusive' = 'Ignore'; 'Output.Style' = 'Plain'; 'Binding.Field' = @{ extra = 'Name'} }
                Name = 'FromFile2', 'FromFile3', 'WithError', 'WithException'
                ErrorVariable = 'errorOut'
                OutputFormat = 'Json'
                OutputPath = $testOutputPath
            }
            # With ErrorAction Stop
            { $Null = $testObject | Assert-PSRule @assertParams -ErrorAction Stop 6>&1 } | Should -Throw;
            Test-Path -Path $testOutputPath | Should -Be $True;
            $resultContent = @((Get-Content -Path $testOutputPath -Raw | ConvertFrom-Json));
            $resultContent.Length | Should -Be 4;
            $resultContent.RuleName | Should -BeIn 'FromFile2', 'FromFile3', 'WithError', 'WithException';
            $resultContent.TargetName | Should -BeIn 'TestObject1';
            $resultContent.Field.extra | Should -BeIn 'TestObject1';
        }
    }

    Context 'With -Style' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
            }
        }

        It 'GitHub Actions' {
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Output.Style' = 'GitHubActions' }
                Name = 'FromFile1', 'FromFile2', 'FromFile3', 'WithWarning'
                ErrorVariable = 'errorOut'
            }
            $result = $testObject | Assert-PSRule @assertParams -ErrorAction SilentlyContinue 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match '\[PASS\] FromFile1';
            $result | Should -Match '::error::TestObject1 failed FromFile2';
            $result | Should -Match '::error::TestObject1 failed FromFile3';
            $result | Should -Match '::warning::This is a warning';
            $errorOut | Should -Not -BeNullOrEmpty;
        }

        It 'Azure Pipelines' {
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Output.Style' = 'AzurePipelines' }
                Name = 'FromFile1', 'FromFile2', 'FromFile3', 'WithWarning'
                ErrorVariable = 'errorOut'
            }
            $result = $testObject | Assert-PSRule @assertParams -ErrorAction SilentlyContinue 6>&1 | Out-String;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result | Should -Match '\[PASS\] FromFile1';
            $result | Should -Match "`#`#vso\[task\.logissue type=error\]TestObject1 failed FromFile2";
            $result | Should -Match "`#`#vso\[task\.logissue type=error\]TestObject1 failed FromFile3";
            $result | Should -Match "`#`#vso\[task\.logissue type=warning\]This is a warning";
            $errorOut | Should -Not -BeNullOrEmpty;
        }
    }

    Context 'With -ResultVariable' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
            }
        }

        It 'Returns output' {
            $assertParams = @{
                Path = $ruleFilePath
                Name = 'FromFile1', 'FromFile2', 'FromFile3'
                ErrorAction = 'SilentlyContinue'
                WarningAction = 'SilentlyContinue'
            }
            $Null = $testObject | Assert-PSRule @assertParams -ResultVariable 'recordsOut' 6>&1 | Out-String;
            $recordsOut | Should -Not -BeNullOrEmpty;
            $recordsOut | Should -BeOfType 'PSRule.Rules.RuleRecord';
            $recordsOut.Length | Should -Be 3;
            $recordsOut[0].IsSuccess() | Should -Be $True;
            $recordsOut[1].IsSuccess() | Should -Be $False;
            $recordsOut[2].IsSuccess() | Should -Be $False;
        }
    }

    Context 'With constrained language' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
                Value = 1
            }

            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }
        }

        It 'Checks if DeviceGuard is enabled' {
            $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1' -Style Plain 6>&1;
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{
                'Execution.Mode' = 'ConstrainedLanguage';
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            };
            { $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop 6>&1 } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop 6>&1 } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }
}

#endregion Assert-PSRule

#region Get-PSRule

Describe 'Get-PSRule' -Tag 'Get-PSRule','Common' {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
        $emptyOptionsFilePath = (Join-Path -Path $here -ChildPath 'PSRule.Tests8.yml');
    }

    Context 'With defaults' {
        BeforeAll {
            # Get a list of rules
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule';
        }

        It 'Returns rules in current path' {
            try {
                Push-Location -Path $searchPath;
                $result = @(Get-PSRule  -Path $PWD -Option @{ 'Execution.InvariantCulture' = 'Ignore' })
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 3;
                $result.Name | Should -BeIn 'M1.Rule1', 'M1.Rule2', 'M1.YamlTestName';
                $result | Should -BeOfType 'PSRule.Definitions.Rules.IRuleV1';
                $result[0].Extent.Line | Should -Be 9
            }
            finally {
                Pop-Location;
            }
        }
    }

    Context 'With -Path' {
        It 'Returns rules' {
            # Get a list of rules
            $result = Get-PSRule -Path $ruleFilePath;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -BeGreaterThan 0;
        }

        It 'Finds rules files' {
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule';
            $result = @(Get-PSRule -Path $searchPath);
            $result.Length | Should -Be 3;
            $result.Name | Should -BeIn 'M1.Rule1', 'M1.Rule2', 'M1.YamlTestName';
        }

        It 'Accepts .ps1 files' {
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule/rules/Test2.ps1';
            $result = @(Get-PSRule -Path $searchPath);
            $result.Length | Should -Be 2;
            $result.Name | Should -BeIn 'M1.Rule3', 'M1.Rule4';
        }

        It 'Filters by name' {
            $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1', 'FromFile3';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.Name | Should -BeIn 'FromFile1', 'FromFile3';
        }

        It 'Filters by tag' {
            $result = Get-PSRule -Path $ruleFilePath -Tag @{ Test = 'Test1' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Name | Should -Be 'FromFile1';

            # Test1 or Test2
            $result = Get-PSRule -Path $ruleFilePath -Tag @{ Test = 'Test1', 'Test2' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.Name | Should -BeIn 'FromFile1', 'FromFile2';
        }

        It 'Reads metadata' {
            [PSRule.Environment]::UseCurrentCulture('en-ZZ');
            try {
                # From markdown
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $emptyOptionsFilePath;
                $result | Should -Not -BeNullOrEmpty;
                $result.Name | Should -Be 'FromFile1';
                $result.Synopsis | Should -Be 'This is a synopsis.';
                $result.Info.Annotations.culture | Should -Be 'en-ZZ';
                Assert-VerifiableMock;

                # From comments
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile2';
                $result | Should -Not -BeNullOrEmpty;
                $result.Name | Should -Be 'FromFile2';
                $result.Synopsis | Should -Be 'Test rule 2';

                # No comments
                $result = Get-PSRule -Path $ruleFilePath -Name 'WithNoSynopsis';
                $result | Should -Not -BeNullOrEmpty;
                $result.Name | Should -Be 'WithNoSynopsis';
                $result.Synopsis | Should -BeNullOrEmpty;

                # Empty markdown
                $result = Get-PSRule -Path $ruleFilePath -Name 'WithNoSynopsis' -Culture 'en-YY';
                $result | Should -Not -BeNullOrEmpty;
                $result.Name | Should -Be 'WithNoSynopsis';
                $result.Synopsis | Should -BeNullOrEmpty;
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }
        }

        if ((Get-Variable -Name 'IsLinux' -ErrorAction SilentlyContinue -ValueOnly) -eq $True) {
            It 'Handles en-US-POSIX' {
                [PSRule.Environment]::UseCurrentCulture('en-US-POSIX');
                try {
                    # From markdown
                    $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1';
                    $result | Should -Not -BeNullOrEmpty;
                    $result.Name | Should -Be 'FromFile1';
                    $result.Synopsis | Should -Be 'This is a synopsis.';
                    $result.Info.Annotations.culture | Should -Be 'en';
                }
                finally {
                    [PSRule.Environment]::UseCurrentCulture();
                }
            }
        }

        It 'Handles empty path' {
            $emptyPath = Join-Path -Path $outputPath -ChildPath 'empty';
            if (!(Test-Path -Path $emptyPath)) {
                $Null = New-Item -Path $emptyPath -ItemType Directory -Force;
            }
            $Null = Get-PSRule -Path $emptyPath;
        }

        It 'Uses rules with include option' {
            Push-Location -Path (Join-Path -Path $here -ChildPath 'rules/')
            try {
                $result = @(Get-PSRule -Path $PWD -Option @{ 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 4;

                $result = @(Get-PSRule -Option @{ 'Include.Path' = 'main/'; 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 1;

                $result = @(Get-PSRule -Option @{ 'Include.Path' = 'main/', 'extra/'; 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 2;

                $result = @(Get-PSRule -Option @{ 'Include.Path' = '.'; 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 4;

                $result = @(Get-PSRule -Path 'main/' -Option @{ 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 2;

                $result = @(Get-PSRule -Path 'main/' -Option @{ 'Include.Path' = @(); 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 1;

                $result = @(Get-PSRule -Path 'main/' -Option @{ 'Include.Path' = 'extra/'; 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 2;

                $result = @(Get-PSRule -Path 'main/' -Option @{ 'Include.Path' = 'extra/', '.ps-rule/'; 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 3;

                $result = @(Get-PSRule -Path 'main/' -Option @{ 'Include.Path' = 'main/'; 'Execution.InvariantCulture' = 'Ignore' })
                $result.Length | Should -Be 1;
            }
            finally {
                Pop-Location;
            }
        }
    }

    Context 'With -Module' {
        BeforeAll {
            $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule';
        }

        It 'Returns module rules' {
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-US');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;

            $filteredResult = @($result | Where-Object { $_.Name -Eq 'M1.Rule1' })
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].Synopsis | Should -Be 'Synopsis en-US.';
            $filteredResult[0].Info.Annotations.culture | Should -Be 'en-US';
            $filteredResult[0].Info.Recommendation | Should -Be 'Recommendation en-US.';

            $filteredResult = @($result | Where-Object { $_.Name -Eq 'M1.Rule2' })
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].Synopsis | Should -Be 'This is the default';
            $filteredResult[0].Info.Annotations | Should -BeNullOrEmpty;

            $filteredResult = @($result | Where-Object { $_.Name -Eq 'M1.YamlTestName' })
            $filteredResult | Should -Not -BeNullOrEmpty;
            $filteredResult[0].Synopsis | Should -Be 'This is an example YAML rule.';
            $filteredResult[0].Info.Description | Should -Be 'An additional description for the YAML test rule.';
            $filteredResult[0].Tag['type'] | Should -Be 'Yaml';
        }

        It 'Loads module with preference' {
            if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
                $Null = Remove-Module -Name TestModule;
            }

            Mock -CommandName 'LoadModule' -ModuleName 'PSRule';
            $currentLoadingPreference = Get-Variable -Name PSModuleAutoLoadingPreference -ErrorAction SilentlyContinue -ValueOnly;

            try {
                # Test negative case
                $Global:PSModuleAutoLoadingPreference = [System.Management.Automation.PSModuleAutoLoadingPreference]::None;
                $Null = Get-PSRule -Module 'TestModule';
                Assert-MockCalled -CommandName 'LoadModule' -ModuleName 'PSRule' -Times 0 -Scope 'It';

                # Test positive case
                $Global:PSModuleAutoLoadingPreference = [System.Management.Automation.PSModuleAutoLoadingPreference]::All;
                $Null = Get-PSRule -Module 'TestModule';
                Assert-MockCalled -CommandName 'LoadModule' -ModuleName 'PSRule' -Times 1 -Scope 'It';
            }
            finally {
                if ($Null -eq $currentLoadingPreference) {
                    Remove-Variable -Name PSModuleAutoLoadingPreference -Force -ErrorAction SilentlyContinue;
                }
                else {
                    $Global:PSModuleAutoLoadingPreference = $currentLoadingPreference;
                }
            }
        }

        It 'Use modules already loaded' {
            Mock -CommandName 'GetAutoloadPreference' -ModuleName 'PSRule' -MockWith {
                return [System.Management.Automation.PSModuleAutoLoadingPreference]::All;
            }
            Mock -CommandName 'LoadModule' -ModuleName 'PSRule';
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSRule -Module 'TestModule')
            Assert-MockCalled -CommandName 'LoadModule' -ModuleName 'PSRule' -Times 0 -Scope 'It';
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;
            $result.Name | Should -BeIn 'M1.Rule1', 'M1.Rule2', 'M1.YamlTestName';
        }

        It 'Handles path spaces' {
            if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
                $Null = Remove-Module -Name TestModule;
            }

            # Copy file
            $testParentPath = Join-Path -Path $outputPath -ChildPath 'Program Files\';
            $testRuleDestinationPath = Join-Path -Path $testParentPath -ChildPath 'FromFile.Rule.ps1';
            if (!(Test-Path -Path $testParentPath)) {
                $Null = New-Item -Path $testParentPath -ItemType Directory -Force;
            }
            $Null = Copy-Item -Path $ruleFilePath -Destination $testRuleDestinationPath -Force;

            $result = @(Get-PSRule -Path $testRuleDestinationPath);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -BeGreaterThan 10;

            # Copy module to test path
            $testModuleDestinationPath = Join-Path -Path $testParentPath -ChildPath 'TestModule';
            $Null = Copy-Item -Path $testModuleSourcePath -Destination $testModuleDestinationPath -Recurse -Force;

            # Test modules with spaces in paths
            $Null = Import-Module $testModuleDestinationPath -Force;
            $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-US');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;
            $result[0].Name | Should -Be 'M1.Rule1';
            $result[0].Synopsis | Should -Be 'Synopsis en-US.';
            $result[0].Info.Annotations.culture | Should -Be 'en-US';
        }

        It 'Returns module and path rules' {
            if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
                $Null = Remove-Module -Name TestModule;
            }

            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSRule -Path $testModuleSourcePath -Module 'TestModule');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 6;
            $result.Name | Should -BeIn 'M1.Rule1', 'M1.Rule2', 'M1.YamlTestName';
        }

        It 'Read from documentation' {
            if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
                $Null = Remove-Module -Name TestModule;
            }

            [PSRule.Environment]::UseCurrentCulture('en-US');
            try {
                # en-US default
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule');
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[0].Synopsis | Should -Be 'Synopsis en-US.';
                $result[0].Info.Annotations.culture | Should -Be 'en-US';

                # en-AU
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-AU');
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[0].Synopsis | Should -Be 'Synopsis en-AU.';
                $result[0].Info.Annotations.culture | Should -Be 'en-AU';
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }

            [PSRule.Environment]::UseCurrentCulture('en-AU');
            try {
                # en-AU default
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Option $emptyOptionsFilePath);
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[0].Synopsis | Should -Be 'Synopsis en-AU.';
                $result[0].Info.Annotations.culture | Should -Be 'en-AU';

                # en-ZZ using parent
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-ZZ');
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[0].Synopsis | Should -Be 'Synopsis en.';
                $result[0].Info.Annotations.culture | Should -Be 'en';
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }

            [PSRule.Environment]::UseCurrentCulture('en-ZZ');
            try {
                # en-ZZ default parent
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Option $emptyOptionsFilePath);
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[0].Synopsis | Should -Be 'Synopsis en.';
                $result[0].Info.Annotations.culture | Should -Be 'en';
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }
        }

        AfterAll {
            if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
                $Null = Remove-Module -Name TestModule;
            }
        }
    }

    Context 'With -IncludeDependencies' {
        It 'Returns rules' {
            # Get a list of rules without dependencies
            $result = @(Get-PSRule -Path $ruleFilePath -Name 'FromFile4');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].DependsOn | Should -BeIn '.\FromFile3';

            # Get a list of rules with dependencies
            $result = @(Get-PSRule -Path $ruleFilePath -Name 'FromFile4' -IncludeDependencies);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Name | Should -Be 'FromFile3';
            $result[1].Name | Should -Be 'FromFile4';
            $result[1].DependsOn | Should -BeIn '.\FromFile3';
        }
    }

    Context 'With -OutputFormat' {
        It 'Wide' {
            $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Wide;
            $result | Should -Not -BeNullOrEmpty;
            ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Definitions.Rules.IRuleV1+Wide';
        }

        It 'Yaml' {
            $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Yaml;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result -cmatch 'name: FromFile1' | Should -Be $True;
        }

        It 'Json' {
            $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Json;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.String;
            $result -cmatch '"name":"FromFile1"' | Should -Be $True;
        }
    }

    Context 'With -Culture' {
        It 'Invariant culture' {
            [PSRule.Environment]::UseCurrentCulture('');
            try {
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $emptyOptionsFilePath;
                $result.Synopsis | Should -Be 'Test rule 1';

                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Culture 'en-US';
                $result.Synopsis | Should -Be 'This is a synopsis.';

                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option @{ 'Output.Culture' = 'en-US' };
                $result.Synopsis | Should -Be 'This is a synopsis.';
            }
            finally {
                [PSRule.Environment]::UseCurrentCulture();
            }
        }
    }

    Context 'With -OutputFormat Json and JsonIndent output option' {
        BeforeDiscovery {
            # Redefining $here since the one above is not visible in discovery phase
            $here = (Resolve-Path $PSScriptRoot).Path;

            $testCases = @(
                @{
                    Title = '0 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 0}
                    YamlPath = (Join-Path -Path $here -ChildPath 'PSRule.Tests9.yml')
                    ExpectedId = '"id":"\.\\\\FromFile1",'
                    ExpectedName = ',"name":"FromFile1"'
                }
                @{
                    Title = '1 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 1}
                    YamlPath = (Join-Path -Path $here -ChildPath 'PSRule.Tests10.yml')
                    ExpectedId = "$([Environment]::Newline)  `"id`": `"\.\\\\FromFile1`",$([Environment]::Newline)"
                    ExpectedName = ",$([Environment]::Newline)  `"name`": `"FromFile1`""
                }
                @{
                    Title = '2 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 2}
                    YamlPath = (Join-Path -Pat $here -ChildPath 'PSRule.Tests11.yml')
                    ExpectedId = "$([Environment]::Newline)    `"id`": `"\.\\\\FromFile1`",$([Environment]::Newline)"
                    ExpectedName = ",$([Environment]::Newline)    `"name`": `"FromFile1`""
                }
                @{
                    Title = '3 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 3}
                    YamlPath = (Join-Path -Pat $here -ChildPath 'PSRule.Tests12.yml')
                    ExpectedId = "$([Environment]::Newline)      `"id`": `"\.\\\\FromFile1`",$([Environment]::Newline)"
                    ExpectedName = ",$([Environment]::Newline)      `"name`": `"FromFile1`""
                }
                @{
                    Title = '4 space indentation'
                    OptionHashtable = @{'Output.JsonIndent' = 4}
                    YamlPath = (Join-Path -Pat $here -ChildPath 'PSRule.Tests13.yml')
                    ExpectedId = "$([Environment]::Newline)        `"id`": `"\.\\\\FromFile1`",$([Environment]::Newline)"
                    ExpectedName = ",$([Environment]::Newline)        `"name`": `"FromFile1`""
                }
            )
        }

        Context 'Using Hashtable option' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedId, $ExpectedName)
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $OptionHashtable;
                $result | Should -MatchExactly $ExpectedId;
                $result | Should -MatchExactly $ExpectedName;
            }
        }

        Context 'Using New-PSRuleOption -Option' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedId, $ExpectedName)
                $option = New-PSRuleOption -Option $OptionHashtable;
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $option;
                $result | Should -MatchExactly $ExpectedId;
                $result | Should -MatchExactly $ExpectedName;
            }
        }

        Context 'Using New-PSRuleOption -OutputJsonIndent' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedId, $ExpectedName)
                $option = New-PSRuleOption -OutputJsonIndent $OptionHashtable['Output.JsonIndent'];
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $option;
                $result | Should -MatchExactly $ExpectedId;
                $result | Should -MatchExactly $ExpectedName;
            }
        }

        Context 'Using New-PSRuleOption with YAML config' {
            It '<title>' -TestCases $testCases {
                param($Title, $YamlPath, $ExpectedId, $ExpectedName)
                $option = New-PSRuleOption -Option $YamlPath;
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option $option;
                $result | Should -MatchExactly $ExpectedId;
                $result | Should -MatchExactly $ExpectedName;
            }
        }

        Context 'Using environment variable' {
            It '<title>' -TestCases $testCases {
                param($Title, $OptionHashtable, $ExpectedId, $ExpectedName)
                try {
                    $env:PSRULE_OUTPUT_JSONINDENT = $OptionHashtable['Output.JsonIndent'];
                    $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json';
                    $result | Should -MatchExactly $ExpectedId;
                    $result | Should -MatchExactly $ExpectedName;
                }
                finally {
                    Remove-Item 'env:PSRULE_OUTPUT_JSONINDENT' -Force;
                }
            }
        }

        Context 'Normalizie range' {
            It 'Normalize to 0 when indentation is less than 0' {
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option @{'Output.JsonIndent' = -1};
                $result | Should -MatchExactly '"id":"\.\\\\FromFile1","';
                $result | Should -MatchExactly ',"name":"FromFile1"';
            }

            It 'Normalize to 4 when indentation is more than 4' {
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat 'Json' -Option @{'Output.JsonIndent' = 5};
                $result | Should -MatchExactly "$([Environment]::Newline)        `"id`": `"\.\\\\FromFile1`",";
                $result | Should -MatchExactly "$([Environment]::Newline)        `"name`": `"FromFile1`",";
            }
        }
    }

    # Context 'Get rule with invalid path' {
    #     # TODO: Test with invalid path
    #     $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath invalid);
    # }

    Context 'With constrained language' {
        BeforeAll {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }
        }
        It 'Checks if DeviceGuard is enabled' {
            $Null = Get-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            { $Null = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'UnconstrainedFile.Rule.ps1') -Name 'UnconstrainedFile1' -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }
}

#endregion Get-PSRule

#region Get-PSRuleHelp

Describe 'Get-PSRuleHelp' -Tag 'Get-PSRuleHelp', 'Common' {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
        Remove-Module TestModule*;
        $Null = Import-Module (Join-Path $here -ChildPath 'TestModule') -Force;
    }

    Context 'With defaults' {
        BeforeAll {
            # Get a list of rules
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule';
            $options = @{ 'Execution.InvariantCulture' = 'Ignore' }
        }

        It 'Docs from imported module' {
            try {
                Push-Location $searchPath;
                $result = @(Get-PSRuleHelp -Path $PWD -Option $options -WarningVariable outWarnings -WarningAction SilentlyContinue);
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[1].Name | Should -Be 'M1.Rule2';
                $result[2].Name | Should -Be 'M1.YamlTestName';
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo+Collection';

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 3;

                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "A rule with the same name 'M1.Rule1' already exists.";
                $warningMessages[1] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[1].Message | Should -BeExactly "A rule with the same name 'M1.Rule2' already exists.";
                $warningMessages[2] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[2].Message | Should -BeExactly "A rule with the same name 'M1.YamlTestName' already exists.";
            }
            finally {
                Pop-Location;
            }
        }

        It 'Using wildcard in name' {
            try {
                Push-Location $searchPath;
                $result = @(Get-PSRuleHelp -Path $PWD -Name M1.* -Option $options -WarningVariable outWarnings -WarningAction SilentlyContinue);
                $result.Length | Should -Be 3;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[1].Name | Should -Be 'M1.Rule2';
                $result[2].Name | Should -Be 'M1.YamlTestName';
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo+Collection';

                $warningMessages = $outwarnings.ToArray();
                $warningMessages.Length | Should -Be 3;

                $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[0].Message | Should -BeExactly "A rule with the same name 'M1.Rule1' already exists.";
                $warningMessages[1] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[1].Message | Should -BeExactly "A rule with the same name 'M1.Rule2' already exists.";
                $warningMessages[2] | Should -BeOfType [System.Management.Automation.WarningRecord];
                $warningMessages[2].Message | Should -BeExactly "A rule with the same name 'M1.YamlTestName' already exists.";
            }
            finally {
                Pop-Location;
            }
        }

        It 'Exception is thrown for duplicate ID' {
            Remove-Module TestModule
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule8';
            $Null = Import-Module $searchPath -Force;

            try {
                Push-Location $searchPath;
                { Get-PSRuleHelp -Path $PWD } | Should -Throw "The resource '.\M1.Rule2' is using a duplicate resource identifier. A resource with the identifier '.\M1.Rule2' already exists. Each resource must have a unique name, ref, and aliases. See https://aka.ms/ps-rule/naming for guidance on naming within PSRule.";
                Get-PSRuleHelp -Path $PWD -Option @{ 'Execution.DuplicateResourceId' = 'Warn'; 'Execution.InvariantCulture' = 'Ignore' } -WarningVariable outWarn -WarningAction SilentlyContinue;
                $warnings = @($outWarn);
                $warnings.Count | Should -Be 1;
                $warnings | Should -Be "The resource '.\M1.Rule2' is using a duplicate resource identifier. A resource with the identifier '.\M1.Rule2' already exists. Each resource must have a unique name, ref, and aliases. See https://aka.ms/ps-rule/naming for guidance on naming within PSRule.";
            }
            finally {
                Pop-Location;
                Remove-Module TestModule8
                $Null = Import-Module (Join-Path $here -ChildPath 'TestModule') -Force;
            }
        }

        It 'With -Full' {
            try {
                Push-Location $searchPath;
                $getParams = @{
                    Module = 'TestModule'
                    Name = 'M1.Rule1'
                }
                $result = @(Get-PSRuleHelp @getParams -Full);
                $result | Should -Not -BeNullOrEmpty;
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo+Full';
            }
            finally {
                Pop-Location;
            }
        }
    }

    Context 'With -Path' {
        It 'Docs from loose files' {
            $result = @(Get-PSRuleHelp -Name 'FromFile1' -Culture 'en-ZZ' -Path $ruleFilePath -WarningAction SilentlyContinue);
            $result.Length | Should -Be 1;
            $result[0].Name | Should -Be 'FromFile1';
            $result[0].DisplayName | Should -Be 'Is FromFile1'
            $result[0].ModuleName | Should -BeNullOrEmpty;
            $result[0].Synopsis | Should -Be 'This is a synopsis.';
            $result[0].Description | Should -Be 'This is a description.';
            $result[0].Recommendation | Should -Be 'This is a recommendation.';
            $result[0].Notes | Should -Be 'These are notes.';
            $result[0].Annotations.culture | Should -Be 'en-ZZ';
            ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo';
        }
    }

    Context 'With -Module' {
        It 'Docs from module' {
            $result = @(Get-PSRuleHelp -Module 'TestModule' -Culture 'en-US');
            $result.Length | Should -Be 3;
            $result[0].Name | Should -Be 'M1.Rule1';
            $result[0].DisplayName | Should -Be 'Module Rule1';
            $result[0].ModuleName | Should -Be 'TestModule';
            $result[0].Synopsis | Should -Be 'Synopsis en-US.'
            $result[0].Recommendation | Should -Be 'Recommendation en-US.'
            $result[1].Name | Should -Be 'M1.Rule2';
            $result[1].DisplayName | Should -Be 'M1.Rule2';
            $result[1].ModuleName | Should -Be 'TestModule';
            $result[2].Name | Should -Be 'M1.YamlTestName';
            $result[2].DisplayName | Should -Be 'Yaml Test Rule 1';
            $result[2].ModuleName | Should -Be 'TestModule';
            $result[2].Synopsis | Should -Be 'This is an example YAML rule.'
            $result[2].Recommendation | Should -Be 'Use YAML rules they are great.'
        }
    }

    Context 'With -Online' {
        It 'Launches browser with single result' {
            $getParams = @{
                Module = 'TestModule'
                Name = 'M1.Rule1'
                Option = @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' }
                Culture = 'en'
            }
            $result = @(Get-PSRuleHelp @getParams -Online);
            $result.Length | Should -Be 1;
            $result[0] | Should -BeLike 'Please open your browser to the following location: *';
        }

        It 'Returns collection' {
            $result = @(Get-PSRuleHelp -Module 'TestModule' -Online);
            $result.Length | Should -Be 3;
        }
    }

    Context 'With constrained language' {
        BeforeAll {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }
        }

        It 'Checks if DeviceGuard is enabled' {
            $Null = Get-PSRuleHelp -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            { $Null = Get-PSRuleHelp -Path (Join-Path -Path $here -ChildPath 'UnconstrainedFile.Rule.ps1') -Name 'UnconstrainedFile1' -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }

    Context 'With current working directory' {
        BeforeAll {
            Remove-Module TestModule*
            $searchPath = Join-Path -Path $here -ChildPath rules/
        }

        It 'Rules returned if -Path is not specified but child .ps-rule/ directory exists' {
            try {
                Push-Location $searchPath;
                $result = @(Get-PSRuleHelp);
                $result.Length | Should -Be 1;
                $result[0].Name | Should -BeExactly 'Local.Common.1';
            }
            finally {
                Pop-Location;
            }
        }

        It 'No rules returned if -Path is not specified and no .ps-rule/ directory exists' {
            try {
                Push-Location (Join-Path -Path $searchPath -ChildPath main/);
                $result = @(Get-PSRuleHelp);
                $result.Length | Should -Be 0;
            }
            finally {
                Pop-Location;
            }
        }

        It 'Rules returned if -Path $PWD is specified and no child .ps-rule/ directory exists' {
            try {
                Push-Location (Join-Path -Path $searchPath -ChildPath main/);
                $result = @(Get-PSRuleHelp -Path $PWD);
                $result.Length | Should -Be 1;
                $result[0].Name | Should -BeExactly 'Local.Main.1';
            }
            finally {
                Pop-Location;
            }
        }

        It 'Rules returned if -Path $PWD is specified and child ps-rule/ directory exists' {
            try {
                Push-Location $searchPath
                $result = @(Get-PSRuleHelp -Path $PWD);
                $result.Length | Should -Be 4;
                $result.Name | Should -BeIn 'Local.Common.1', 'Local.Test.1', 'Local.Extra.1', 'Local.Main.1';
            }
            finally {
                Pop-Location;
            }
        }
    }
}

#endregion Get-PSRuleHelp

#region Rules

Describe 'Rules' -Tag 'Common', 'Rules' {
    BeforeAll {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }
        $testParams = @{
            ErrorVariable = 'outError'
            ErrorAction = 'SilentlyContinue'
            WarningAction = 'SilentlyContinue'
        }
    }

    Context 'Parsing' {
        BeforeAll {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileParseError.Rule.ps1';
            $Null = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithNestedRule -Option @{
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            };
            $messages = @($outError);
        }

        It 'Error on nested rules' {
            $filteredResult = @($messages | Where-Object { $_.Exception -is [PSRule.Pipeline.ParseException] -and $_.Exception.ErrorId -eq 'PSRule.Parse.InvalidRuleNesting' });
            $filteredResult.Length | Should -Be 1;
            $filteredResult[0].Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike 'Rule nesting was detected for rule at *';
        }

        It 'Error on missing parameter' {
            $filteredResult = @($messages | Where-Object { $_.Exception -is [PSRule.Pipeline.ParseException] -and $_.Exception.ErrorId -eq 'PSRule.Parse.RuleParameterNotFound' });
            $filteredResult.Length | Should -Be 2;
            $filteredResult.Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike 'Could not find required rule definition parameter ''Name'' on rule at * line *';
            $filteredResult[1].Exception.Message | Should -BeLike 'Could not find required rule definition parameter ''Body'' on rule at * line *';
        }

        It 'Error on invalid ErrorAction' {
            $filteredResult = @($messages | Where-Object { $_.Exception -is [PSRule.Pipeline.ParseException] -and $_.Exception.ErrorId -eq 'PSRule.Parse.InvalidErrorAction' });
            $filteredResult.Length | Should -Be 1;
            $filteredResult.Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike 'An invalid ErrorAction (*) was specified for rule at *';
        }

        It 'Error on invalid name' {
            $filteredResult = @($messages | Where-Object { $_.Exception -is [PSRule.Pipeline.ParseException] -and $_.Exception.ErrorId -eq 'PSRule.Parse.InvalidResourceName' });
            $filteredResult.Length | Should -Be 1;
            $filteredResult.Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike "The resource name '' is not valid at * line 16. Each resource name must be between 3-128 characters in length, must start and end with a letter or number, and only contain letters, numbers, hyphens, dots, or underscores. See https://aka.ms/ps-rule/naming for more information.";
        }
    }

    Context 'Conditions' {
        It 'Error on non-boolean results' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $result = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithNonBoolean;
            $messages = @($outError);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.Outcome | Should -Be 'Error';
            $messages.Length | Should -BeGreaterThan 0;
            $messages.Exception | Should -BeOfType PSRule.Pipeline.RuleException;
            $messages.Exception.Message | Should -BeLike 'An invalid rule result was returned for *';
        }

        It 'Error with default ErrorAction' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $result = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithRuleErrorActionDefault;
            $errorsOut = @($outError);
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Error';

            # Errors
            $errorsOut.Length | Should -Be 1;
            $errorsOut[0] | Should -BeLike '*Some error 1*';
            $errorsOut[0].FullyQualifiedErrorId | Should -BeLike '*,WithRuleErrorActionDefault,Invoke-PSRule';
        }

        It 'Ignore handled exception' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $result = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithTryCatch;
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Fail';
        }

        It 'Ignore cmdlet suppressed error' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $result = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithCmdletErrorActionIgnore;
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Fail';
        }

        It 'Ignore rule suppressed error' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $result = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithRuleErrorActionIgnore;
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Pass';
        }

        It 'Rules are processed on error' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $result = @($testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name  WithParameterNotFound,WithRuleErrorActionDefault,WithRuleErrorActionIgnore,WithCmdletErrorActionIgnore,WithThrow);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 5;
        }
    }

    Context 'Dependencies' {
        It 'Error on circular dependency' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $messages = @({ $Null = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithDependency1; $outError; } | Should -Throw -PassThru);
            $messages.Length | Should -BeGreaterThan 0;
            $messages.Exception | Should -BeOfType PSRule.Pipeline.RuleException;
            $messages.Exception.Message | Should -BeLike 'A circular rule dependency was detected.*';
        }

        It 'Error on $null DependsOn' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileInvalid.Rule.ps1';
            $Null = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name InvalidRule1, InvalidRule2  -Option @{
                'Execution.NoMatchingRules' = 'Ignore';
                'Execution.NoValidInput' = 'Ignore';
                'Execution.NoValidSources' = 'Ignore';
            }
            $messages = @($outError);
            $messages.Exception | Should -BeOfType System.Management.Automation.ParameterBindingException;
            $messages.Exception.Message | Should -BeLike '*The argument is null*';
            $messages.Length | Should -Be 2;
        }

        It 'Error on missing dependency' {
            $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileWithError.Rule.ps1';
            $messages = @({ $Null = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithDependency4; $outError; } | Should -Throw -PassThru);
            $messages.Length | Should -BeGreaterThan 0;
            $messages.Exception | Should -BeOfType PSRule.Pipeline.RuleException;
            $messages.Exception.Message | Should -BeLike 'The dependency * for * could not be found.*';
        }
    }
}

#endregion Rules

#region Binding

Describe 'Binding' -Tag Common, Binding {
    BeforeAll {
        $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    }

    Context 'TargetName binding' {
        It 'Binds to TargetName' {
            $testObject = [PSCustomObject]@{
                TargetName = 'ObjectTargetName'
                Name = 'ObjectName'
                Value = 1
            }

            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'ObjectTargetName';
        }

        It 'Binds to Name' {
            $testObject = @(
                [PSCustomObject]@{ Name = 'TestObject1' }
                (1 | Select-Object -Property Name)
            )

            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].IsSuccess() | Should -Be $True;
            $result[1].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'TestObject1';
        }

        It 'Binds to object hash' {
            $testObject = @(
                [PSCustomObject]@{ NotName = 'TestObject1' }
                [PSCustomObject]@{ NotName = 'TestObject2' }
                (1 | Select-Object -Property Name)
                'a'
                'b'
                1
                2
            )

            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 7;
            $result[0..6].IsSuccess() | Should -BeIn $True;
            $result[0].TargetName | Should -BeIn 'f3d2f8ce966af96a8d320e8f5c088604324885a0d02f44b174';
            $result[1].TargetName | Should -BeIn '839b3457fca709821c89e23263a070fdca7cb8c4a86b5862f9';
            $result[2].TargetName | Should -BeIn '1c23e67aab1f653e2ead0b0e71153d02eb249f1c8382821598';
            $result[3].TargetName | Should -BeIn '72dd48c5f3cef36c66f5633955719b5cdb5f679539ec39b087';
            $result[4].TargetName | Should -BeIn '35f8cb2a8d4c26a7d53839be143c5d5b82e1543ce27adb94d4';
            $result[5].TargetName | Should -BeIn '4dff4ea340f0a823f15d3f4f01ab62eae0e5da579ccb851f8d';
            $result[6].TargetName | Should -BeIn '40b244112641dd78dd4f93b6c9190dd46e0099194d5a44257b';
        }

        It 'Binds to custom name' {
            $testObject = @(
                [PSCustomObject]@{
                    resourceName = 'ResourceName'
                    AlternateName = 'AlternateName'
                    TargetName = 'TargetName'
                }
                [PSCustomObject]@{
                    AlternateName = 'AlternateName'
                    TargetName = 'TargetName'
                }
                [PSCustomObject]@{
                    TargetName = 'TargetName'
                }
                [PSCustomObject]@{
                    Metadata = @{
                        Name = 'MetadataName'
                    }
                }
            )

            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName', 'Metadata.Name'; 'Binding.IgnoreCase' = $True };
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result[0].TargetName | Should -Be 'ResourceName';
            $result[1].TargetName | Should -Be 'AlternateName';
            $result[2].TargetName | Should -Be 'TargetName';
            $result[3].TargetName | Should -Be 'MetadataName';

            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName'; 'Binding.IgnoreCase' = $False };
            $result = $testObject[0..1] | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result[0].TargetName | Should -Be 'AlternateName';
            $result[1].TargetName | Should -Be 'AlternateName';
        }

        It 'Binds with qualified name' {
            $testObject = @(
                [PSCustomObject]@{ PSTypeName = 'UnitTest'; Name = 'TestObject1' }
            )
            $options = @{
                'Binding.UseQualifiedName' = $True
            }

            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $options);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'UnitTest/TestObject1';

            $options['Binding.NameSeparator'] = '::';
            $result = @($testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $options);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].IsSuccess() | Should -Be $True;
            $result[0].TargetName | Should -Be 'UnitTest::TestObject1';
        }
    }

    Context 'TargetType binding' {
        BeforeAll {
            $testObject = [PSCustomObject]@{
                ResourceType = 'ResourceType'
                kind = 'kind'
                OtherType = 'OtherType'
            }
            $testObject.PSObject.TypeNames.Insert(0, 'TestType');
        }

        It 'Uses default TypeName' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'TestType';
        }

        It 'Binds to custom type property by order' {
            $option = @{ 'Binding.TargetType' = 'ResourceType' };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'ResourceType';

            $option = @{ 'Binding.TargetType' = 'NotType', 'ResourceType' };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'ResourceType';

            $option = @{ 'Binding.TargetType' = 'ResourceType', 'kind' };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'ResourceType';

            $option = @{ 'Binding.TargetType' = 'kind', 'ResourceType' };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'kind';

            $option = @{ 'Binding.TargetType' = 'NotType' };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'TestType';
        }
    }
}

#endregion Binding
