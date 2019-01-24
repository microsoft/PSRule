#
# Unit tests for core PSRule functionality
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
    Context 'With defaults' {
        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
            Value = 1
        }

        It 'Returns passed' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Returns failure' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile2';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Returns inconclusive' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile3' -Outcome All -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.OutcomeReason | Should -Be 'Inconclusive';
        }

        It 'Propagates PowerShell logging' {
            # Warnings
            $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1') -Name 'WithWarning' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $warningMessages = $outWarnings.ToArray();
            $warningMessages.Length | Should -Be 2;
            $warningMessages[0] | Should -Be 'Script warning message';
            $warningMessages[1] | Should -Be 'Rule warning message';

            # Errors
            $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1') -Name 'WithError' -ErrorVariable outErrors -ErrorAction SilentlyContinue -WarningAction SilentlyContinue;
            $outErrors | Should -Be 'Rule error message';

            # Verbose
            $outVerbose = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1') -Name 'WithVerbose' -WarningAction SilentlyContinue -Verbose 4>&1 | Where-Object {
                $_ -is [System.Management.Automation.VerboseRecord] -and
                $_.Message -like "* verbose message"
            };
            $outVerbose.Length | Should -Be 2;
            $outVerbose[0] | Should -Be 'Script verbose message';
            $outVerbose[1] | Should -Be 'Rule verbose message';

            # Information
            $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFileWithLogging.Rule.ps1') -Name 'WithInformation' -InformationVariable outInformation -InformationAction Continue -WarningAction SilentlyContinue 6>&1;
            $informationMessages = $outInformation.ToArray();
            $informationMessages.Length | Should -Be 2;
            $informationMessages[0] | Should -Be 'Script information message';
            $informationMessages[1] | Should -Be 'Rule information message';
        }

        It 'Propagates PowerShell exceptions' {
            $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFileWithException.Rule.ps1') -ErrorVariable outErrors -ErrorAction SilentlyContinue -WarningAction SilentlyContinue;
            $outErrors | Should -Be 'You cannot call a method on a null-valued expression.';
        }

        It 'Returns error with bad path' {
            { $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'NotAFile.ps1') } | Should -Throw -ExceptionType System.Management.Automation.ItemNotFoundException;
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
            $warningMessages[0].Message | Should -Be 'Path not found';
        }

        It 'Processes rule tags' {
            # Ensure that rules can be selected by tag and that tags are mapped back to the rule results
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ feature = 'tag' };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            $result.Tag.feature | Should -BeIn 'tag';

            # Ensure that tag selection is and'ed together, requiring all tags to be selected
            # Tag values, will be matched without case sensitivity, but values are case sensitive
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ feature = 'tag'; severity = 'critical'; };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.Tag.feature | Should -BeIn 'tag';

            # Using a * wildcard in tag filter, matches rules with the tag regardless of value 
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ feature = 'tag'; severity = '*'; };
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.Tag.feature | Should -BeIn 'tag';
            $result.Tag.severity | Should -BeIn 'critical', 'information';
        }

        It 'Processes rule preconditions' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'precondition' } -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionTrue' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionFalse' }).Outcome | Should -Be 'None';
        }

        It 'Processes rule dependencies' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name WithDependency1 -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency5' }).Outcome | Should -Be 'Fail';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency4' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency3' }).Outcome | Should -Be 'Pass';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency2' }).Outcome | Should -Be 'None';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency1' }).Outcome | Should -Be 'None';
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
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name 'FromFile1', 'FromFile2' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
            ($result | Where-Object { $_.TargetName -eq 'TestObject1' }).OutcomeReason | Should -BeIn 'Suppressed';
            ($result | Where-Object { $_.TargetName -eq 'TestObject2' }).OutcomeReason | Should -BeIn 'Processed';
        }

        It 'Processes configuration' {
            $option = New-PSRuleOption -BaselineConfiguration @{ Value1 = 1 };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Option $option -Name WithConfiguration;
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
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
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'group1' } -As Detail -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
        }

        It 'Returns summary' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'group1' } -As Summary -Outcome All -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result | Should -BeOfType PSRule.Rules.RuleSummaryRecord;
            $result.RuleName | Should -BeIn 'FromFile1', 'FromFile2', 'FromFile3', 'FromFile4'
            $result.Tag.category | Should -BeIn 'group1';
            ($result | Where-Object { $_.RuleName -eq 'FromFile1'}).Outcome | Should -Be 'Pass';
            ($result | Where-Object { $_.RuleName -eq 'FromFile1'}).Pass | Should -Be 2;
            ($result | Where-Object { $_.RuleName -eq 'FromFile2'}).Outcome | Should -Be 'Fail';
            ($result | Where-Object { $_.RuleName -eq 'FromFile2'}).Fail | Should -Be 2;
            ($result | Where-Object { $_.RuleName -eq 'FromFile4'}).Outcome | Should -Be 'None';
            ($result | Where-Object { $_.RuleName -eq 'FromFile4'}).Pass | Should -Be 0;
            ($result | Where-Object { $_.RuleName -eq 'FromFile4'}).Fail | Should -Be 0;
        }

        It 'Returns filtered summary' {
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'group1' } -As Summary -Outcome Fail -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleSummaryRecord;
            $result.RuleName | Should -BeIn 'FromFile2', 'FromFile3'
            $result.Tag.category | Should -BeIn 'group1';
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

            $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            $option = @{ 'execution.mode' = 'ConstrainedLanguage' };
            { $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'ConstrainedTest2' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            { $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'ConstrainedTest3' -Option $option -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';

            $bindFn = {
                param ($TargetObject)
                $Null = [Console]::WriteLine('Should fail');
                return 'BadName';
            }

            $option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -BindTargetName $bindFn;
            { $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'ConstrainedTest1' -Option $option -ErrorAction Stop } | Should -Throw 'Binding functions are not supported in this language mode.';
        }
    }

    Context 'TargetName binding' {
        It 'Binds to TargetName' {
            $testObject = [PSCustomObject]@{
                TargetName = "ObjectTargetName"
                Name = "ObjectName"
                Value = 1
            }

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'ObjectTargetName';
        }

        It 'Binds to Name' {
            $testObject = [PSCustomObject]@{
                Name = 'TestObject1'
            }

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Binds to object hash' {
            $testObject = [PSCustomObject]@{
                NotName = 'TestObject1'
            }

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -BeIn $True;
            $result.TargetName | Should -BeIn 'f209c623345144be61087d91f30c17b01c6e86d2';
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
            )

            $bindFn = {
                param ($TargetObject)

                $otherName = $TargetObject.PSObject.Properties['OtherName'];

                if ($otherName -eq $Null) {
                    return $Null
                }

                return $otherName.Value;
            }

            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' } -BindTargetName $bindFn;
            $result = $testObject | Invoke-PSRule -Option $option -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result[0].TargetName | Should -Be 'ResourceName';
            $result[1].TargetName | Should -Be 'AlternateName';
            $result[2].TargetName | Should -Be 'TargetName';
            $result[3].TargetName | Should -Be 'OtherName';
        }
    }
}

