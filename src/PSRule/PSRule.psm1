# Copyright (c) Bernie White. All rights reserved.

#
# PSRule module
#

Set-StrictMode -Version latest;

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module;
$PSModuleRoot = $PSModule.ModuleBase;

# Import the appropriate nested binary module based on the current PowerShell version
$binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/core/PSRule.dll';

$binaryModule = Import-Module -Name $binModulePath -PassThru;

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule;
}

[PSRule.Configuration.PSRuleOption]::UseExecutionContext($ExecutionContext);

#
# Localization
#

# $LocalizedData = data {

# }

Import-LocalizedData -BindingVariable LocalizedData -FileName 'PSRule.Resources.psd1' -ErrorAction SilentlyContinue;

#
# Public functions
#

# .ExternalHelp PSRule-Help.xml
function Invoke-PSRule {

    [CmdletBinding(DefaultParameterSetName = 'Input')]
    [OutputType([PSRule.Rules.RuleRecord])]
    [OutputType([PSRule.Rules.RuleSummaryRecord])]
    [OutputType([System.String])]
    param (
        # A list of paths to check for rule definitions
        [Parameter(Position = 0)]
        [Alias('p')]
        [String[]]$Path = $PWD,

        # Filter to rules with the following names
        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Input')]
        [Alias('TargetObject')]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [PSRule.Rules.RuleOutcome]$Outcome = [PSRule.Rules.RuleOutcome]::Processed,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [ValidateSet('Detail', 'Summary')]
        [PSRule.Configuration.ResultFormat]$As = [PSRule.Configuration.ResultFormat]::Detail,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Detect')]
        [PSRule.Configuration.InputFormat]$Format,

        [Parameter(Mandatory = $False)]
        [String]$ObjectPath,

        [Parameter(Mandatory = $False)]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3')]
        [PSRule.Configuration.OutputFormat]$OutputFormat,

        [Parameter(Mandatory = $True, ParameterSetName = 'InputPath')]
        [String[]]$InputPath
    )

    begin {
        Write-Verbose -Message "[Invoke-PSRule] BEGIN::";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        # Get an options object
        $Option = New-PSRuleOption @optionParams;

        # Discover scripts in the specified paths
        $sourceParams = @{ };

        if ($PSBoundParameters.ContainsKey('Path')) {
            $sourceParams['Path'] = $Path;
        }
        if ($PSBoundParameters.ContainsKey('Module')) {
            $sourceParams['Module'] = $Module;
        }
        if ($sourceParams.Count -eq 0) {
            $sourceParams['Path'] = $Path;
        }
        [PSRule.Rules.RuleSource[]]$sourceFiles = GetRuleScriptPath @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Warning -Message $LocalizedData.PathNotFound;
            return; # continue causes issues with Pester
        }

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        if ($PSBoundParameters.ContainsKey('Name')) {
            $Option.Baseline.RuleName = $Name;
        }

        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }

        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }

        if ($PSBoundParameters.ContainsKey('As')) {
            $Option.Output.As = $As;
        }

        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Invoke().Configure($Option);
        $builder.FilterBy($Tag);
        $builder.Source($sourceFiles);
        $builder.Limit($Outcome);

        if ($PSBoundParameters.ContainsKey('InputPath')) {
            $inputPaths = GetFilePath -Path $InputPath -Verbose:$VerbosePreference;
            $builder.InputPath($inputPaths);
        }

        $builder.UseCommandRuntime($PSCmdlet.CommandRuntime);
        $builder.UseLoggingPreferences($ErrorActionPreference, $WarningPreference, $VerbosePreference, $InformationPreference);
        $pipeline = $builder.Build();
        $pipeline.Begin();
    }

    process {
        if ($Null -ne (Get-Variable -Name pipeline -ErrorAction SilentlyContinue) -and $pipeline.RuleCount -gt 0) {
            try {
                # Process pipeline objects
                $pipeline.Process($InputObject);
            }
            catch {
                $pipeline.Dispose();
                throw;
            }
        }
    }

    end {
        if ($Null -ne (Get-Variable -Name pipeline -ErrorAction SilentlyContinue)) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Invoke-PSRule] END::";
    }
}

