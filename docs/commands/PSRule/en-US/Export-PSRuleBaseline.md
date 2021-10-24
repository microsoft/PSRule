---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/commands/PSRule/en-US/Export-PSRuleBaseline.html
schema: 2.0.0
---

# Export-PSRuleBaseline

## SYNOPSIS

Exports a list of baselines.

## SYNTAX

```text
Export-PSRuleBaseline [[-Path] <string[]>] -OutputPath <string> [-Module <string[]>] [-Name <string[]>]
 [-Option <PSRuleOption>] [-Culture <string>] [-OutputFormat <OutputFormat>] [-OutputEncoding <OutputEncoding>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

Exports a list of baselines to a file.

## EXAMPLES

### Example 1

```powershell
Export-PSRuleBaseline -Module PSRule.Rules.Azure -OutputFormat Yaml -OutputPath Baseline.Rule.yml
```

Exports list of baselines from `PSRule.Rules.Azure` module to file `Baseline.Rule.yml` in YAML output format.

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
Accept wildcard characters: True
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

- Yaml - Output is serialized as YAML. This is the default.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases: o
Accepted values: Yaml

Required: False
Position: Named
Default value: Yaml
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputEncoding

Sets the option `Output.Encoding`.
The `Output.Encoding` option configured the encoding used to write results to file.

```yaml
Type: OutputEncoding
Parameter Sets: (All)
Aliases:
Accepted values: Default, UTF8, UTF7, Unicode, UTF32, ASCII

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath

Sets the option `Output.Path`.
The `Output.Path` option configures the output path the results are written to.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf

Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

[Get-PSRuleBaseline](Get-PSRuleBaseline.md)
