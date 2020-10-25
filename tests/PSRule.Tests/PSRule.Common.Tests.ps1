# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for core PSRule cmdlets
#

[CmdletBinding()]
param (

)

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

#region Invoke-PSRule

Describe 'Invoke-PSRule' -Tag 'Invoke-PSRule','Common' {
    $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1';
    $emptyOptionsFilePath = Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml';

    Context 'With defaults' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }
        $testObject.PSObject.TypeNames.Insert(0, 'TestType');

        It 'Returns passed' {
            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-ZZ');
            try {
                $result = $testObject | Invoke-PSRule -Option $emptyOptionsFilePath -Path $ruleFilePath -Name 'FromFile1';
                $result | Should -Not -BeNullOrEmpty;
                $result.IsSuccess() | Should -Be $True;
                $result.TargetName | Should -Be 'TestObject1';
                $result.Info.Annotations.culture | Should -Be 'en-ZZ';
                $result.Recommendation | Should -Be 'This is a recommendation.';
                Assert-VerifiableMock;
            }
            finally {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
            }
        }

        It 'Returns failure' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'FromFile2' -ErrorAction Stop;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.TargetName | Should -Be 'TestObject1';

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
            $option = @{ 'Execution.InconclusiveWarning' = $False };
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
            $outErrors | Should -Be 'You cannot call a method on a null-valued expression.';
        }

        It 'Processes rule tags' {
            # Ensure that rules can be selected by tag and that tags are mapped back to the rule results
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ feature = 'tag' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
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
        }

        It 'Processes configuration' {
            $option = New-PSRuleOption -BaselineConfiguration @{ Value1 = 1 };
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Option $option -Name WithConfiguration;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
        }
    }

    Context 'Using -Path' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }

        It 'Returns error with bad path' {
            { $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'NotAFile.ps1') } | Should -Throw -ExceptionType System.IO.FileNotFoundException;
        }

        It 'Returns warning with empty path' {
            $emptyPath = Join-Path -Path $outputPath -ChildPath 'empty';
            if (!(Test-Path -Path $emptyPath)) {
                $Null = New-Item -Path $emptyPath -ItemType Directory -Force;
            }
            $Null = $testObject | Invoke-PSRule -Path $emptyPath -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $warningMessages = $outwarnings.ToArray();
            $warningMessages.Length | Should -Be 1;
            $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
            $warningMessages[0].Message | Should -Be 'No matching .Rule.ps1 files were found. Rule definitions should be saved into script files with the .Rule.ps1 extension.';
        }
    }

    Context 'Using -As' {
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

        It 'Returns detail' {
            $option = Join-Path -Path $here -ChildPath 'PSRule.Tests5.yml';
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Tag @{ category = 'group1' } -As Detail -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
        }

        It 'Returns summary' {
            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-ZZ');
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
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
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

    Context 'Using -Format' {
        It 'Yaml String' {
            $yaml = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $yaml -Format Yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }

        It 'Yaml FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -Format Yaml);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }

        It 'Json String' {
            $json = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.json') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $json -Format Json);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }

        It 'Json FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.json') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -Format Json);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }

        It 'Markdown String' {
            $markdown = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.md') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $markdown -Format Markdown);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -Be $True;
        }

        It 'Markdown FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.md') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -Format Markdown);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -Be $True;
        }

        It 'PowerShellData String' {
            $data = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.psd1') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $data -Format PowerShellData);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -Be $True;
        }

        It 'PowerShellData FileInfo' {
            $file = Get-ChildItem -Path (Join-Path -Path $here -ChildPath 'ObjectFromFile.psd1') -File;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $file -Format PowerShellData);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1';
            $result.IsSuccess() | Should -Be $True;
        }
    }

    Context 'Using -OutputFormat' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }
        $testObject.PSObject.TypeNames.Insert(0, 'TestType');

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
            ($resultCsv[2].Recommendation -replace "`r`n", "`n") | Should -Be "This is an extended recommendation.`n`n- That includes line breaks`n- And lists";

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
            ($resultCsv[2].Recommendation -replace "`r`n", "`n") | Should -Be "This is an extended recommendation.`n`n- That includes line breaks`n- And lists";
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
                Format = 'Yaml'
                Option = (New-PSRuleOption -OutputEncoding UTF7)
            }
            $Null = Invoke-PSRule @testOptions -OutputFormat Json -OutputPath $testOutputPath;
            $result = @((Get-Content -Path $testOutputPath -Encoding utf7 -Raw | ConvertFrom-Json));
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'WithFormat';
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';
        }
    }

    Context 'Using -InputPath' {
        It 'Yaml' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'ObjectFromFile.yaml')
                (Join-Path -Path $here -ChildPath 'ObjectFromFile2.yaml')
            )

            # Single file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles[0]);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';

            # Multiple files, check that there are no duplicates
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles);
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
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles[0]);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2';

            # Multiple file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.Length | Should -Be 4;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            $result.TargetName | Should -BeIn 'TestObject1', 'TestObject2','TestObject3', 'TestObject4';
        }

        It 'File' {
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFileFormat' -InputPath $rootPath -Format File);
            $result.Length | Should -BeGreaterThan 100;
            $result.Length | Should -BeLessOrEqual 1000;

            # No ignored path
            $filteredResult = @($result | Where-Object { $_.Data.FullName.Replace('\', '/') -like '*/out/*' });
            $filteredResult.Length | Should -Be 0;

            # Contains nested
            $filteredResult = @($result | Where-Object { $_.Data.FullName.Replace('\', '/') -like '*/Assert.cs' });
            $filteredResult.Length | Should -Be 1;

            # Success only
            $filteredResult = @($result | Where-Object { $_.Outcome -ne 'Pass' });
            $filteredResult | Should -BeNullOrEmpty;
        }

        It 'Globbing processes paths' {
            # Wildcards capture both files
            $inputFiles = Join-Path -Path $here -ChildPath 'ObjectFromFile*.yaml';
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'FromFile1' -InputPath $inputFiles);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 3;
        }

        It 'Returns error with bad path' {
            $inputFiles = @(
                (Join-Path -Path $here -ChildPath 'not-a-file1.yaml')
                (Join-Path -Path $here -ChildPath 'not-a-file2.yaml')
            )

            # Single file
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles[0] -ErrorVariable outErrors -ErrorAction SilentlyContinue);
            $result | Should -BeNullOrEmpty;
            $records = @($outErrors);
            $records | Should -Not -BeNullOrEmpty;
            $records.Length | Should -Be 1;
            $records.CategoryInfo.Category | Should -BeIn 'ObjectNotFound';

            # Multiple files
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputPath $inputFiles -ErrorVariable outErrors -ErrorAction SilentlyContinue);
            $result | Should -BeNullOrEmpty;
            $records = @($outErrors);
            $records | Should -Not -BeNullOrEmpty;
            $records.Length | Should -Be 2;
            $records.CategoryInfo.Category | Should -BeIn 'ObjectNotFound';
        }
    }

    Context 'Using -ObjectPath' {
        It 'Processes nested objects' {
            $yaml = Get-Content -Path (Join-Path -Path $here -ChildPath 'ObjectFromNestedFile.yaml') -Raw;
            $result = @(Invoke-PSRule -Path $ruleFilePath -Name 'WithFormat' -InputObject $yaml -Format Yaml -ObjectPath items);
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
            $result.Length | Should -Be 1;
            $result[0].TargetName | Should -Be 'TestObject1';
            $result[0].TargetType | Should -Be 'TestType';
            $result[0].Outcome | Should -Be 'Pass';

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
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }

        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }

            $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{ 'execution.mode' = 'ConstrainedLanguage' };
            { $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';

            $bindFn = {
                param ($TargetObject)
                $Null = [Console]::WriteLine('Should fail');
                return 'BadName';
            }

            $option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -BindTargetName $bindFn;
            { $Null = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1' -Option $option -ErrorAction Stop } | Should -Throw 'Binding functions are not supported in this language mode.';
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
}

#endregion Invoke-PSRule

#region Test-PSRuleTarget

Describe 'Test-PSRuleTarget' -Tag 'Test-PSRuleTarget','Common' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'With defaults' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
        }

        It 'Returns boolean' {
            # Check passing rule
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $True;

            # Check result with one failing rule
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'FromFile1', 'FromFile2', 'FromFile3' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $False;
        }

        It 'Returns warnings on inconclusive' {
            # Check result with an inconculsive rule
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'FromFile3' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $False;
            $outWarnings | Should -BeLike "Inconclusive result reported for *";
        }

        It 'Returns warnings on no rules' {
            # Check result with no matching rules
            $result = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'NotARule' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $result | Should -BeNullOrEmpty;
            $outWarnings | Should -Be 'Could not find a matching rule. Please check that Path, Name and Tag parameters are correct.';
        }

        It 'Returns warning with empty path' {
            $emptyPath = Join-Path -Path $outputPath -ChildPath 'empty';
            if (!(Test-Path -Path $emptyPath)) {
                $Null = New-Item -Path $emptyPath -ItemType Directory -Force;
            }
            $Null = $testObject | Test-PSRuleTarget -Path $emptyPath -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $warningMessages = $outwarnings.ToArray();
            $warningMessages.Length | Should -Be 1;
            $warningMessages[0] | Should -BeOfType [System.Management.Automation.WarningRecord];
            $warningMessages[0].Message | Should -Be 'No matching .Rule.ps1 files were found. Rule definitions should be saved into script files with the .Rule.ps1 extension.';
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
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }

        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }

            $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{ 'execution.mode' = 'ConstrainedLanguage' };
            { $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';

            $bindFn = {
                param ($TargetObject)
                $Null = [Console]::WriteLine('Should fail');
                return 'BadName';
            }

            $option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -BindTargetName $bindFn;
            { $Null = $testObject | Test-PSRuleTarget -Path $ruleFilePath -Name 'ConstrainedTest1' -Option $option -ErrorAction Stop } | Should -Throw 'Binding functions are not supported in this language mode.';
        }
    }
}

