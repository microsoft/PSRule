#
# Unit tests for the TypeOf keyword
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

Describe 'PSRule -- TypeOf keyword' -Tag 'TypeOf' {
    Context 'TypeOf' {
        It 'Matches type names' {
            $hashTableObject = @{ Key = 'Value' }
            $hashTableObjectWithName1 = @{ Key = 'Value' }; $hashTableObjectWithName1.PSObject.TypeNames.Insert(0, 'AdditionalTypeName');
            $hashTableObjectWithName2 = @{ Key = 'Value' }; $hashTableObjectWithName2.PSObject.TypeNames.Add('AdditionalTypeName');
            $customObjectWithName = [PSCustomObject]@{ Key = 'Value' }; $customObjectWithName.PSObject.TypeNames.Add('PSRule.Test.OtherType');
            $testObject = @($hashTableObject, $hashTableObjectWithName1, $hashTableObjectWithName2, $customObjectWithName);

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'TypeOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 4;
            $result.IsSuccess() | Should -BeIn $True;
            $result.RuleName | Should -BeIn 'TypeOfTest';
        }

        It 'Does not match type names' {
            $testObject = [PSCustomObject]@{
                Key = 'Value'
            }

            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'TypeOfTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $False;
            $result.RuleName | Should -Be 'TypeOfTest';
        }
    }
}
