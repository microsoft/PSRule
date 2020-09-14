# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# PSRule module
#

Set-StrictMode -Version latest;

[PSRule.Configuration.PSRuleOption]::UseExecutionContext($ExecutionContext);
[PSRule.Configuration.PSRuleOption]::UseCurrentCulture();

#
# Localization
#

Import-LocalizedData -BindingVariable LocalizedHelp -FileName 'PSRule.Resources.psd1' -ErrorAction SilentlyContinue;
if ($Null -eq (Get-Variable -Name LocalizedHelp -ErrorAction SilentlyContinue)) {
    Import-LocalizedData -BindingVariable LocalizedHelp -FileName 'PSRule.Resources.psd1' -UICulture 'en-US' -ErrorAction SilentlyContinue;
}

#
# Public functions
#

# .ExternalHelp PSRule-Help.xml
function Invoke-PSRule {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSShouldProcess', '', Justification = 'ShouldProcess is used within CSharp code.')]
    [CmdletBinding(DefaultParameterSetName = 'Input', SupportsShouldProcess = $True)]
    [OutputType([PSRule.Rules.RuleRecord])]
    [OutputType([PSRule.Rules.RuleSummaryRecord])]
    [OutputType([System.String])]
    param (
        [Parameter(Mandatory = $True, ParameterSetName = 'InputPath')]
        [Alias('f')]
        [String[]]$InputPath,

        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [PSRule.Rules.RuleOutcome]$Outcome = [PSRule.Rules.RuleOutcome]::Processed,

        [Parameter(Mandatory = $False)]
        [ValidateSet('Detail', 'Summary')]
        [PSRule.Configuration.ResultFormat]$As = [PSRule.Configuration.ResultFormat]::Detail,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'File', 'Detect')]
        [PSRule.Configuration.InputFormat]$Format = [PSRule.Configuration.InputFormat]::Detect,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'NUnit3', 'Csv', 'Wide')]
        [Alias('o')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = [PSRule.Configuration.OutputFormat]::None,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.BaselineOption]$Baseline,

        # A list of paths to check for rule definitions
        [Parameter(Mandatory = $False, Position = 0)]
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
        [String]$ObjectPath,

        [Parameter(Mandatory = $False)]
        [String[]]$TargetType,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Input')]
        [Alias('TargetObject')]
        [PSObject]$InputObject
    )

    begin {
        Write-Verbose -Message '[Invoke-PSRule] BEGIN::';
        $pipelineReady = $False;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] = $Option;
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
        $sourceParams['Option'] = $Option;
        [PSRule.Rules.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }
        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }
        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }
        if ($PSBoundParameters.ContainsKey('TargetType')) {
            $Option.Input.TargetType = $TargetType;
        }
        if ($PSBoundParameters.ContainsKey('As')) {
            $Option.Output.As = $As;
        }
        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }
        if ($PSBoundParameters.ContainsKey('Outcome')) {
            $Option.Output.Outcome = $Outcome;
        }
        if ($PSBoundParameters.ContainsKey('OutputPath')) {
            $Option.Output.Path = $OutputPath;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Invoke($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);
        $builder.Name($Name);
        $builder.Tag($Tag);
        $builder.UseBaseline($Baseline);

        if ($PSBoundParameters.ContainsKey('InputPath')) {
            $builder.InputPath($InputPath);
        }

        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $pipeline.RuleCount -gt 0;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    process {
        if ($pipelineReady) {
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
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message '[Invoke-PSRule] END::';
    }
}

# .ExternalHelp PSRule-Help.xml
function Test-PSRuleTarget {
    [CmdletBinding(DefaultParameterSetName = 'Input')]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Mandatory = $True, ParameterSetName = 'InputPath')]
        [Alias('f')]
        [String[]]$InputPath,

        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [PSRule.Rules.RuleOutcome]$Outcome = [PSRule.Rules.RuleOutcome]::Processed,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'File', 'Detect')]
        [PSRule.Configuration.InputFormat]$Format,

        # A list of paths to check for rule definitions
        [Parameter(Mandatory = $False, Position = 0)]
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
        [String]$ObjectPath,

        [Parameter(Mandatory = $False)]
        [String[]]$TargetType,

        [Parameter(Mandatory = $False)]
        [String]$Culture
    )

    begin {
        Write-Verbose -Message "[Test-PSRuleTarget] BEGIN::";
        $pipelineReady = $False;

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
        $sourceParams['Option'] = $Option;
        [PSRule.Rules.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }
        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }
        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }
        if ($PSBoundParameters.ContainsKey('TargetType')) {
            $Option.Input.TargetType = $TargetType;
        }
        if ($PSBoundParameters.ContainsKey('Outcome')) {
            $Option.Output.Outcome = $Outcome;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Test($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);
        $builder.Name($Name);
        $builder.Tag($Tag);

        if ($PSBoundParameters.ContainsKey('InputPath')) {
            $builder.InputPath($InputPath);
        }

        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $pipeline.RuleCount -gt 0;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    process {
        if ($pipelineReady) {
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
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Test-PSRuleTarget] END::";
    }
}