#endregion Invoke-PSRule

#region Test-PSRuleTarget

Describe 'Test-PSRuleTarget' -Tag 'Test-PSRuleTarget','Common' {
    Context 'With defaults' {
        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
        }

        It 'Returns boolean' {
            # Check passing rule
            $result = $testObject | Test-PSRuleTarget -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $True;

            # Check result with one failing rule
            $option = @{ 'Execution.InconclusiveWarning' = $False };
            $result = $testObject | Test-PSRuleTarget -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1', 'FromFile2', 'FromFile3' -Option $option;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $False;
        }

        It 'Returns warnings on inconclusive' {
            # Check result with an inconculsive rule
            $result = $testObject | Test-PSRuleTarget -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile3' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $False;
            $outWarnings | Should -Be "Inconclusive result reported for 'TestObject1' @FromFile.Rule.ps1/FromFile3.";
        }

        It 'Returns warnings on no rules' {
            # Check result with no matching rules
            $result = $testObject | Test-PSRuleTarget -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'NotARule' -WarningVariable outWarnings -WarningAction SilentlyContinue;
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
            $warningMessages[0].Message | Should -Be 'Path not found';
        }

        It 'Returns warning when not processed' {
            # Check result with no rules matching precondition
            $result = $testObject | Test-PSRuleTarget -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'WithPreconditionFalse' -WarningVariable outWarnings -WarningAction SilentlyContinue;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType System.Boolean;
            $result | Should -Be $True;
            $outWarnings | Should -Be "Target object 'TestObject1' has not been processed because no matching rules were found.";
        }
    }
}

#endregion Test-PSRuleTarget

#region Get-PSRule

