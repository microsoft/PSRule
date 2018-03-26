---
external help file: PSRule-help.xml
Module Name: PSRule
online version:
schema: 2.0.0
---

# Invoke-RuleEngine

## SYNOPSIS

Evaluate pipeline objects against matching rules.

## SYNTAX

```
Invoke-RuleEngine [-Path] <String> [[-ConfigurationData] <Object>] [-InputObject] <PSObject>
 [[-Status] <String[]>] [<CommonParameters>]
```

## DESCRIPTION

Evaluate pipeline objects against matching rules.

## EXAMPLES

### Example 1

```powershell
PS C:\> @{ Name = 'Item 1' } | Invoke-RuleEngine .
```

{{ Add example description here }}

## PARAMETERS

### -ConfigurationData
{{Fill ConfigurationData Description}}

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

The pipeline object to process rules for.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Path

A path to one or more rules to evaluate.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
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
Position: 3
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
