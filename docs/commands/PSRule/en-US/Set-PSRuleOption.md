---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://berniewhite.github.io/PSRule/commands/PSRule/en-US/Set-PSRuleOption.html
schema: 2.0.0
---

# Set-PSRuleOption

## SYNOPSIS

Sets options that configure PSRule execution.

## SYNTAX

```text
Set-PSRuleOption [[-Path] <String>] [-Option <PSRuleOption>] [-PassThru] [-Force] [-Overwrite]
 [-BindingIgnoreCase <Boolean>] [-TargetName <String[]>] [-TargetType <String[]>]
 [-InconclusiveWarning <Boolean>] [-NotProcessedWarning <Boolean>] [-Format <InputFormat>]
 [-ObjectPath <String>] [-LoggingRuleFail <OutcomeLogStream>] [-LoggingRulePass <OutcomeLogStream>]
 [-OutputAs <ResultFormat>] [-OutputFormat <OutputFormat>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

Sets options that configure PSRule execution.

## EXAMPLES

### Example 1

```powershell
PS C:\> Set-PSRuleOption -OutputFormat Yaml;
```

Sets the `Output.Format` to `Yaml` for `ps-rule.yaml` in the current working path. If the `ps-rule.yaml` file exists, it is merged with the existing file and overwritten. If the file does not exist, a new file is created.

## PARAMETERS

### -Path

The path to a YAML file where options will be set. By default the current working path (`$PWD`) is used.

Either a directory or file path can be specified. When a directory is used, `ps-rule.yaml` will be used as the file name.

The file will be created if it does not exist. If the file already exists it will be merged with the existing options and **overwritten**.

If the directory does not exist an error will be generated. To force the creation of the directory path use the `-Force` switch.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: .\ps-rule.yml
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
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PassThru

Use this option to return the options object to the pipeline instead of saving to disk.

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

### -Force

Force creation of directory path for Path parameter, when the directory does not already exist.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Overwrite

Overwrite YAML files that contain comments.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BindingIgnoreCase

Sets the option `Binding.IgnoreCase`. The option `Binding.IgnoreCase` determines if binding operations are case-sensitive or not. See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetName

Sets the option `Binding.TargetName`. This option specifies one or more properties of _TargetObject_ to use to bind _TargetName_ to. See about_PSRule_Options for more information.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: BindingTargetName

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetType

Sets the option `Binding.TargetType`. This option specifies one or more properties of _TargetObject_ to use to bind _TargetType_ to. See about_PSRule_Options for more information.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: BindingTargetType

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InconclusiveWarning

Sets the option `Execution.InconclusiveWarning`. The `Execution.InconclusiveWarning` option determines if a warning is generated when the outcome of a rule is inconclusive. See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionInconclusiveWarning

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -NotProcessedWarning

Sets the `Execution.NotProcessedWarning` option. The `Execution.NotProcessedWarning` option determines if a warning is generated when an object is not processed by any rule. See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionNotProcessedWarning

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -Format

Sets the `Input.Format` option to configure the input format for when a string is passed in as a target object.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases: InputFormat
Accepted values: None, Yaml, Json, Detect

Required: False
Position: Named
Default value: Detect
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectPath

Sets the `Input.ObjectPath` option to use an object path to use instead of the pipeline object.

```yaml
Type: String
Parameter Sets: (All)
Aliases: InputObjectPath

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LoggingRuleFail

Sets the `Logging.RuleFail` option to generate an informational message for each rule fail.

```yaml
Type: OutcomeLogStream
Parameter Sets: (All)
Aliases:
Accepted values: None, Error, Warning, Information

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LoggingRulePass

Sets the `Logging.RulePass` option to generate an informational message for each rule pass.

```yaml
Type: OutcomeLogStream
Parameter Sets: (All)
Aliases:
Accepted values: None, Error, Warning, Information

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputAs

Sets the option `Output.As`. The `Output.As` option configures the type of results to produce, either detail or summary.

```yaml
Type: ResultFormat
Parameter Sets: (All)
Aliases:
Accepted values: Detail, Summary

Required: False
Position: Named
Default value: Detail
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputFormat

Sets the option `Output.Format`. The `Output.Format` option configures the format that results will be presented in.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, NUnit3

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf

Shows what would happen if the cmdlet runs. The cmdlet is not run.

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

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### PSRule.Configuration.PSRuleOption

When you use the `-PassThru` switch, an options object is returned to the pipeline.

## NOTES

## RELATED LINKS

[New-PSRuleOption](New-PSRuleOption.md)
