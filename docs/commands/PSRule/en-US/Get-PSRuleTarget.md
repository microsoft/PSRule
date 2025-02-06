---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Get-PSRuleTarget/
schema: 2.0.0
---

# Get-PSRuleTarget

## SYNOPSIS

Get a list of target objects.

## SYNTAX

### Input (Default)

```text
Get-PSRuleTarget [-Formats <String[]>] [-InputStringFormat <String>] [-Option <PSRuleOption>]
 [-ObjectPath <String>] -InputObject <PSObject> [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### InputPath

```text
Get-PSRuleTarget -InputPath <String[]> [-Formats <String[]>] [-InputStringFormat <String>]
 [-Option <PSRuleOption>] [-ObjectPath <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

Get a list of target objects from input.

## EXAMPLES

### Example 1

```powershell
Get-PSRuleTarget -InputPath .\resources.json;
```

Get target objects from `resources.json`.

## PARAMETERS

### -InputPath

Instead of processing objects from the pipeline, import objects from the specified file paths.

```yaml
Type: String[]
Parameter Sets: InputPath
Aliases: f

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Formats

Enables one or more formats by name to process files and deserialized objects.
Parameter is equivalent to setting `Format.<name>.Enabled` = `true` for each of the specified formats.

This parameter takes precedence over the `Format.<name>.Enabled` option if set.

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

### -InputStringFormat

Configures the input format for when a string is passed in as a target object.
This parameter also enables the format if it is not already enabled.

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
Set this option to an available format for example: `yaml`, `json`, `markdown`, `powershell_data`.

This parameter takes precedence over the `Input.StringFormat` option if set.

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

### -Option

Additional options that configure execution.
A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet.
Alternatively, a hashtable or path to YAML file can be specified with options.

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

### -ObjectPath

The name of a property to use instead of the pipeline object.
If the property specified by `ObjectPath` is a collection or an array, then each item in evaluated separately.

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

### -InputObject

The pipeline object to process rules for.

```yaml
Type: PSObject
Parameter Sets: Input
Aliases: TargetObject

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### System.Management.Automation.PSObject

## NOTES

## RELATED LINKS
