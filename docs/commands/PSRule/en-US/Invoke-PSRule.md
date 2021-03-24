---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html
schema: 2.0.0
---

# Invoke-PSRule

## SYNOPSIS

Evaluate objects against matching rules and output the results.

## SYNTAX

### Input (Default)

```text
Invoke-PSRule [-Module <String[]>] [-Outcome <RuleOutcome>] [-As <ResultFormat>] [-Format <InputFormat>]
 [-OutputPath <String>] [-OutputFormat <OutputFormat>] [-Baseline <BaselineOption>] [-Convention <String[]>]
 [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>] [-ObjectPath <String>]
 [-TargetType <String[]>] [-Culture <String[]>] -InputObject <PSObject> [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### InputPath

```text
Invoke-PSRule -InputPath <String[]> [-Module <String[]>] [-Outcome <RuleOutcome>] [-As <ResultFormat>]
 [-Format <InputFormat>] [-OutputPath <String>] [-OutputFormat <OutputFormat>] [-Baseline <BaselineOption>]
 [-Convention <String[]>] [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>]
 [-ObjectPath <String>] [-TargetType <String[]>] [-Culture <String[]>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

Evaluate objects against matching rules and output the results.
Objects can be specified directly from the pipeline or provided from file.

The commands `Invoke-PSRule` and `Assert-PSRule` provide similar functionality, as differ as follows:

- `Invoke-PSRule` writes results as structured objects
- `Assert-PSRule` writes results as a formatted string.

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

RuleName                            Outcome    Recommendation
--------                            -------    --------------
isFruit                             Fail       Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Recommendation
--------                            -------    --------------
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

RuleName                            Outcome    Recommendation
--------                            -------    --------------
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

Evaluate an array of objects. The results for each rule is returned as a summary.
Outcome is represented as the worst outcome.

## PARAMETERS

### -Name

The name of a specific rule to evaluate.
If this parameter is not specified all rules in search paths will be evaluated.

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

Filter output to only show rule results with a specific outcome.

```yaml
Type: RuleOutcome
Parameter Sets: (All)
Aliases:
Accepted values: Pass, Fail, Error, None, Processed, All

Required: False
Position: Named
Default value: Pass, Fail, Error
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

Only get rules with the specified tags set.
If this parameter is not specified all rules in search paths will be returned.

When more than one tag is used, all tags must match. Tags are not case sensitive.
A tag value of `*` may be used to filter rules to any rule with the tag set, regardless of tag value.

An array of tag values can be used to match a rule with either value.
i.e. `severity = important, critical` matches rules with a category of either `important` or `critical`.

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

Additional options that configure execution.
A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet.
Alternatively, a hashtable or path to YAML file can be specified with options.

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

The type of results to produce.
Detailed results are generated by default.

The following result formats are available:

- `Detail` - Returns pass/ fail results for each rule per object.
- `Summary` - Returns summarized results for the rule and the worst outcome.

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

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
Set this option to either `Yaml`, `Json`, `Markdown`, `PowerShellData` to have PSRule deserialize the object.

When the `-InputPath` parameter is used with a file path or URL.
If the `Detect` format is used, the file extension will be used to automatically detect the format.
When `-InputPath` is not used, `Detect` is the same as `None`.

When this option is set to `File` PSRule scans the path and subdirectories specified by `-InputPath`.
Files are treated as objects instead of being deserialized.
Additional, PSRule uses the file extension as the object type.
When files have no extension the whole file name is used.

See `about_PSRule_Options` for details.

This parameter takes precedence over the `Input.Format` option if set.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, Markdown, PowerShellData, File, Detect

Required: False
Position: Named
Default value: Detect
Accept pipeline input: False
Accept wildcard characters: False
```

### -Baseline

Specifies an explicit baseline by name to use for evaluating rules.
Baselines can contain filters and custom configuration that overrides the defaults.

```yaml
Type: BaselineOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Convention

Specifies conventions by name to execute in the pipeline when processing objects.

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

### -Culture

Specifies the culture to use for rule documentation and messages.
By default, the culture of PowerShell is used.

This option does not affect the culture used for the PSRule engine, which always uses the culture of PowerShell.

The PowerShell cmdlet `Get-Culture` shows the current culture of PowerShell.

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

### -ObjectPath

The name of a property to use instead of the pipeline object.
If the property specified by `ObjectPath` is a collection or an array, then each item in evaluated separately.

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

### -TargetType

Filters input objects by TargetType.

If specified, only objects with the specified TargetType are processed.
Objects that do not match TargetType are ignored.
If multiple values are specified, only one TargetType must match. This parameter is not case-sensitive.

By default, all objects are processed.

This parameter if set, overrides the `Input.TargetType` option.

To change the field TargetType is bound to set the `Binding.TargetType` option.
For details see the about_PSRule_Options help topic.

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

### -Module

Search for rule definitions within a module.
When specified without the `-Path` parameter, only rule definitions in the module will be discovered.

When both `-Path` and `-Module` are specified, rule definitions from both are discovered.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: m

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
Aliases: f

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath

Specifies the output file path to write results.
Directories along the file path will automatically be created if they do not exist.

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

The following format options are available:

- None - Output is presented as an object using PowerShell defaults.
This is the default.
- Yaml - Output is serialized as YAML.
- Json - Output is serialized as JSON.
- Markdown - Output is serialized as Markdown.
- NUnit3 - Output is serialized as NUnit3 (XML).
- Csv - Output is serialized as a comma separated values (CSV).
- Wide - Output is presented using the wide table format, which includes reason and wraps columns.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases: o
Accepted values: None, Yaml, Json, Markdown, NUnit3, Csv, Wide

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

[Assert-PSRule](Assert-PSRule.md)

[Test-PSRuleTarget](Test-PSRuleTarget.md)