Describe 'Get-PSRule' -Tag 'Get-PSRule','Common' {
    Context 'Using -Path' {
        It 'Returns rules' {
            # Get a list of rules
            $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -BeGreaterThan 0;
        }

        It 'Filters by name' {
            $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1', 'FromFile3';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.RuleName | Should -BeIn @('FromFile1', 'FromFile3');
        }

        It 'Filters by tag' {
            $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ Test = "Test1" };
            $result | Should -Not -BeNullOrEmpty;
            $result.RuleName | Should -Be 'FromFile1';
        }

        It 'Reads metadata' {
            $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.RuleName | Should -Be 'FromFile1';
            $result.Description | Should -Be 'Test rule 1';
        }

        It 'Handles empty path' {
            $emptyPath = Join-Path -Path $outputPath -ChildPath 'empty';
            if (!(Test-Path -Path $emptyPath)) {
                $Null = New-Item -Path $emptyPath -ItemType Directory -Force;
            }
            $Null = Get-PSRule -Path $emptyPath;
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

            $Null = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSRule -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            { $Null = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'UnconstrainedFile.Rule.ps1') -Name 'UnconstrainedFile1' -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }
}

#endregion Get-PSRule

#region New-PSRuleOption

Describe 'New-PSRuleOption' -Tag 'Option','Common','New-PSRuleOption' {
    Context 'Read Baseline.RuleName' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Baseline.RuleName | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Baseline.RuleName' = 'rule1' };
            $option.Baseline.RuleName | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option @{ 'Baseline.RuleName' = 'rule1', 'rule2' };
            $option.Baseline.RuleName | Should -BeIn 'rule1', 'rule2';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.RuleName | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Baseline.RuleName | Should -BeIn 'rule1', 'rule2';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Baseline.RuleName | Should -BeIn 'rule1';
        }
    }

    Context 'Read Baseline.Exclude' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Baseline.Exclude | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Baseline.Exclude' = 'rule3' };
            $option.Baseline.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option @{ 'Baseline.Exclude' = 'rule3', 'rule4' };
            $option.Baseline.Exclude | Should -BeIn 'rule3', 'rule4';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Baseline.Exclude | Should -BeIn 'rule3', 'rule4';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Baseline.Exclude | Should -BeIn 'rule3';
        }
    }

    Context 'Read Baseline.Configuration' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Baseline.Configuration.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -BaselineConfiguration @{ 'option1' = 'option'; 'option2' = 2; option3 = 'option3a', 'option3b' };
            $option.Baseline.Configuration.option1 | Should -BeIn 'option';
            $option.Baseline.Configuration.option2 | Should -Be 2;
            $option.Baseline.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.Configuration.option1 | Should -Be 'option';
            $option.Baseline.Configuration.option2 | Should -Be 2;
            $option.Baseline.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
        }
    }

    Context 'Read Binding.TargetName' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Binding.TargetName | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName' };
            $option.Binding.TargetName | Should -BeIn 'ResourceName';

            # With array
            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' };
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName';
        }
    }

    Context 'Read Execution.LanguageMode' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Execution.LanguageMode | Should -Be FullLanguage;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
            $option.Execution.LanguageMode | Should -Be ConstrainedLanguage;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.LanguageMode | Should -Be ConstrainedLanguage
        }
    }

    Context 'Read Execution.InconclusiveWarning' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Execution.InconclusiveWarning | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.InconclusiveWarning' = $False };
            $option.Execution.InconclusiveWarning | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.InconclusiveWarning | Should -Be $False
        }
    }

    Context 'Read Execution.NotProcessedWarning' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Execution.NotProcessedWarning | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.NotProcessedWarning' = $False };
            $option.Execution.NotProcessedWarning | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.NotProcessedWarning | Should -Be $False
        }
    }

    Context 'Read Suppression' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Suppression.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -SuppressTargetName @{ 'SuppressionTest' = 'testObject1', 'testObject3' };
            $option.Suppression['SuppressionTest'].TargetName | Should -BeIn 'testObject1', 'testObject3';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Suppression['SuppressionTest1'].TargetName | Should -BeIn 'TestObject1', 'TestObject3';
            $option.Suppression['SuppressionTest2'].TargetName | Should -BeIn 'TestObject1', 'TestObject3';
        }
    }
}

#endregion New-PSRuleOption

#region PSRule variables

Describe 'PSRule variables' -Tag 'Variables','Common' {
    Context 'PowerShell automatic variables' {
        $testObject = [PSCustomObject]@{
            Name = 'VariableTest'
        }

        It '$Rule' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'VariableTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
        }
    }
}

#endregion PSRule variables
