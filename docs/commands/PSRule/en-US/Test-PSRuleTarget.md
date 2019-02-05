---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Test-PSRuleTarget.md
schema: 2.0.0
---

# Test-PSRuleTarget

## SYNOPSIS

Pass or fail pipeline objects against matching rules.

## SYNTAX

```text
Test-PSRuleTarget [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] -InputObject <PSObject>
 [-Option <PSRuleOption>] [-Format <InputFormat>] [<CommonParameters>]
```

## DESCRIPTION

Evaluate pipeline objects against matching rules and return an overall pass or fail for the object as `$True` (pass) or `$False` (fail).

PSRule uses the following logic to determine overall pass or fail for an object:

- The object fails if:
  - Any rules fail or error.
  - Any rules are inconclusive.
- The object passes if:
  - No rules were found that match preconditions, name and tag filters.
  - All rules pass.

## EXAMPLES

### Example 1

```powershell
@{ Name = 'Item 1' } | Test-PSRuleTarget;
```

Evaluate a simple hashtable on the pipeline against rules loaded from the current working path.

## PARAMETERS

### -Path

One or more paths to search for rule definitions within. If this parameter is not specified the current working path will be used.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: f

Required: False
Position: 1
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

The name of a specific rule to evaluate. If this parameter is not specified all rules in search paths will be evaluated.

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

### -InputObject

The pipeline object to process rules for.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases: TargetObject

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### -Format

Configures the input format for when a string is passed in as a target object. By default, strings are just treated as raw text. However when set strings can be read as YAML or JSON and converted to an object.

The following formats are available:

- None
- Yaml
- Json

```yaml
Type: InputFormat
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

## OUTPUTS

### System.Boolean

Returns `$True` when the object passes and `$False` when the object fails.

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
[Get-PSRule](Get-PSRule.md)
