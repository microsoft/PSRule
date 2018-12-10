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
        [PSRule.Rules.RuleOutcome]$Status = [PSRule.Rules.RuleOutcome]::Default,

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
        $builder.Limit($Status);
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
        [String[]]$includePaths = GetRuleScriptPath -Path $Path -Verbose:$VerbosePreference;

        Write-Verbose -Message "[Get-PSRule] -- Found $($includePaths.Length) script(s)";
        Write-Debug -Message "[Get-PSRule] -- Found scripts: $([String]::Join(' ', $includePaths))";
    }

    process {
        # Get matching deployment definitions
        $filter = New-Object -TypeName PSRule.Rules.RuleFilter -ArgumentList @($Name, $Tag);
        GetRule -Path $includePaths -Option $Option -Filter $filter -Verbose:$VerbosePreference;
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
        [String[]]$DependsOn
    )

    begin {
        # Check for if we are executing within a Rule Engine
        if ($Null -eq $Engine) {
            Write-Error -Message "Rule expression can only be used within a Rule engine. Please call with Invoke-RuleEngine";

            return;
        }
    }
}

#
# Helper functions
#

function AssertAllOf {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Expression
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[AllOf]::BEGIN";

        [Boolean[]]$innerResult = $Expression.InvokeReturnAsIs();

        Write-Verbose -Message "[AllOf] - Expression returned $($innerResult.Count) results";

        [System.Boolean]$result = $True;

        for ($i = 0; $i -lt $innerResult.Length -and $result -eq $True; $i++) {
            $result = $innerResult[$i];

            Write-Verbose -Message "[AllOf] - Result is $result"
        }

        return $result;

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[AllOf]::END";
    }
}

function AssertAnyOf {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Expression
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[AnyOf]::BEGIN";

        [Boolean]$result = $False;

        try {
            $innerResult = $Expression.InvokeReturnAsIs();

            $numResult = $innerResult.Count;
            $successCount = 0;

            foreach ($r in $innerResult) {
                if ($r -eq $True) {
                    $successCount++;
                }
            }

            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[AnyOf] -- [$successCount/$numResult]";

            if ($successCount -gt 0) {
                $result = $True;
            }

            return $result;
        } finally {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[AnyOf]::END [$result]";
        }
    }
}

function AssertExists {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Field,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False
    )

    process {

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Exists]::BEGIN";

        [Boolean]$result = $False;

        try {
            if ($Null -eq $Rule) {
                Write-Error -Message "Exists expression can only be used within a Rule block";

                return;
            }

            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Exists] -- Checking for field '$Field'";

            $fieldProp = Get-ObjectField -InputObject $This -Field $Field;

            if ($Null -eq $fieldProp) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Exists] -- The field '$Field' not found";

                $result = $False;
            } elseif ($CaseSensitive -and $fieldProp.Path -cne $Field) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Exists] -- The field '$($fieldProp.Path)' was found but did not match the expected case";

                $result = $False;
            } else {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Exists] -- The field '$Field' was found";

                $result = $True;
            }

            return $result;
        } finally {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Exists]::END [$result]";
        }
    }
}

function AssertWithin {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Field,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Expression,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within]::BEGIN";

        [Boolean]$result = $False;

        try {

            $fieldProp = Get-ObjectField -InputObject $This -Field $Field;

            if ($Null -eq $fieldProp) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within] -- The field '$Field' does not exist and was skipped";

                return $result;
            }

            try {
                $innerResult = $Expression.InvokeReturnAsIs();
            }
            catch {
                Write-Error -Message "Failed to invoke Within expression block. $($_.Exception.Message)" -Exception $_.Exception -TargetObject $Parameter;
            }

            if ($Null -eq $innerResult) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within] -- The expression was empty";
            }

            if ((!$CaseSensitive -and $innerResult -contains $fieldProp.Value) -or ($innerResult -ccontains $fieldProp.Value)) {
                # The field is one of the values returned by Expression
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within] -- The value '$($fieldProp.Value)' was found in field '$Field'";

                $result = $True;
            } elseif ($CaseSensitive -and $innerResult -contains $fieldProp.Value) {
                # The field is one of the values returned by Expression but did not match case
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within] -- The value '$($fieldProp.Value)' was found in field '$Field' but did not match the expected case";

                $result = $False;
            } else {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within] -- The value '$($fieldProp.Value)' was not found in field '$Field'";

                $result = $False;
            }

            return $result;
        }
        finally {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Within]::END [$result]";
        }
    }
}

