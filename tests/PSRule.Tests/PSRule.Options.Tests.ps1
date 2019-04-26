#
# Unit tests for PSRule options
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
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Options;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

#region New-PSRuleOption

Describe 'New-PSRuleOption' -Tag 'Option','New-PSRuleOption' {
    $emptyOptionsFilePath = (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml');

    Context 'Read Baseline.RuleName' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Baseline.RuleName | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Baseline.RuleName' = 'rule1' };
            $option.Baseline.RuleName | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option @{ 'Baseline.RuleName' = 'rule1', 'rule2' };
            $option.Baseline.RuleName | Should -BeIn 'rule1', 'rule2';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.RuleName | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Baseline.RuleName | Should -BeIn 'rule1', 'rule2';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Baseline.RuleName | Should -BeIn 'rule1';
        }
    }

    Context 'Read Baseline.Exclude' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Baseline.Exclude | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Baseline.Exclude' = 'rule3' };
            $option.Baseline.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option @{ 'Baseline.Exclude' = 'rule3', 'rule4' };
            $option.Baseline.Exclude | Should -BeIn 'rule3', 'rule4';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Baseline.Exclude | Should -BeIn 'rule3', 'rule4';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Baseline.Exclude | Should -BeIn 'rule3';
        }
    }

    Context 'Read Baseline.Configuration' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Baseline.Configuration.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -BaselineConfiguration @{ 'option1' = 'option'; 'option2' = 2; option3 = 'option3a', 'option3b' };
            $option.Baseline.Configuration.option1 | Should -BeIn 'option';
            $option.Baseline.Configuration.option2 | Should -Be 2;
            $option.Baseline.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.Configuration.option1 | Should -Be 'option';
            $option.Baseline.Configuration.option2 | Should -Be 2;
            $option.Baseline.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
        }
    }

    Context 'Read Binding.IgnoreCase' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Binding.IgnoreCase | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Binding.IgnoreCase' = $False };
            $option.Binding.IgnoreCase | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.IgnoreCase | Should -Be $False;
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingIgnoreCase $False -Path $emptyOptionsFilePath;
            $option.Binding.IgnoreCase | Should -Be $False;
        }
    }

    Context 'Read Binding.TargetName' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Binding.TargetName | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName' };
            $option.Binding.TargetName | Should -BeIn 'ResourceName';

            # With array
            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' };
            $option.Binding.TargetName.Length | Should -Be 2;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingTargetName 'ResourceName', 'AlternateName' -Path $emptyOptionsFilePath;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }
    }

    Context 'Read Binding.TargetType' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Binding.TargetType | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'ResourceType' };
            $option.Binding.TargetType | Should -BeIn 'ResourceType';

            # With array
            $option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'ResourceType', 'Kind' };
            $option.Binding.TargetType.Length | Should -Be 2;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.TargetType | Should -BeIn 'ResourceType';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Binding.TargetType | Should -BeIn 'ResourceType';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingTargetType 'ResourceType', 'Kind' -Path $emptyOptionsFilePath;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }
    }

    Context 'Read Execution.LanguageMode' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Execution.LanguageMode | Should -Be 'FullLanguage';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionLanguageMode 'ConstrainedLanguage' -Path $emptyOptionsFilePath;
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }
    }

    Context 'Read Execution.InconclusiveWarning' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Execution.InconclusiveWarning | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.InconclusiveWarning' = $False };
            $option.Execution.InconclusiveWarning | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.InconclusiveWarning | Should -Be $False;
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionInconclusiveWarning $False -Path $emptyOptionsFilePath;
            $option.Execution.InconclusiveWarning | Should -Be $False;
        }
    }

    Context 'Read Execution.NotProcessedWarning' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Execution.NotProcessedWarning | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.NotProcessedWarning' = $False };
            $option.Execution.NotProcessedWarning | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.NotProcessedWarning | Should -Be $False;
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionNotProcessedWarning $False -Path $emptyOptionsFilePath;
            $option.Execution.NotProcessedWarning | Should -Be $False;
        }
    }

    Context 'Read Input.Format' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Input.Format | Should -Be 'Detect';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.Format' = 'Yaml' };
            $option.Input.Format | Should -Be 'Yaml';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.Format | Should -Be 'Yaml';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputFormat 'Yaml' -Path $emptyOptionsFilePath;
            $option.Input.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Input.ObjectPath' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Input.ObjectPath | Should -Be $Null;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.ObjectPath' = 'items' };
            $option.Input.ObjectPath | Should -Be 'items';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.ObjectPath | Should -Be 'items';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputObjectPath 'items' -Path $emptyOptionsFilePath;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Logging.RuleFail' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Logging.RuleFail | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
            $option.Logging.RuleFail | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.RuleFail | Should -Be 'Warning';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingRuleFail 'Warning' -Path $emptyOptionsFilePath;
            $option.Logging.RuleFail | Should -Be 'Warning';
        }
    }

    Context 'Read Logging.RulePass' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Logging.RulePass | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Error' };
            $option.Logging.RulePass | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.RulePass | Should -Be 'Warning';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingRulePass 'Warning' -Path $emptyOptionsFilePath;
            $option.Logging.RulePass | Should -Be 'Warning';
        }
    }

    Context 'Read Output.As' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Output.As | Should -Be 'Detail';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.As' = 'Summary' };
            $option.Output.As | Should -Be 'Summary';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.As | Should -Be 'Summary';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputAs 'Summary' -Path $emptyOptionsFilePath;
            $option.Output.As | Should -Be 'Summary';
        }
    }

    Context 'Read Output.Format' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Output.Format | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Format' = 'Yaml' };
            $option.Output.Format | Should -Be 'Yaml';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Format | Should -Be 'Json';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputFormat 'Yaml' -Path $emptyOptionsFilePath;
            $option.Output.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Suppression' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Suppression.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -SuppressTargetName @{ 'SuppressionTest' = 'testObject1', 'testObject3' };
            $option.Suppression['SuppressionTest'].TargetName | Should -BeIn 'testObject1', 'testObject3';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Suppression['SuppressionTest1'].TargetName | Should -BeIn 'TestObject1', 'TestObject3';
            $option.Suppression['SuppressionTest2'].TargetName | Should -BeIn 'TestObject1', 'TestObject3';
        }
    }
}

