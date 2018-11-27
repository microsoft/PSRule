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

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'FromFile1' -Verbose;
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.TargetName | Should -Be 'TestTarget1'
        }

        It 'Return failure' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'FromFile2' -Verbose;
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $False;
            $result.TargetName | Should -Be 'TestTarget2'
        }
    }
}

Describe 'Get-PSRule' {

    Context 'Using -Path' {

        It 'Returns rules' {
            # Get a list of rules
            $result = Get-PSRule -Path $here;
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -BeGreaterThan 0;
        }

        It 'Filters by name' {
            $result = Get-PSRule -Path $here -Name 'FromFile1', 'FromFile3';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 2;
            $result.Name | Should -BeIn @('FromFile1', 'FromFile3')
        }

        It 'Filters by tag' {
            $result = Get-PSRule -Path $here -Tag @{ Test = "Test1" };
            $result | Should -Not -BeNullOrEmpty;
            $result.Name | Should -Be 'FromFile1'
        }
    }

    # Context 'Get rule with invalid path' {

    #     $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath invalid);
    # }
}

