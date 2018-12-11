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

[PSRule.Configuration.PSRuleOption]::GetWorkingPath = {
    return Get-Location;
}

#
# Localization
#

# $LocalizedData = data {

# }

Import-LocalizedData -BindingVariable LocalizedData -FileName 'PSRule.Resources.psd1' -ErrorAction SilentlyContinue;

#
# Public functions
#

function Invoke-PSRule {

    [CmdletBinding()]
    [OutputType([PSRule.Rules.RuleRecord])]
    [OutputType([PSRule.Rules.RuleSummaryRecord])]
    param (
        [Parameter(Position = 0)]
        [String[]]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [PSRule.Rules.RuleOutcome]$Outcome = [PSRule.Rules.RuleOutcome]::Default,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.ResultFormat]$As = [PSRule.Configuration.ResultFormat]::Default
    )

    begin {
        Write-Verbose -Message "[PSRule] BEGIN::";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        # Get an options object
        $Option = New-PSRuleOption @optionParams;

        Write-Verbose -Message "[PSRule][D] -- Scanning for source files: $Path";

        # Discover scripts in the specified paths
        [String[]]$sourceFiles = GetRuleScriptPath -Path $Path -Verbose:$VerbosePreference;

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Invoke();
        $builder.FilterBy($Name, $Tag);
        $builder.Source($sourceFiles);
        $builder.Option($Option);
        $builder.Limit($Outcome);
        $builder.As($As);
        $builder.UseCommandRuntime($PSCmdlet.CommandRuntime);
        $pipeline = $builder.Build();
    }

    process {
        $pipeline.Process($InputObject);
    }

    end {
        if ($As -eq [PSRule.Configuration.ResultFormat]::Summary) {
            $pipeline.GetSummary();
        }

        Write-Verbose -Message "[PSRule] END::";
    }
}

# Get a list of rules
function Get-PSRule {

    [CmdletBinding()]
    [OutputType([PSRule.Rules.Rule])]
    param (
        # A list of deployments to run by name
        [Parameter(Mandatory = $False)]
        [SupportsWildcards()]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag,

        # A list of paths to check for deployments
        [Parameter(Position = 0, Mandatory = $False)]
        [String[]]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option
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
        [String[]]$sourceFiles = GetRuleScriptPath -Path $Path -Verbose:$VerbosePreference;

        Write-Verbose -Message "[Get-PSRule] -- Found $($sourceFiles.Length) script(s)";
        Write-Debug -Message "[Get-PSRule] -- Found scripts: $([String]::Join(' ', $sourceFiles))";

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        $builder = [PSRule.Pipeline.PipelineBuilder]::Get();
        $builder.FilterBy($Name, $Tag);
        $builder.Source($sourceFiles);
        $builder.Option($Option);
        $builder.UseCommandRuntime($PSCmdlet.CommandRuntime);
        $pipeline = $builder.Build();
    }

    process {
        # Get matching rule definitions
        $pipeline.Process();
        # GetRule -Path $includePaths -Option $Option -Filter $filter -Verbose:$VerbosePreference;
    }

    end {
        Write-Verbose -Message "[Get-PSRule]::END";
    }
}

function New-PSRuleOption {

    [CmdletBinding()]
    [OutputType([PSRule.Configuration.PSRuleOption])]
    param (
        [Parameter(Mandatory = $False)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $False)]
        [String]$Path = '.\psrule.yml'
    )

    process {

        if ($PSBoundParameters.ContainsKey('Option')) {
            $Option = $Option.Clone();
        }
        elseif ($PSBoundParameters.ContainsKey('Path')) {

            if (!(Test-Path -Path $Path)) {

            }

            $Path = Resolve-Path -Path $Path;

            $Option = [PSRule.Configuration.PSRuleOption]::FromFile($Path);
        }
        else {
            Write-Verbose -Message "Attempting to read: $Path";

            $Option = [PSRule.Configuration.PSRuleOption]::FromFile($Path, $True);
        }

        # if ($PSBoundParameters.ContainsKey('Encoding')) {
        #     $Option.Markdown.Encoding = $Encoding;
        # }

        return $Option;
    }
}

<#
.SYNOPSIS
Create a rule definition.

.DESCRIPTION
Create a rule definition.

.INPUTS
None

.OUTPUTS
None

.NOTES
A rule definition can be used by the rule analysis engine.

.LINK
Invoke-RuleEngine
#>
function Rule {
    [CmdletBinding()]
    param (
        # The name of the rule
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$RuleName,

        # The body of the rule
        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body,

        # Any dependencies for this rule
        [Parameter(Mandatory = $False)]
        [String[]]$DependsOn,

        [Hashtable]$Tag
    )

    begin {
        # Just a stub
        Write-Error -Message 'Rule keyword can only be called within PSRule. To call rules use Invoke-PSRule';
    }
}

#
# Helper functions
#

# Used to discover the rule blocks
function GetRule {

    [CmdletBinding()]
    [OutputType([PSRule.Rules.Rule])]
    param (
        [Parameter(Mandatory = $True)]
        [String[]]$Path,

        [Parameter(Mandatory = $True)]
        [PSRule.Configuration.PSRuleOption]$Option,

        [Parameter(Mandatory = $True)]
        [AllowNull()]
        [PSRule.Rules.RuleFilter]$Filter
    )

    begin {
        
    }

    process {
        
    }
}

# Get a list of rule script files in the matching paths
function GetRuleScriptPath {

    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(Mandatory = $True)]
        [String[]]$Path
    )

    process {

        (Get-ChildItem -Path $Path -Recurse -File -Include '*.rule.ps1').FullName;
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

#
# Export module
#

Export-ModuleMember -Function 'Rule','Invoke-PSRule','Get-PSRule','New-PSRuleOption';

# EOM
