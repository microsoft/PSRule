---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/v1/commands/PSRule/en-US/Get-PSRuleBaseline/
schema: 2.0.0
---

# Get-PSRuleBaseline

## SYNOPSIS

Get a list of baselines.

## SYNTAX

```text
Get-PSRuleBaseline [-Module <String[]>] [-ListAvailable] [[-Path] <String[]>] [-Name <String[]>]
 [-Option <PSRuleOption>] [-Culture <String>] [-OutputFormat <OutputFormat>] [<CommonParameters>]
```

## DESCRIPTION

Get a list of matching baselines within the search path.

## EXAMPLES

### Example 1

```powershell
Get-PSRuleBaseline;
```

Get a list of baselines from the current working path.

## PARAMETERS

### -Module

Search for baselines definitions within a module.
If no sources are specified by `-Path`, `-Module`, or options, the current working directory is used.

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

### -ListAvailable

Look for modules containing baselines including modules that are currently not imported.

This switch is used with the `-Module` parameter.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

One or more paths to search for baselines within.
If no sources are specified by `-Path`, `-Module`, or options, the current working directory is used.

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

The name of a specific baseline to list.
If this parameter is not specified all baselines in search paths will be listed.

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

### -Option

Additional options that configure execution.
A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet.
Alternatively a hashtable or path to YAML file can be specified with options.

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

### -Culture

Specifies the culture to use for documentation and messages. By default, the culture of PowerShell is used.

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
- Yaml - Output is serialized as YAML.
- Json - Output is serialized as JSON.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases: o
Accepted values: None, Yaml, Json

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### PSRule.Definitions.Baseline

This is the default.

### System.String

When you use `-OutputFormat Yaml` or `-OutputFormat Json`.

## NOTES

## RELATED LINKS

[Get-PSRule](Get-PSRule.md)
