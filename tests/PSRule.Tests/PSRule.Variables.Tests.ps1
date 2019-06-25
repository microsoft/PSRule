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
    $ruleFilePath = (Join-Path -Path $here -ChildPath 'FromFile.Rule.ps1');

    Context 'PowerShell automatic variables' {
        $testObject = [PSCustomObject]@{
            Name = 'VariableTest'
            Type = 'TestType'
        }
        $testObject.PSObject.TypeNames.Insert(0, $testObject.Type);

        It '$Rule' {
            $result = $testObject | Invoke-PSRule -Path $ruleFilePath -Name 'VariableTest';
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
        }

        It '$LocalizedData' {
            $invokeParams = @{
                Path = $ruleFilePath
                Name = 'WithLocalizedData'
                Culture = 'en-ZZ'
                WarningAction = 'SilentlyContinue'
            }
            $result = $testObject | Invoke-PSRule @invokeParams -WarningVariable outWarning;
            $messages = @($outwarning);
            $result | Should -Not -BeNullOrEmpty;
            $result.IsSuccess() | Should -Be $True;
            $result.TargetName | Should -Be 'VariableTest';
            $messages[0] | Should -Be 'LocalizedMessage for en-ZZ. Format=TestType.';
        }
    }
}

#endregion PSRule variables
