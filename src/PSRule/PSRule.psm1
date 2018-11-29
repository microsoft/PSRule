#
# PSRule module
#

Set-StrictMode -Version latest;

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module;
$PSModuleRoot = $PSModule.ModuleBase;

# Import the appropriate nested binary module based on the current PowerShell version
$binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/desktop/PSRule.dll';

if (($PSVersionTable.Keys -contains 'PSEdition') -and ($PSVersionTable.PSEdition -ne 'Desktop')) {
    $binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/core/PSRule.dll';
}

$binaryModule = Import-Module -Name $binModulePath -PassThru;

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule;
}

[PSRule.Configuration.PSRuleOption]::GetWorkingPath = {
    return Get-Location;
}

$Script:UTF8_NO_BOM = New-Object -TypeName System.Text.UTF8Encoding -ArgumentList $False;

#
# Localization
#

$LocalizedData = data {

}

Import-LocalizedData -BindingVariable LocalizedData -FileName 'PSRule.Resources.psd1' -ErrorAction SilentlyContinue;

#
# Public functions
#

function Invoke-PSRule {

    [CmdletBinding()]
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
        [PSRule.Rules.RuleResultOutcome]$Status = [PSRule.Rules.RuleResultOutcome]::Default
    )

    begin {
        Write-Verbose -Message "[PSRule] BEGIN::";

        Write-Verbose -Message "[PSRule] -- Scanning for source files: $Path";

        # Discover scripts in the specified paths
        [String[]]$sourceFiles = GetRuleScriptPath -Path $Path -Verbose:$VerbosePreference;

        $filter = New-Object -TypeName PSRule.Rules.RuleFilter -ArgumentList @($Name, $Tag);
    }

    process {
        InvokeRulePipeline -Path $sourceFiles -Filter $filter -InputObject $InputObject -Outcome $Status -Verbose:$VerbosePreference;
    }

    end {
        Write-Verbose -Message "[PSRule] END::";
    }
}

