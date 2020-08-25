---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/commands/PSRule/en-US/Test-PSRuleTarget.html
schema: 2.0.0
---

# Test-PSRuleTarget

## SYNOPSIS

Pass or fail objects against matching rules.

## SYNTAX

### Input (Default)

```text
Test-PSRuleTarget [-Module <String[]>] [-Outcome <RuleOutcome>] [-Format <InputFormat>] [[-Path] <String[]>]
 [-Name <String[]>] [-Tag <Hashtable>] -InputObject <PSObject> [-Option <PSRuleOption>] [-ObjectPath <String>]
 [-TargetType <String[]>] [-Culture <String>] [<CommonParameters>]
```

### InputPath

```text
Test-PSRuleTarget -InputPath <String[]> [-Module <String[]>] [-Outcome <RuleOutcome>] [-Format <InputFormat>]
 [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>] [-ObjectPath <String>]
 [-TargetType <String[]>] [-Culture <String>] [<CommonParameters>]
```

## DESCRIPTION

Evaluate objects against matching rules and return an overall pass or fail for the object as `$True` (pass) or `$False` (fail).

PSRule uses the following logic to determine overall pass or fail for an object:

- The object fails if:
  - Any rules fail or error.
  - Any rules are inconclusive.
- The object passes if:
  - No matching rules were found.
  - All rules pass.

By default, objects that do match any rules are not returned in results.
To return `$True` for these objects, use `-Outcome All`.

## EXAMPLES

### Example 1

```powershell
@{ Name = 'Item 1' } | Test-PSRuleTarget;
```

Evaluate a simple hashtable on the pipeline against rules loaded from the current working path.

## PARAMETERS

### -Path

One or more paths to search for rule definitions within.

If this parameter is not specified the current working path will be used, unless the `-Module` parameter is used.

If the `-Module` parameter is used, rule definitions from the currently working path will not be included by default.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: p

Required: False
Position: 1
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -Outcome

Filter output to only show pipeline objects with a specific outcome.

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

### -Format

Configures the input format for when a string is passed in as a target object.

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
Set this option to either `Yaml`, `Json`, `Markdown`, `PowerShellData` to have PSRule deserialize the object.

When the `-InputPath` parameter is used with a file path or URL.
If the `Detect` format is used, the file extension will be used to automatically detect the format.
When `-InputPath` is not used, `Detect` is the same as `None`.

See `about_PSRule_Options` for details.

This parameter takes precedence over the `Input.Format` option if set.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, Markdown, PowerShellData, Repository, Detect

Required: False
Position: Named
Default value: Detect
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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

You can pipe any object to **Test-PSRuleTarget**.

## OUTPUTS

### System.Boolean

Returns `$True` when the object passes and `$False` when the object fails.

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)

[Assert-PSRule](Assert-PSRule.md)

[Get-PSRule](Get-PSRule.md)