# .ExternalHelp PSRule-Help.xml
function Get-PSRuleTarget {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSShouldProcess', '', Justification = 'ShouldProcess is used within CSharp code.')]
    [CmdletBinding(DefaultParameterSetName = 'Input', SupportsShouldProcess = $True)]
    [OutputType([PSObject])]
    param (
        [Parameter(Mandatory = $True, ParameterSetName = 'InputPath')]
        [Alias('f')]
        [String[]]$InputPath,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'File', 'Detect')]
        [PSRule.Configuration.InputFormat]$Format = [PSRule.Configuration.InputFormat]::Detect,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [String]$ObjectPath,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Input')]
        [Alias('TargetObject')]
        [PSObject]$InputObject
    )
    begin {
        Write-Verbose -Message '[Get-PSRuleTarget] BEGIN::';
        $pipelineReady = $False;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] = $Option;
        }

        # Get an options object
        $Option = New-PSRuleOption @optionParams;

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }
        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }
        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }
        if ($PSBoundParameters.ContainsKey('TargetType')) {
            $Option.Input.TargetType = $TargetType;
        }
        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }
        if ($PSBoundParameters.ContainsKey('OutputPath')) {
            $Option.Output.Path = $OutputPath;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::GetTarget($Option, $PSCmdlet, $ExecutionContext);

        if ($PSBoundParameters.ContainsKey('InputPath')) {
            $builder.InputPath($InputPath);
        }

        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $True;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    process {
        if ($pipelineReady) {
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
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message '[Get-PSRuleTarget] END::';
    }
}

# .ExternalHelp PSRule-Help.xml
function Assert-PSRule {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSShouldProcess', '', Justification = 'ShouldProcess is used within CSharp code.')]
    [CmdletBinding(DefaultParameterSetName = 'Input', SupportsShouldProcess = $True)]
    [OutputType([System.String])]
    param (
        [Parameter(Mandatory = $True, ParameterSetName = 'InputPath')]
        [Alias('f')]
        [String[]]$InputPath,

        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'File', 'Detect')]
        [PSRule.Configuration.InputFormat]$Format = [PSRule.Configuration.InputFormat]::Detect,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.BaselineOption]$Baseline,

        [Parameter(Mandatory = $False)]
        [ValidateSet('Client', 'Plain', 'AzurePipelines', 'GitHubActions')]
        [PSRule.Configuration.OutputStyle]$Style = [PSRule.Configuration.OutputStyle]::Client,

        # A list of paths to check for rule definitions
        [Parameter(Mandatory = $False, Position = 0)]
        [Alias('p')]
        [String[]]$Path = $PWD,

        # Filter to rules with the following names
        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'NUnit3', 'Csv')]
        [Alias('o')]
        [PSRule.Configuration.OutputFormat]$OutputFormat,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [String]$ObjectPath,

        [Parameter(Mandatory = $False)]
        [String[]]$TargetType,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Input')]
        [Alias('TargetObject')]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [String]$ResultVariable
    )
    begin {
        Write-Verbose -Message '[Assert-PSRule] BEGIN::';
        $pipelineReady = $False;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] = $Option;
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
        $sourceParams['Option'] = $Option;
        [PSRule.Rules.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }
        if ($PSBoundParameters.ContainsKey('Format')) {
            $Option.Input.Format = $Format;
        }
        if ($PSBoundParameters.ContainsKey('ObjectPath')) {
            $Option.Input.ObjectPath = $ObjectPath;
        }
        if ($PSBoundParameters.ContainsKey('TargetType')) {
            $Option.Input.TargetType = $TargetType;
        }
        if ($PSBoundParameters.ContainsKey('Style')) {
            $Option.Output.Style = $Style;
        }
        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }
        if ($PSBoundParameters.ContainsKey('OutputPath')) {
            $Option.Output.Path = $OutputPath;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Assert($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);;
        $builder.Name($Name);
        $builder.Tag($Tag);
        $builder.UseBaseline($Baseline);
        $builder.ResultVariable($ResultVariable);

        if ($PSBoundParameters.ContainsKey('InputPath')) {
            $builder.InputPath($InputPath);
        }

        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $pipeline.RuleCount -gt 0;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    process {
        if ($pipelineReady) {
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
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message '[Assert-PSRule] END::';
    }
}

# .ExternalHelp PSRule-Help.xml
function Get-PSRule {
    [CmdletBinding()]
    [OutputType([PSRule.Rules.Rule])]
    param (
        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable,

        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Wide')]
        [Alias('o')]
        [PSRule.Configuration.OutputFormat]$OutputFormat,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.BaselineOption]$Baseline,

        # A list of paths to check for rule definitions
        [Parameter(Mandatory = $False, Position = 0)]
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
        [String]$Culture,

        [Parameter(Mandatory = $False)]
        [Switch]$IncludeDependencies
    )
    begin {
        Write-Verbose -Message "[Get-PSRule]::BEGIN";
        $pipelineReady = $False;

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
        $sourceParams['Option'] = $Option;
        [PSRule.Rules.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Verbose -Message "[Get-PSRule] -- Could not find any .Rule.ps1 script files in the path";
            return; # continue causes issues with Pester
        }

        Write-Verbose -Message "[Get-PSRule] -- Found $($sourceFiles.Length) source file(s)";

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }
        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Get($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);
        $builder.Name($Name);
        $builder.Tag($Tag);
        $builder.UseBaseline($Baseline);

        if ($IncludeDependencies) {
            $builder.IncludeDependencies();
        }

        # $builder.UseCommandRuntime($PSCmdlet);
        # $builder.UseExecutionContext($ExecutionContext);
        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $True;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    end {
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Get-PSRule]::END";
    }
}