function Invoke-EngineRule {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Object]$ConfigurationData,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [ValidateSet('Success', 'Failed')]
        [String[]]$Status,

        [Parameter(Mandatory = $False)]
        [Hashtable]$Tag
    )

    begin {
        Write-Verbose -Message "[PSRule] BEGIN::";

        # Ensure that the supplied path exists
        if (!(Test-Path -Path $Path))
        {
            Write-Error -Message "The path to the rules does not exist: $Path";

            return;
        }

        Write-Verbose -Message "[PSRule] BEGIN::";

        $configData = @{ };
        
        # Handle ConfigurationData
        if ($PSBoundParameters.ContainsKey('ConfigurationData')) {

            # Check the ConfigurationData is a supported type
            if (!($ConfigurationData -is [String] -or $ConfigurationData -is [Hashtable])) {
                Write-Error -Message "The format or type of configuration data is not valid";

                return;
            }

            # If ConfigurationData is a string, attempt to load a .psd1
            if ($ConfigurationData -is [String]) {

                # Check the path exists
                if (!(Test-Path -Path $ConfigurationData)) {
                    Write-Error -Message "The path to configuration data does not exist: $ConfigurationData";

                    return;
                } else {
                    Write-Verbose -Message "[PSRule] -- Importing configuration data: $ConfigurationData";

                    $configDataDirectory = Split-Path -Path $ConfigurationData -Parent;
                    $configDataFileName = Split-Path -Path $ConfigurationData -Leaf;

                    # Import the .psd1
                    Import-LocalizedData -BindingVariable 'configData' -BaseDirectory $configDataDirectory -FileName $configDataFileName;
                }
            } elseif ($ConfigurationData -is [Hashtable]) {

                # Use the provided hashtable
                $configData += $ConfigurationData;
            }
        }
        
        # Create Engine object
        $Engine = NewEngine;

        Write-Verbose -Message "[PSRule] -- Scanning for source files: $Path";

        # Discover scripts in the specified paths
        [String[]]$sourceFiles = GetRuleScriptPath -Path $Path -Verbose:$VerbosePreference;

        Write-Verbose -Message "[PSRule] -- Found $($sourceFiles.Count) source files";

        # Discover rule blocks in each source file
        foreach ($SourceFile in $sourceFiles) {
            # Set variables for discovery
            $variablesToDefine = [PSVariable[]]@(
                New-Object -TypeName PSVariable -ArgumentList ('Engine', $Engine)
                New-Object -TypeName PSVariable -ArgumentList ('SourceFile', $SourceFile)
            );

            # Get source file content
            [ScriptBlock]$sourceBlock = [ScriptBlock]::Create((Get-Content -Path $SourceFile -Raw));

            # Discover rule blocks
            $sourceBlock.InvokeWithContext($Null, $variablesToDefine) | Out-Null;
        }

        Write-Verbose -Message "[PSRule] -- Found $($Engine.Rule.Count) rules";
    }

    process {

        # $This is the current object in the pipeline
        $Local:This = $_;

        
        $Local:Context =  @{
            Index = 0; ScriptName = (Split-Path -Path $Path -Leaf); Rule = $ruleBook;
        };

        # $Parameter is any configuration data provided
        $Local:Parameter = $configData;

        # Create a list of scoped variables.
        $variablesToDefine = @(
            New-Object -TypeName PSVariable -ArgumentList ('This', $This)
            New-Object -TypeName PSVariable -ArgumentList ('Context', $Local:Context)
            New-Object -TypeName PSVariable -ArgumentList ('Parameter', $configData)
        );

        # Create history for current pipeline object
        $history = @{ };

        Write-Verbose -Message "[Engine][$($Local:Context.Index)]`tBEGIN::";

        # Setup helper functions

        function ShouldRun { param ([String]$RuleName) return !$history.ContainsKey($RuleName); }

        function GetDependency {
            param ([String]$RuleName)

            foreach ($dependencyRuleName in $Engine.Rule[$RuleName].DependsOn) {
                $Engine.Rule[$dependencyRuleName];
            }
        }

        function TryRun {
            param ([String]$RuleName)

            # Check history
            if (ShouldRun($RuleName)) {
                # The rule has not run yet
                
                # Get the rule
                $rule = $Engine.Rule[$RuleName];

                # Check dependencies
                if ($Null -ne $rule.DependsOn -and $r.DependsOn.Length -gt 0) {

                    # Rule has dependencies
                    foreach ($d in GetDependency($rule.RuleName)) {
                        Write-Verbose -Message "[Engine][$($Local:Context.Index)]`t-- $($rule.RuleName) depends on: $($d.RuleName)";
                        
                        # Try to run the dependency
                        TryRun($d.RuleName);

                        # Check if dependency failed
                        if (!$history[$d.RuleName].Success) {
                            # Dependency failed

                            $history.Add($rule.RuleName, (NewRuleResult -Success $False -Status 'Skipped'));

                            return;
                        }
                    }
                }

                Write-Verbose -Message "[Engine][$($Local:Context.Index)]`t-- Running $($rule.RuleName)";

                # Invoke the rule
                $ruleResult = InvokeRule -Rule $rule -Verbose:$VerbosePreference;

                # Add result to history
                $history.Add($rule.RuleName, $ruleResult);
            } else {
                Write-Verbose -Message "[Engine][$($Local:Context.Index)]`t-- $($RuleName) has already run";
            }
        }

        # Rule each rule for the pipeline object
        foreach ($r in $Engine.Rule.Values.GetEnumerator()) {
            # Invoke the rules within the engine context

            TryRun($r.RuleName);
        }

        # Emit results to the pipeline
        $history.Values.GetEnumerator() | Where-Object -FilterScript {
            # Filter by status
            ([String]::IsNullOrEmpty($Status) -or $Status -contains $_.Status)
        }

        Write-Verbose -Message "[Engine][$($Local:Context.Index)]`tEND::";

        # Keep track of the number of objects that have passed though the pipeline
        $Local:Context.Index++;
    }

    end {
        Write-Verbose -Message "[PSRule]`tEND::";
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
        [String[]]$Path = $PWD
    )

    begin {
        Write-Verbose -Message "[Get-PSRule]::BEGIN";

        # Discover scripts in the specified paths
        [String[]]$includePaths = GetRuleScriptPath -Path $Path -Verbose:$VerbosePreference;

        Write-Verbose -Message "[Get-PSRule] -- Found $($includePaths.Length) script(s)";
        Write-Debug -Message "[Get-PSRule] -- Found scripts: $([String]::Join(' ', $includePaths))";
    }

    process {
        # Get matching deployment definitions
        $filter = New-Object -TypeName PSRule.Rules.RuleFilter -ArgumentList @($Name, $Tag);
        GetRule -Path $includePaths -Filter $filter -Verbose:$VerbosePreference;
    }

    end {
        Write-Verbose -Message "[Get-PSRule]::END";
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

function InvokeRule {
    [CmdletBinding()]
    [OutputType([PSRule.Rules.RuleResult])]
    param (
        [Parameter(Mandatory = $True)]
        [PSObject]$Rule
    )

    begin {
        $RuleName = $Rule.RuleName;
        $Body = $Rule.Body;

        # Create a progress record
        $progressRecord = @{
            Activity = "Running rule $RuleName";
            Status = 'Running rule';
            CurrentOperation = '';
            PercentComplete = 0;
        };

        # Update progress display
        Write-Progress @progressRecord;
    }

    process {

        if ($Null -eq $Context) {
            Write-Error -Message "Rule expression can only be used within a Rule engine. Please call with Invoke-RuleEngine";

            return;
        }

        Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`tBEGIN::";
        Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t-- Setting context";

        # Set Rule context variables
        $Rule = @{
            Name = $RuleName;
            Warning = @();
            Error = @();
            Output = @();
            Invocation = $PSCmdlet.MyInvocation;
            TargetName = '';
            Message = '';
            Input = $This;
            OnSuccess = @();
            OnFailure = @();
        };

        $functionsToDefine = New-Object -TypeName 'System.Collections.Generic.Dictionary[string,ScriptBlock]'([System.StringComparer]::OrdinalIgnoreCase);
        $functionsToDefine.Add('AllOf', ${function:AssertAllOf});
        $functionsToDefine.Add('AnyOf', ${function:AssertAnyOf});
        $functionsToDefine.Add('Exists', ${function:AssertExists});
        $functionsToDefine.Add('Within', ${function:AssertWithin});
        $functionsToDefine.Add('Match', ${function:AssertMatch});
        $functionsToDefine.Add('When', ${function:WhenExpression});
        $functionsToDefine.Add('Warn', ${function:Write-RuleWarning});
        $functionsToDefine.Add('Hint', ${function:Set-RuleHint});
        $functionsToDefine.Add('TypeOf', ${function:AssertTypeOf});

        $functionsToDefine.Add('OnSuccess', ${function:Add-RuleSuccessTrigger});
        $functionsToDefine.Add('OnFailure', ${function:Add-RuleFailureTrigger});

        $variablesToDefine = New-Object -TypeName 'System.Collections.Generic.List[PSVariable]';
        $variablesToDefine.Add((Get-Variable -Name 'Rule'));

        # Add helper methods
        Add-Member -InputObject $Rule -MemberType ScriptMethod -Name 'GetField' -Value { param([String]$Field) process { Get-ObjectField -InputObject $This.Input -Field $Field; } };

        # Invoke body within the context of this Rule
        $innerResult = ($Body.InvokeWithContext($functionsToDefine, $variablesToDefine));

        # Count the results that are boolean $True
        $numResult = $innerResult.Count;
        $successCount = 0;

        foreach ($r in $innerResult) {
            if ($r -eq $True) {
                $successCount++;
            }
        }

        # Determine overall success of rule
        $success = $successCount -eq $numResult;

        # Create a result object
        $resultObject = NewRuleResult -Success $success;

        # Set Status to Success if all results are successful, otherwise default to Failed
        if ($success) {
            $resultObject.Status = 'Success'
        }

        if ($numResult -eq 0) {
            $resultObject.Status = 'Skipped'
        }

        Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t-- Status = $($resultObject.Status)";

        # Invoke triggers
        if ($success -and $Rule.OnSuccess.Length -gt 0) {
            # OnSuccess

            Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t-- Triggering OnSuccess";

            # Run each trigger
            foreach ($trigger in $Rule.OnSuccess) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t[OnSuccess]::BEGIN";

                $innerResult = $trigger.InvokeWithContext($functionsToDefine, $variablesToDefine);

                Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t[OnSuccess]::END";
            }
        } elseif (!$success -and $Rule.OnFailure.Length -gt 0) {
            # OnFailure

            Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t-- Triggering OnFailure";

            # Run each trigger
            foreach ($trigger in $Rule.OnFailure) {
                Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t[OnFailure]::BEGIN";

                $innerResult = $trigger.InvokeWithContext($functionsToDefine, $variablesToDefine);

                Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t[OnFailure]::END";
            }
        }

        # Add output objects to result object
        if ($Rule.Output.Length -gt 0) {
            foreach ($a in $Rule.Output) {
                $resultObject.Output += $a;
            }
        }

        Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`t-- Output = $($resultObject.Output.Length)";

        # Emit the result object to the pipeline
        $resultObject;

        Write-Verbose -Message "[Rule][$($Context.Index)][$RuleName]`tEND:: [$successCount/$numResult]";
    }

    end {
        # Update progress display
        $progressRecord.PercentComplete = 100;
        Write-Progress @progressRecord -Completed;
    }
}

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

function Out-Rule {
    [CmdletBinding(DefaultParameterSetName = 'NewObject')]
    [OutputType([void])]
    param (
        # The TypeName that will be used for the emitted PSObject
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'NewObject')]
        [String]$TypeName,

        # A set of properties for the PSObject
        [Parameter(Position = 1, Mandatory = $True, ParameterSetName = 'NewObject')]
        [System.Collections.IDictionary]$Property,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'InputObject')]
        [PSObject]$InputObject
    )

    process {
        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule]::BEGIN";

        if ($PSBoundParameters.ContainsKey('InputObject')) {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule] -- Using input object";

            # Write verbose TypeName information
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule] -- TypeName: $($InputObject.PSObject.TypeNames[0])";

            # Write verbose information about the properties of the supplied object
            $InputObject.PSObject.Properties.GetEnumerator() | ForEach-Object `
            -Process {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule] -- $($_.Name): $($_.Value)";
            }

            # Add to Output objects for this Rule
            $Rule.Output += $InputObject;
        } else {
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule] -- Creating object from hashtable";

            # Write verbose TypeName information
            Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule] -- TypeName: $TypeName";

            # Write verbose information about the property hashtable supplied
            $Property.GetEnumerator() | ForEach-Object `
            -Process {
                Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule] -- $($_.Key): $($_.Value)";
            }

            # Build the output object as a PSObject
            $outputObject = New-Object -TypeName PSObject -Property $Property;

            # Set TypeName
            $outputObject.PSObject.TypeNames.Insert(0, $TypeName);

            # Add to Output objects for this Rule
            $Rule.Output += $outputObject;
        }

        Write-Verbose -Message "[Rule][$($Context.Index)][$($Rule.Name)]`t[Out-Rule]::END";
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
        [AllowNull()]
        [PSRule.Rules.RuleFilter]$Filter
    )

    process {

        Write-Verbose -Message "[PSRule] -- Getting rules";

        [PSRule.Pipeline.PipelineBuilder]::Get().Build($Path, $Filter).Process();
    }
}

