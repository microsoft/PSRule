#
# Unit tests for end to end language tests
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

Describe 'PSRule -- end to end tests' -Tag 'EndToEnd' {

    $jsonData = Get-Content -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-resources/resources.json) | ConvertFrom-Json;
    $result = $jsonData | Invoke-PSRule -Path (Join-Path -Path $here -ChildPath Rules);

    Context 'App Service' {

        $scopedResult = $result | Where-Object -FilterScript { $_.RuleName -like 'appService*' };

        It 'Processes rules' {
            $scopedResult | Should -Not -BeNullOrEmpty;
            $scopedResult.Count | Should -Be 4;

            # Should pass
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServicePlan.MinPlan' }).Status | Should -Be 'Passed';
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.ARRAfinity' }).Status | Should -Be 'Passed';

            # Should fail
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServicePlan.MinInstanceCount' }).Status | Should -Be 'Failed';
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.UseHTTPS' }).Status | Should -Be 'Failed';
        }
    }

    Context 'Storage Accounts' {

        $scopedResult = $result | Where-Object -FilterScript { $_.RuleName -like 'storageAccounts.*' };

        It 'Processes rules' {
            $scopedResult | Should -Not -BeNullOrEmpty;
            $scopedResult.Count | Should -Be 2;

            # Should pass
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'storageAccounts.UseEncryption' }).Status | Should -Be 'Passed';

            # Should fail
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'storageAccounts.UseHttps' }).Status | Should -Be 'Failed';
        }
    }
}