function AssertMatch {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Field,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Expression,

        [Parameter(Mandatory = $False)]
        [Switch]$CaseSensitive = $False
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Match]::BEGIN";

        [Boolean]$result = $False;

        try {

            $fieldProp = Get-ObjectField -InputObject $This -Field $Field;

            if ($Null -eq $fieldProp) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Match] -- The field '$Field' does not exist and was skipped";

                return $result;
            }

            [String[]]$expressionResult = $Expression.InvokeReturnAsIs();

            for ($i = 0; $i -lt $expressionResult.Length -and $result -eq $False; $i++) {

                $result = ($CaseSensitive -and $fieldProp.Value -cmatch $expressionResult[$i]) -or (!$CaseSensitive -and $fieldProp.Value -match $expressionResult[$i]);

                if ($result) {
                    Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Match] -- The value '$($fieldProp.Value)' matched the expression '$($expressionResult[$i])'";
                } elseif ($CaseSensitive -and $fieldProp.Value -match $expressionResult[$i]) {
                    Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Match] -- The value '$($fieldProp.Value)' matched the expression '$($expressionResult[$i])' but did not match the expected case";
                } else {
                    Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Match] -- The value '$($fieldProp.Value)' did not match the expression '$($expressionResult[$i])'";
                }
            }

            return $result;
        } finally {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Match]::END [$result]";
        }
    }
}

function AssertTypeOf {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String[]]$TypeName
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[TypeOf]::BEGIN";

        [Boolean]$result = $False;

        try {
            if ($Null -eq $Rule) {
                Write-Error -Message "TypeOf expression can only be used within a Rule block";

                return;
            }

            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[TypeOf] -- Checking type '$TypeName'";

            $actualTypeNames = $This.PSTypeNames;

            foreach ($tn in $TypeName) {

                if ($tn -in $actualTypeNames) {

                    $result = $True;

                    Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[TypeOf] -- The type was '$tn'";

                    break;
                }
            }

            if (!$result) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[TypeOf] -- Was not the type";
            }

            return $result;
        } finally {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[TypeOf]::END [$result]";
        }
    }
}

function WhenExpression {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Condition,

        [Parameter(Position = 1, Mandatory = $False)]
        [ScriptBlock]$Then,

        [Parameter(Mandatory = $False)]
        [ScriptBlock]$Otherwise
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[When]::BEGIN";

        $innerResult = $Condition.InvokeReturnAsIs();

        if ($innerResult -contains $True) {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[When] -- The condition returned True";

            if ($PSBoundParameters.ContainsKey('Then')) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Then]::BEGIN";

                $Then.Invoke();

                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Then]::END";
            }
        } else {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[When] -- The condition returned False";

            if ($PSBoundParameters.ContainsKey('Otherwise')) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Otherwise]::BEGIN";

                $Otherwise.Invoke();

                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Otherwise]::END";
            }
        }

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[When]::END";
    }
}

function Write-RuleWarning {


    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Message,

        [Parameter(Mandatory = $False)]
        [Object]$TargetObject,

        [Parameter(Mandatory = $False)]
        [String]$TargetName,

        [Parameter(Mandatory = $False)]
        [String]$Reason,

        [Parameter(Mandatory = $False)]
        [String]$RecommendedAction
    )

    process {

        $warningObject = New-Object -TypeName PSObject -Property @{ RuleName = $Rule.Name; Message = $Message; Severity = 'Warning'; Line = $Rule.Invocation.ScriptLineNumber; ScriptName = $Context.ScriptName; TargetObject = $TargetObject; TargetName = $TargetName; Reason = $Reason; RecommendedAction = $RecommendedAction; };

        $warningObject.PSObject.TypeNames.Add('PSRule.Message');

        $Rule.Warning += $warningObject;

    }
}