# .ExternalHelp PSRule-Help.xml
function Get-PSRuleBaseline {
    [CmdletBinding()]
    [OutputType([PSRule.Definitions.Baseline])]
    param (
        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable,

        [Parameter(Mandatory = $False, Position = 0)]
        [Alias('p')]
        [String[]]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [SupportsWildcards()]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [String]$Culture
    )
    begin {
        Write-Verbose -Message "[Get-PSRuleBaseline] BEGIN::";
        $pipelineReady = $False;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] = $Option;
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
        $sourceParams['Option'] = $Option;
        [PSRule.Rules.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Verbose -Message "[Get-PSRuleBaseline] -- Could not find any .Rule.ps1 script files in the path";
            return; # continue causes issues with Pester
        }

        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::GetBaseline($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);;
        $builder.Name($Name);
        # $builder.UseCommandRuntime($PSCmdlet);
        # $builder.UseExecutionContext($ExecutionContext);
        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $True;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    end {
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message '[Get-PSRuleBaseline] END::';
    }
}

# .ExternalHelp PSRule-Help.xml
function Get-PSRuleHelp {
    [CmdletBinding()]
    [OutputType([PSRule.Rules.RuleHelpInfo])]
    param (
        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$Online = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$Full = $False,

        # The name of the rule to get documentation for.
        [Parameter(Position = 0, Mandatory = $False)]
        [Alias('n')]
        [SupportsWildcards()]
        [String]$Name,

        # A path to check documentation for.
        [Parameter(Mandatory = $False)]
        [Alias('p')]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [String]$Culture
    )

    begin {
        Write-Verbose -Message "[Get-PSRuleHelp]::BEGIN";
        $pipelineReady = $False;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        # Get an options object
        $Option = New-PSRuleOption @optionParams;

        # Discover scripts in the specified paths
        $sourceParams = @{ };
        $sourceParams['PreferModule'] = $True;
        $sourceParams['PreferPath'] = $True;

        if ($PSBoundParameters.ContainsKey('Path')) {
            $sourceParams['Path'] = $Path;
            $sourceParams['PreferModule'] = $False;
        }
        if ($PSBoundParameters.ContainsKey('Module')) {
            $sourceParams['Module'] = $Module;
            $sourceParams['PreferPath'] = $False;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $sourceParams['Culture'] = $Culture;
        }
        if ($sourceParams['PreferPath']) {
            $sourceParams['Path'] = $Path;
        }
        $sourceParams['Option'] = $Option;
        [PSRule.Rules.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Verbose -Message "[Get-PSRuleHelp] -- Could not find any .Rule.ps1 script files in the path";
            return; # continue causes issues with Pester
        }

        Write-Verbose -Message "[Get-PSRuleHelp] -- Found $($sourceFiles.Length) source file(s)";

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }
        if ($PSBoundParameters.ContainsKey('Name')) {
            $Option.Rule.Include = $Name;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::GetHelp($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);;

        if ($Online) {
            $builder.Online();
        }
        if ($Full) {
            $builder.Full();
        }

        # $builder.UseCommandRuntime($PSCmdlet);
        # $builder.UseExecutionContext($ExecutionContext);
        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $True;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }

    end {
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Get-PSRuleHelp]::END";
    }
}

