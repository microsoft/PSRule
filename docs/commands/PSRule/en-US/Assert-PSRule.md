---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/commands/PSRule/en-US/Assert-PSRule.html
schema: 2.0.0
---

# Assert-PSRule

## SYNOPSIS

Evaluate objects against matching rules and assert any failures.

## SYNTAX

### Input (Default)

```text
Assert-PSRule [-Module <String[]>] [-Format <InputFormat>] [-Baseline <BaselineOption>] [-Style <OutputStyle>]
 [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-OutputPath <String>]
 [-OutputFormat <OutputFormat>] [-Option <PSRuleOption>] [-ObjectPath <String>] [-TargetType <String[]>]
 [-Culture <String[]>] -InputObject <PSObject> [-ResultVariable <String>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### InputPath

```text
Assert-PSRule -InputPath <String[]> [-Module <String[]>] [-Format <InputFormat>] [-Baseline <BaselineOption>]
 [-Style <OutputStyle>] [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-OutputPath <String>]
 [-OutputFormat <OutputFormat>] [-Option <PSRuleOption>] [-ObjectPath <String>] [-TargetType <String[]>]
 [-Culture <String[]>] [-ResultVariable <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

Evaluate objects against matching rules and assert any failures.
Objects can be specified directly from the pipeline or provided from file.

The commands `Invoke-PSRule` and `Assert-PSRule` provide similar functionality, as differ as follows:

- `Invoke-PSRule` writes results as structured objects
- `Assert-PSRule` writes results as a formatted string.

## EXAMPLES

### Example 1

```powershell
@{ Name = 'Item 1' } | Assert-PSRule;
```

Evaluate a simple hashtable on the pipeline against rules loaded from the current working path.

### Example 2

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item using rules saved in current working path
$items | Assert-PSRule -Path .\docs\scenarios\fruit\
```

```text
-> Fridge : System.Management.Automation.PSCustomObject

   [FAIL] isFruit

-> Apple : System.Management.Automation.PSCustomObject

   [PASS] isFruit

Assert-PSRule : One or more rules reported failure.
At line:1 char:10
+ $items | Assert-PSRule -Path .\docs\scenarios\fruit\
+          ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
+ CategoryInfo          : InvalidData: (:) [Assert-PSRule], FailPipelineException
+ FullyQualifiedErrorId : PSRule.Fail,Assert-PSRule
```

Evaluate an array of objects on the pipeline against rules loaded a specified relative path.

### Example 3

```powershell
$items | Assert-PSRule -Module PSRule.Rules.Azure -o NUnit3 -OutputPath .\reports\results.xml
```

Evaluate items from a pre-installed rules module PSRule.Rules.Azure.
Additionally save the results as a NUnit report.

### Example 4

```powershell
$items | Assert-PSRule -Path .\docs\scenarios\fruit\ -ResultVariable resultRecords;
```

Evaluate items and additionally save the results into a variable `resultRecords`.

## PARAMETERS

### -InputPath

Instead of processing objects from the pipeline, import objects file the specified file paths.

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

### -Format

Configures the input format for when a string is passed in as a target object.

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
Set this option to either `Yaml`, `Json`, `Markdown`, `PowerShellData` to have PSRule deserialize the object.

When the `-InputPath` parameter is used with a file path or URL.
If the `Detect` format is used, the file extension will be used to automatically detect the format.
When `-InputPath` is not used, `Detect` is the same as `None`.

See `about_PSRule_Options` for details.

This parameter takes precedence over the `Input.Format` option if set.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, Markdown, PowerShellData, File, Detect

Required: False
Position: Named
Default value: Detect
Accept pipeline input: False
Accept wildcard characters: False
```

### -Baseline

Specifies an explicit baseline by name to use for evaluating rules.
Baselines can contain filters and custom configuration that overrides the defaults.

```yaml
Type: BaselineOption
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
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Module

Search for rule definitions within a module.
When specified without the `-Path` parameter, only rule definitions in the module will be discovered.

When both `-Path` and `-Module` are specified, rule definitions from both are discovered.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: m

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

The name of a specific rule to evaluate.
If this parameter is not specified all rules in search paths will be evaluated.

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

### -TargetType

Filters input objects by TargetType.

If specified, only objects with the specified TargetType are processed.
Objects that do not match TargetType are ignored.
If multiple values are specified, only one TargetType must match. This parameter is not case-sensitive.

By default, all objects are processed.

This parameter if set, overrides the `Input.TargetType` option.

To change the field TargetType is bound to set the `Binding.TargetType` option.
For details see the about_PSRule_Options help topic.

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

### -OutputPath

Specifies the output file path to write results.
Directories along the file path will automatically be created if they do not exist.

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

### -OutputFormat

Configures the format that output is written.
This parameter has no affect when `-OutputPath` is not specified.

The following format options are available:

- None - Output is presented as an object using PowerShell defaults.
This is the default.
- Yaml - Output is serialized as YAML.
- Json - Output is serialized as JSON.
- Markdown - Output is serialized as Markdown.
- NUnit3 - Output is serialized as NUnit3 (XML).
- Csv - Output is serialized as a comma separated values (CSV).

The `Wide` format is not applicable to `Assert-PSRule`.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases: o
Accepted values: None, Yaml, Json, Markdown, NUnit3, Csv

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Style

Configures the style that results will be presented in.

The following styles are available:

- Client - Output is written to the host directly in green/ red to indicate outcome. This is the default.
- Plain - Output is written to the host as a plain string.
- AzurePipelines - Output is written with commands that can be interpreted by Azure Pipelines.
- GitHubActions - Output is written with commands that can be interpreted by GitHub Actions.

Each of these styles outputs to the host. To capture output as a string redirect the information stream.
For example: `6>&1`

```yaml
Type: OutputStyle
Parameter Sets: (All)
Aliases:
Accepted values: Client, Plain, AzurePipelines, GitHubActions

Required: False
Position: Named
Default value: Client
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

One or more paths to search for rule definitions within.

If this parameter is not specified the current working path will be used, unless the `-Module` parameter is used.

If the `-Module` parameter is used, rule definitions from the currently working path will not be included by default.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: p

Required: False
Position: 0
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

Only get rules with the specified tags set.
If this parameter is not specified all rules in search paths will be returned.

When more than one tag is used, all tags must match. Tags are not case sensitive.
A tag value of `*` may be used to filter rules to any rule with the tag set, regardless of tag value.

An array of tag values can be used to match a rule with either value.
i.e. `severity = important, critical` matches rules with a category of either `important` or `critical`.

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

### -ResultVariable

Stores output result objects in the specified variable.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

You can pipe any object to **Assert-PSRule**.

## OUTPUTS

### System.String

## NOTES

## RELATED LINKS

[Get-PSRule](Get-PSRule.md)

[Invoke-PSRule](Invoke-PSRule.md)

[Test-PSRuleTarget](Test-PSRuleTarget.md)