function Set-RuleHint {
    [CmdletBinding(SupportsShouldProcess = $True)]
    param (
        [Parameter(Position = 0, Mandatory = $False)]
        [String]$Message,

        [Parameter(Mandatory = $False)]
        [Object]$TargetObject,

        [Parameter(Mandatory = $False)]
        [String]$TargetName,

        [Parameter(Mandatory = $False)]
        [String]$Reason,

        [Parameter(Mandatory = $False)]
        [String]$RecommendedAction
    )

    process {

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Hint]::BEGIN";

        # Set TargetName if specified
        if ($PSBoundParameters.ContainsKey('TargetName')) {
            if ($PSCmdlet.ShouldProcess('Rule', 'Set TargetName')) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Hint] -- Setting TargetName: $TargetName";

                $Rule.TargetName = $TargetName;
            }
        }

        # Set Message if specified
        if ($PSBoundParameters.ContainsKey('Message')) {
            if ($PSCmdlet.ShouldProcess($Rule.RuleName, 'Set Message')) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Hint] -- Setting Message: $Message";

                $Rule.Message = $Message;
            }
        }

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Hint]::END";
    }
}

function Add-RuleSuccessTrigger {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[OnSuccess]::BEGIN";

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[OnSuccess] -- Adding OnSuccess trigger";

        $Rule.OnSuccess += $Body;

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[OnSuccess]::END";
    }
}

function Add-RuleFailureTrigger {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[OnFailure]::BEGIN";

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[OnFailure] -- Adding OnFailure trigger";

        $Rule.OnFailure += $Body;

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[OnFailure]::END";
    }
}


# Define a function to walk dotted notation of a field
function Get-ObjectField {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $True)]
        [String]$Field
    )

    process {
        # Split field into dotted notation
        $fieldParts = $Field.Split('.');

        if ($Null -eq $InputObject) {
            Write-Error -Message "Failed to bind to InputObject"

            return;
        }

        Write-Debug -Message "[Get-ObjectField] - Splitting into fields: $([String]::Join(',', $fieldParts))";

        # Write-Verbose -Message "[Get-ObjectField] - Detecting type as $($InputObject.GetType())";

        $resultProperty = $Null;

        $nextObj = $InputObject;
        $partIndex = 0;

        $resultPropertyPath = New-Object -TypeName 'System.Collections.Generic.List[String]';

        while ($Null -ne $nextObj -and $partIndex -lt $fieldParts.Length -and $Null -eq $resultProperty) {

            Write-Debug -Message "[Get-ObjectField] - Checking field part $($fieldParts[$partIndex])";

            # Find a property of the object that matches the current field part

            $property = $Null;

            if ($nextObj -is [System.Collections.Hashtable]) {
                # Handle hash table

                $property = $nextObj.GetEnumerator() | Where-Object `
                -FilterScript {
                    $_.Name -eq $fieldParts[$partIndex]
                }
            } elseif ($nextObj -is [PSObject]) {
                # Handle regular object

                $property = $nextObj.PSObject.Properties.GetEnumerator() | Where-Object `
                -FilterScript {
                    $_.Name -eq $fieldParts[$partIndex]
                }
            }

            if ($Null -ne $property -and $partIndex -eq ($fieldParts.Length - 1)) {
                # We have reached the last field part and found a property

                # Build the remaining field path
                $resultPropertyPath.Add($property.Name);

                # Create a result property object
                $resultProperty = New-Object -TypeName PSObject -Property @{ Name = $property.Name; Value = $property.Value; Path = [String]::Join('.', $resultPropertyPath); };
            } else {
                $nextObj = $property.Value;

                $resultPropertyPath.Add($property.Name);

                $partIndex++;
            }
        }

        # Return the result property
        return $resultProperty;
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

    process {

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSRule.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        Write-Verbose -Message "[PSRule] -- Getting rules";

        [PSRule.Pipeline.PipelineBuilder]::Get().Build($Option, $Path, $Filter).Process();
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