# .ExternalHelp PSRule-Help.xml
function New-PSRuleOption {
    [CmdletBinding(DefaultParameterSetName = 'FromPath')]
    [OutputType([PSRule.Configuration.PSRuleOption])]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Creates an in memory object only')]
    param (
        [Parameter(Position = 0, Mandatory = $False, ParameterSetName = 'FromPath')]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $True, ParameterSetName = 'FromOption')]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $True, ParameterSetName = 'FromDefault')]
        [Switch]$Default,

        [Parameter(Mandatory = $False)]
        [Alias('BaselineConfiguration')]
        [PSRule.Configuration.ConfigurationOption]$Configuration,

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

        # Sets the Binding.Field option
        [Parameter(Mandatory = $False)]
        [Hashtable]$BindingField,

        # Sets the Binding.NameSeparator option
        [Parameter(Mandatory = $False)]
        [String]$BindingNameSeparator = '/',

        # Sets the Binding.TargetName option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetName')]
        [String[]]$TargetName,

        # Sets the Binding.TargetType option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetType')]
        [String[]]$TargetType,

        # Sets the Binding.UseQualifiedName option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingUseQualifiedName = $False,

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
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'Detect')]
        [Alias('InputFormat')]
        [PSRule.Configuration.InputFormat]$Format = 'Detect',

        # Sets the Input.ObjectPath option
        [Parameter(Mandatory = $False)]
        [Alias('InputObjectPath')]
        [String]$ObjectPath = '',

        # Sets the Input.TargetType option
        [Parameter(Mandatory = $False)]
        [String[]]$InputTargetType,

        # Sets the Input.PathIgnore option
        [Parameter(Mandatory = $False)]
        [String[]]$InputPathIgnore = '',

        # Sets the Logging.LimitDebug option
        [Parameter(Mandatory = $False)]
        [String[]]$LoggingLimitDebug = $Null,

        # Sets the Logging.LimitVerbose option
        [Parameter(Mandatory = $False)]
        [String[]]$LoggingLimitVerbose = $Null,

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

        # Sets the Output.Culture option
        [Parameter(Mandatory = $False)]
        [String[]]$OutputCulture,

        # Sets the Output.Encoding option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Default', 'UTF8', 'UTF7', 'Unicode', 'UTF32', 'ASCII')]
        [PSRule.Configuration.OutputEncoding]$OutputEncoding = 'Default',

        # Sets the Output.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3', 'Csv')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = 'None',

        # Sets the Output.Outcome option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Fail', 'Pass', 'Error', 'Processed', 'All')]
        [Alias('Outcome')]
        [PSRule.Rules.RuleOutcome]$OutputOutcome = 'Processed',

        # Sets the Output.Path option
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = '',

        # Sets the Output.Style option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Client', 'Plain', 'AzurePipelines', 'GitHubActions')]
        [PSRule.Configuration.OutputStyle]$OutputStyle = [PSRule.Configuration.OutputStyle]::Client
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
        if ($optionParams.ContainsKey('Default')) {
            $optionParams.Remove('Default');
        }
        if ($optionParams.ContainsKey('Verbose')) {
            $optionParams.Remove('Verbose');
        }
        if ($optionParams.ContainsKey('Configuration')) {
            $optionParams.Remove('Configuration');
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
            $Option = [PSRule.Configuration.PSRuleOption]::FromFileOrEmpty($Option, $Path);
        }
        elseif ($PSBoundParameters.ContainsKey('Path')) {
            Write-Verbose -Message "Attempting to read: $Path";
            $Option = [PSRule.Configuration.PSRuleOption]::FromFile($Path);
        }
        elseif ($PSBoundParameters.ContainsKey('Default')) {
            $Option = [PSRule.Configuration.PSRuleOption]::FromDefault();
        }
        else {
            Write-Verbose -Message "Attempting to read: $Path";
            $Option = [PSRule.Configuration.PSRuleOption]::FromFileOrEmpty($Option, $Path);
        }
    }

    end {
        if ($PSBoundParameters.ContainsKey('Configuration')) {
            $Option.Configuration = $Configuration;
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

        Write-Verbose -Message "[New-PSRuleOption] END::";
    }
}

