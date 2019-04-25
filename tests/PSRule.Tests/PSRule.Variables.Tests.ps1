#
# Unit tests for PSRule variables
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

#region PSRule variables

Describe 'PSRule variables' -Tag 'Variables' {
    Context 'PowerShell automatic variables' {
        $testObject = [PSCustomObject]@{
            Name = 'VariableTest'
            Type = 'TestType'
        }
        $testObject.PSObject.TypeNames.Insert(0, $testObject.Type);

        It '$Rule' {
            $result = $testObject | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1') -Name 'VariableTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
        }
    }
}

#endregion PSRule variables