# .ExternalHelp PSRule-Help.xml
function Test-PSRuleTarget {
    [CmdletBinding(DefaultParameterSetName = 'Input')]
    [OutputType([System.Boolean])]
    param (
        # A list of paths to check for rule definitions
        [Parameter(Position = 0)]
        [Alias('p')]
        [String[]]$Path = $PWD,

        # Filter to rules with the following names
        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Input')]
        [Alias('TargetObject')]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Detect')]
        [PSRule.Configuration.InputFormat]$Format,

        [Parameter(Mandatory = $False)]
        [String]$ObjectPath,

        [Parameter(Mandatory = $False)]
        [String[]]$Module,

        [Parameter(Mandatory = $True, ParameterSetName = 'InputPath')]
        [String[]]$InputPath
    )

    begin {
        Write-Verbose -Message "[Test-PSRuleTarget] BEGIN::";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        # Get an options object
        $Option = New-PSRuleOption @optionParams;

        # Discover scripts in the specified paths
        $sourceParams = @{ };

        if ($PSBoundParameters.ContainsKey('Path')) {
            $sourceParams['Path'] = $Path;
        }
        if ($PSBoundParameters.ContainsKey('Module')) {
            $sourceParams['Module'] = $Module;
        }
        if ($sourceParams.Count -eq 0) {
            $sourceParams['Path'] = $Path;
        }
        [PSRule.Rules.RuleSource[]]$sourceFiles = GetRuleScriptPath @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Warning -Message $LocalizedData.PathNotFound;
            return; # continue causes issues with Pester
        }

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        if ($PSBoundParameters.ContainsKey('Name')) {
            $Option.Baseline.RuleName = $Name;
        }

        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }

        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Invoke().Configure($Option);
        $builder.FilterBy($Tag);
        $builder.Source($sourceFiles);

        if ($PSBoundParameters.ContainsKey('InputPath')) {
            $inputPaths = GetFilePath -Path $InputPath -Verbose:$VerbosePreference;
            $builder.InputPath($inputPaths);
        }

        $builder.UseCommandRuntime($PSCmdlet.CommandRuntime);
        $builder.UseLoggingPreferences($ErrorActionPreference, $WarningPreference, $VerbosePreference, $InformationPreference);
        $builder.ReturnBoolean();
        $pipeline = $builder.Build();
        $pipeline.Begin();
    }

    process {
        if ($Null -ne (Get-Variable -Name pipeline -ErrorAction SilentlyContinue) -and $pipeline.RuleCount -gt 0) {
            try {
                # Process pipeline objects
                $pipeline.Process($InputObject);
            }
            catch {
                $pipeline.Dispose();
                throw;
            }
        }
    }

    end {
        if ($Null -ne (Get-Variable -Name pipeline -ErrorAction SilentlyContinue)) {
            try
            {
                $pipeline.End();
            }
            finally
            {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Test-PSRuleTarget] END::";
    }
}

# .ExternalHelp PSRule-Help.xml
function Get-PSRule {

    [CmdletBinding()]
    [OutputType([PSRule.Rules.Rule])]
    param (
        # A list of paths to check for rule definitions
        [Parameter(Position = 0, Mandatory = $False)]
        [Alias('p')]
        [String[]]$Path = $PWD,

        # Filter to rules with the following names
        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable
    )

    begin {
        Write-Verbose -Message "[Get-PSRule]::BEGIN";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        # Get an options object
        $Option = New-PSRuleOption @optionParams;

        # Discover scripts in the specified paths
        $sourceParams = @{ };

        if ($PSBoundParameters.ContainsKey('Path')) {
            $sourceParams['Path'] = $Path;
        }
        if ($PSBoundParameters.ContainsKey('Module')) {
            $sourceParams['Module'] = $Module;
        }
        if ($PSBoundParameters.ContainsKey('ListAvailable')) {
            $sourceParams['ListAvailable'] = $ListAvailable;
        }
        if ($sourceParams.Count -eq 0) {
            $sourceParams['Path'] = $Path;
        }
        [PSRule.Rules.RuleSource[]]$sourceFiles = GetRuleScriptPath @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Verbose -Message "[Get-PSRule] -- Could not find any .Rule.ps1 script files in the path";
            return; # continue causes issues with Pester
        }

        Write-Verbose -Message "[Get-PSRule] -- Found $($sourceFiles.Length) script(s)";

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        if ($PSBoundParameters.ContainsKey('Name')) {
            $Option.Baseline.RuleName = $Name;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Get().Configure($Option);
        $builder.FilterBy($Tag);
        $builder.Source($sourceFiles);
        $builder.UseCommandRuntime($PSCmdlet.CommandRuntime);
        $builder.UseLoggingPreferences($ErrorActionPreference, $WarningPreference, $VerbosePreference, $InformationPreference);
        $pipeline = $builder.Build();
    }

    process {
        if ($Null -ne (Get-Variable -Name pipeline -ErrorAction SilentlyContinue)) {
            try {
                # Get matching rule definitions
                $pipeline.Process();
            }
            catch {
                $pipeline.Dispose();
                throw;
            }
        }
    }

    end {
        if ($Null -ne (Get-Variable -Name pipeline -ErrorAction SilentlyContinue)) {
            $pipeline.Dispose();
        }
        Write-Verbose -Message "[Get-PSRule]::END";
    }
}

