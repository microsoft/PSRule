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
 [-Status <String[]>] [<CommonParameters>]
```

## DESCRIPTION

Evaluate pipeline objects against matching rules.

## EXAMPLES

### Example 1

```powershell
PS C:\> @{ Name = 'Item 1' } | Invoke-PSRule .
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

A path to one or more rules to evaluate.

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
Type: String[]
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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
