---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Set-PSRuleOption/
schema: 2.0.0
---

# Set-PSRuleOption

## SYNOPSIS

Sets options that configure PSRule execution.

## SYNTAX

```text
Set-PSRuleOption [[-Path] <String>] [-Option <PSRuleOption>] [-PassThru] [-Force] [-AllowClobber]
 [-BindingIgnoreCase <Boolean>] [-BindingField <Hashtable>] [-BindingNameSeparator <String>]
 [-BindingPreferTargetInfo <Boolean>] [-TargetName <String[]>] [-TargetType <String[]>]
 [-BindingUseQualifiedName <Boolean>] [-Convention <String[]>] [-AliasReferenceWarning <Boolean>]
 [-DuplicateResourceId <ExecutionActionPreference>] [-InconclusiveWarning <Boolean>]
 [-NotProcessedWarning <Boolean>] [-SuppressedRuleWarning <Boolean>] [-InvariantCultureWarning <Boolean>]
 [-InitialSessionState <SessionState>] [-IncludeModule <String[]>] [-IncludePath <String[]>]
 [-Format <InputFormat>] [-InputIgnoreGitPath <Boolean>] [-InputIgnoreObjectSource <Boolean>]
 [-InputIgnoreRepositoryCommon <Boolean>] [-ObjectPath <String>] [-InputPathIgnore <String[]>]
 [-InputTargetType <String[]>] [-LoggingLimitDebug <String[]>] [-LoggingLimitVerbose <String[]>]
 [-LoggingRuleFail <OutcomeLogStream>] [-LoggingRulePass <OutcomeLogStream>] [-OutputAs <ResultFormat>]
 [-OutputBanner <BannerFormat>] [-OutputCulture <String[]>] [-OutputEncoding <OutputEncoding>]
 [-OutputFooter <FooterFormat>] [-OutputFormat <OutputFormat>] [-OutputOutcome <RuleOutcome>]
 [-OutputPath <String>] [-OutputSarifProblemsOnly <Boolean>] [-OutputStyle <OutputStyle>]
 [-OutputJsonIndent <Int32>] [-RepositoryUrl <String>] [-RuleIncludeLocal <Boolean>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

Sets options that configure PSRule execution.

## EXAMPLES

### Example 1

```powershell
PS C:\> Set-PSRuleOption -OutputFormat Yaml;
```

Sets the `Output.Format` to `Yaml` for `ps-rule.yaml` in the current working path.
If the `ps-rule.yaml` file exists, it is merged with the existing file and overwritten.
If the file does not exist, a new file is created.

### Example 2

```powershell
PS C:\> Set-PSRuleOption -OutputFormat Yaml -Path .\project-options.yaml;
```

Sets the `Output.Format` to `Yaml` for `project-options.yaml` in the current working path.
If the `project-options.yaml` file exists, it is merged with the existing file and overwritten.
If the file does not exist, a new file is created.

## PARAMETERS

### -Path

The path to a YAML file where options will be set.

Either a directory or file path can be specified.
When a directory is used, `ps-rule.yaml` will be used as the file name.

The file will be created if it does not exist.
If the file already exists it will be merged with the existing options and **overwritten**.

If the directory does not exist an error will be generated.
To force the creation of the directory path use the `-Force` switch.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Option

An options object to use.

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

### -AllowClobber

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

Sets the option `Binding.IgnoreCase`.
The option `Binding.IgnoreCase` determines if binding operations are case-sensitive or not.
See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -BindingField

Sets the option `Binding.Field`.
The option specified one or more custom field bindings.
See about_PSRule_Options for more information.

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

### -BindingNameSeparator

Sets the option `Binding.NameSeparator`.
This option specifies the separator to use for qualified names.
See about_PSRule_Options for more information.

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

### -BindingPreferTargetInfo

Sets the option `Binding.PreferTargetInfo`.
This option specifies if automatic binding is preferred over configured binding options.
See about_PSRule_Options for more information.

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

### -Convention

Sets the `Option.ConventionInclude` option.
This option specifies the name of conventions to execute in the pipeline when processing objects.
See about_PSRule_Options for more information.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: ConventionInclude

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetName

Sets the option `Binding.TargetName`.
This option specifies one or more properties of _TargetObject_ to use to bind _TargetName_ to.
See about_PSRule_Options for more information.

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

Sets the option `Binding.TargetType`.
This option specifies one or more properties of _TargetObject_ to use to bind _TargetType_ to.
See about_PSRule_Options for more information.

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

### -BindingUseQualifiedName

Sets the option `Binding.UseQualifiedName`.
This option specifies is qualified target names are used.
See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InconclusiveWarning

Sets the option `Execution.InconclusiveWarning`.
The `Execution.InconclusiveWarning` option determines if a warning is generated when the outcome of a rule is inconclusive.
See about_PSRule_Options for more information.

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

Sets the `Execution.NotProcessedWarning` option.
The `Execution.NotProcessedWarning` option determines if a warning is generated when an object is not processed by any rule.
See about_PSRule_Options for more information.

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

### -SuppressedRuleWarning

Sets the `Execution.SuppressedRuleWarning` option.
The `Execution.SuppressedRuleWarning` option determines if a warning is generated when a rule is suppressed.
See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionSuppressedRuleWarning

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeModule

Sets the `Include.Module` option to include additional module sources.
See about_PSRule_Options for more information.

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

### -IncludePath

Sets the `Include.Path` option to include additional standalone sources.
See about_PSRule_Options for more information.

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

### -Format

Sets the `Input.Format` option to configure the input format for when a string is passed in as a target object.
See about_PSRule_Options for more information.

```yaml
Type: InputFormat
Parameter Sets: (All)
Aliases: InputFormat
Accepted values: None, Yaml, Json, Markdown, PowerShellData, File, Detect

