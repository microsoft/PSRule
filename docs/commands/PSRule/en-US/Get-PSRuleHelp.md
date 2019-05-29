---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://berniewhite.github.io/PSRule/commands/PSRule/en-US/Get-PSRuleHelp.html
schema: 2.0.0
---

# Get-PSRuleHelp

## SYNOPSIS

Get documentation for a rule.

## SYNTAX

```text
Get-PSRuleHelp [[-Path] <String>] -Name <String> [-Module <String>] [-Culture <String>] [-Online]
 [<CommonParameters>]
```

## DESCRIPTION

Get documentation for a rule.

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-PSRuleHelp Azure.ACR.AdminUser
```

{{ Add example description here }}

## PARAMETERS

### -Name

The name of the rule to get documentation for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: n

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

A list of paths to check for rule definitions

```yaml
Type: String
Parameter Sets: (All)
Aliases: p

Required: False
Position: 1
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Module

{{ Fill Module Description }}

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

### -Online

Instead of displaying documentation within PowerShell, browse to the online version using the default web browser.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### PSRule.Rules.RuleHelpInfo

## NOTES

## RELATED LINKS