# .ExternalHelp PSRule-Help.xml
function New-PSRuleOption {
    [CmdletBinding()]
    [OutputType([PSRule.Configuration.PSRuleOption])]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Creates an in memory object only')]
    param (
        [Parameter(Position = 0, Mandatory = $False)]
        [PSDefaultValue(Help = '.\ps-rule.yml')]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.BaselineConfiguration]$BaselineConfiguration,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.SuppressionOption]$SuppressTargetName,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.BindTargetName[]]$BindTargetName,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.BindTargetName[]]$BindTargetType,

        # Options

        # Sets the Binding.IgnoreCase option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingIgnoreCase = $True,

        # Sets the Binding.TargetName option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetName')]
        [String[]]$TargetName,

        # Sets the Binding.TargetType option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetType')]
        [String[]]$TargetType,

        # Sets the Execution.InconclusiveWarning option
        [Parameter(Mandatory = $False)]
        [Alias('ExecutionInconclusiveWarning')]
        [System.Boolean]$InconclusiveWarning = $True,
    
        # Sets the Execution.NotProcessedWarning option
        [Parameter(Mandatory = $False)]
        [Alias('ExecutionNotProcessedWarning')]
        [System.Boolean]$NotProcessedWarning = $True,

        # Sets the Input.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Detect')]
        [Alias('InputFormat')]
        [PSRule.Configuration.InputFormat]$Format = 'Detect',

        # Sets the Input.ObjectPath option
        [Parameter(Mandatory = $False)]
        [Alias('InputObjectPath')]
        [String]$ObjectPath = '',

        # Sets the Logging.RuleFail option
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.OutcomeLogStream]$LoggingRuleFail = 'None',

        # Sets the Logging.RulePass option
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.OutcomeLogStream]$LoggingRulePass = 'None',

        # Sets the Output.As option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Detail', 'Summary')]
        [PSRule.Configuration.ResultFormat]$OutputAs = 'Detail',

        # Sets the Output.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = 'None'
    )

    begin {
        Write-Verbose -Message "[New-PSRuleOption] BEGIN::";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };
        $optionParams += $PSBoundParameters;

        # Remove invalid parameters
        if ($optionParams.ContainsKey('Path')) {
            $optionParams.Remove('Path');
        }

        if ($optionParams.ContainsKey('Option')) {
            $optionParams.Remove('Option');
        }

        if ($optionParams.ContainsKey('BaselineConfiguration')) {
            $optionParams.Remove('BaselineConfiguration');
        }

        if ($optionParams.ContainsKey('SuppressTargetName')) {
            $optionParams.Remove('SuppressTargetName');
        }

        if ($optionParams.ContainsKey('BindTargetName')) {
            $optionParams.Remove('BindTargetName');
        }

        if ($optionParams.ContainsKey('BindTargetType')) {
            $optionParams.Remove('BindTargetType');
        }

        if ($PSBoundParameters.ContainsKey('Option')) {
            $Option = $Option.Clone();
        }
        elseif ($PSBoundParameters.ContainsKey('Path')) {
            Write-Verbose -Message "Attempting to read: $Path";
            $Option = [PSRule.Configuration.PSRuleOption]::FromFile($Path, $False);
        }
        else {
            Write-Verbose -Message "Attempting to read: $Path";
            $Option = [PSRule.Configuration.PSRuleOption]::FromFile($Path, $True);
        }
    }

    end {
        if ($PSBoundParameters.ContainsKey('BaselineConfiguration')) {
            $Option.Baseline.Configuration = $BaselineConfiguration;
        }

        if ($PSBoundParameters.ContainsKey('SuppressTargetName')) {
            $Option.Suppression = $SuppressTargetName;
        }

        if ($PSBoundParameters.ContainsKey('BindTargetName')) {
            Write-Verbose -Message 'Set BindTargetName pipeline hook';
            $Option.Pipeline.BindTargetName.AddRange($BindTargetName);
        }

        if ($PSBoundParameters.ContainsKey('BindTargetType')) {
            Write-Verbose -Message 'Set BindTargetType pipeline hook';
            $Option.Pipeline.BindTargetType.AddRange($BindTargetType);
        }

        # Options
        $Option | SetOptions @optionParams -Verbose:$VerbosePreference;

        Write-Verbose -Message "[Set-PSRuleOption] END::";
    }
}

