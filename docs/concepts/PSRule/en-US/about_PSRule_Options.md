# PSRule_Options

## about_PSRule_Options

## SHORT DESCRIPTION

Describes additional options that can be used during rule execution.

## LONG DESCRIPTION

PSRule lets you use options when calling cmdlets such as `Invoke-PSRule` and `Test-PSRuleTarget` to change how rules are processed. This topic describes what options are available, when to and how to use them.

The following options are available for use:

- [Baseline.RuleName](#baselinerulename)
- [Baseline.Exclude](#baselineexclude)
- [Baseline.Configuration](#baselineconfiguration)
- [Binding.IgnoreCase](#bindingignorecase)
- [Binding.TargetName](#bindingtargetname)
- [Binding.TargetType](#bindingtargettype)
- [Execution.LanguageMode](#executionlanguagemode)
- [Execution.InconclusiveWarning](#inconclusive-warning)
- [Execution.NotProcessedWarning](#not-processed-warning)
- [Input.Format](#inputformat)
- [Input.ObjectPath](#inputobjectpath)
- [Logging.RuleFail](#loggingrulefail)
- [Logging.RulePass](#loggingrulepass)
- [Output.As](#outputas)
- [Output.Format](#outputformat)
- [Suppression](#rule-suppression)

Options can be used with the following PSRule cmdlets:

- Get-PSRule
- Invoke-PSRule
- Test-PSRuleTarget

Each of these cmdlets support:

- Using the `-Option` parameter with an object created with the `New-PSRuleOption` cmdlet. See cmdlet help for syntax and examples.
- Using the `-Option` parameter with a hashtable object.
- Using the `-Option` parameter with a YAML file path.

When using a hashtable object `@{}`, one or more options can be specified as keys using a dotted notation.

For example:

```powershell
$option = @{ 'execution.languageMode' = 'ConstrainedLanguage' };
Invoke-PSRule -Path . -Option $option;
```

The above example shows how the `execution.languageMode` option as a hashtable key can be used. Continue reading for a full list of options and how each can be used.

Alternatively, options can be stored in a YAML formatted file and loaded from disk. Storing options as YAML allows different configurations to be loaded in a repeatable way instead of having to create an options object each time.

Options are stored as YAML properties using a lower camel case naming convention, for example:

```yaml
execution:
  languageMode: ConstrainedLanguage
```

By default PSRule will automatically look for a file named `psrule.yml` in the current working directory. Alternatively, you can specify a YAML file in the `-Option` parameter.

For example:

```powershell
Invoke-PSRule -Path . -Option '.\myconfig.yml';
```

### Baseline.RuleName

The name of specific rules to evaluate. If this option is not specified all rules in search paths will be evaluated.

This option can be overridden at runtime by using the `-Name` parameter of `Invoke-PSRule`, `Get-PSRule` and `Test-PSRuleTarget`.

This option can be specified using:

```powershell
# PowerShell: Using the Baseline.RuleName hashtable key
$option = New-PSRuleOption -Option @{ 'Baseline.RuleName' = 'Rule1','Rule2' };
```

```yaml
# YAML: Using the baseline/ruleName property
baseline:
  ruleName:
  - rule1
  - rule2
```

### Baseline.Exclude

The name of specific rules to exclude from being evaluated. This will exclude rules specified by `Baseline.RuleName` or discovered from a search path.

This option can be specified using:

```powershell
# PowerShell: Using the Baseline.Exclude hashtable key
$option = New-PSRuleOption -Option @{ 'Baseline.Exclude' = 'Rule3','Rule4' };
```

```yaml
# YAML: Using the baseline/exclude property
baseline:
  exclude:
  - rule3
  - rule4
```

### Baseline.Configuration

Configures a set of baseline configuration values that can be used in rule definitions instead of using hard coded values.

This option can be specified using:

```powershell
# PowerShell: Using the BaselineConfiguration option with a hashtable
$option = New-PSRuleOption -BaselineConfiguration @{ appServiceMinInstanceCount = 2 };
```

```yaml
# YAML: Using the baseline/configuration property
baseline:
  configuration:
    appServiceMinInstanceCount: 2
```

### Binding.IgnoreCase

When evaluating an object, PSRule extracts a few key properties from the object to help filter rules and display output results. The process of extract these key properties is called _binding_. The properties that PSRule uses for binding can be customized by providing a order list of alternative properties to use. See [`Binding.TargetName`](#bindingtargetname) and [`Binding.TargetType`](#bindingtargettype) for these options.

- By default, custom property binding finds the first matching property by name regardless of case. i.e. `Binding.IgnoreCase` is `true`.
- To change the default, set the `Binding.IgnoreCase` option to `false` and a case sensitive match will be used.
  - Changing this option will affect all custom property bindings, including _TargetName_ and _TargetType_.
- PSRule also has binding defaults, and an option to use a custom script. Setting this option has no affect on binding defaults or custom scripts.

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

### Binding.TargetName

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_. _TargetName_ is used in output results to identify one object from another. Many objects could be passed down the pipeline at the same time, so using a _TargetName_ that is meaningful is important. _TargetName_ is also used for advanced features such as rule suppression.

The value that PSRule uses for _TargetName_ is configurable. PSRule uses the following logic to determine what _TargetName_ should be used:

- By default PSRule will:
  - Use `TargetName` or `Name` properties on the object. These property names are case insensitive.
  - If both `TargetName` and `Name` properties exist, `TargetName` will take precedence over `Name`.
  - If neither `TargetName` or `Name` properties exist, a SHA1 hash of the object will be used as _TargetName_.
- If custom _TargetName_ binding properties are configured, the property names specified will override the defaults.
  - If **none** of the configured property names exist, PSRule will revert back to `TargetName` then `Name`.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity. To use a case sensitive match, configure the [`Binding.IgnoreCase`](#bindingignorecase) option.
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

    if ($otherName -eq $Null) {
        return $Null
    }

    return $otherName.Value;
}

# Specify the binding function script block code to execute
$option = New-PSRuleOption -BindTargetName $bindFn;
```

### Binding.TargetType

When an object is passed from the pipeline, PSRule assigns the object a _TargetType_. _TargetType_ is used to filter rules based on object type and appears in output results.

The value that PSRule uses for _TargetType_ is configurable. PSRule uses the following logic to determine what _TargetType_ should be used:

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
``

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

### Execution.LanguageMode

Unless PowerShell has been constrained, full language features of PowerShell are available to use within rule definitions. In locked down environments, a reduced set of language features may be desired.

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

### Inconclusive warning

When defining rules it is possible not return a valid `$True` or `$False` result within the definition script block.

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

### Not processed warning

When evaluating rules it is possible to incorrectly select a path with rules that use pre-conditions that do not accept the pipeline object.

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

Using this option with `Invoke-PSRule` or `Test-PSRuleTarget`:

- When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default. When this option is used and set to either `Yaml` or `Json`, strings are read as YAML or JSON and are converted to an object.
- When the `-InputPath` parameter is used with a file path or URL, by default the file extension (either `.yaml`, `.yml` or `.json`) will be used to automatically detect the format as YAML or JSON.
- The `-Format` parameter will override any value set in configuration.

The following formats are available:

- None - Treat strings as plain text.
- Yaml - Treat strings as one or more YAML objects.
- Json - Treat strings as one or more JSON objects.
- Detect - Detect format based on file extension. Detection only applies when used with the `-InputPath` parameter. In all other cases, `Detect` is the same as `None`. This is the default configuration.

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

By default, PSRule processes objects passed from the pipeline against selected rules. When this option is set, instead of evaluating the pipeline object, PSRule looks for a property of the pipeline object specified by `ObjectPath` and uses that instead. If the property specified by `ObjectPath` is a collection/ array, then each item is evaluated separately.

If the property specified by `ObjectPath` does not exist, PSRule skips the object.

When using `Invoke-PSRule` and `Test-PSRuleTarget` the `-ObjectPath` parameter will override any value set in configuration.

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

### Logging.RuleFail

When an object fails a rule condition the results are written to output as a structured object marked with the outcome of _Fail_. If the rule executed successfully regardless of outcome no other informational messages are shown by default.

In some circumstances such as a continuous integration (CI) pipeline, it may be preferable to see informational messages or abort the CI process if one or more _Fail_ outcomes are returned.

By settings this option, error, warning or information messages will be generated for each rule _fail_ outcome in addition to structured output. By default, outcomes are not logged to an informational stream (i.e. None).

The following streams available:

- None
- Error
- Warning
- Information

This option can be specified using:

```powershell
# PowerShell: Using the Logging.RuleFail hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
```

```yaml
# YAML: Using the logging/ruleFail property
logging:
  ruleFail: Error
```

### Logging.RulePass

When an object passes a rule condition the results are written to output as a structured object marked with the outcome of _Pass_. If the rule executed successfully regardless of outcome no other informational messages are shown by default.

In some circumstances such as a continuous integration (CI) pipeline, it may be preferable to see informational messages.

By settings this option, error, warning or information messages will be generated for each rule _pass_ outcome in addition to structured output. By default, outcomes are not logged to an informational stream (i.e. None).

The following streams available:

- None
- Error
- Warning
- Information

This option can be specified using:

```powershell
# PowerShell: Using the Logging.RulePass hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Information' };
```

```yaml
# YAML: Using the logging/rulePass property
logging:
  rulePass: Information
```

### Output.As

Configures the type of results to produce.

This option only applies to `Invoke-PSRule`. `Invoke-PSRule` also includes a parameter `-As` to set this option at runtime. If specified, the `-As` parameter take precedence, over this option.

The following options are available:

- Detail - Return a record per rule per object.
- Summary - Return summary information for per rule.

This option can be specified using:

```powershell
# PowerShell: Using the Output.As hashtable key
$option = New-PSRuleOption -Option @{ 'Output.As' = 'Summary' };
```

```yaml
# YAML: Using the output/as property
output:
  as: Summary
```

### Output.Format

Configures the format that results will be presented in.

The following format options are available:

- None - Output is presented as an object using PowerShell defaults. This is the default configuration.
- Yaml - Output is serialized as YAML.
- Json - Output is serialized as JSON.

This option can be specified using:

```powershell
# PowerShell: Using the Output.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Format' = 'Yaml' };
```

```yaml
# YAML: Using the output/format property
output:
  format: Yaml
```

### Rule suppression

In certain circumstances it may be necessary to exclude or suppress rules from processing objects that are in a known failed state.

PSRule allows objects to be suppressed for a rule by TargetName. Objects that are suppressed are not processed by the rule at all but will continue to be processed by other rules.

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

### Example PSRule.yml

```yaml
#
# PSRule example configuration
#

# Configure baseline
baseline:
  ruleName:
  - rule1
  - rule2
  exclude:
  - rule3
  - rule4
  configuration:
    appServiceMinInstanceCount: 2

# Configure TargetName binding
binding:
  ignoreCase: false
  targetName:
  - ResourceName
  - AlternateName
  targetType:
  - ResourceType
  - kind

# Configure execution options
execution:
  languageMode: ConstrainedLanguage
  inconclusiveWarning: false
  notProcessedWarning: false

# Configures input options
input:
  format: Yaml
  objectPath: items

# Configures outcome logging options
logging:
  ruleFail: Error
  rulePass: Information

output:
  as: Summary
  format: Json

# Configure rule suppression
suppression:
  storageAccounts.UseHttps:
    targetName:
    - TestObject1
    - TestObject3
```

### Default PSRule.yml

```yaml
#
# PSRule defaults
#

# Note: Only properties that differ from the default values need to be specified.

# Configure baseline
baseline:
  ruleName: [ ]
  exclude: [ ]
  configuration: { }

# Configure TargetName binding
binding:
  ignoreCase: true
  targetName:
  - TargetName
  - Name
  targetType:
  - PSObject.TypeNames[0]

# Configure execution options
execution:
  languageMode: FullLanguage
  inconclusiveWarning: true
  notProcessedWarning: true

# Configures input options
input:
  format: Detect
  objectPath:

# Configures outcome logging options
logging:
  ruleFail: None
  rulePass: None

output:
  as: Detail
  format: None

# Configure rule suppression
suppression: { }
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Options.md.

## SEE ALSO

- [Invoke-PSRule](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md)
- [New-PSRuleOption](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/New-PSRuleOption.md)
- [Set-PSRuleOption](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Set-PSRuleOption.md)

## KEYWORDS

- Options
- PSRule
