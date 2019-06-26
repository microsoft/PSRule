---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://berniewhite.github.io/PSRule/commands/PSRule/en-US/Get-PSRule.html
schema: 2.0.0
---

# Get-PSRule

## SYNOPSIS

Get a list of rule definitions.

## SYNTAX

```text
Get-PSRule [-Module <String[]>] [-ListAvailable] [-OutputFormat <OutputFormatGet>] [[-Path] <String[]>]
 [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>] [-Culture <String>] [<CommonParameters>]
```

## DESCRIPTION

Get a list of matching rule definitions within the search path.

## EXAMPLES

### Example 1

```powershell
Get-PSRule;
```

```text
RuleName                            Description
--------                            -----------
isFruit                             An example rule
```

Get a list of rule definitions from the current working path.

## PARAMETERS

### -Name

The name of a specific rule to list. If this parameter is not specified all rules in search paths will be listed.

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

### -Tag

Only get rules with the specified tags set. If this parameter is not specified all rules in search paths will be returned.

When more then one tag is used, all tags must match. Tag names are not case sensitive, tag values are case sensitive. A tag value of `*` may be used to filter rules to any rule with the tag set, regardless of tag value.

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

### -Option

Additional options that configure execution. A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet. Alternatively a hashtable or path to YAML file can be specified with options.

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

### -ListAvailable

Look for modules containing rule definitions including modules that are currently not imported.

This switch is used with the `-Module` parameter.

```yaml
Type: SwitchParameter
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

### -OutputFormat

Configures the format that output is presented in.

The following format options are available:

- None - Output is presented as an object using PowerShell defaults. This is the default.
- Wide - Output is presented using the wide table format, which includes tags and wraps columns.

```yaml
Type: OutputFormatGet
Parameter Sets: (All)
Aliases:
Accepted values: None, Wide

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

### PSRule.Rules.Rule

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