# .ExternalHelp PSRule-Help.xml
function Set-PSRuleOption {
    [CmdletBinding(SupportsShouldProcess = $True)]
    [OutputType([PSRule.Configuration.PSRuleOption])]
    param (
        # The path to a YAML file where options will be set
        [Parameter(Position = 0, Mandatory = $False)]
        [PSDefaultValue(Help = '.\ps-rule.yml')]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [Switch]$PassThru = $False,

        # Force creation of directory path for Path parameter
        [Parameter(Mandatory = $False)]
        [Switch]$Force = $False,

        # Overwrite YAML files that contain comments
        [Parameter(Mandatory = $False)]
        [Switch]$Overwrite = $False,

        # Options

        # Sets the Binding.IgnoreCase option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingIgnoreCase = $True,

        # Sets the Binding.TargetName option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetName')]
        [String[]]$TargetName,

        # Sets the Binding.TargetType option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetType')]
        [String[]]$TargetType,

        # Sets the Execution.InconclusiveWarning option
        [Parameter(Mandatory = $False)]
        [Alias('ExecutionInconclusiveWarning')]
        [System.Boolean]$InconclusiveWarning = $True,
    
        # Sets the Execution.NotProcessedWarning option
        [Parameter(Mandatory = $False)]
        [Alias('ExecutionNotProcessedWarning')]
        [System.Boolean]$NotProcessedWarning = $True,

        # Sets the Input.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Detect')]
        [Alias('InputFormat')]
        [PSRule.Configuration.InputFormat]$Format = 'Detect',

        # Sets the Input.ObjectPath option
        [Parameter(Mandatory = $False)]
        [Alias('InputObjectPath')]
        [String]$ObjectPath = '',

        # Sets the Logging.RuleFail option
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.OutcomeLogStream]$LoggingRuleFail = 'None',

        # Sets the Logging.RulePass option
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.OutcomeLogStream]$LoggingRulePass = 'None',

        # Sets the Output.As option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Detail', 'Summary')]
        [PSRule.Configuration.ResultFormat]$OutputAs = 'Detail',

        # Sets the Output.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = 'None'
    )

    begin {
        Write-Verbose -Message "[Set-PSRuleOption] BEGIN::";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };
        $optionParams += $PSBoundParameters;

        # Remove invalid parameters
        if ($optionParams.ContainsKey('Path')) {
            $optionParams.Remove('Path');
        }

        if ($optionParams.ContainsKey('Option')) {
            $optionParams.Remove('Option');
        }

        if ($optionParams.ContainsKey('PassThru')) {
            $optionParams.Remove('PassThru');
        }

        if ($optionParams.ContainsKey('Force')) {
            $optionParams.Remove('Force');
        }

        if ($optionParams.ContainsKey('Overwrite')) {
            $optionParams.Remove('Overwrite');
        }

        # Build options object
        if ($PSBoundParameters.ContainsKey('Option')) {
            $Option = $Option.Clone();
        }
        else {
            Write-Verbose -Message "[Set-PSRuleOption] -- Attempting to read: $Path";
            $Option = [PSRule.Configuration.PSRuleOption]::FromFile($Path, $False);
        }
    }

    process {
        try {
            $result = $Option | SetOptions @optionParams;
            if ($PassThru) {
                $result;
            }
            else {
                $result.ToFile($Path);
            }
        }
        finally {

        }
    }

    end {
        Write-Verbose -Message "[Set-PSRuleOption] END::";
    }
}

