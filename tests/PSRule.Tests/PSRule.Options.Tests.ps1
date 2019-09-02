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

    Context 'Use -Path' {
        It 'With file' {
            $filePath = Join-Path -Path $outputPath -ChildPath 'new.file/ps-rule.yaml';
            Set-PSRuleOption -Path $filePath -Force;
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Not -Throw;
        }

        It 'With directory' {
            $filePath = Join-Path -Path $outputPath -ChildPath 'new.file';
            Set-PSRuleOption -Path $filePath -Force;
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Not -Throw;
        }

        It 'With missing file' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'new-not-a-file.yaml');
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw;
        }

        It 'With missing directory' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'new-dir/ps-rule.yaml');
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw;
        }
    }

    Context 'Read Rule.Include' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Rule.Include | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Rule.Include' = 'rule1' };
            $option.Rule.Include | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option @{ 'Rule.Include' = 'rule1', 'rule2' };
            $option.Rule.Include | Should -BeIn 'rule1', 'rule2';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Rule.Include | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Rule.Include | Should -BeIn 'rule1', 'rule2';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Rule.Include | Should -BeIn 'rule1';
        }
    }

    Context 'Read Rule.Exclude' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Rule.Exclude | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Rule.Exclude' = 'rule3' };
            $option.Rule.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option @{ 'Rule.Exclude' = 'rule3', 'rule4' };
            $option.Rule.Exclude | Should -BeIn 'rule3', 'rule4';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Rule.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Rule.Exclude | Should -BeIn 'rule3', 'rule4';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Rule.Exclude | Should -BeIn 'rule3';
        }
    }

    Context 'Read Baseline.Configuration' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Configuration.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -BaselineConfiguration @{ 'option1' = 'option'; 'option2' = 2; option3 = 'option3a', 'option3b' };
            $option.Configuration.option1 | Should -BeIn 'option';
            $option.Configuration.option2 | Should -Be 2;
            $option.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Configuration.option1 | Should -Be 'option';
            $option.Configuration.option2 | Should -Be 2;
            $option.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
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
            $option = New-PSRuleOption -TargetName 'ResourceName', 'AlternateName' -Path $emptyOptionsFilePath;
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
            $option = New-PSRuleOption -TargetType 'ResourceType', 'Kind' -Path $emptyOptionsFilePath;
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
            $option = New-PSRuleOption -InconclusiveWarning $False -Path $emptyOptionsFilePath;
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
            $option = New-PSRuleOption -NotProcessedWarning $False -Path $emptyOptionsFilePath;
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
            $option = New-PSRuleOption -Format 'Yaml' -Path $emptyOptionsFilePath;
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
            $option = New-PSRuleOption -ObjectPath 'items' -Path $emptyOptionsFilePath;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Logging.LimitDebug' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Logging.LimitDebug | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.LimitDebug' = 'TestRule1' };
            $option.Logging.LimitDebug | Should -Be 'TestRule1';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.LimitDebug | Should -Be 'TestRule2';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingLimitDebug 'TestRule2' -Path $emptyOptionsFilePath;
            $option.Logging.LimitDebug | Should -Be 'TestRule2';
        }
    }

    Context 'Read Logging.LimitVerbose' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Logging.LimitVerbose | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.LimitVerbose' = 'TestRule1' };
            $option.Logging.LimitVerbose | Should -Be 'TestRule1';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.LimitVerbose | Should -Be 'TestRule2';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingLimitVerbose 'TestRule2' -Path $emptyOptionsFilePath;
            $option.Logging.LimitVerbose | Should -Be 'TestRule2';
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

    Context 'Read Output.Encoding' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Output.Encoding | Should -Be 'Default';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Encoding' = 'UTF7' };
            $option.Output.Encoding | Should -Be 'UTF7';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Encoding | Should -Be 'UTF7';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputEncoding 'UTF7' -Path $emptyOptionsFilePath;
            $option.Output.Encoding | Should -Be 'UTF7';
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

    Context 'Read Output.Path' {
        It 'from default' {
            $option = New-PSRuleOption;
            $option.Output.Path | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Path' = 'out/OutputPath.txt' };
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputPath 'out/OutputPath.txt' -Path $emptyOptionsFilePath;
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
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

    Context 'Use -AllowClobber' {
        $filePath = (Join-Path -Path $outputPath -ChildPath 'PSRule.Tests4.yml');
        Copy-Item -Path (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml') -Destination $filePath;

        It 'Errors with comments' {
            { Set-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw -ErrorId 'PSRule.PSRuleOption.YamlContainsComments';
        }

        it 'Overwrites file' {
            Set-PSRuleOption -Path $filePath -BindingIgnoreCase $True -AllowClobber;
            $result = New-PSRuleOption -Path $filePath;
            $result.Binding.IgnoreCase | Should -Be $True;
        }
    }

    Context 'Use -Path' {
        It 'With missing file' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'set-not-a-file.yml');
            { Set-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Not -Throw;
            Test-Path -Path $filePath | Should -Be $True;
        }

        It 'With missing directory' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'set-dir/ps-rule.yaml');
            { Set-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw -ErrorId 'PSRule.PSRuleOption.ParentPathNotFound';
            Test-Path -Path $filePath | Should -Be $False;
        }

        It 'With missing directory with -Force' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'set-dir/ps-rule.yaml');
            Set-PSRuleOption -Path $filePath -Force;
            Test-Path -Path $filePath | Should -Be $True;
        }
    }

    Context 'Default YAML file detection' {
        It 'Find default options files' {
            $defaultPath = (Join-Path -Path $outputPath -ChildPath 'default');

            # psrule.yml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'psrule.yml');
            Set-PSRuleOption -Path $filePath -TargetName 'psrule.yml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'psrule.yml';

            # psrule.yaml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'psrule.yaml');
            Set-PSRuleOption -Path $filePath -TargetName 'psrule.yaml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'psrule.yaml';

            # ps-rule.yml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'ps-rule.yml');
            Set-PSRuleOption -Path $filePath -TargetName 'ps-rule.yml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'ps-rule.yml';

            # ps-rule.yaml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'ps-rule.yaml');
            Set-PSRuleOption -Path $filePath -TargetName 'ps-rule.yaml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'ps-rule.yaml';

            (Get-ChildItem -Path $defaultPath | Measure-Object).Count | Should -Be 4;
        }
    }

    Context 'Read Binding.IgnoreCase' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingIgnoreCase $False @optionParams;
            $option.Binding.IgnoreCase | Should -Be $False;
        }
    }

    Context 'Read Binding.TargetName' {
        It 'from parameter' {
            $option = Set-PSRuleOption -TargetName 'ResourceName', 'AlternateName' @optionParams;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }
    }

    Context 'Read Binding.TargetType' {
        It 'from parameter' {
            $option = Set-PSRuleOption -TargetType 'ResourceType', 'Kind' @optionParams;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }
    }

    Context 'Read Execution.InconclusiveWarning' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InconclusiveWarning $False @optionParams;
            $option.Execution.InconclusiveWarning | Should -Be $False;
        }
    }

    Context 'Read Execution.NotProcessedWarning' {
        It 'from parameter' {
            $option = Set-PSRuleOption -NotProcessedWarning $False @optionParams;
            $option.Execution.NotProcessedWarning | Should -Be $False;
        }
    }

    Context 'Read Input.Format' {
        It 'from parameter' {
            $option = Set-PSRuleOption -Format 'Yaml' @optionParams;
            $option.Input.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Input.ObjectPath' {
        It 'from parameter' {
            $option = Set-PSRuleOption -ObjectPath 'items' @optionParams;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Logging.LimitDebug' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingLimitDebug 'TestRule2' @optionParams;
            $option.Logging.LimitDebug | Should -Be 'TestRule2';
        }
    }

    Context 'Read Logging.LimitVerbose' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingLimitVerbose 'TestRule2' @optionParams;
            $option.Logging.LimitVerbose | Should -Be 'TestRule2';
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

    Context 'Read Output.Encoding' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputEncoding 'UTF7' @optionParams;
            $option.Output.Encoding | Should -Be 'UTF7';
        }
    }

    Context 'Read Output.Format' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputFormat 'Yaml' @optionParams;
            $option.Output.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Output.Path' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputPath 'out/OutputPath.txt' @optionParams;
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }
    }
}

#endregion Set-PSRuleOption
