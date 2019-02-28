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

Describe 'Scenarios -- azure-resources' -Tag 'EndToEnd','azure-resources' {
    $option = @{ 'Execution.NotProcessedWarning' = $False };
    $jsonData = Get-Content -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-resources/resources.json) | ConvertFrom-Json;
    $result = $jsonData | Invoke-PSRule -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-resources) -Option $option;

    Context 'App Service' {
        $scopedResult = $result | Where-Object -FilterScript { $_.RuleName -like 'appService*' };

        It 'Processes rules' {
            $scopedResult | Should -Not -BeNullOrEmpty;
            $scopedResult.Count | Should -Be 4;

            # Should pass
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServicePlan.MinPlan' }).Outcome | Should -Be 'Pass';
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.ARRAfinity' }).Outcome | Should -Be 'Pass';

            # Should fail
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServicePlan.MinInstanceCount' }).Outcome | Should -Be 'Fail';
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.UseHTTPS' }).Outcome | Should -Be 'Fail';
        }
    }

    Context 'Storage Accounts' {
        $scopedResult = $result | Where-Object -FilterScript { $_.RuleName -like 'storageAccounts.*' };

        It 'Processes rules' {
            $scopedResult | Should -Not -BeNullOrEmpty;
            $scopedResult.Count | Should -Be 2;

            # Should pass
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'storageAccounts.UseEncryption' }).Outcome | Should -Be 'Pass';

            # Should fail
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'storageAccounts.UseHttps' }).Outcome | Should -Be 'Fail';
        }
    }
}

Describe 'Scenarios -- azure-tags' -Tag 'EndToEnd','azure-tags' {
    $option = Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-tags/PSRule.yaml;
    $jsonData = Get-Content -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-tags/resources.json) | ConvertFrom-Json;
    $result = $jsonData | Invoke-PSRule -Path (Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-tags) -Option $option;

    Context 'Resources' {
        It 'Processes rules' {
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -Be 12;

            # environmentTag
            $filteredResults = ($result | Where-Object -FilterScript { $_.RuleName -eq 'environmentTag' });
            ($filteredResults | Where-Object -FilterScript { $_.Outcome -eq 'Pass' }).TargetName | Should -BeIn 'storage', 'app-service-plan';
            ($filteredResults | Where-Object -FilterScript { $_.Outcome -eq 'Fail' }).TargetName | Should -BeIn 'web-app', 'web-app/staging';

            # costCentreTag
            $filteredResults = ($result | Where-Object -FilterScript { $_.RuleName -eq 'costCentreTag' });
            ($filteredResults | Where-Object -FilterScript { $_.Outcome -eq 'Pass' }).TargetName | Should -BeIn 'app-service-plan';
            ($filteredResults | Where-Object -FilterScript { $_.Outcome -eq 'Fail' }).TargetName | Should -BeIn 'storage', 'web-app', 'web-app/staging';

            # businessUnitTag
            $filteredResults = ($result | Where-Object -FilterScript { $_.RuleName -eq 'businessUnitTag' });
            ($filteredResults | Where-Object -FilterScript { $_.Outcome -eq 'Pass' }).TargetName | Should -BeIn 'app-service-plan', 'web-app', 'web-app/staging';
            ($filteredResults | Where-Object -FilterScript { $_.Outcome -eq 'Fail' }).TargetName | Should -BeIn 'storage';
        }
    }
}

Describe 'Scenarios -- fruit' -Tag 'EndToEnd','fruit' {

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

Describe 'Scenarios -- kubernetes-resources' -Tag 'EndToEnd','kubernetes-resources' {
    $scenarioPath = Join-Path -Path $rootPath -ChildPath docs/scenarios/kubernetes-resources;
    $yamlData = Get-Content -Path (Join-Path -Path $scenarioPath -ChildPath 'resources.yaml') -Raw;

    Context 'Invoke-PSRule' {
        $invokeParams = @{
            Path = $scenarioPath
            Format = 'Yaml'
            Option = (Join-Path -Path $scenarioPath -ChildPath 'PSRule.yaml')
        }
        $result = @($yamlData | Invoke-PSRule @invokeParams);

        It 'Processes rules' {
            $result.Count | Should -Be 18;
        }

        It 'Deployment app1-cache' {
            $instance = $result | Where-Object -FilterScript { $_.TargetName -eq 'app1-cache' };
            $fail = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Fail' });
            $pass = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Pass' });
            $fail.Length | Should -Be 3;
            $pass.Length | Should -Be 3;
        }

        It 'Service app1-cache-service' {
            $instance = $result | Where-Object -FilterScript { $_.TargetName -eq 'app1-cache-service' };
            $fail = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Fail' });
            $pass = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Pass' });
            $fail.Length | Should -Be 3;
            $pass.Length | Should -Be 0;
        }

        It 'Deployment app1-ui' {
            $instance = $result | Where-Object -FilterScript { $_.TargetName -eq 'app1-ui' };
            $fail = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Fail' });
            $pass = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Pass' });
            $fail.Length | Should -Be 1;
            $pass.Length | Should -Be 5;
        }

        It 'Service app1-ui-service' {
            $instance = $result | Where-Object -FilterScript { $_.TargetName -eq 'app1-ui-service' };
            $fail = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Fail' });
            $pass = @($instance | Where-Object -FilterScript { $_.Outcome -eq 'Pass' });
            $fail.Length | Should -Be 0;
            $pass.Length | Should -Be 3;
        }
    }
}