#
# Keywords
#

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#rule
#>
function Rule {
    [CmdletBinding()]
    param (
        # The name of the rule
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Name,

        # The body of the rule
        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        [Parameter(Mandatory = $False)]
        [ScriptBlock]$If,

        [Parameter(Mandatory = $False)]
        [String[]]$Type,

        # Any dependencies for this rule
        [Parameter(Mandatory = $False)]
        [String[]]$DependsOn,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Configure
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'Rule keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#allof
#>
function AllOf {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [ScriptBlock]$Body
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'AllOf keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#anyof
#>
function AnyOf {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [ScriptBlock]$Body
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'AnyOf keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#exists
#>
function Exists {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String[]]$Field,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$Not = $False,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'Exists keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#match
#>
function Match {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String]$Field,

        [Parameter(Mandatory = $True, Position = 1)]
        [String[]]$Expression,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'Match keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#within
#>
function Within {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String]$Field,

        [Parameter(Mandatory = $True, Position = 1)]
        [PSObject[]]$AllowedValue,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'Within keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

<#
.LINK
https://berniewhite.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#typeof
#>
function TypeOf {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String[]]$TypeName,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message 'TypeOf keyword can only be called within PSRule. To call rules use Invoke-PSRule.' -Category InvalidOperation;
    }
}

#
# Helper functions
#

# Get a list of rule script files in the matching paths
function GetRuleScriptPath {

    [CmdletBinding()]
    [OutputType([PSRule.Rules.RuleSource])]
    param (
        [Parameter(Mandatory = $False)]
        [String[]]$Path,

        [Parameter(Mandatory = $False)]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable
    )

    process {
        $builder = New-Object -TypeName 'PSRule.Rules.RuleSourceBuilder';

        if ($PSBoundParameters.ContainsKey('Path')) {
            Write-Verbose -Message "[PSRule][D] -- Scanning for source files: $Path";
            $fileObjects = (Get-ChildItem -Path $Path -Recurse -File -Include '*.rule.ps1' -ErrorAction Stop);

            if ($Null -ne $fileObjects) {
                $builder.Add($fileObjects.FullName, $Null);
            }
        }

        $moduleParams = @{};

        if ($PSBoundParameters.ContainsKey('Module')) {
            $moduleParams['Name'] = $Module;
        }

        if ($PSBoundParameters.ContainsKey('ListAvailable')) {
            $moduleParams['ListAvailable'] = $ListAvailable.ToBool();
        }

        if ($moduleParams.Count -gt 0) {
            $modules = Microsoft.PowerShell.Core\Get-Module @moduleParams | Where-Object -FilterScript {
                'PSRule' -in $_.Tags
            }

            if ($Null -ne $modules) {
                foreach ($m in $modules) {
                    Write-Verbose -Message "[PSRule][D] -- Found module: $($m.Name)";
                    $fileObjects = (Get-ChildItem -Path $m.ModuleBase -Recurse -File -Include '*.rule.ps1' -ErrorAction Stop);

                    if ($Null -ne $fileObjects) {
                        $builder.Add($fileObjects.FullName, $m.Name);
                    }
                }
            }
        }

        $builder.Build();
    }
}

function GetFilePath {
    [CmdletBinding()]
    [OutputType([System.String])]
    param (
        [Parameter(Mandatory = $True)]
        [String[]]$Path
    )

    process {
        $builder = New-Object -TypeName 'PSRule.Pipeline.InputPathBuilder';
        Write-Verbose -Message "[PSRule][D] -- Scanning for input files: $Path";

        foreach ($p in $Path) {
            if ($p -notlike 'https://*' -and $p -notlike 'http://*') {
                if (Test-Path -Path $p -PathType Leaf) {
                    Resolve-Path -Path $p;
                }
                else {
                    $builder.Add((Get-ChildItem -Path $p -ErrorAction Ignore -Recurse -File).FullName);
                }
            }
            elseif (!$p.Contains('*')) {
                $builder.Add($p);
            }
            else {
                throw 'The path is not valid. Wildcards are not supported in URL input paths.';
            }
        }

        $builder.Build();
    }
}

function SetOptions {
    [CmdletBinding()]
    [OutputType([PSRule.Configuration.PSRuleOption])]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSRule.Configuration.PSRuleOption]$InputObject,

        # Options

        # Sets the Binding.IgnoreCase option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingIgnoreCase = $True,

        # Sets the Binding.TargetName option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetName')]
        [String[]]$TargetName,

        # Sets the Binding.TargetType option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetType')]
        [String[]]$TargetType,

        # Sets the Execution.InconclusiveWarning option
        [Parameter(Mandatory = $False)]
        [Alias('ExecutionInconclusiveWarning')]
        [System.Boolean]$InconclusiveWarning = $True,
    
        # Sets the Execution.NotProcessedWarning option
        [Parameter(Mandatory = $False)]
        [Alias('ExecutionNotProcessedWarning')]
        [System.Boolean]$NotProcessedWarning = $True,

        # Sets the Input.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Detect')]
        [Alias('InputFormat')]
        [PSRule.Configuration.InputFormat]$Format = 'Detect',

        # Sets the Input.ObjectPath option
        [Parameter(Mandatory = $False)]
        [Alias('InputObjectPath')]
        [String]$ObjectPath = '',

        # Sets the Logging.RuleFail option
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.OutcomeLogStream]$LoggingRuleFail = 'None',

        # Sets the Logging.RulePass option
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.OutcomeLogStream]$LoggingRulePass = 'None',

        # Sets the Output.As option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Detail', 'Summary')]
        [PSRule.Configuration.ResultFormat]$OutputAs = 'Detail',

        # Sets the Output.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = 'None'
    )

    process {
        # Options

        # Sets option Binding.IgnoreCase
        if ($PSBoundParameters.ContainsKey('BindingIgnoreCase')) {
            $Option.Binding.IgnoreCase = $BindingIgnoreCase;
        }

        # Sets option Binding.TargetName
        if ($PSBoundParameters.ContainsKey('TargetName')) {
            $Option.Binding.TargetName = $TargetName;
        }

        # Sets option Binding.TargetType
        if ($PSBoundParameters.ContainsKey('TargetType')) {
            $Option.Binding.TargetType = $TargetType;
        }

        # Sets option Execution.InconclusiveWarning
        if ($PSBoundParameters.ContainsKey('InconclusiveWarning')) {
            $Option.Execution.InconclusiveWarning = $InconclusiveWarning;
        }

        # Sets option Execution.NotProcessedWarning
        if ($PSBoundParameters.ContainsKey('NotProcessedWarning')) {
            $Option.Execution.NotProcessedWarning = $NotProcessedWarning;
        }

        # Sets option Input.Format
        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }

        # Sets option Input.ObjectPath
        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }

        # Sets option Logging.RuleFail
        if ($PSBoundParameters.ContainsKey('LoggingRuleFail')) {
            $Option.Logging.RuleFail = $LoggingRuleFail;
        }

        # Sets option Logging.RulePass
        if ($PSBoundParameters.ContainsKey('LoggingRulePass')) {
            $Option.Logging.RulePass = $LoggingRulePass;
        }

        # Sets option Output.As
        if ($PSBoundParameters.ContainsKey('OutputAs')) {
            $Option.Output.As = $OutputAs;
        }

        # Sets option Output.Format
        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }

        return $Option;
    }
}

