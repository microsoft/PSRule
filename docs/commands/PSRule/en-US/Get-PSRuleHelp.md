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
Get-PSRuleHelp [-Module <String>] [-Online] [[-Name] <String>] [-Path <String>] [-Option <PSRuleOption>]
 [-Culture <String>] [<CommonParameters>]
```

## DESCRIPTION

Get documentation for a rule.

## EXAMPLES

### Example 1

```powershell
Get-PSRuleHelp;
```

Get a list of rule help within the current path or loaded modules.

### Example 2

```powershell
Get-PSRuleHelp Azure.ACR.AdminUser;
```

Get rule documentation for the rule `Azure.ACR.AdminUser`.

### Example 3

```powershell
Get-PSRuleHelp Azure.ACR.AdminUser -Online;
```

Browse to the online version of documentation for `Azure.ACR.AdminUser` using the default web browser.

## PARAMETERS

### -Name

The name of the rule to get documentation for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: n

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Path

A path to check documentation for.
By default, help from the current working path and loaded modules is listed.
Results can be filtered by using `-Name`, `-Path` or `-Module`.

```yaml
Type: String
Parameter Sets: (All)
Aliases: p

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Module

Limit returned information to rules in the specified module.
By default, help from the current working path and loaded modules is listed.
Results can be filtered by using `-Name`, `-Path` or `-Module`.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSRule.Rules.RuleHelpInfo

## NOTES

## RELATED LINKS
