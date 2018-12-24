---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Get-PSRule.md
schema: 2.0.0
---

# Get-PSRule

## SYNOPSIS

Get a list of rule definitions.

## SYNTAX

```text
Get-PSRule [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>]
 [<CommonParameters>]
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

One or more paths to search for rule definitions within. If this parameter is not specified the current working path will be used.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: f

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSRule.Rules.Rule

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