#endregion Test-PSRuleTarget

#region Get-PSRuleTarget

Describe 'Get-PSRuleTarget' -Tag 'Get-PSRuleTarget','Common' {
    Context 'With defaults' {
        It 'Yaml' {
            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $rootPath -ChildPath 'ps-project.yaml'));
            $result.Length | Should -Be 1;
            $result[0].info.name | Should -Be 'PSRule';

            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml'));
            $result.Length | Should -Be 1;
            $result[0].input.format | Should -Be 'Yaml';
        }

        It 'Json' {
            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $here -ChildPath 'ObjectFromFileSingle.json'));
            $result.Length | Should -Be 1;
            $result[0].TargetName | Should -Be 'TestObject1';

            $result = @(Get-PSRuleTarget -InputPath (Join-Path -Path $here -ChildPath 'ObjectFromFileSingle.jsonc'));
            $result.Length | Should -Be 1;
            $result[0].TargetName | Should -Be 'TestObject1';
        }
    }

    Context 'With -Format' {
        It 'File' {
            $result = @(Get-PSRuleTarget -InputPath $rootPath -Format File);
            $result.Length | Should -BeGreaterThan 100;
            $result.Length | Should -BeLessOrEqual 1000;

            # No ignored path
            $filteredResult = @($result | Where-Object { $_.FullName.Replace('\', '/') -like '*/out/*' });
            $filteredResult.Length | Should -Be 0;

            # Contains nested
            $filteredResult = @($result | Where-Object { $_.FullName.Replace('\', '/') -like '*/Assert.cs' });
            $filteredResult.Length | Should -Be 1;
        }
    }
}

#endregion Get-PSRuleTarget

#region Assert-PSRule

Describe 'Assert-PSRule' -Tag 'Assert-PSRule','Common' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'With defaults' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
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
                Option = @{ 'Execution.InconclusiveWarning' = $False; 'Output.Style' = 'Plain' }
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
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
        }

        It 'Returns output' {
            $testOutputPath = (Join-Path -Path $outputPath -ChildPath 'newPath/assert.results.json');
            $assertParams = @{
                Path = $ruleFilePath
                Option = @{ 'Execution.InconclusiveWarning' = $False; 'Output.Style' = 'Plain'; 'Binding.Field' = @{ extra = 'Name'} }
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
                Option = @{ 'Execution.InconclusiveWarning' = $False; 'Output.Style' = 'Plain' }
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
                Option = @{ 'Execution.InconclusiveWarning' = $False; 'Output.Style' = 'Plain'; 'Binding.Field' = @{ extra = 'Name'} }
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
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
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
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
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
        $testObject = [PSCustomObject]@{
            Name = 'TestObject1'
            Value = 1
        }

        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }
            $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1' -Style Plain 6>&1;
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{ 'execution.mode' = 'ConstrainedLanguage' };
            { $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop 6>&1 } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop 6>&1 } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';

            $bindFn = {
                param ($TargetObject)
                $Null = [Console]::WriteLine('Should fail');
                return 'BadName';
            }

            $option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -BindTargetName $bindFn;
            { $Null = $testObject | Assert-PSRule -Path $ruleFilePath -Name 'ConstrainedTest1' -Option $option -ErrorAction Stop 6>&1 } | Should -Throw 'Binding functions are not supported in this language mode.';
        }
    }
}

