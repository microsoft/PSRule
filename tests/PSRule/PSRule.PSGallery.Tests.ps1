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

    Context 'Module' {

        It 'Can be imported' {
            Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;
        }
    }

    Context 'Manifest' {
        $manifestPath = (Join-Path -Path $rootPath -ChildPath out/modules/PSRule/PSRule.psd1);
        $result = Test-ModuleManifest -Path $manifestPath;

        It 'Has required fields' {
            $result.Name | Should -Be 'PSRule';
            $result.Description | Should -Not -BeNullOrEmpty;
            $result.LicenseUri | Should -Not -BeNullOrEmpty;
            $result.ReleaseNotes | Should -Not -BeNullOrEmpty;
        }

        It 'Exports functions' {
            $result.ExportedFunctions.Keys | Should -BeIn 'Invoke-PSRule', 'Get-PSRule', 'New-PSRuleOption', 'Rule';
        }
    }

    Context 'Static analysis' {
        $result = Invoke-ScriptAnalyzer -Path (Join-Path -Path $rootPath -ChildPath out/modules/PSRule);

        $warningCount = ($result | Where-Object { $_.Severity -eq 'Warning' } | Measure-Object).Count;
        $errorCount = ($result | Where-Object { $_.Severity -eq 'Error' } | Measure-Object).Count;

        Write-Warning -Message "PSScriptAnalyzer reports $warningCount warnings.";

        It 'Has no quality errors' {
            $errorCount | Should -BeLessOrEqual 0;
        }
    }
}