function IsDeviceGuardEnabled {

    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (

    )

    process {

        if ((Get-Variable -Name IsMacOS -ErrorAction Ignore) -or (Get-Variable -Name IsLinux -ErrorAction Ignore)) {
            return $False;
        }

        # PowerShell 6.0.x does not support Device Guard
        if ($PSVersionTable.PSVersion -ge '6.0' -and $PSVersionTable.PSVersion -lt '6.1') {
            return $False;
        }

        return [System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy() -eq [System.Management.Automation.Security.SystemEnforcementMode]::Enforce;
    }
}

function InitEditorServices {

    [CmdletBinding()]
    param (

    )

    process {
        if ($Null -ne (Get-Variable -Name psEditor -ErrorAction Ignore)) {
            # Export keywords
            Export-ModuleMember -Function @(
                'AllOf'
                'AnyOf'
                'Exists'
                'Match'
                'TypeOf'
                'Within'
                'Hint'
            );

            # Export variables
            Export-ModuleMember -Variable @(
                'Configuration'
                'Rule'
                'TargetObject'
            );
        }
    }
}

#
# Editor services
#

# Define variables and types
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSObject]$Configuration = $Null;
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSRule.Runtime.Rule]$Rule = New-Object -TypeName 'PSRule.Runtime.Rule';
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSObject]$TargetObject = New-Object -TypeName 'PSObject';

InitEditorServices;

#
# Export module
#

Export-ModuleMember -Function @(
    'Rule'
    'Invoke-PSRule'
    'Test-PSRuleTarget'
    'Get-PSRule'
    'New-PSRuleOption'
    'Set-PSRuleOption'
)

# EOM
