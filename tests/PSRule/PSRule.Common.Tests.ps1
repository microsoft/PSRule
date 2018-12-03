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

        It 'Return passed' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile1';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Return failure' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile2';
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $False;
            $result.TargetName | Should -Be 'TestObject1';
        }

        It 'Returns inconclusive' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'FromFile3' -Status All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Status | Should -Be 'Inconclusive';
        }

        It 'Processes rule preconditions' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Tag @{ category = 'precondition' } -Status All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionTrue' }).Status | Should -Be 'Passed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithPreconditionFalse' }).Status | Should -Be 'None';
        }

        It 'Processes rule dependencies' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name WithDependency1 -Status All;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 5;
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency5' }).Status | Should -Be 'Failed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency4' }).Status | Should -Be 'Passed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency3' }).Status | Should -Be 'Passed';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency2' }).Status | Should -Be 'None';
            ($result | Where-Object -FilterScript { $_.RuleName -eq 'WithDependency1' }).Status | Should -Be 'None';
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