Required: False
Position: Named
Default value: Detect
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputIgnoreGitPath

Sets the `Input.IgnoreGitPath` option to determine if files within the .git path are ignored.
See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputIgnoreRepositoryCommon

Sets the `Input.IgnoreRepositoryCommon` option to determine if files common repository files are ignored.
See about_PSRule_Options for more information.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectPath

Sets the `Input.ObjectPath` option to use an object path to use instead of the pipeline object.
See about_PSRule_Options for more information.

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

### -InputPathIgnore

Sets the `Input.PathIgnore` option.
If specified, files that match the path spec will not be processed.
See about_PSRule_Options for more information.

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

### -InputTargetType

Sets the `Input.TargetType` option to only process objects with the specified TargetType.

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

### -InputIgnoreObjectSource

Sets the option `Input.IgnoreObjectSource`.
The `Input.IgnoreObjectSource` option determines if objects will be skipped if the source path has been ignored.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LoggingLimitDebug

Sets the `Logging.LimitDebug` option to limit debug messages to a list of named debug scopes.

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

### -LoggingLimitVerbose

Sets the `Logging.LimitVerbose` option to limit verbose messages to a list of named verbose scopes.

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

Sets the option `Output.As`.
The `Output.As` option configures the type of results to produce, either detail or summary.

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

### -OutputBanner

Sets the option `Output.Banner`.
The `Output.Banner` option configure information displayed with PSRule banner.
This option is only applicable when using `Assert-PSRule` cmdlet.

```yaml
Type: BannerFormat
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Default
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputCulture

Sets the option `Output.Culture`.
The `Output.Culture` option configures the culture used to generated output.
When multiple cultures are specified, the first matching culture will be used.

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

### -OutputEncoding

Sets the option `Output.Encoding`.
The `Output.Encoding` option configured the encoding used to write results to file.

```yaml
Type: OutputEncoding
Parameter Sets: (All)
Aliases:
Accepted values: Default, UTF8, UTF7, Unicode, UTF32, ASCII

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputFooter

Sets the option `Output.Footer`.
The `Output.Footer` option configures the information displayed for PSRule footer.
See about_PSRule_Options for more information.

```yaml
Type: FooterFormat
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Default
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputFormat

Sets the option `Output.Format`.
The `Output.Format` option configures the format that results will be presented in.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Yaml, Json, Markdown, NUnit3, Csv, Wide, Sarif

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputJsonIndent

Sets the option `Output.JsonIndent`.
The `Output.JsonIndent` option configures indentation for JSON output.

This option only applies to `Get-PSRule`, `Invoke-PSRule` and `Assert-PSRule` cmdlets.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: JsonIndent
Accepted values: 0, 1, 2, 3, 4

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputOutcome

Sets the `Output.Outcome` option.
This option can be set to include or exclude output results.
See about_PSRule_Options for more information.

```yaml
Type: RuleOutcome
Parameter Sets: (All)
Aliases: Outcome

Required: False
Position: Named
Default value: Processed
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath

Sets the option `Output.Path`.
The `Output.Path` option configures an output file path to write results.

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

### -OutputSarifProblemsOnly

Sets the option `Option.SarifProblemsOnly`.
The `Output.SarifProblemsOnly` option determines if SARIF output only includes fail and error outcomes.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputStyle

Sets the option `Option.Style`.
The `Output.Style` option configures the style that results will be presented in.

This option only applies to `Assert-PSRule`.

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

### -RepositoryUrl

Sets the option `Repository.Url`.
The `Repository.Url` option sets the repository URL reported in output.

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

### -RuleIncludeLocal

Sets the option `Rule.IncludeLocal`.
The `Rule.IncludeLocal` option configures if local rules are automatically included.
See about_PSRule_Options for more information.

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

### -AliasReferenceWarning

Sets the option `Execution.AliasReferenceWarning`.
The `Execution.AliasReferenceWarning` option determines if a warning is logged when alises are referenced.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionAliasReferenceWarning

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InvariantCultureWarning

Sets the option `Execution.InvariantCultureWarning`.
The `Execution.InvariantCultureWarning` option set if a warning is logged when invarient culture is detected.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: ExecutionInvariantCultureWarning

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DuplicateResourceId

Sets the option `Execution.DuplicateResourceId`.
The `Execution.DuplicateResourceId` option determines how to handle duplicate resources identifiers during execution.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases: ExecutionDuplicateResourceId

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InitialSessionState

Sets the option `Execution.InitialSessionState`.
The `Execution.InitialSessionState` option determines how the initial session state for executing PowerShell code is created.

```yaml
Type: SessionState
Parameter Sets: (All)
Aliases: ExecutionInitialSessionState

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

### PSRule.Configuration.PSRuleOption

When you use the `-PassThru` switch, an options object is returned to the pipeline.

## NOTES

## RELATED LINKS

[New-PSRuleOption](New-PSRuleOption.md)
