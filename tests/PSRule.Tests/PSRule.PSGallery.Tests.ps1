#
# Unit tests for validating module for publishing
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;

Describe 'PSRule' -Tag 'PowerShellGallery' {
    $modulePath = (Join-Path -Path $rootPath -ChildPath out/modules/PSRule);

    Context 'Module' {
        It 'Can be imported' {
            Import-Module $modulePath -Force;
        }
    }

    Context 'Manifest' {
        $manifestPath = (Join-Path -Path $rootPath -ChildPath out/modules/PSRule/PSRule.psd1);
        $result = Test-ModuleManifest -Path $manifestPath;
        $Global:psEditor = $True;
        $Null = Import-Module $modulePath -Force;
        $commands = Get-Command -Module PSRule -All;

        It 'Has required fields' {
            $result.Name | Should -Be 'PSRule';
            $result.Description | Should -Not -BeNullOrEmpty;
            $result.LicenseUri | Should -Not -BeNullOrEmpty;
            $result.ReleaseNotes | Should -Not -BeNullOrEmpty;
        }

        It 'Exports functions' {
            $filteredCommands = @($commands | Where-Object -FilterScript { $_ -is [System.Management.Automation.FunctionInfo] });
            $filteredCommands | Should -Not -BeNullOrEmpty;
            $expected = @(
                'Invoke-PSRule'
                'Get-PSRule'
                'Get-PSRuleHelp'
                'Test-PSRuleTarget'
                'New-PSRuleOption'
                'Set-PSRuleOption'
                'Rule'
            )
            $expected | Should -BeIn $filteredCommands.Name;
            $expected | Should -BeIn $result.ExportedFunctions.Keys;
        }

        It 'Exports aliases' {
            $filteredCommands = @($commands | Where-Object -FilterScript { $_ -is [System.Management.Automation.AliasInfo] });
            $filteredCommands | Should -Not -BeNullOrEmpty;
            $expected = @(
                'Hint'
            )
            $expected | Should -BeIn $filteredCommands.Name;
            $expected | Should -BeIn $result.ExportedAliases.Keys;
        }
    }

    Context 'Static analysis' {
        It 'Has no quality errors' {
            $result = Invoke-ScriptAnalyzer -Path $modulePath;

            $warningCount = ($result | Where-Object { $_.Severity -eq 'Warning' } | Measure-Object).Count;
            $errorCount = ($result | Where-Object { $_.Severity -eq 'Error' } | Measure-Object).Count;

            if ($warningCount -gt 0) {
                Write-Warning -Message "PSScriptAnalyzer reports $warningCount warnings.";
            }

            $errorCount | Should -BeLessOrEqual 0;
        }
    }
}