#endregion Assert-PSRule

#region Get-PSRule

Describe 'Get-PSRule' -Tag 'Get-PSRule','Common' {
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    $emptyOptionsFilePath = (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml');

    Context 'With defaults' {
        # Get a list of rules
        $searchPath = Join-Path -Path $here -ChildPath 'TestModule';

        try {
            Push-Location -Path $searchPath;
            It 'Returns rules in current path' {
                $result = @(Get-PSRule)
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 2;
                $result.RuleName | Should -BeIn 'M1.Rule1', 'M1.Rule2';
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.Rule';
            }
        }
        finally {
            Pop-Location;
        }
    }

    Context 'With -Path' {
        It 'Returns rules' {
            # Get a list of rules
            $result = Get-PSRule -Path $ruleFilePath;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -BeGreaterThan 0;
        }

        It 'Finds .Rule.ps1 files' {
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule';
            $result = @(Get-PSRule -Path $searchPath);
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'M1.Rule1', 'M1.Rule2';
        }

        It 'Accepts .ps1 files' {
            $searchPath = Join-Path -Path $here -ChildPath 'TestModule/rules/Test2.ps1';
            $result = @(Get-PSRule -Path $searchPath);
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'M1.Rule3', 'M1.Rule4';
        }

        It 'Filters by name' {
            $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1', 'FromFile3';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleName | Should -BeIn 'FromFile1', 'FromFile3';
        }

        It 'Filters by tag' {
            $result = Get-PSRule -Path $ruleFilePath -Tag @{ Test = 'Test1' };
            $result | Should -Not -BeNullOrEmpty;
            $result.RuleName | Should -Be 'FromFile1';

            # Test1 or Test2
            $result = Get-PSRule -Path $ruleFilePath -Tag @{ Test = 'Test1', 'Test2' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleName | Should -BeIn 'FromFile1', 'FromFile2';
        }

        It 'Reads metadata' {
            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-ZZ');
            try {
                # From markdown
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $emptyOptionsFilePath;
                $result | Should -Not -BeNullOrEmpty;
                $result.RuleName | Should -Be 'FromFile1';
                $result.Synopsis | Should -Be 'This is a synopsis.';
                $result.Info.Annotations.culture | Should -Be 'en-ZZ';
                Assert-VerifiableMock;

                # From comments
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile2';
                $result | Should -Not -BeNullOrEmpty;
                $result.RuleName | Should -Be 'FromFile2';
                $result.Synopsis | Should -Be 'Test rule 2';

                # No comments
                $result = Get-PSRule -Path $ruleFilePath -Name 'WithNoSynopsis';
                $result | Should -Not -BeNullOrEmpty;
                $result.RuleName | Should -Be 'WithNoSynopsis';
                $result.Synopsis | Should -BeNullOrEmpty;

                # Empty markdown
                $result = Get-PSRule -Path $ruleFilePath -Name 'WithNoSynopsis' -Culture 'en-YY';
                $result | Should -Not -BeNullOrEmpty;
                $result.RuleName | Should -Be 'WithNoSynopsis';
                $result.Synopsis | Should -BeNullOrEmpty;
            }
            finally {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
            }
        }

        if ((Get-Variable -Name 'IsLinux' -ErrorAction SilentlyContinue -ValueOnly) -eq $True) {
            It 'Handles en-US-POSIX' {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-US-POSIX');
                try {
                    # From markdown
                    $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1';
                    $result | Should -Not -BeNullOrEmpty;
                    $result.RuleName | Should -Be 'FromFile1';
                    $result.Synopsis | Should -Be 'This is a synopsis.';
                    $result.Info.Annotations.culture | Should -Be 'en';
                }
                finally {
                    [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
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
    }

    Context 'With -Module' {
        $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule';

        It 'Returns module rules' {
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-US');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M1.Rule1';
            $result[0].Description | Should -Be 'Synopsis en-US.';
            $result[0].Info.Annotations.culture | Should -Be 'en-US';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Loads module with preference' {
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
            $result.Length | Should -Be 2;
            $result.RuleName | Should -BeIn 'M1.Rule1', 'M1.Rule2';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Handles path spaces' {
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
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'M1.Rule1';
            $result[0].Description | Should -Be 'Synopsis en-US.';
            $result[0].Info.Annotations.culture | Should -Be 'en-US';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Returns module and path rules' {
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSRule -Path $testModuleSourcePath -Module 'TestModule');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 4;
            $result.RuleName | Should -BeIn 'M1.Rule1', 'M1.Rule2';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Read from documentation' {
            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-US');
            try {
                # en-US default
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule');
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 2;
                $result[0].RuleName | Should -Be 'M1.Rule1';
                $result[0].Description | Should -Be 'Synopsis en-US.';
                $result[0].Info.Annotations.culture | Should -Be 'en-US';

                # en-AU
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-AU');
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 2;
                $result[0].RuleName | Should -Be 'M1.Rule1';
                $result[0].Description | Should -Be 'Synopsis en-AU.';
                $result[0].Info.Annotations.culture | Should -Be 'en-AU';
            }
            finally {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
            }

            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-AU');
            try {
                # en-AU default
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Option $emptyOptionsFilePath);
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 2;
                $result[0].RuleName | Should -Be 'M1.Rule1';
                $result[0].Description | Should -Be 'Synopsis en-AU.';
                $result[0].Info.Annotations.culture | Should -Be 'en-AU';

                # en-ZZ using parent
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Culture 'en-ZZ');
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 2;
                $result[0].RuleName | Should -Be 'M1.Rule1';
                $result[0].Description | Should -Be 'Synopsis en.';
                $result[0].Info.Annotations.culture | Should -Be 'en';
            }
            finally {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
            }

            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('en-ZZ');
            try {
                # en-ZZ default parent
                $Null = Import-Module $testModuleSourcePath -Force;
                $result = @(Get-PSRule -Module 'TestModule' -Option $emptyOptionsFilePath);
                $result | Should -Not -BeNullOrEmpty;
                $result.Length | Should -Be 2;
                $result[0].RuleName | Should -Be 'M1.Rule1';
                $result[0].Description | Should -Be 'Synopsis en.';
                $result[0].Info.Annotations.culture | Should -Be 'en';
            }
            finally {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
            }
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }
    }

    Context 'With -IncludeDependencies' {
        It 'Returns rules' {
            # Get a list of rules without dependencies
            $result = @(Get-PSRule -Path $ruleFilePath -Name 'FromFile4');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 1;
            $result[0].DependsOn | Should -BeIn 'FromFile3';

            # Get a list of rules with dependencies
            $result = @(Get-PSRule -Path $ruleFilePath -Name 'FromFile4' -IncludeDependencies);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].RuleName | Should -Be 'FromFile3';
            $result[1].RuleName | Should -Be 'FromFile4';
            $result[1].DependsOn | Should -BeIn 'FromFile3';
        }
    }

    Context 'With -OutputFormat' {
        It 'Wide' {
            $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Wide;
            $result | Should -Not -BeNullOrEmpty;
            ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.Rule+Wide';
        }

        # It 'Yaml' {
        #     $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Yaml;
        #     $result | Should -Not -BeNullOrEmpty;
        #     $result | Should -BeOfType System.String;
        #     $result -cmatch 'ruleName: FromFile1' | Should -Be $True;
        # }

        # It 'Json' {
        #     $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -OutputFormat Json;
        #     $result | Should -Not -BeNullOrEmpty;
        #     $result | Should -BeOfType System.String;
        #     $result -cmatch '"ruleName":"FromFile1"' | Should -Be $True;
        # }
    }

    Context 'With -Culture' {
        It 'Invariant culture' {
            [PSRule.Configuration.PSRuleOption]::UseCurrentCulture('');
            try {
                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option $emptyOptionsFilePath;
                $result.Synopsis | Should -Be 'Test rule 1';

                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Culture 'en-US';
                $result.Synopsis | Should -Be 'This is a synopsis.';

                $result = Get-PSRule -Path $ruleFilePath -Name 'FromFile1' -Option @{ 'Output.Culture' = 'en-US' };
                $result.Synopsis | Should -Be 'This is a synopsis.';
            }
            finally {
                [PSRule.Configuration.PSRuleOption]::UseCurrentCulture();
            }
        }
    }

    # Context 'Get rule with invalid path' {
    #     # TODO: Test with invalid path
    #     $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath invalid);
    # }

    Context 'With constrained language' {
        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }

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
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
    Remove-Module TestModule*;
    $Null = Import-Module (Join-Path $here -ChildPath 'TestModule') -Force;

    Context 'With defaults' {
        # Get a list of rules
        $searchPath = Join-Path -Path $here -ChildPath 'TestModule';

        try {
            Push-Location $searchPath;
            It 'Docs from imported module' {
                $result = @(Get-PSRuleHelp);
                $result.Length | Should -Be 4;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[1].Name | Should -Be 'M1.Rule2';
                $result[2].Name | Should -Be 'M1.Rule1';
                $result[3].Name | Should -Be 'M1.Rule2';
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo+Collection';
            }

            It 'Using wildcard in name' {
                $result = @(Get-PSRuleHelp -Name M1.*);
                $result.Length | Should -Be 4;
                $result[0].Name | Should -Be 'M1.Rule1';
                $result[1].Name | Should -Be 'M1.Rule2';
                $result[2].Name | Should -Be 'M1.Rule1';
                $result[3].Name | Should -Be 'M1.Rule2';
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo+Collection';
            }

            It 'With -Full' {
                $getParams = @{
                    Module = 'TestModule'
                    Name = 'M1.Rule1'
                }
                $result = @(Get-PSRuleHelp @getParams -Full);
                $result | Should -Not -BeNullOrEmpty;
                ($result | Get-Member).TypeName | Should -BeIn 'PSRule.Rules.RuleHelpInfo+Full';
            }
        }
        finally {
            Pop-Location;
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
            $result.Length | Should -Be 2;
            $result[0].Name | Should -Be 'M1.Rule1';
            $result[0].DisplayName | Should -Be 'Module Rule1';
            $result[0].ModuleName | Should -Be 'TestModule';
            $result[0].Synopsis | Should -Be 'Synopsis en-US.'
            $result[0].Recommendation | Should -Be 'Recommendation en-US.'
            $result[1].Name | Should -Be 'M1.Rule2';
            $result[1].DisplayName | Should -Be 'M1.Rule2';
            $result[1].ModuleName | Should -Be 'TestModule';
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
            $result.Length | Should -Be 2;
        }
    }

    Context 'With constrained language' {
        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Verifiable -MockWith {
                return $True;
            }

            $Null = Get-PSRuleHelp -Path $ruleFilePath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            { $Null = Get-PSRuleHelp -Path (Join-Path -Path $here -ChildPath 'UnconstrainedFile.Rule.ps1') -Name 'UnconstrainedFile1' -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }
}

#endregion Get-PSRuleHelp

#region Rules

Describe 'Rules' -Tag 'Common', 'Rules' {
    $testObject = [PSCustomObject]@{
        Name = 'TestObject1'
        Value = 1
    }
    $testParams = @{
        ErrorVariable = 'outError'
        ErrorAction = 'SilentlyContinue'
        WarningAction = 'SilentlyContinue'
    }

    Context 'Parsing' {
        $ruleFilePath = Join-Path -Path $here -ChildPath 'FromFileParseError.Rule.ps1';
        $Null = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name WithNestedRule;
        $messages = @($outError);

        It 'Error on nested rules' {
            $filteredResult = @($messages | Where-Object { $_.Exception.ErrorId -eq 'PSRule.Parse.InvalidRuleNesting' });
            $filteredResult.Length | Should -Be 1;
            $filteredResult[0].Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike 'Rule nesting was detected for rule at *';
        }

        It 'Error on missing parameter' {
            $filteredResult = @($messages | Where-Object { $_.Exception.ErrorId -eq 'PSRule.Parse.RuleParameterNotFound' });
            $filteredResult.Length | Should -Be 3;
            $filteredResult.Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike 'Could not find required rule definition parameter ''Name'' on rule at * line *';
            $filteredResult[1].Exception.Message | Should -BeLike 'Could not find required rule definition parameter ''Name'' on rule at * line *';
            $filteredResult[2].Exception.Message | Should -BeLike 'Could not find required rule definition parameter ''Body'' on rule at * line *';
        }

        It 'Error on invalid ErrorAction' {
            $filteredResult = @($messages | Where-Object { $_.Exception.ErrorId -eq 'PSRule.Parse.InvalidErrorAction' });
            $filteredResult.Length | Should -Be 1;
            $filteredResult.Exception | Should -BeOfType PSRule.Pipeline.ParseException;
            $filteredResult[0].Exception.Message | Should -BeLike 'An invalid ErrorAction (*) was specified for rule at *';
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
            $Null = $testObject | Invoke-PSRule @testParams -Path $ruleFilePath -Name InvalidRule1, InvalidRule2;
            $messages = @($outError);
            $messages.Length | Should -Be 2;
            $messages.Exception | Should -BeOfType System.Management.Automation.ParameterBindingException;
            $messages.Exception.Message | Should -BeLike '*The argument is null*';
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
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

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
            $result[0].TargetName | Should -BeIn 'f209c623345144be61087d91f30c17b01c6e86d2';
            $result[1].TargetName | Should -BeIn '28e156a7121bc57b0461029208daf0b48d1c4fd0';
            $result[2].TargetName | Should -BeIn '3b8eeb35831ea8f7b5de4e0cf04f32b9a1233a0d';
            $result[3].TargetName | Should -BeIn '7b3ce68b6c2f7d67dae4210eeb83be69f978e2a8';
            $result[4].TargetName | Should -BeIn '205c97d9248d2cd12db1c55ba421eb8df84b22a7';
            $result[5].TargetName | Should -BeIn '356a192b7913b04c54574d18c28d46e6395428ab';
            $result[6].TargetName | Should -BeIn 'da4b9237bacccdf19c0760cab7aec4a8359010b0';
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
                    OtherName = 'OtherName'
                    resourceName = 'ResourceName'
                    AlternateName = 'AlternateName'
                    TargetName = 'TargetName'
                }
                [PSCustomObject]@{
                    Metadata = @{
                        Name = 'MetadataName'
                    }
                }
            )

            $bindFn = {
                param ($TargetObject)
                $otherName = $TargetObject.PSObject.Properties['OtherName'];
                if ($otherName -eq $Null) {
                    return $Null
                }
                return $otherName.Value;
            }

            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName', 'Metadata.Name'; 'Binding.IgnoreCase' = $True } -BindTargetName $bindFn;
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            $result[0].TargetName | Should -Be 'ResourceName';
            $result[1].TargetName | Should -Be 'AlternateName';
            $result[2].TargetName | Should -Be 'TargetName';
            $result[3].TargetName | Should -Be 'OtherName';
            $result[4].TargetName | Should -Be 'MetadataName';

            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName'; 'Binding.IgnoreCase' = $False } -BindTargetName $bindFn;
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
        $testObject = [PSCustomObject]@{
            ResourceType = 'ResourceType'
            kind = 'kind'
            OtherType = 'OtherType'
        }
        $testObject.PSObject.TypeNames.Insert(0, 'TestType');

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

        It 'Binds to custom type by script' {
            $bindFn = {
                param ($TargetObject)

                $otherType = $TargetObject.PSObject.Properties['OtherType'];

                if ($otherType -eq $Null) {
                    return $Null
                }

                return $otherType.Value;
            }

            $option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'kind' } -BindTargetType $bindFn;
            $result = $testObject | Invoke-PSRule -Option $option -Path $ruleFilePath -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.TargetType | Should -Be 'OtherType';
        }
    }
}

#endregion Binding
