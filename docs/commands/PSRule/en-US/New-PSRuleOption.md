---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://berniewhite.github.io/PSRule/commands/PSRule/en-US/New-PSRuleOption.html
schema: 2.0.0
---

# New-PSRuleOption

## SYNOPSIS

Create options to configure PSRule execution.

## SYNTAX

```text
New-PSRuleOption [[-Path] <String>] [[-Option] <PSRuleOption>] [-BaselineConfiguration <BaselineConfiguration>]
 [-SuppressTargetName <SuppressionOption>] [-BindTargetName <BindTargetName[]>]
 [-BindTargetType <BindTargetName[]>] [-BindingIgnoreCase <Boolean>] [-TargetName <String[]>]
 [-TargetType <String[]>] [-InconclusiveWarning <Boolean>] [-NotProcessedWarning <Boolean>]
 [-Format <InputFormat>] [-ObjectPath <String>] [-LoggingRuleFail <OutcomeLogStream>]
 [-LoggingRulePass <OutcomeLogStream>] [-OutputAs <ResultFormat>] [-OutputFormat <OutputFormat>]
 [<CommonParameters>]
```

## DESCRIPTION

The **New-PSRuleOption** cmdlet creates an options object that can be passed to PSRule cmdlets to configure execution.

## EXAMPLES

### Example 1

```powershell
$option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' }
@{ Name = 'Item 1' } | Invoke-PSRule -Option $option
```

Create an options object and run rules in constrained mode.

### Example 2

```powershell
$option = New-PSRuleOption -SuppressTargetName @{ 'storageAccounts.UseHttps' = 'TestObject1', 'TestObject3' };
```

Create an options object that suppresses `TestObject1` and `TestObject3` for a rule named `storageAccounts.UseHttps`.

### Example 3

```powershell
# Create a custom function that returns a TargetName string
$bindFn = {
    param ($TargetObject)

    $otherName = $TargetObject.PSObject.Properties['OtherName'];

    if ($otherName -eq $Null) {
        return $Null
    }

    return $otherName.Value;
}

# Specify the binding function script block code to execute
$option = New-PSRuleOption -BindTargetName $bindFn;
```

Creates an options object that uses a custom function to bind the _TargetName_ of an object.

### Example 4

```powershell
$option = New-PSRuleOption -BaselineConfiguration @{ 'appServiceMinInstanceCount' = 2 };
```

Create an options object that sets the `appServiceMinInstanceCount` baseline configuration option to `2`.

## PARAMETERS

### -Option

Additional options that configure execution. Option also accepts a hashtable to configure options.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: PSRuleOption
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

The path to a YAML file containing options. By default the current working path (`$PWD`) is used.

Either a directory or file path can be specified. When a directory is used, `ps-rule.yaml` will be used as the file name.

If the `-Path` parameter is specified and the file does not exist, an exception will be generated.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -SuppressTargetName

Configures suppression for a list of objects by TargetName. SuppressTargetName also accepts a hashtable to configure rule suppression.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: SuppressionOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BindTargetName

Configures a custom function to use to bind TargetName of an object.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: BindTargetName[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BaselineConfiguration

Configures a set of baseline configuration values that can be used in rule definitions instead of using hard coded values. BaselineConfiguration also accepts a hashtable of configuration values as key/ value pairs.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: BaselineConfiguration
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BindTargetType

Configures a custom function to use to bind TargetType of an object.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: BindTargetName[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BindingIgnoreCase

Sets the option `Binding.IgnoreCase`. The option `Binding.IgnoreCase` determines if binding operations are case-sensitive or not. See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetName

Sets the option `Binding.TargetName`. This option specifies one or more properties of _TargetObject_ to use to bind _TargetName_ to. See about_PSRule_Options for more information.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: BindingTargetName

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetType

Sets the option `Binding.TargetType`. This option specifies one or more properties of _TargetObject_ to use to bind _TargetType_ to. See about_PSRule_Options for more information.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: BindingTargetType

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InconclusiveWarning

Sets the option `Execution.InconclusiveWarning`. The `Execution.InconclusiveWarning` option determines if a warning is generated when the outcome of a rule is inconclusive. See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionInconclusiveWarning

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NotProcessedWarning

Sets the option `Execution.NotProcessedWarning`. The `Execution.NotProcessedWarning` option determines if a warning is generated when an object is not processed by any rule. See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionNotProcessedWarning

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Format

Sets the `Input.Format` option to configure the input format for when a string is passed in as a target object.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases: InputFormat

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectPath

Sets the `Input.ObjectPath` option to use an object path to use instead of the pipeline object.

```yaml
Type: String
Parameter Sets: (All)
Aliases: InputObjectPath

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LoggingRuleFail

Sets the `Logging.RuleFail` option to generate an informational message for each rule fail.

```yaml
Type: OutcomeLogStream
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LoggingRulePass

Sets the `Logging.RulePass` option to generate an informational message for each rule pass.

```yaml
Type: OutcomeLogStream
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputAs

Sets the option `Output.As`. The `Output.As` option configures the type of results to produce, either detail or summary.

```yaml
Type: ResultFormat
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputFormat

Sets the option `Output.Format`. The `Output.Format` option configures the format that results will be presented in.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSRule.Configuration.PSRuleOption

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
[Set-PSRuleOption](Set-PSRuleOption.md)
