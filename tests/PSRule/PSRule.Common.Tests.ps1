#
# Unit tests for core PSRule functionality
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
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Common;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'Invoke-PSRule' {

    Context 'Using -Path' {

        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
            Value = 1
        }

        It 'Returns passed' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Returns failure' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile2';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $False;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Returns inconclusive' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile3' -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Outcome | Should -Be 'Inconclusive';
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
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionTrue' }).Outcome | Should -Be 'Passed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionFalse' }).Outcome | Should -Be 'None';
        }

        It 'Processes rule dependencies' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name WithDependency1 -Outcome All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency5' }).Outcome | Should -Be 'Failed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency4' }).Outcome | Should -Be 'Passed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency3' }).Outcome | Should -Be 'Passed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency2' }).Outcome | Should -Be 'None';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency1' }).Outcome | Should -Be 'None';
        }

        It 'Binds to TargetName' {
            $testObject = [PSCustomObject]@{
                TargetName = "ObjectTargetName"
                Name = "ObjectName"
                Value = 1
            }

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.TargetName | Should -Be 'ObjectTargetName';
        }

        It 'Binds to Name' {
            $testObject = [PSCustomObject]@{
                Name = "ObjectName"
                Value = 1
            }

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.TargetName | Should -Be 'ObjectName';
        }
    }

    Context 'Using -As' {
        $testObject = @(
            [PSCustomObject]@{
                Name = "TestObject1"
                Value = 1
            }
            [PSCustomObject]@{
                Name = "TestObject1"
                Value = 1
            }
        );

        It 'Returns detail' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'group1' } -As Detail;
            $result | Should -Not -BeNullOrEmpty;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
        }

        It 'Returns summary' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'group1' } -As Summary;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 3;
            $result | Should -BeOfType PSRule.Rules.RuleSummaryRecord;
            $result.RuleId | Should -BeIn 'FromFile1', 'FromFile2', 'FromFile3'
            $result.Tag.category | Should -BeIn 'group1';

            ($result | Where-Object { $_.RuleId -eq 'FromFile1'}).Outcome | Should -Be 'Passed';
            ($result | Where-Object { $_.RuleId -eq 'FromFile1'}).Pass | Should -Be 2;
            ($result | Where-Object { $_.RuleId -eq 'FromFile2'}).Outcome | Should -Be 'Failed';
            ($result | Where-Object { $_.RuleId -eq 'FromFile2'}).Fail | Should -Be 2;
        }
    }

    Context 'With constrained language' {

        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
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
            { $Null = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'ConstrainedTest2' -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
        }
    }
}

Describe 'Get-PSRule' {

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
            $result.Name | Should -BeIn @('FromFile1', 'FromFile3');
        }

        It 'Filters by tag' {
            $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ Test = "Test1" };
            $result | Should -Not -BeNullOrEmpty;
            $result.Name | Should -Be 'FromFile1';
        }

        It 'Reads metadata' {
            $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Name | Should -Be 'FromFile1';
            $result.Description | Should -Be 'Test rule 1'
        }
    }

    # Context 'Get rule with invalid path' {

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
