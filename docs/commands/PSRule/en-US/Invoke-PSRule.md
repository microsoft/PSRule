---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://berniewhite.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html
schema: 2.0.0
---

# Invoke-PSRule

## SYNOPSIS

Evaluate objects against matching rules.

## SYNTAX

### Input (Default)

```text
Invoke-PSRule [-Module <String[]>] [-Outcome <RuleOutcome>] [-As <ResultFormat>] [-Format <InputFormat>]
 [-OutputPath <String>] [-OutputFormat <OutputFormat>] [[-Path] <String[]>] [-Name <String[]>]
 [-Tag <Hashtable>] [-Option <PSRuleOption>] [-ObjectPath <String>] [-Culture <String>] -InputObject <PSObject>
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### InputPath

```text
Invoke-PSRule -InputPath <String[]> [-Module <String[]>] [-Outcome <RuleOutcome>] [-As <ResultFormat>]
 [-Format <InputFormat>] [-OutputPath <String>] [-OutputFormat <OutputFormat>] [[-Path] <String[]>]
 [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>] [-ObjectPath <String>] [-Culture <String>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

Evaluate objects against matching rules.

## EXAMPLES

### Example 1

```powershell
@{ Name = 'Item 1' } | Invoke-PSRule;
```

Evaluate a simple hashtable on the pipeline against rules loaded from the current working path.

### Example 2

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item using rules saved in current working path
$items | Invoke-PSRule;
```

```text
TargetName: Fridge

RuleName                            Outcome    Message
--------                            -------    -------
isFruit                             Fail       Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Message
--------                            -------    -------
isFruit                             Pass       Fruit is only Apple, Orange and Pear
```

Evaluate an array of objects on the pipeline against rules loaded from the current working path.

### Example 3

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item and only return failing results
$items | Invoke-PSRule -Outcome Fail;
```

```text
TargetName: Fridge

RuleName                            Outcome    Message
--------                            -------    -------
isFruit                             Fail       Fruit is only Apple, Orange and Pear
```

Evaluate an array of objects, only failing object results are returned.

### Example 4

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item and show rule summary
$items | Invoke-PSRule -As Summary;
```

```text
RuleName                            Pass  Fail  Outcome
--------                            ----  ----  -------
isFruit                             1     1     Fail
```

Evaluate an array of objects. The results for each rule is returned as a summary. Outcome is represented as the worst outcome.

## PARAMETERS

### -Name

The name of a specific rule to evaluate. If this parameter is not specified all rules in search paths will be evaluated.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: n

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

One or more paths to search for rule definitions within.

If this parameter is not specified the current working path will be used, unless the `-Module` parameter is used.

If the `-Module` parameter is used, rule definitions from the currently working path will not be included by default.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: p

Required: False
Position: 0
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Outcome

Filter output to only show rules with a specific outcome.

```yaml
Type: RuleOutcome
Parameter Sets: (All)
Aliases:
Accepted values: Pass, Fail, Error, None

Required: False
Position: Named
Default value: Pass, Fail, Error
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

Only evaluate rules with the specified tags set. If this parameter is not specified all rules in search paths will be evaluated.

When more than one tag is used, all tags must match. Tag names are not case sensitive, tag values are case sensitive. A tag value of `*` may be used to filter rules to any rule with the tag set, regardless of tag value.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

The pipeline object to process rules for.

```yaml
Type: PSObject
Parameter Sets: Input
Aliases: TargetObject

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Option

Additional options that configure execution. A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet. Alternatively, a hashtable or path to YAML file can be specified with options.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: PSRuleOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -As

The type of results to produce. Detailed results are generated by default.

The following result formats are available:

- `Detail` - Returns pass/ fail results for each individual object
- `Summary` - Returns summarized results for the rule and the worst outcome

```yaml
Type: ResultFormat
Parameter Sets: (All)
Aliases:
Accepted values: Detail, Summary

Required: False
Position: Named
Default value: Detail
Accept pipeline input: False
Accept wildcard characters: False
```

### -Format

Configures the input format for when a string is passed in as a target object.

- When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default. When this option is used and set to either `Yaml` or `Json`, strings are read as YAML or JSON and are converted to an object.
- When the `-InputPath` parameter is used with a file path or URL, by default the file extension (either `.yaml`, `.yml` or `.json`) will be used to automatically detect the format as YAML or JSON.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, Detect

Required: False
Position: Named
Default value: Detect
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectPath

The name of a property to use instead of the pipeline object. If the property specified by `ObjectPath` is a collection or an array, then each item in evaluated separately.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Module

Search for rule definitions within a module. When specified without the `-Path` parameter, only rule definitions in the module will be discovered.

When both `-Path` and `-Module` are specified, rule definitions from both are discovered.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputPath

Instead of processing objects from the pipeline, import objects file the specified file paths.

```yaml
Type: String[]
Parameter Sets: InputPath
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath

Specifies the output file path to write results. Directories along the file path will automatically be created if they do not exist.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputFormat

Configures the format that output is presented in.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, NUnit3, Csv

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Culture

Specifies the culture to use for rule documentation and messages. By default, the culture of PowerShell is used.

This option does not affect the culture used for the PSRule engine, which always uses the culture of PowerShell.

The PowerShell cmdlet `Get-Culture` shows the current culture of PowerShell.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm

Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf

Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

You can pipe any object to **Invoke-PSRule**.

## OUTPUTS

### PSRule.Rules.RuleRecord

This is the default.

### PSRule.Rules.RuleSummaryRecord

When you use the `-As Summary`. Otherwise, it returns a `RuleRecord` object.

## NOTES

## RELATED LINKS

[Get-PSRule](Get-PSRule.md)

[Test-PSRuleTarget](Test-PSRuleTarget.md)
