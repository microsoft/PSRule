---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/New-PSRuleOption/
schema: 2.0.0
---

# New-PSRuleOption

## SYNOPSIS

Create options to configure PSRule execution.

## SYNTAX

### FromPath (Default)

```text
New-PSRuleOption [[-Path] <String>] [-Configuration <ConfigurationOption>]
 [-SuppressTargetName <SuppressionOption>] [-BaselineGroup <Hashtable>] [-BindingIgnoreCase <Boolean>]
 [-BindingField <Hashtable>] [-BindingNameSeparator <String>]
 [-TargetName <String[]>] [-TargetType <String[]>] [-BindingUseQualifiedName <Boolean>]
 [-Convention <String[]>] [-ExecutionBreak <BreakLevel>] [-DuplicateResourceId <ExecutionActionPreference>]
 [-InitialSessionState <SessionState>] [-RestrictScriptSource <RestrictScriptSource>]
 [-SuppressionGroupExpired <ExecutionActionPreference>] [-ExecutionRuleExcluded <ExecutionActionPreference>]
 [-ExecutionRuleSuppressed <ExecutionActionPreference>] [-ExecutionAliasReference <ExecutionActionPreference>]
 [-ExecutionRuleInconclusive <ExecutionActionPreference>]
 [-ExecutionInvariantCulture <ExecutionActionPreference>]
 [-ExecutionUnprocessedObject <ExecutionActionPreference>] [-IncludeModule <String[]>]
 [-IncludePath <String[]>] [-InputFileObjects <Boolean>] [-InputStringFormat <String>]
 [-InputIgnoreGitPath <Boolean>] [-InputIgnoreRepositoryCommon <Boolean>] [-InputIgnoreObjectSource <Boolean>]
 [-InputIgnoreUnchangedPath <Boolean>] [-ObjectPath <String>] [-InputTargetType <String[]>]
 [-InputPathIgnore <String[]>] [-OutputAs <ResultFormat>]
 [-OutputBanner <BannerFormat>] [-OutputCulture <String[]>] [-OutputEncoding <OutputEncoding>]
 [-OutputFooter <FooterFormat>] [-OutputFormat <OutputFormat>] [-OutputJobSummaryPath <String>]
 [-OutputJsonIndent <Int32>] [-OutputOutcome <RuleOutcome>] [-OutputPath <String>]
 [-OutputSarifProblemsOnly <Boolean>] [-OutputStyle <OutputStyle>] [-OverrideLevel <Hashtable>]
 [-RepositoryBaseRef <String>] [-RepositoryUrl <String>] [-RuleIncludeLocal <Boolean>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromOption

```text
New-PSRuleOption [-Option] <PSRuleOption> [-Configuration <ConfigurationOption>]
 [-SuppressTargetName <SuppressionOption>] [-BaselineGroup <Hashtable>] [-BindingIgnoreCase <Boolean>]
 [-BindingField <Hashtable>] [-BindingNameSeparator <String>]
 [-TargetName <String[]>] [-TargetType <String[]>] [-BindingUseQualifiedName <Boolean>]
 [-Convention <String[]>] [-ExecutionBreak <BreakLevel>] [-DuplicateResourceId <ExecutionActionPreference>]
 [-InitialSessionState <SessionState>] [-RestrictScriptSource <RestrictScriptSource>]
 [-SuppressionGroupExpired <ExecutionActionPreference>] [-ExecutionRuleExcluded <ExecutionActionPreference>]
 [-ExecutionRuleSuppressed <ExecutionActionPreference>] [-ExecutionAliasReference <ExecutionActionPreference>]
 [-ExecutionRuleInconclusive <ExecutionActionPreference>]
 [-ExecutionInvariantCulture <ExecutionActionPreference>]
 [-ExecutionUnprocessedObject <ExecutionActionPreference>] [-IncludeModule <String[]>]
 [-IncludePath <String[]>] [-InputFileObjects <Boolean>] [-InputStringFormat <String>]
 [-InputIgnoreGitPath <Boolean>] [-InputIgnoreRepositoryCommon <Boolean>] [-InputIgnoreObjectSource <Boolean>]
 [-InputIgnoreUnchangedPath <Boolean>] [-ObjectPath <String>] [-InputTargetType <String[]>]
 [-InputPathIgnore <String[]>] [-OutputAs <ResultFormat>]
 [-OutputBanner <BannerFormat>] [-OutputCulture <String[]>] [-OutputEncoding <OutputEncoding>]
 [-OutputFooter <FooterFormat>] [-OutputFormat <OutputFormat>] [-OutputJobSummaryPath <String>]
 [-OutputJsonIndent <Int32>] [-OutputOutcome <RuleOutcome>] [-OutputPath <String>]
 [-OutputSarifProblemsOnly <Boolean>] [-OutputStyle <OutputStyle>] [-OverrideLevel <Hashtable>]
 [-RepositoryBaseRef <String>] [-RepositoryUrl <String>] [-RuleIncludeLocal <Boolean>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromDefault

```text
New-PSRuleOption [-Default] [-Configuration <ConfigurationOption>] [-SuppressTargetName <SuppressionOption>]
 [-BaselineGroup <Hashtable>] [-BindingIgnoreCase <Boolean>] [-BindingField <Hashtable>]
 [-BindingNameSeparator <String>] [-TargetName <String[]>]
 [-TargetType <String[]>] [-BindingUseQualifiedName <Boolean>] [-Convention <String[]>]
 [-ExecutionBreak <BreakLevel>] [-DuplicateResourceId <ExecutionActionPreference>]
 [-InitialSessionState <SessionState>] [-RestrictScriptSource <RestrictScriptSource>]
 [-SuppressionGroupExpired <ExecutionActionPreference>] [-ExecutionRuleExcluded <ExecutionActionPreference>]
 [-ExecutionRuleSuppressed <ExecutionActionPreference>] [-ExecutionAliasReference <ExecutionActionPreference>]
 [-ExecutionRuleInconclusive <ExecutionActionPreference>]
 [-ExecutionInvariantCulture <ExecutionActionPreference>]
 [-ExecutionUnprocessedObject <ExecutionActionPreference>] [-IncludeModule <String[]>]
 [-IncludePath <String[]>] [-InputFileObjects <Boolean>] [-InputStringFormat <String>]
 [-InputIgnoreGitPath <Boolean>] [-InputIgnoreRepositoryCommon <Boolean>] [-InputIgnoreObjectSource <Boolean>]
 [-InputIgnoreUnchangedPath <Boolean>] [-ObjectPath <String>] [-InputTargetType <String[]>]
 [-InputPathIgnore <String[]>] [-OutputAs <ResultFormat>]
 [-OutputBanner <BannerFormat>] [-OutputCulture <String[]>] [-OutputEncoding <OutputEncoding>]
 [-OutputFooter <FooterFormat>] [-OutputFormat <OutputFormat>] [-OutputJobSummaryPath <String>]
 [-OutputJsonIndent <Int32>] [-OutputOutcome <RuleOutcome>] [-OutputPath <String>]
 [-OutputSarifProblemsOnly <Boolean>] [-OutputStyle <OutputStyle>] [-OverrideLevel <Hashtable>]
 [-RepositoryBaseRef <String>] [-RepositoryUrl <String>] [-RuleIncludeLocal <Boolean>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

The **New-PSRuleOption** cmdlet creates an options object that can be passed to PSRule cmdlets to configure execution.

## EXAMPLES

### Example 1

```powershell
$option = New-PSRuleOption -Option @{ 'execution.mode' = 'ConstrainedLanguage' }
@{ Name = 'Item 1' } | Invoke-PSRule -Option $option
```

Create an options object and run rules in constrained mode.

### Example 2

```powershell
$option = New-PSRuleOption -SuppressTargetName @{ 'storageAccounts.UseHttps' = 'TestObject1', 'TestObject3' };
```

Create an options object that suppresses `TestObject1` and `TestObject3` for a rule named `storageAccounts.UseHttps`.

### Example 3

```powershell
$option = New-PSRuleOption -Configuration @{ 'appServiceMinInstanceCount' = 2 };
```

Create an options object that sets the `appServiceMinInstanceCount` baseline configuration option to `2`.

## PARAMETERS

### -Option

Additional options that configure execution.
Option also accepts a hashtable to configure options.
See about_PSRule_Options for more information.

```yaml
Type: PSRuleOption
Parameter Sets: FromOption
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

The path to a YAML file containing options.

Either a directory or file path can be specified.
When a directory is used, `ps-rule.yaml` will be used as the file name.

If the `-Path` parameter is specified and the file does not exist, an exception will be generated.

```yaml
Type: String
Parameter Sets: FromPath
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Default

When specified, defaults are used for any options not overridden.

```yaml
Type: SwitchParameter
Parameter Sets: FromDefault
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SuppressTargetName

Configures suppression for a list of objects by TargetName.
SuppressTargetName also accepts a hashtable to configure rule suppression.
See about_PSRule_Options for more information.

```yaml
Type: SuppressionOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Configuration

Configures a set of baseline configuration values that can be used in rule definitions instead of using hard coded values.
Configuration also accepts a hashtable of configuration values as key/ value pairs.
See about_PSRule_Options for more information.

```yaml
Type: ConfigurationOption
Parameter Sets: (All)
Aliases: BaselineConfiguration

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BaselineGroup

Sets the option `Baseline.Group`.
The option `Baseline.Group` allows a named group of baselines to be defined and later referenced.
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

### -ExecutionAliasReference

Sets the `Execution.AliasReference` option.
Determines how to handle when an alias to a resource is used.
See about_PSRule_Options for more information.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExecutionInvariantCulture

Sets the `Execution.InvariantCulture` option.
Determines how to report when an invariant culture is used.
See about_PSRule_Options for more information.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExecutionRuleInconclusive

Sets the `Execution.RuleInconclusive` option.
Determines how to handle rules that generate inconclusive results.
See about_PSRule_Options for more information.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExecutionUnprocessedObject

Sets the `Execution.UnprocessedObject` option.
Determines how to report objects that are not processed by any rule.
See about_PSRule_Options for more information.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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

### -InputIgnoreUnchangedPath

Sets the option `Input.IgnoreUnchangedPath`.
The `Input.IgnoreUnchangedPath` option determine if unchanged files are ignored.

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

### -OutputAs

Sets the option `Output.As`.
The `Output.As` option configures the type of results to produce, either detail or summary.

```yaml
Type: ResultFormat
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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
See about_PSRule_Options for more information.

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

### -OutputJobSummaryPath

Set the option `Output.JobSummaryPath`.
The `Output.JobSummaryPath` option configures the path to a job summary output file.

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

### -RepositoryBaseRef

Sets the option `Repository.BaseRef`.
The `Repository.BaseRef` option sets the repository base ref used for comparisons of changed files.

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

### -ExecutionRuleExcluded

Sets the option `Execution.RuleExcluded`.
The `Execution.RuleExcluded` option determines how to handle excluded rules.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExecutionRuleSuppressed

Sets the option `Execution.RuleSuppressed`.
The `Execution.RuleSuppressed` option determines how to handle suppressed rules.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SuppressionGroupExpired

Sets the option `Execution.SuppressionGroupExpired`.
The `Execution.SuppressionGroupExpired` option determines how to handle expired suppression groups.

```yaml
Type: ExecutionActionPreference
Parameter Sets: (All)
Aliases: ExecutionSuppressionGroupExpired

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExecutionBreak

Sets the option `Execution.Break`.
The `Execution.Break` option determines the minimum rule severity level that breaks the pipeline.

```yaml
Type: BreakLevel
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputFileObjects

Sets the option `Input.FileObjects`.
The `Input.FileObjects` option determines if file objects are processed by rules.

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

### -InputStringFormat

Sets the option `Input.StringFormat`.
The `Input.StringFormat` option determines how string input objects are processed.

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

### -OverrideLevel

Sets the option `Override.Level`.
The `Override.Level` option is used to override the severity level of one or more rules.

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

### -RestrictScriptSource

Sets the option `Execution.RestrictScriptSource`.
The `Execution.RestrictScriptSource` option configures where PowerShell language features are allowed to run from.

```yaml
Type: RestrictScriptSource
Parameter Sets: (All)
Aliases: ExecutionRestrictScriptSource

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSRule.Configuration.PSRuleOption

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)

[Set-PSRuleOption](Set-PSRuleOption.md)
