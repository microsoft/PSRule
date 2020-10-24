# PSRule_Options

## about_PSRule_Options

## SHORT DESCRIPTION

Describes additional options that can be used during rule execution.

## LONG DESCRIPTION

PSRule lets you use options when calling cmdlets such as `Invoke-PSRule` and `Test-PSRuleTarget` to change how rules are processed.
This topic describes what options are available, when to and how to use them.

The following workspace options are available for use:

- [Execution.LanguageMode](#executionlanguagemode)
- [Execution.InconclusiveWarning](#executioninconclusivewarning)
- [Execution.NotProcessedWarning](#executionnotprocessedwarning)
- [Input.Format](#inputformat)
- [Input.ObjectPath](#inputobjectpath)
- [Input.PathIgnore](#inputpathignore)
- [Input.TargetType](#inputtargettype)
- [Logging.LimitDebug](#logginglimitdebug)
- [Logging.LimitVerbose](#logginglimitverbose)
- [Logging.RuleFail](#loggingrulefail)
- [Logging.RulePass](#loggingrulepass)
- [Output.As](#outputas)
- [Output.Culture](#outputculture)
- [Output.Encoding](#outputencoding)
- [Output.Format](#outputformat)
- [Output.Outcome](#outputoutcome)
- [Output.Path](#outputpath)
- [Output.Style](#outputstyle)
- [Requires](#requires)
- [Suppression](#suppression)

Additionally the following baseline options can be included:

- [Binding.Field](#bindingfield)
- [Binding.IgnoreCase](#bindingignorecase)
- [Binding.NameSeparator](#bindingnameseparator)
- [Binding.TargetName](#bindingtargetname)
- [Binding.TargetType](#bindingtargettype)
- [Binding.UseQualifiedName](#bindingusequalifiedname)
- [Configuration](#configuration)
- [Rule.Include](#ruleinclude)
- [Rule.Exclude](#ruleexclude)
- [Rule.Tag](#ruletag)

See [about_PSRule_Baseline](about_PSRule_Baseline.md) for more information on baseline options.

Options can be used with the following PSRule cmdlets:

- Get-PSRule
- Get-PSRuleBaseline
- Get-PSRuleHelp
- Invoke-PSRule
- Test-PSRuleTarget

Each of these cmdlets support:

- Using the `-Option` parameter with an object created with the `New-PSRuleOption` cmdlet. See cmdlet help for syntax and examples.
- Using the `-Option` parameter with a hashtable object.
- Using the `-Option` parameter with a YAML file path.

When using a hashtable object `@{}`, one or more options can be specified as keys using a dotted notation.

For example:

```powershell
$option = @{ 'Output.Format' = 'Yaml' };
Invoke-PSRule -Path . -Option $option;
```

```powershell
Invoke-PSRule -Path . -Option @{ 'Output.Format' = 'Yaml' };
```

The above example shows how the `Output.Format` option as a hashtable key can be used.
Continue reading for a full list of options and how each can be used.

Alternatively, options can be stored in a YAML formatted file and loaded from disk.
Storing options as YAML allows different configurations to be loaded in a repeatable way instead of having to create an options object each time.

Options are stored as YAML properties using a lower camel case naming convention, for example:

```yaml
output:
  format: Yaml
```

The `Set-PSRuleOption` cmdlet can be used to set options stored in YAML or the YAML file can be manually edited.

```powershell
Set-PSRuleOption -OutputFormat Yaml;
```

By default, PSRule will automatically look for a default YAML options file in the current working directory. Alternatively, you can specify a specific file path.

For example:

```powershell
Invoke-PSRule -Option '.\myconfig.yml';
```

```powershell
New-PSRuleOption -Path '.\myconfig.yaml';
```

PSRule uses any of the following file names (in order) as the default YAML options file.
If more than one of these files exist, the following order will be used to find the first match.

- `ps-rule.yaml`
- `ps-rule.yml`
- `psrule.yaml`
- `psrule.yml`

We recommend only using lowercase characters as shown above.
This is because not all operation systems treat case in the same way.

### Binding.Field

When an object is passed from the pipeline, PSRule automatically extracts fields from object properties.
PSRule provides standard fields such as `TargetName` and `TargetType`.
In addition to standard fields, custom fields can be bound.
Custom fields are available to rules and included in output.

PSRule uses the following logic to determine which property should be used for binding:

- By default PSRule will not extract any custom fields.
- If custom fields are configured, PSRule will attempt to bind the field.
  - If **none** of the configured property names exist, the field will be skipped.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity. To use a case sensitive match, configure the [Binding.IgnoreCase](#bindingignorecase) option.

Custom field bindings can be specified using:

```powershell
# PowerShell: Using the BindingField parameter
$option = New-PSRuleOption -BindingField @{ id = 'ResourceId', 'AlternativeId' };
```

```powershell
# PowerShell: Using the Binding.Field hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.Field' = @{ id = 'ResourceId', 'AlternativeId' } };
```

```powershell
# PowerShell: Using the BindingField parameter to set YAML
Set-PSRuleOption -BindingField @{ id = 'ResourceId', 'AlternativeId' };
```

```yaml
# YAML: Using the binding/field property
binding:
  field:
    id:
    - ResourceId
    - AlternativeId
```

### Binding.IgnoreCase

When evaluating an object, PSRule extracts a few key properties from the object to help filter rules and display output results.
The process of extract these key properties is called _binding_.
The properties that PSRule uses for binding can be customized by providing a order list of alternative properties to use.
See [`Binding.TargetName`](#bindingtargetname) and [`Binding.TargetType`](#bindingtargettype) for these options.

- By default, custom property binding finds the first matching property by name regardless of case. i.e. `Binding.IgnoreCase` is `true`.
- To make custom bindings case sensitive, set the `Binding.IgnoreCase` option to `false`.
  - Changing this option will affect custom property bindings for both _TargetName_ and _TargetType_.
  - Setting this option has no affect on binding defaults or custom scripts.

This option can be specified using:

```powershell
# PowerShell: Using the BindingIgnoreCase parameter
$option = New-PSRuleOption -BindingIgnoreCase $False;
```

```powershell
# PowerShell: Using the Binding.IgnoreCase hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.IgnoreCase' = $False };
```

```powershell
# PowerShell: Using the BindingIgnoreCase parameter to set YAML
Set-PSRuleOption -BindingIgnoreCase $False;
```

```yaml
# YAML: Using the binding/ignoreCase property
binding:
  ignoreCase: false
```

### Binding.NameSeparator

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_.
_TargetName_ is used in output results to identify one object from another.

In cases where different types of objects share the same _TargetName_, this may become confusing.
Using a qualified name, prefixes the _TargetName_ with _TargetType_.
i.e. _TargetType/TargetName_

To use a qualified name, see the `Binding.UseQualifiedName` option.

By default, PSRule uses `/` to separate _TargetType_ from _TargetName_.
This option configures the separator that PSRule uses between the two components.

This option can be specified using:

```powershell
# PowerShell: Using the BindingNameSeparator parameter
$option = New-PSRuleOption -BindingNameSeparator '::';
```

```powershell
# PowerShell: Using the Binding.NameSeparator hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.NameSeparator' = '::' };
```

```powershell
# PowerShell: Using the BindingNameSeparator parameter to set YAML
Set-PSRuleOption -BindingNameSeparator '::';
```

```yaml
# YAML: Using the binding/nameSeparator property
binding:
  nameSeparator: '::'
```

### Binding.TargetName

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_.
_TargetName_ is used in output results to identify one object from another.
Many objects could be passed down the pipeline at the same time, so using a _TargetName_ that is meaningful is important.
_TargetName_ is also used for advanced features such as rule suppression.

The value that PSRule uses for _TargetName_ is configurable. PSRule uses the following logic to determine what _TargetName_ should be used:

- By default PSRule will:
  - Use `TargetName` or `Name` properties on the object. These property names are case insensitive.
  - If both `TargetName` and `Name` properties exist, `TargetName` will take precedence over `Name`.
  - If neither `TargetName` or `Name` properties exist, a SHA1 hash of the object will be used as _TargetName_.
- If custom _TargetName_ binding properties are configured, the property names specified will override the defaults.
  - If **none** of the configured property names exist, PSRule will revert back to `TargetName` then `Name`.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity. To use a case sensitive match, configure the [Binding.IgnoreCase](#bindingignorecase) option.
- If a custom _TargetName_ binding function is specified, the function will be evaluated first before any other option.
  - If the function returns `$Null` then custom properties, `TargetName` and `Name` properties will be used.
  - The custom binding function is executed outside the PSRule engine, so PSRule keywords and variables will not be available.
  - Custom binding functions are blocked in constrained language mode is used. See [language mode](#executionlanguagemode) for more information.

Custom property names to use for binding can be specified using:

```powershell
# PowerShell: Using the TargetName parameter
$option = New-PSRuleOption -TargetName 'ResourceName', 'AlternateName';
```

```powershell
# PowerShell: Using the Binding.TargetName hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' };
```

```powershell
# PowerShell: Using the TargetName parameter to set YAML
Set-PSRuleOption -TargetName 'ResourceName', 'AlternateName';
```

```yaml
# YAML: Using the binding/targetName property
binding:
  targetName:
  - ResourceName
  - AlternateName
```

To specify a custom binding function use:

```powershell
# Create a custom function that returns a TargetName string
$bindFn = {
    param ($TargetObject)

    $otherName = $TargetObject.PSObject.Properties['OtherName'];
    if ($Null -eq $otherName) { return $Null }
    return $otherName.Value;
}

# Specify the binding function script block code to execute
$option = New-PSRuleOption -BindTargetName $bindFn;
```

### Binding.TargetType

When an object is passed from the pipeline, PSRule assigns the object a _TargetType_.
_TargetType_ is used to filter rules based on object type and appears in output results.

The value that PSRule uses for _TargetType_ is configurable.
PSRule uses the following logic to determine what _TargetType_ should be used:

- By default PSRule will:
  - Use the default type presented by PowerShell from `TypeNames`. i.e. `.PSObject.TypeNames[0]`
- If custom _TargetType_ binding properties are configured, the property names specified will override the defaults.
  - If **none** of the configured property names exist, PSRule will revert back to the type presented by PowerShell.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity. To use a case sensitive match, configure the [`Binding.IgnoreCase`](#bindingignorecase) option.
- If a custom _TargetType_ binding function is specified, the function will be evaluated first before any other option.
  - If the function returns `$Null` then custom properties, or the type presented by PowerShell will be used in order instead.
  - The custom binding function is executed outside the PSRule engine, so PSRule keywords and variables will not be available.
  - Custom binding functions are blocked in constrained language mode is used. See [language mode](#executionlanguagemode) for more information.

Custom property names to use for binding can be specified using:

```powershell
# PowerShell: Using the TargetType parameter
$option = New-PSRuleOption -TargetType 'ResourceType', 'kind';
```

```powershell
# PowerShell: Using the Binding.TargetType hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'ResourceType', 'kind' };
```

```powershell
# PowerShell: Using the TargetType parameter to set YAML
Set-PSRuleOption -TargetType 'ResourceType', 'kind';
```

```yaml
# YAML: Using the binding/targetType property
binding:
  targetType:
  - ResourceType
  - kind
```

To specify a custom binding function use:

```powershell
# Create a custom function that returns a TargetType string
$bindFn = {
    param ($TargetObject)

    $otherType = $TargetObject.PSObject.Properties['OtherType'];

    if ($otherType -eq $Null) {
        return $Null
    }

    return $otherType.Value;
}

# Specify the binding function script block code to execute
$option = New-PSRuleOption -BindTargetType $bindFn;
```

### Binding.UseQualifiedName

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_.
_TargetName_ is used in output results to identify one object from another.

In cases where different types of objects share the same _TargetName_, this may become confusing.
Using a qualified name, prefixes the _TargetName_ with _TargetType_.
i.e. _TargetType/TargetName_

This option determines if PSRule uses qualified or unqualified names (default).

By default, PSRule uses `/` to separate _TargetType_ from _TargetName_.
Set `Binding.NameSeparator` to change.

This option can be specified using:

```powershell
# PowerShell: Using the BindingUseQualifiedName parameter
$option = New-PSRuleOption -BindingUseQualifiedName $True;
```

```powershell
# PowerShell: Using the Binding.UseQualifiedName hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.UseQualifiedName' = $True };
```

```powershell
# PowerShell: Using the BindingUseQualifiedName parameter to set YAML
Set-PSRuleOption -BindingUseQualifiedName $True;
```

```yaml
# YAML: Using the binding/useQualifiedName property
binding:
  useQualifiedName: true
```

### Configuration

Configures a set of baseline configuration values that can be used in rule definitions instead of using hard coded values.

This option can be specified using:

```powershell
# PowerShell: Using the BaselineConfiguration option with a hashtable
$option = New-PSRuleOption -BaselineConfiguration @{ appServiceMinInstanceCount = 2 };
```

```yaml
# YAML: Using the configuration property
configuration:
  appServiceMinInstanceCount: 2
```

### Execution.LanguageMode

Unless PowerShell has been constrained, full language features of PowerShell are available to use within rule definitions.
In locked down environments, a reduced set of language features may be desired.

When PSRule is executed in an environment configured for Device Guard, only constrained language features are available.

The following language modes are available for use in PSRule:

- FullLanguage
- ConstrainedLanguage

This option can be specified using:

```powershell
# PowerShell: Using the Execution.LanguageMode hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
```

```yaml
# YAML: Using the execution/languageMode property
execution:
  languageMode: ConstrainedLanguage
```

### Execution.InconclusiveWarning

When defining rules, it is possible not return a valid `$True` or `$False` result within the definition script block.

Rule authors should not intentionally avoid returning a result, however a possible cause for not returning a result may be a rule logic error.

If a rule should not be evaluated, use pre-conditions to avoid processing the rule for objects where the rule is not applicable.

In cases where the rule does not return a result it is marked as inconclusive.

Inconclusive results will:

- Generate a warning by default.
- Fail the object. Outcome will be reported as `Fail` with an OutcomeReason of `Inconclusive`.

The inconclusive warning can be disabled by using:

```powershell
# PowerShell: Using the InconclusiveWarning parameter
$option = New-PSRuleOption -InconclusiveWarning $False;
```

```powershell
# PowerShell: Using the Execution.InconclusiveWarning hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.InconclusiveWarning' = $False };
```

```powershell
# PowerShell: Using the InconclusiveWarning parameter to set YAML
Set-PSRuleOption -InconclusiveWarning $False;
```

```yaml
# YAML: Using the execution/inconclusiveWarning property
execution:
  inconclusiveWarning: false
```

### Execution.NotProcessedWarning

When evaluating rules, it is possible to incorrectly select a path with rules that use pre-conditions that do not accept the pipeline object.
In this case the object has not been processed by any rule.

Not processed objects will:

- Generate a warning by default.
- Pass the object. Outcome will be reported as `None`.

The not processed warning can be disabled by using:

```powershell
# PowerShell: Using the NotProcessedWarning parameter
$option = New-PSRuleOption -NotProcessedWarning $False;
```

```powershell
# PowerShell: Using the Execution.NotProcessedWarning hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.NotProcessedWarning' = $False };
```

```powershell
# PowerShell: Using the NotProcessedWarning parameter to set YAML
Set-PSRuleOption -NotProcessedWarning $False;
```

```yaml
# YAML: Using the execution/notProcessedWarning property
execution:
  notProcessedWarning: false
```

### Input.Format

Configures the input format for when a string is passed in as a target object.
This option determines if the target object is deserialized into an alternative form.

Use this option with `Assert-PSRule`, `Invoke-PSRule` or `Test-PSRuleTarget`.
Set this option to either `Yaml`, `Json`, `Markdown`, `PowerShellData` to deserialize as a specific format.
The `-Format` parameter will override any value set in configuration.

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
`FileInfo` objects for supported file formats will be deserialized based on file extension.

When the `-InputPath` parameter is used, supported file formats will be deserialized based on file extension.
The `-InputPath` parameter can be used with a file path or URL.

The following formats are available:

- None - Treat strings as plain text and do not deserialize files.
- Yaml - Deserialize as one or more YAML objects.
- Json - Deserialize as one or more JSON objects.
- Markdown - Deserialize as a markdown object.
- PowerShellData - Deserialize as a PowerShell data object.
- File - Files are not deserialized.
- Detect - Detect format based on file extension. This is the default.

If the `Detect` format is used, the file extension will be used to automatically detect the format.
When the file extension can not be determined `Detect` is the same as `None`.

The `Markdown` format does not parse the whole markdown document.
Specifically this format deserializes YAML front matter from the top of the document if any exists.

The `File` format does not deserialize file contents.
Files within `.git` sub-directories are ignored.
Path specs specified in `.gitignore` directly in the current working path are ignored.
A `RepositoryInfo` object is generated if the current working path if a `.git` sub-directory is present.

Detect uses the following file extensions:

- Yaml - `.yaml` or `.yml`
- Json - `.json` or `.jsonc`
- Markdown - `.md` or `.markdown`
- PowerShellData - `.psd1`

This option can be specified using:

```powershell
# PowerShell: Using the Format parameter
$option = New-PSRuleOption -Format Yaml;
```

```powershell
# PowerShell: Using the Input.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Input.Format' = 'Yaml' };
```

```powershell
# PowerShell: Using the Format parameter to set YAML
Set-PSRuleOption -Format Yaml;
```

```yaml
# YAML: Using the input/format property
input:
  format: Yaml
```

### Input.ObjectPath

The object path to a property to use instead of the pipeline object.

By default, PSRule processes objects passed from the pipeline against selected rules.
When this option is set, instead of evaluating the pipeline object, PSRule looks for a property of the pipeline object specified by `ObjectPath` and uses that instead.
If the property specified by `ObjectPath` is a collection/ array, then each item is evaluated separately.

If the property specified by `ObjectPath` does not exist, PSRule skips the object.

When using `Invoke-PSRule`, `Test-PSRuleTarget` and `Assert-PSRule` the `-ObjectPath` parameter will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the ObjectPath parameter
$option = New-PSRuleOption -ObjectPath 'items';
```

```powershell
# PowerShell: Using the Input.ObjectPath hashtable key
$option = New-PSRuleOption -Option @{ 'Input.ObjectPath' = 'items' };
```

```powershell
# PowerShell: Using the ObjectPath parameter to set YAML
Set-PSRuleOption -ObjectPath 'items';
```

```yaml
# YAML: Using the input/objectPath property
input:
  objectPath: items
```

### Input.PathIgnore

Ignores input files that match the path spec when using `-InputPath`.
If specified, files that match the path spec will not be processed.
By default, all files are processed.

This option can be specified using:

```powershell
# PowerShell: Using the InputPathIgnore parameter
$option = New-PSRuleOption -InputPathIgnore '*.Designer.cs';
```

```powershell
# PowerShell: Using the Input.PathIgnore hashtable key
$option = New-PSRuleOption -Option @{ 'Input.PathIgnore' = '*.Designer.cs' };
```

```powershell
# PowerShell: Using the InputPathIgnore parameter to set YAML
Set-PSRuleOption -InputPathIgnore '*.Designer.cs';
```

```yaml
# YAML: Using the input/pathIgnore property
input:
  pathIgnore:
  - '*.Designer.cs'
```

### Input.TargetType

Filters input objects by TargetType.

If specified, only objects with the specified TargetType are processed.
Objects that do not match TargetType are ignored.
If multiple values are specified, only one TargetType must match. This option is not case-sensitive.

By default, all objects are processed.

To change the field TargetType is bound to set the `Binding.TargetType` option.

When using `Invoke-PSRule`, `Test-PSRuleTarget` and `Assert-PSRule` the `-TargetType` parameter will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the InputTargetType parameter
$option = New-PSRuleOption -InputTargetType 'virtualMachine';
```

```powershell
# PowerShell: Using the Input.TargetType hashtable key
$option = New-PSRuleOption -Option @{ 'Input.TargetType' = 'virtualMachine' };
```

```powershell
# PowerShell: Using the InputTargetType parameter to set YAML
Set-PSRuleOption -InputTargetType 'virtualMachine';
```

```yaml
# YAML: Using the input/targetType property
input:
  targetType:
  - virtualMachine
```

### Logging.LimitDebug

Limits debug messages to a list of named debug scopes.

When using the `-Debug` switch or preference variable, by default PSRule cmdlets log all debug output.
When using debug output for debugging a specific rule, it may be helpful to limit debug message to a specific rule.

To identify a rule to include in debug output use the rule name.

The following built-in scopes exist in addition to rule names:

- `[Discovery.Source]` - Discovery messages for `.Rule.ps1` files and rule modules.
- `[Discovery.Rule]` - Discovery messages for individual rules within `.Rule.ps1` files.

This option can be specified using:

```powershell
# PowerShell: Using the LoggingLimitDebug parameter
$option = New-PSRuleOption -LoggingLimitDebug Rule1, Rule2;
```

```powershell
# PowerShell: Using the Logging.LimitDebug hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.LimitDebug' = Rule1, Rule2 };
```

```powershell
# PowerShell: Using the LoggingLimitDebug parameter to set YAML
Set-PSRuleOption -LoggingLimitDebug Rule1, Rule2;
```

```yaml
# YAML: Using the logging/limitDebug property
logging:
  limitDebug:
  - Rule1
  - Rule2
```

### Logging.LimitVerbose

Limits verbose messages to a list of named verbose scopes.

When using the `-Verbose` switch or preference variable, by default PSRule cmdlets log all verbose output.
When using verbose output for troubleshooting a specific rule, it may be helpful to limit verbose messages to a specific rule.

To identify a rule to include in verbose output use the rule name.

The following built-in scopes exist in addition to rule names:

- `[Discovery.Source]` - Discovery messages for `.Rule.ps1` files and rule modules.
- `[Discovery.Rule]` - Discovery messages for individual rules within `.Rule.ps1` files.

This option can be specified using:

```powershell
# PowerShell: Using the LoggingLimitVerbose parameter
$option = New-PSRuleOption -LoggingLimitVerbose Rule1, Rule2;
```

```powershell
# PowerShell: Using the Logging.LimitVerbose hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.LimitVerbose' = Rule1, Rule2 };
```

```powershell
# PowerShell: Using the LoggingLimitVerbose parameter to set YAML
Set-PSRuleOption -LoggingLimitVerbose Rule1, Rule2;
```

```yaml
# YAML: Using the logging/limitVerbose property
logging:
  limitVerbose:
  - Rule1
  - Rule2
```

### Logging.RuleFail

When an object fails a rule condition the results are written to output as a structured object marked with the outcome of _Fail_.
If the rule executed successfully regardless of outcome no other informational messages are shown by default.

In some circumstances such as a continuous integration (CI) pipeline, it may be preferable to see informational messages or abort the CI process if one or more _Fail_ outcomes are returned.

By settings this option, error, warning or information messages will be generated for each rule _fail_ outcome in addition to structured output.
By default, outcomes are not logged to an informational stream (i.e. None).

The following streams available:

- None
- Error
- Warning
- Information

This option can be specified using:

```powershell
# PowerShell: Using the LoggingRuleFail parameter
$option = New-PSRuleOption -LoggingRuleFail Error;
```

```powershell
# PowerShell: Using the Logging.RuleFail hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
```

```powershell
# PowerShell: Using the LoggingRuleFail parameter to set YAML
Set-PSRuleOption -LoggingRuleFail Error;
```

```yaml
# YAML: Using the logging/ruleFail property
logging:
  ruleFail: Error
```

### Logging.RulePass

When an object passes a rule condition the results are written to output as a structured object marked with the outcome of _Pass_.
If the rule executed successfully regardless of outcome no other informational messages are shown by default.

In some circumstances such as a continuous integration (CI) pipeline, it may be preferable to see informational messages.

By settings this option, error, warning or information messages will be generated for each rule _pass_ outcome in addition to structured output.
By default, outcomes are not logged to an informational stream (i.e. None).

The following streams available:

- None
- Error
- Warning
- Information

This option can be specified using:

```powershell
# PowerShell: Using the LoggingRulePass parameter
$option = New-PSRuleOption -LoggingRulePass Information;
```

```powershell
# PowerShell: Using the Logging.RulePass hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Information' };
```

```powershell
# PowerShell: Using the LoggingRulePass parameter to set YAML
Set-PSRuleOption -LoggingRulePass Information;
```

```yaml
# YAML: Using the logging/rulePass property
logging:
  rulePass: Information
```

### Output.As

Configures the type of results to produce.

This option only applies to `Invoke-PSRule`.
`Invoke-PSRule` also include a `-As` parameter to set this option at runtime.
If specified, the `-As` parameter take precedence, over this option.

The following options are available:

- Detail - Return a record per rule per object.
- Summary - Return summary information for per rule.

This option can be specified using:

```powershell
# PowerShell: Using the OutputAs parameter
$option = New-PSRuleOption -OutputAs Summary;
```

```powershell
# PowerShell: Using the Output.As hashtable key
$option = New-PSRuleOption -Option @{ 'Output.As' = 'Summary' };
```

```powershell
# PowerShell: Using the OutputAs parameter to set YAML
Set-PSRuleOption -OutputAs Summary;
```

```yaml
# YAML: Using the output/as property
output:
  as: Summary
```

### Output.Culture

Specified the name of one or more cultures to use for generating output.
When multiple cultures are specified, the first matching culture will be used.
If a culture is not specified, PSRule will use the current PowerShell culture.

PSRule cmdlets also include a `-Culture` parameter to set this option at runtime.
If specified, the `-Culture` parameter take precedence, over this option.

To get a list of cultures use the `Get-Culture -ListAvailable` cmdlet.

This option can be specified using:

```powershell
# PowerShell: Using the OutputCulture parameter
$option = New-PSRuleOption -OutputCulture 'en-AU';
```

```powershell
# PowerShell: Using the Output.Culture hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Culture' = 'en-AU' };
```

```powershell
# PowerShell: Using the OutputCulture parameter to set YAML
Set-PSRuleOption -OutputCulture 'en-AU', 'en-US';
```

```yaml
# YAML: Using the output/culture property
output:
  culture: [ 'en-AU', 'en-US' ]
```

### Output.Encoding

Configures the encoding used when output is written to file.
This option has no affect when `Output.Path` is not set.

The following encoding options are available:

- Default
- UTF-8
- UTF-7
- Unicode
- UTF-32
- ASCII

This option can be specified using:

```powershell
# PowerShell: Using the OutputEncoding parameter
$option = New-PSRuleOption -OutputEncoding UTF8;
```

```powershell
# PowerShell: Using the Output.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Encoding' = 'UTF8' };
```

```powershell
# PowerShell: Using the OutputEncoding parameter to set YAML
Set-PSRuleOption -OutputEncoding UTF8;
```

```yaml
# YAML: Using the output/encoding property
output:
  encoding: UTF8
```

### Output.Format

Configures the format that results will be presented in.
This option applies to `Invoke-PSRule`, `Assert-PSRule`, and `Get-PSRule`.
This options is ignored by other cmdlets.

The following format options are available:

- None - Output is presented as an object using PowerShell defaults. This is the default.
- Yaml - Output is serialized as YAML.
- Json - Output is serialized as JSON.
- Markdown - Output is serialized as Markdown.
- NUnit3 - Output is serialized as NUnit3 (XML).
- Csv - Output is serialized as a comma separated values (CSV).
  - The following columns are included for `Detail` output:
RuleName, TargetName, TargetType, Outcome, OutcomeReason, Synopsis, Recommendation
  - The following columns are included for `Summary` output:
RuleName, Pass, Fail, Outcome, Synopsis, Recommendation
- Wide -  Output is presented using the wide table format, which includes reason and wraps columns.

The Wide format is ignored by `Assert-PSRule`. `Get-PSRule` only accepts `Wide` or `None`.
Usage of other formats are treated as `None`.

This option can be specified using:

```powershell
# PowerShell: Using the OutputFormat parameter
$option = New-PSRuleOption -OutputFormat Yaml;
```

```powershell
# PowerShell: Using the Output.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Format' = 'Yaml' };
```

```powershell
# PowerShell: Using the OutputFormat parameter to set YAML
Set-PSRuleOption -OutputFormat Yaml;
```

```yaml
# YAML: Using the output/format property
output:
  format: Yaml
```

### Output.Outcome

Filters output to include results with the specified outcome.
This option applies to `Invoke-PSRule`, and `Test-PSRule`.

The following outcome options are available:

- `None` - Results for rules that did not get processed are returned.
- `Pass` - Results for rules that passed are returned.
- `Fail` - Results for rules that failed are returned.
- `Error` - Results for rules that raised an error are returned.
- `Processed` - Results for rules that either passed, failed, or raised an error are returned.
This is the default option.
- `All` - All results for rules are returned.

This option is ignored by `Assert-PSRule`.
`Assert-PSRule` will always use `Processed`.

This option can be specified using:

```powershell
# PowerShell: Using the OutputOutcome parameter
$option = New-PSRuleOption -OutputOutcome Fail;
```

```powershell
# PowerShell: Using the Output.Outcome hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Outcome' = 'Fail' };
```

```powershell
# PowerShell: Using the OutputOutcome parameter to set YAML
Set-PSRuleOption -OutputOutcome Fail;
```

```yaml
# YAML: Using the output/outcome property
output:
  outcome: 'Fail'
```

### Output.Path

Specifies the output file path to write results.
Directories along the file path will automatically be created if they do not exist.

This option only applies to `Invoke-PSRule`.
`Invoke-PSRule` also includes a parameter `-OutputPath` to set this option at runtime.
If specified, the `-OutputPath` parameter take precedence, over this option.

This option can be specified using:

```powershell
# PowerShell: Using the OutputPath parameter
$option = New-PSRuleOption -OutputPath 'out/results.yaml';
```

```powershell
# PowerShell: Using the Output.Path hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Path' = 'out/results.yaml' };
```

```powershell
# PowerShell: Using the OutputPath parameter to set YAML
Set-PSRuleOption -OutputPath 'out/results.yaml';
```

```yaml
# YAML: Using the output/path property
output:
  path: 'out/results.yaml'
```

### Output.Style

Configures the style that results will be presented in.

This option only applies to output generated from `Assert-PSRule`.
`Assert-PSRule` also include a parameter `-Style` to set this option at runtime.
If specified, the `-Style` parameter takes precedence, over this option.

The following styles are available:

- Client - Output is written to the host directly in green/ red to indicate outcome. This is the default.
- Plain - Output is written as an unformatted string. This option can be redirected to a file.
- AzurePipelines - Output is written with commands that can be interpreted by Azure Pipelines.
- GitHubActions - Output is written with commands that can be interpreted by GitHub Actions.

This option can be specified using:

```powershell
# PowerShell: Using the OutputStyle parameter
$option = New-PSRuleOption -OutputStyle AzurePipelines;
```

```powershell
# PowerShell: Using the Output.Style hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Style' = 'AzurePipelines' };
```

```powershell
# PowerShell: Using the OutputStyle parameter to set YAML
Set-PSRuleOption -OutputFormat AzurePipelines;
```

```yaml
# YAML: Using the output/style property
output:
  style: AzurePipelines
```

### Requires

Specifies module version constraints for running PSRule.
When set PSRule will error if a module version is used that does not satisfy the requirements.
The format for version constraints are the same as the `Version` assertion method.
See [about_PSRule_Assert] for more information.

Module version constraints a not enforced prior to PSRule v0.19.0.

The version constraint for a rule module is enforced when the module is included with `-Module`.
A version constraint does not require a rule module to be included.

This option can be specified using:

```powershell
# PowerShell: Using the Requires.module hashtable key
$option = New-PSRuleOption -Option @{ 'Requires.PSRule' = '>=0.18.0' };
```

```yaml
# YAML: Using the requires property
requires:
  PSRule: '>=0.18.0'              # Require v0.18.0 or greater.
  PSRule.Rules.Azure: '>=0.13.0'  # Require v0.13.0 or greater.
```

### Rule.Include

The name of specific rules to evaluate.
If this option is not specified all rules in search paths will be evaluated.

This option can be overridden at runtime by using the `-Name` cmdlet parameter.

This option can be specified using:

```powershell
# PowerShell: Using the Rule.Include hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.Include' = 'Rule1','Rule2' };
```

```yaml
# YAML: Using the rule/include property
rule:
  include:
  - rule1
  - rule2
```

### Rule.Exclude

The name of specific rules to exclude from being evaluated.
This will exclude rules specified by `Rule.Include` or discovered from a search path.

This option can be specified using:

```powershell
# PowerShell: Using the Rule.Exclude hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.Exclude' = 'Rule3','Rule4' };
```

```yaml
# YAML: Using the rule/exclude property
rule:
  exclude:
  - rule3
  - rule4
```

### Rule.Tag

A set of required key value pairs (tags) that rules must have applied to them to be included.

Multiple values can be specified for the same tag.
When multiple values are used, only one must match.

This option can be overridden at runtime by using the `-Tag` cmdlet parameter.

This option can be specified using:

```powershell
# PowerShell: Using the Rule.Tag hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.Tag' = @{ severity = 'Critical','Warning' } };
```

```yaml
# YAML: Using the rule/tag property
rule:
  tag:
    severity: Critical
```

```yaml
# YAML: Using the rule/tag property, with multiple values
rule:
  tag:
    severity:
    - Critical
    - Warning
```

In the example above, rules must have a tag of `severity` set to either `Critical` or `Warning` to be included.

### Suppression

In certain circumstances it may be necessary to exclude or suppress rules from processing objects that are in a known failed state.

PSRule allows objects to be suppressed for a rule by TargetName.
Objects that are suppressed are not processed by the rule at all but will continue to be processed by other rules.

Rule suppression complements pre-filtering and pre-conditions.

This option can be specified using:

```powershell
# PowerShell: Using the SuppressTargetName option with a hashtable
$option = New-PSRuleOption -SuppressTargetName @{ 'storageAccounts.UseHttps' = 'TestObject1', 'TestObject3' };
```

```yaml
# YAML: Using the suppression property
suppression:
  storageAccounts.UseHttps:
    targetName:
    - TestObject1
    - TestObject3
```

In both of the above examples, `TestObject1` and `TestObject3` have been suppressed from being processed by a rule named `storageAccounts.UseHttps`.

When **to** use rule suppression:

- A temporary exclusion for an object that is in a known failed state.

When **not** to use rule suppression:

- An object should never be processed by any rule. Pre-filter the pipeline instead.
- The rule is not applicable because the object is the wrong type. Use pre-conditions on the rule instead.

An example of pre-filtering:

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge'; Type = 'Equipment'; Category = 'White goods'; };
$items += [PSCustomObject]@{ Name = 'Apple'; Type = 'Food'; Category = 'Produce'; };
$items += [PSCustomObject]@{ Name = 'Carrot'; Type = 'Food'; Category = 'Produce'; };

# Example of pre-filtering, only food items are sent to Invoke-PSRule
$items | Where-Object { $_.Type -eq 'Food' } | Invoke-PSRule;
```

An example of pre-conditions:

```powershell
# A rule with a pre-condition to only process produce
Rule 'isFruit' -If { $TargetObject.Category -eq 'Produce' } {
    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

## EXAMPLES

### Example ps-rule.yaml

```yaml
#
# PSRule example configuration
#

# Configure required module versions
requires:
  PSRule.Rules.Azure: '>=0.13.0'

# Configure execution options
execution:
  languageMode: ConstrainedLanguage
  inconclusiveWarning: false
  notProcessedWarning: false

# Configures input options
input:
  format: Yaml
  objectPath: items
  pathIgnore:
  - '*.Designer.cs'
  targetType:
  - Microsoft.Compute/virtualMachines
  - Microsoft.Network/virtualNetworks

# Configures outcome logging options
logging:
  limitDebug:
  - Rule1
  - Rule2
  limitVerbose:
  - Rule1
  - Rule2
  ruleFail: Error
  rulePass: Information

output:
  as: Summary
  culture:
  - en-US
  encoding: UTF8
  format: Json
  outcome: Fail
  style: GitHubActions

# Configure rule suppression
suppression:
  storageAccounts.UseHttps:
    targetName:
    - TestObject1
    - TestObject3

# Configure baseline options
binding:
  field:
    id:
    - ResourceId
    - AlternativeId
  ignoreCase: false
  nameSeparator: '::'
  targetName:
  - ResourceName
  - AlternateName
  targetType:
  - ResourceType
  - kind
  useQualifiedName: true

configuration:
  appServiceMinInstanceCount: 2

rule:
  include:
  - rule1
  - rule2
  exclude:
  - rule3
  - rule4
  tag:
    severity:
    - Critical
    - Warning
```

### Default ps-rule.yaml

```yaml
#
# PSRule defaults
#

# Note: Only properties that differ from the default values need to be specified.

# Configure required module versions
requires: { }

# Configure execution options
execution:
  languageMode: FullLanguage
  inconclusiveWarning: true
  notProcessedWarning: true

# Configures input options
input:
  format: Detect
  objectPath: null
  pathIgnore: [ ]
  targetType: [ ]

# Configures outcome logging options
logging:
  limitDebug: [ ]
  limitVerbose: [ ]
  ruleFail: None
  rulePass: None

output:
  as: Detail
  culture: [ ]
  encoding: Default
  format: None
  outcome: Processed
  style: Client

# Configure rule suppression
suppression: { }

# Configure baseline options
binding:
  field: { }
  ignoreCase: true
  nameSeparator: '/'
  targetName:
  - TargetName
  - Name
  targetType:
  - PSObject.TypeNames[0]
  useQualifiedName: false

configuration: { }

rule:
  include: [ ]
  exclude: [ ]
  tag: { }
```

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSRule/blob/main/docs/concepts/PSRule/en-US/about_PSRule_Options.md.

## SEE ALSO

- [Invoke-PSRule](https://github.com/Microsoft/PSRule/blob/main/docs/commands/PSRule/en-US/Invoke-PSRule.md)
- [New-PSRuleOption](https://github.com/Microsoft/PSRule/blob/main/docs/commands/PSRule/en-US/New-PSRuleOption.md)
- [Set-PSRuleOption](https://github.com/Microsoft/PSRule/blob/main/docs/commands/PSRule/en-US/Set-PSRuleOption.md)

## KEYWORDS

- Options
- PSRule

[about_PSRule_Assert]: about_PSRule_Assert.md
