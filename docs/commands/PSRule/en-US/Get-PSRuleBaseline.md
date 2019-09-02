---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://berniewhite.github.io/PSRule/commands/PSRule/en-US/Get-PSRuleBaseline.html
schema: 2.0.0
---

# Get-PSRuleBaseline

## SYNOPSIS

Get a list of baselines.

## SYNTAX

```
Get-PSRuleBaseline [-Module <String[]>] [-ListAvailable] [[-Path] <String[]>] [-Name <String[]>]
 [-Option <PSRuleOption>] [-Culture <String>] [<CommonParameters>]
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

{{ Fill Module Description }}

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

If this parameter is not specified the current working path will be used, unless the `-Module` parameter is used.

If the `-Module` parameter is used, baselines from the currently working path will not be included by default.

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

{{ Fill Option Description }}

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

{{ Fill Culture Description }}

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

## OUTPUTS

### PSRule.Rules.Baseline

## NOTES

## RELATED LINKS
