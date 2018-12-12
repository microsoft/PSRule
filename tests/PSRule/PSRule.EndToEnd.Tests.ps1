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

Describe 'Scenarios -- azure-resources' -Tag 'EndToEnd' {

    $jsonData = Get-Content -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-resources/resources.json) | ConvertFrom-Json;
    $result = $jsonData | Invoke-PSRule -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-resources);

    Context 'App Service' {

        $scopedResult = $result | Where-Object -FilterScript { $_.RuleName -like 'appService*' };

        It 'Processes rules' {
            $scopedResult | Should -Not -BeNullOrEmpty;
            $scopedResult.Count | Should -Be 4;

            # Should pass
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServicePlan.MinPlan' }).Outcome | Should -Be 'Passed';
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.ARRAfinity' }).Outcome | Should -Be 'Passed';

            # Should fail
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServicePlan.MinInstanceCount' }).Outcome | Should -Be 'Failed';
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.UseHTTPS' }).Outcome | Should -Be 'Failed';
        }
    }

    Context 'Storage Accounts' {

        $scopedResult = $result | Where-Object -FilterScript { $_.RuleName -like 'storageAccounts.*' };

        It 'Processes rules' {
            $scopedResult | Should -Not -BeNullOrEmpty;
            $scopedResult.Count | Should -Be 2;

            # Should pass
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'storageAccounts.UseEncryption' }).Outcome | Should -Be 'Passed';

            # Should fail
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'storageAccounts.UseHttps' }).Outcome | Should -Be 'Failed';
        }
    }
}

Describe 'Scenarios -- fruit' -Tag 'EndToEnd' {

    # Define objects
    $items = @();
    $items += [PSCustomObject]@{ Name = 'Fridge' };
    $items += [PSCustomObject]@{ Name = 'Apple' };

    Context 'Invoke-PSRule' {
        It 'Detailed results' {
            $result = @($items | Invoke-PSRule -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/fruit) -As Detail);
            $result.Count | Should -Be 2;
            $result | Should -BeOfType PSRule.Rules.RuleRecord;
        }

        It 'Summary results' {
            $result = @($items | Invoke-PSRule -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/fruit) -As Summary);
            $result.Count | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.RuleSummaryRecord;
        }
    }

    Context 'Get-PSRule' {
        It 'Returns rules' {
            $result = @(Get-PSRule -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/fruit));
            $result.Count | Should -Be 1;
            $result | Should -BeOfType PSRule.Rules.Rule;
        }
    }
}