# .ExternalHelp PSRule-Help.xml
function Set-PSRuleOption {
    [CmdletBinding(SupportsShouldProcess = $True)]
    [OutputType([PSRule.Configuration.PSRuleOption])]
    param (
        # The path to a YAML file where options will be set
        [Parameter(Position = 0, Mandatory = $False)]
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
        [Switch]$AllowClobber = $False,

        # Options

        # Sets the Binding.IgnoreCase option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingIgnoreCase = $True,

        # Sets the Binding.Field option
        [Parameter(Mandatory = $False)]
        [Hashtable]$BindingField,

        # Sets the Binding.NameSeparator option
        [Parameter(Mandatory = $False)]
        [String]$BindingNameSeparator = '/',

        # Sets the Binding.TargetName option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetName')]
        [String[]]$TargetName,

        # Sets the Binding.TargetType option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetType')]
        [String[]]$TargetType,

        # Sets the Binding.UseQualifiedName option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingUseQualifiedName = $False,

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
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'Detect')]
        [Alias('InputFormat')]
        [PSRule.Configuration.InputFormat]$Format = 'Detect',

        # Sets the Input.ObjectPath option
        [Parameter(Mandatory = $False)]
        [Alias('InputObjectPath')]
        [String]$ObjectPath = '',

        # Sets the Input.PathIgnore option
        [Parameter(Mandatory = $False)]
        [String[]]$InputPathIgnore = '',

        # Sets the Input.TargetType option
        [Parameter(Mandatory = $False)]
        [String[]]$InputTargetType,

        # Sets the Logging.LimitDebug option
        [Parameter(Mandatory = $False)]
        [String[]]$LoggingLimitDebug = $Null,

        # Sets the Logging.LimitVerbose option
        [Parameter(Mandatory = $False)]
        [String[]]$LoggingLimitVerbose = $Null,

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

        # Sets the Output.Culture option
        [Parameter(Mandatory = $False)]
        [String[]]$OutputCulture,

        # Sets the Output.Encoding option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Default', 'UTF8', 'UTF7', 'Unicode', 'UTF32', 'ASCII')]
        [PSRule.Configuration.OutputEncoding]$OutputEncoding = 'Default',

        # Sets the Output.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3', 'Csv')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = 'None',

        # Sets the Output.Outcome option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Fail', 'Pass', 'Error', 'Processed', 'All')]
        [Alias('Outcome')]
        [PSRule.Rules.RuleOutcome]$OutputOutcome = 'Processed',

        # Sets the Output.Path option
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = '',

        # Sets the Output.Style option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Client', 'Plain', 'AzurePipelines', 'GitHubActions')]
        [PSRule.Configuration.OutputStyle]$OutputStyle = [PSRule.Configuration.OutputStyle]::Client
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

        if ($optionParams.ContainsKey('AllowClobber')) {
            $optionParams.Remove('AllowClobber');
        }

        if ($optionParams.ContainsKey('WhatIf')) {
            $optionParams.Remove('WhatIf');
        }

        if ($optionParams.ContainsKey('Confirm')) {
            $optionParams.Remove('Confirm');
        }

        if ($optionParams.ContainsKey('Verbose')) {
            $optionParams.Remove('Verbose');
        }

        # Build options object
        if ($PSBoundParameters.ContainsKey('Option')) {
            $Option = $Option.Clone();
        }
        else {
            Write-Verbose -Message "[Set-PSRuleOption] -- Attempting to read: $Path";
            $Option = [PSRule.Configuration.PSRuleOption]::FromFileOrEmpty($Path);
        }

        $filePath = [PSRule.Configuration.PSRuleOption]::GetFilePath($Path);
        $containsComments = YamlContainsComments -Path $filePath;
    }
    process {
        try {
            $result = $Option | SetOptions @optionParams -Verbose:$VerbosePreference;
            if ($PassThru) {
                $result;
            }
            elseif ($containsComments -and !$AllowClobber) {
                Write-Error -Message $LocalizedHelp.YamlContainsComments -Category ResourceExists -ErrorId 'PSRule.PSRuleOption.YamlContainsComments';
            }
            else {
                $parentPath = Split-Path -Path $filePath -Parent;
                if (!(Test-Path -Path $parentPath)) {
                    if ($Force) {
                        if ($PSCmdlet.ShouldProcess('Create directory', $parentPath)) {
                            $Null = New-Item -Path $parentPath -ItemType Directory -Force;
                        }
                    }
                    else {
                        Write-Error -Message $LocalizedHelp.PathNotFound -Category ObjectNotFound -ErrorId 'PSRule.PSRuleOption.ParentPathNotFound';
                    }
                }
                if ($PSCmdlet.ShouldProcess('Write options to file', $filePath)) {
                    $result.ToFile($Path);
                }
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

#region Keywords

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#rule
#>
function Rule {
    [CmdletBinding()]
    [OutputType([void])]
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
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#allof
#>
function AllOf {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [ScriptBlock]$Body
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#anyof
#>
function AnyOf {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [ScriptBlock]$Body
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#exists
#>
function Exists {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String[]]$Field,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$Not = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$All = $False,

        [Parameter(Mandatory = $False)]
        [String]$Reason,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#match
#>
function Match {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String]$Field,

        [Parameter(Mandatory = $True, Position = 1)]
        [String[]]$Expression,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$Not = $False,

        [Parameter(Mandatory = $False)]
        [String]$Reason,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#within
#>
function Within {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String]$Field,

        [Parameter(Mandatory = $False)]
        [Switch]$Not = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$Like = $False,

        [Parameter(Mandatory = $True, Position = 1)]
        [Alias('AllowedValue')]
        [PSObject[]]$Value,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False,

        [Parameter(Mandatory = $False)]
        [String]$Reason,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#typeof
#>
function TypeOf {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True, Position = 0)]
        [String[]]$TypeName,

        [Parameter(Mandatory = $False)]
        [String]$Reason,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#reason
#>
function Reason {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $False, Position = 0)]
        [String]$Text
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

<#
.LINK
https://microsoft.github.io/PSRule/keywords/PSRule/en-US/about_PSRule_Keywords.html#recommend
#>
function Recommend {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $False, Position = 0)]
        [String]$Text
    )

    begin {
        # This is just a stub to improve rule authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

#endregion Keywords

#
# Helper functions
#

# Get a list of rule script files in the matching paths
function GetSource {
    [CmdletBinding()]
    [OutputType([PSRule.Rules.Source])]
    param (
        [Parameter(Mandatory = $False)]
        [String[]]$Path,

        [Parameter(Mandatory = $False)]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable,

        [Parameter(Mandatory = $False)]
        [String]$Culture,

        [Parameter(Mandatory = $False)]
        [Switch]$PreferPath = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$PreferModule = $False,

        [Parameter(Mandatory = $True)]
        [PSRule.Configuration.PSRuleOption]$Option
    )
    process {
        $builder = [PSRule.Pipeline.PipelineBuilder]::RuleSource($Option, $PSCmdlet, $ExecutionContext);
        if ($PSBoundParameters.ContainsKey('Path')) {
            try {
                $builder.Directory($Path);
            }
            catch {
                throw $_.Exception.GetBaseException();
            }
        }

        $moduleParams = @{};
        if ($PSBoundParameters.ContainsKey('Module')) {
            $moduleParams['Name'] = $Module;

            # Determine if module should be automatically loaded
            if (GetAutoloadPreference) {
                foreach ($m in $Module) {
                    if ($Null -eq (GetRuleModule -Name $m)) {
                        LoadModule -Name $m -Verbose:$VerbosePreference;
                    }
                }
            }
        }

        if ($PSBoundParameters.ContainsKey('ListAvailable')) {
            $moduleParams['ListAvailable'] = $ListAvailable.ToBool();
        }

        if ($moduleParams.Count -gt 0 -or $PreferModule) {
            $modules = @(GetRuleModule @moduleParams);
            $builder.Module($modules);
        }
        $builder.Build();
    }
}

function GetAutoloadPreference {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param ()
    process {
        $v = Microsoft.PowerShell.Utility\Get-Variable -Name 'PSModuleAutoLoadingPreference' -ErrorAction SilentlyContinue;
        return ($Null -eq $v) -or ($v.Value -eq [System.Management.Automation.PSModuleAutoLoadingPreference]::All);
    }
}

function GetRuleModule {
    [CmdletBinding()]
    [OutputType([System.Management.Automation.PSModuleInfo])]
    param (
        [Parameter(Mandatory = $False)]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable = $False
    )
    process {
        $moduleResults = (Microsoft.PowerShell.Core\Get-Module @PSBoundParameters | Microsoft.PowerShell.Core\Where-Object -FilterScript {
            'PSRule' -in $_.Tags -or 'PSRule-rules' -in $_.Tags
        } | Microsoft.PowerShell.Utility\Group-Object -Property Name)

        if ($Null -ne $moduleResults) {
            foreach ($m in $moduleResults) {
                @($m.Group | Microsoft.PowerShell.Utility\Sort-Object -Descending -Property Version)[0];
            }
        }
    }
}

function LoadModule {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Name
    )
    process{
        $Null = GetRuleModule -Name $Name -ListAvailable | Microsoft.PowerShell.Core\Import-Module -Global;
    }
}

function GetFilePath {
    [CmdletBinding()]
    [OutputType([System.String])]
    param (
        [Parameter(Mandatory = $True)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $True)]
        [String[]]$Path
    )
    process {
        $builder = [PSRule.Pipeline.InputPathBuilder]::new();
        Write-Verbose -Message "[PSRule][D] -- Scanning for input files: $Path";

        foreach ($p in $Path) {
            if ($p -notlike 'https://*' -and $p -notlike 'http://*') {
                if ([System.IO.Path]::HasExtension($p)) {
                    $items = [System.Management.Automation.PathInfo[]]@(Resolve-Path -Path $p);
                    $builder.Add($items);
                }
                else {
                    $builder.Add([System.IO.FileInfo[]]@(Get-ChildItem -Path $p -ErrorAction Ignore -Recurse -File));
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

        # Sets the Binding.Field option
        [Parameter(Mandatory = $False)]
        [Hashtable]$BindingField,

        # Sets the Binding.NameSeparator option
        [Parameter(Mandatory = $False)]
        [String]$BindingNameSeparator = '/',

        # Sets the Binding.TargetName option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetName')]
        [String[]]$TargetName,

        # Sets the Binding.TargetType option
        [Parameter(Mandatory = $False)]
        [Alias('BindingTargetType')]
        [String[]]$TargetType,

        # Sets the Binding.UseQualifiedName option
        [Parameter(Mandatory = $False)]
        [System.Boolean]$BindingUseQualifiedName = $False,

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
        [ValidateSet('None', 'Yaml', 'Json', 'Markdown', 'PowerShellData', 'Detect')]
        [Alias('InputFormat')]
        [PSRule.Configuration.InputFormat]$Format = 'Detect',

        # Sets the Input.ObjectPath option
        [Parameter(Mandatory = $False)]
        [Alias('InputObjectPath')]
        [String]$ObjectPath = '',

        # Sets the Input.PathIgnore option
        [Parameter(Mandatory = $False)]
        [String[]]$InputPathIgnore = '',

        # Sets the Input.TargetType option
        [Parameter(Mandatory = $False)]
        [String[]]$InputTargetType,

        # Sets the Logging.LimitDebug option
        [Parameter(Mandatory = $False)]
        [String[]]$LoggingLimitDebug = $Null,

        # Sets the Logging.LimitVerbose option
        [Parameter(Mandatory = $False)]
        [String[]]$LoggingLimitVerbose = $Null,

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

        # Sets the Output.Culture option
        [Parameter(Mandatory = $False)]
        [String[]]$OutputCulture,

        # Sets the Output.Encoding option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Default', 'UTF8', 'UTF7', 'Unicode', 'UTF32', 'ASCII')]
        [PSRule.Configuration.OutputEncoding]$OutputEncoding = 'Default',

        # Sets the Output.Format option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Yaml', 'Json', 'NUnit3')]
        [PSRule.Configuration.OutputFormat]$OutputFormat = 'None',

        # Sets the Output.Outcome option
        [Parameter(Mandatory = $False)]
        [ValidateSet('None', 'Fail', 'Pass', 'Error', 'Processed', 'All')]
        [Alias('Outcome')]
        [PSRule.Rules.RuleOutcome]$OutputOutcome = 'Processed',

        # Sets the Output.Path option
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = '',

        # Sets the Output.Style option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Client', 'Plain', 'AzurePipelines', 'GitHubActions')]
        [PSRule.Configuration.OutputStyle]$OutputStyle = [PSRule.Configuration.OutputStyle]::Client
    )
    process {
        # Options

        # Sets option Binding.IgnoreCase
        if ($PSBoundParameters.ContainsKey('BindingIgnoreCase')) {
            $Option.Binding.IgnoreCase = $BindingIgnoreCase;
        }

        # Sets option Binding.Field
        if ($PSBoundParameters.ContainsKey('BindingField')) {
            $Option.Binding.Field = $BindingField;
        }

         # Sets option Binding.NameSeparator
         if ($PSBoundParameters.ContainsKey('BindingNameSeparator')) {
            $Option.Binding.NameSeparator = $BindingNameSeparator;
        }

        # Sets option Binding.TargetName
        if ($PSBoundParameters.ContainsKey('TargetName')) {
            $Option.Binding.TargetName = $TargetName;
        }

        # Sets option Binding.TargetType
        if ($PSBoundParameters.ContainsKey('TargetType')) {
            $Option.Binding.TargetType = $TargetType;
        }

         # Sets option Binding.UseQualifiedName
         if ($PSBoundParameters.ContainsKey('BindingUseQualifiedName')) {
            $Option.Binding.UseQualifiedName = $BindingUseQualifiedName;
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

        # Sets option Input.PathIgnore
        if ($PSBoundParameters.ContainsKey('InputPathIgnore')) {
            $Option.Input.PathIgnore = $InputPathIgnore;
        }

        # Sets option Input.TargetType
        if ($PSBoundParameters.ContainsKey('InputTargetType')) {
            $Option.Input.TargetType = $InputTargetType;
        }

        # Sets option Logging.LimitDebug
        if ($PSBoundParameters.ContainsKey('LoggingLimitDebug')) {
            $Option.Logging.LimitDebug = $LoggingLimitDebug;
        }

        # Sets option Logging.LimitVerbose
        if ($PSBoundParameters.ContainsKey('LoggingLimitVerbose')) {
            $Option.Logging.LimitVerbose = $LoggingLimitVerbose;
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

        # Sets option Output.Culture
        if ($PSBoundParameters.ContainsKey('OutputCulture')) {
            $Option.Output.Culture = $OutputCulture;
        }

        # Sets option Output.Encoding
        if ($PSBoundParameters.ContainsKey('OutputEncoding')) {
            $Option.Output.Encoding = $OutputEncoding;
        }

        # Sets option Output.Format
        if ($PSBoundParameters.ContainsKey('OutputFormat')) {
            $Option.Output.Format = $OutputFormat;
        }

        # Sets option Output.Outcome
        if ($PSBoundParameters.ContainsKey('OutputOutcome')) {
            $Option.Output.Outcome = $OutputOutcome;
        }

        # Sets option Output.Path
        if ($PSBoundParameters.ContainsKey('OutputPath')) {
            $Option.Output.Path = $OutputPath;
        }

        # Sets option Output.Style
        if ($PSBoundParameters.ContainsKey('OutputStyle')) {
            $Option.Output.Style = $OutputStyle;
        }

        return $Option;
    }
}

function YamlContainsComments {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        if (!(Test-Path -Path $Path)) {
            return $False;
        }
        return (Get-Content -Path $Path -Raw) -match '(?:(^[ \t]*)|[ \t]+|)(?=\#|^\#)';
    }
}

function IsDeviceGuardEnabled {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param ()
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
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidGlobalAliases', '', Justification = 'Alias is used for editor discovery only.', Scope = 'Function')]
    [CmdletBinding()]
    param ()

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
                'Reason'
                'Recommend'
            );

            # Export variables
            Export-ModuleMember -Variable @(
                'Assert'
                'Configuration'
                'LocalizedData'
                'PSRule'
                'Rule'
                'TargetObject'
            );
        }
    }
}

function InitCompletionServices {
    [CmdletBinding()]
    param ()
    process {
        # Complete -Module parameter
        Register-ArgumentCompleter -CommandName Assert-PSRule,Get-PSRule,Get-PSRuleBaseline,Get-PSRuleHelp,Invoke-PSRule,Test-PSRuleTarget -ParameterName Module -ScriptBlock {
            param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
            GetRuleModule -Name "$wordToComplete*" -ListAvailable | ForEach-Object -Process {
                [System.Management.Automation.CompletionResult]::new($_.Name, $_.Name, 'ParameterValue', ([String]::Concat("ModuleName: ", $_.Name, ", ModuleVersion: ", $_.Version.ToString())))
            }
        }
    }
}

#
# Editor services
#

# Define variables and types
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSRule.Runtime.Assert]$Assert = New-Object -TypeName 'PSRule.Runtime.Assert';
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSObject]$Configuration = $Null;
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSRule.Runtime.PSRule]$PSRule = New-Object -TypeName 'PSRule.Runtime.PSRule';
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSRule.Runtime.Rule]$Rule = New-Object -TypeName 'PSRule.Runtime.Rule';
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSObject]$TargetObject = New-Object -TypeName 'PSObject';
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignment', '', Justification = 'Variable is used for editor discovery only.')]
[PSObject]$LocalizedData = $Null;

InitEditorServices;
InitCompletionServices;

#
# Export module
#

Export-ModuleMember -Function @(
    'Rule'
    'Invoke-PSRule'
    'Test-PSRuleTarget'
    'Get-PSRuleTarget'
    'Assert-PSRule'
    'Get-PSRule'
    'Get-PSRuleHelp'
    'Get-PSRuleBaseline'
    'New-PSRuleOption'
    'Set-PSRuleOption'
)

# EOM