function InvokeRulePipeline {

    [CmdletBinding()]
    [OutputType([PSRule.Rules.RuleResult])]
    param (
        [Parameter(Mandatory = $True)]
        [String[]]$Path,

        [Parameter(Mandatory = $True)]
        [AllowNull()]
        [PSRule.Rules.RuleFilter]$Filter,

        [Parameter(Mandatory = $True)]
        [PSRule.Rules.RuleResultOutcome]$Outcome,

        [Parameter(Mandatory = $True)]
        [PSObject]$InputObject
    )

    process {

        Write-Verbose -Message "[PSRule] -- Invoking rules";
        [PSRule.Pipeline.PipelineBuilder]::Invoke().Build($Path, $Filter, $Outcome).Process($InputObject);
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

function NewEngine {

    param (

    )

    process {

        $result = New-Object -TypeName PSObject -Property @{
            Rule = New-Object -TypeName 'System.Collections.Generic.Dictionary[string,PSObject]'([System.StringComparer]::OrdinalIgnoreCase)
        }

        $result;
    }
}

function NewContext {

    param (

    )

    process {

    }
}

function NewRuleResult {
    [CmdletBinding()]
    [OutputType([PSRule.Rules.RuleResult])]
    param (
        [System.Boolean]$Success,

        [String]$Status = 'Failed'
    )

    process {
        # Create a result object
        $result = New-Object -TypeName PSRule.Rules.RuleResult -Property @{ RuleName = $RuleName; Success = $Success; Status = $Status; Message = $Rule.Message; TargetName = $Rule.TargetName; Output = @(); }

        # $result.PSObject.TypeNames.Insert(0, 'PSRule.Rules.RuleResult');

        $result;
    }
}

#
# Export module
#

Export-ModuleMember -Function 'Rule','Get-PSRule','Invoke-PSRule','Out-Rule','Get-ObjectField';

# EOM