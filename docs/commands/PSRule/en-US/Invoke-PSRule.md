---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md
schema: 2.0.0
---

# Invoke-PSRule

## SYNOPSIS

Evaluate pipeline objects against matching rules.

## SYNTAX

```text
Invoke-PSRule [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] -InputObject <PSObject>
 [-Status <RuleResultOutcome>] [-Option <PSRuleOption>] [<CommonParameters>]
```

## DESCRIPTION

Evaluate pipeline objects against matching rules.

## EXAMPLES

### Example 1

```powershell
PS C:\> @{ Name = 'Item 1' } | Invoke-PSRule
```

Evaluate a simple hashtable on the pipeline against rules loaded from the current working path.

## PARAMETERS

### -InputObject

The pipeline object to process rules for.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Name

The name of a specific rule to evaluate. If this parameter is not specified all rules in search paths will be evaluated.

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

### -Path

One or more paths to search for rule definitions within. If this parameter is not specified the current working path will be used.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status

Filter output to only show rules with a specific status.

```yaml
Type: RuleResultOutcome
Parameter Sets: (All)
Aliases:
Accepted values: Success, Failed

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

Only evaluate rules with the specified tags set. If this parameter is not specified all rules in search paths will be evaluated.

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

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### PSRule.Rules.RuleResult

## NOTES

## RELATED LINKS

[Get-PSRule](Get-PSRule.md)
