# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

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

$reportPath = Join-Path -Path $rootPath -ChildPath reports/;
if (!(Test-Path -Path $reportPath)) {
    $Null = New-Item -Path $reportPath -ItemType Directory -Force;
}

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
            ($scopedResult | Where-Object -FilterScript { $_.RuleName -eq 'appServiceApp.ARRAffinity' }).Outcome | Should -Be 'Pass';

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
    $option = Join-Path -Path $rootPath -ChildPath docs/scenarios/azure-tags/ps-rule.yaml;
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
            Option = (Join-Path -Path $scenarioPath -ChildPath 'ps-rule.yaml')
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

Describe 'Scenarios -- rule-module' -Tag 'EndToEnd', 'rule-module' {
    $scenarioPath = Join-Path -Path $rootPath -ChildPath docs/scenarios/rule-module;
    $inputPath = Join-Path -Path $scenarioPath -ChildPath 'resources.json';

    Context 'Invoke-PSRule' {
        Import-Module (Join-Path -Path $scenarioPath -ChildPath 'Enterprise.Rules') -Force;
        $result = @(Invoke-PSRule -InputPath $inputPath -Module 'Enterprise.Rules' -WarningAction SilentlyContinue);

        It 'Binds fields' {
            $result.TargetName | Should -BeIn 'storage', 'app-service-plan', 'web-app', 'web-app/staging';
            $result.TargetType | Should -BeIn @(
                'Microsoft.Storage/storageAccounts'
                'Microsoft.Web/serverfarms'
                'Microsoft.Web/sites'
                'Microsoft.Web/sites/slots'
            )
            $result.Field.SubscriptionId | Should -Be '00000000-0000-0000-0000-000000000000';
            $result.Field.ResourceGroupName | Should -Be 'test-rg';
        }
    }
}

Describe 'Scenarios -- validation-pipeline' -Tag 'EndToEnd', 'validation-pipeline' {
    $scenarioPath = Join-Path -Path $rootPath -ChildPath docs/scenarios/validation-pipeline;
    $sourcePath = Join-Path -Path $rootPath -ChildPath src/PSRule;
    $sourceFiles = Get-ChildItem -Path $sourcePath -Recurse -Include *.ps1,*.psm1,*.psd1;

    Context 'Invoke-PSRule' {
        $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error'; };
        $result = $sourceFiles | Invoke-PSRule -Path $scenarioPath -Option $option;

        It 'Module quality' {
            $fail = @($result | Where-Object -FilterScript { !$_.IsSuccess() });
            $fail.Length | Should -Be 0;
            $pass = @($result | Where-Object -FilterScript { $_.IsSuccess() });
            $pass.Length | Should -BeGreaterThan 0;
        }

        It 'Use header' {
            $filteredResult = @($result | Where-Object -FilterScript { $_.RuleName -eq 'file.Header' });
            $filteredResult | Should -Not -BeNullOrEmpty;
        }

        It 'Use encoding' {
            $filteredResult = @($result | Where-Object -FilterScript { $_.RuleName -eq 'file.Encoding' });
            $filteredResult | Should -Not -BeNullOrEmpty;
        }

        It 'Use NUnit output' {
            $report = [Xml]($sourceFiles | Invoke-PSrule -Path $scenarioPath -Option $option -OutputFormat NUnit3);
            $report.Save((Join-Path -Path $reportPath -ChildPath 'rule.report.xml'));
            $items = @($report.DocumentElement.'test-suite');
            Test-Path -Path (Join-Path -Path $reportPath -ChildPath 'rule.report.xml') | Should -be $True;
            $items.Length | Should -Be 5;
        }
    }
}