#endregion New-PSRuleOption

#region Set-PSRuleOption

Describe 'Set-PSRuleOption' -Tag 'Option','Set-PSRuleOption' {
    $emptyOptionsFilePath = (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml');
    $optionParams = @{
        Path = $emptyOptionsFilePath
        PassThru = $True
    }

    # Baseline.RuleName
    # Baseline.Exclude
    # Baseline.Configuration

    Context 'Read Binding.IgnoreCase' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingIgnoreCase $False @optionParams;
            $option.Binding.IgnoreCase | Should -Be $False;
        }
    }

    Context 'Read Binding.TargetName' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingTargetName 'ResourceName', 'AlternateName' @optionParams;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }
    }

    Context 'Read Binding.TargetType' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingTargetType 'ResourceType', 'Kind' @optionParams;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }
    }

    Context 'Read Execution.LanguageMode' {
        It 'from parameter' {
            $option = Set-PSRuleOption -ExecutionLanguageMode 'ConstrainedLanguage' @optionParams;
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }
    }

    Context 'Read Execution.InconclusiveWarning' {
        It 'from parameter' {
            $option = Set-PSRuleOption -ExecutionInconclusiveWarning $False @optionParams;
            $option.Execution.InconclusiveWarning | Should -Be $False;
        }
    }

    Context 'Read Execution.NotProcessedWarning' {
        It 'from parameter' {
            $option = Set-PSRuleOption -ExecutionNotProcessedWarning $False @optionParams;
            $option.Execution.NotProcessedWarning | Should -Be $False;
        }
    }

    Context 'Read Input.Format' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputFormat 'Yaml' @optionParams;
            $option.Input.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Input.ObjectPath' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputObjectPath 'items' @optionParams;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Logging.RuleFail' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingRuleFail 'Warning' @optionParams;
            $option.Logging.RuleFail | Should -Be 'Warning';
        }
    }

    Context 'Read Logging.RulePass' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingRulePass 'Warning' @optionParams;
            $option.Logging.RulePass | Should -Be 'Warning';
        }
    }

    Context 'Read Output.As' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputAs 'Summary' @optionParams;
            $option.Output.As | Should -Be 'Summary';
        }
    }

    Context 'Read Output.Format' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputFormat 'Yaml' @optionParams;
            $option.Output.Format | Should -Be 'Yaml';
        }
    }
    
    # Suppression
}

#endregion Set-PSRuleOption
