---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/New-PSRuleOption.md
schema: 2.0.0
---

# New-PSRuleOption

## SYNOPSIS

Create options to configure PSRule execution.

## SYNTAX

```text
New-PSRuleOption [[-Option] <PSRuleOption>] [[-Path] <String>] [<CommonParameters>]
```

## DESCRIPTION

The **New-PSRuleOption** cmdlet creates an options object that can be passed to PSRule cmdlets to configure execution.

## EXAMPLES

### Example 1

```powershell
$option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' }
@{ Name = 'Item 1' } | Invoke-PSRule -Option $option
```

Create a options object and run rules in constrained mode.

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

The path to a YAML file containing options.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSRule.Configuration.PSRuleOption

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
