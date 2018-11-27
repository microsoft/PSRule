#
# Unit tests for the Exists keyword
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
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Exists;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSRule -- Exists keyword' -Tag 'Exists' {

    Context 'Exists' {
        $testObject = [PSCustomObject]@{
            Name = "TestObject1"
            Value = @{
                Value1 = 1
            }
        }

        It 'Return success' {

            $result = $testObject | Invoke-PSRule -Path $here -Name 'ExistsTest' -Verbose;
            $result | Should -Not -BeNullOrEmpty;
            $result.Success | Should -Be $True;
            $result.RuleName | Should -Be 'ExistsTest'
        }
    }
}
