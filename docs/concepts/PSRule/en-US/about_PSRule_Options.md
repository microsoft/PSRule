# PSRule_Options

## about_PSRule_Options

## SHORT DESCRIPTION

Describes additional options that can be used during rule execution.

## LONG DESCRIPTION

PSRule lets you use options when calling `Invoke-PSRule` to change how rules are executed. This topic describes what options are available, when to and how to use them.

Options can be used by:

- Using the `-Option` parameter of `Invoke-PSRule` with an object created with `New-PSRuleOption`
- Using the `-Option` parameter of `Invoke-PSRule` with a hash table
- Using the `-Option` parameter of `Invoke-PSRule` with a YAML file
- Configuring the default options file `psrule.yml`

As mentioned above, a options object can be created with `New-PSRuleOption` see cmdlet help for syntax and examples.

When using a hash table, `@{}`, one or more options can be specified with the `-Option` parameter using a dotted notation.

For example:

```powershell
$option = @{ 'execution.languageMode' = 'ConstrainedLanguage' };
Invoke-PSRule -Path . -Option $option;
```

`execution.languageMode` is an example of an option that can be used. Please see the following sections for other options can be used.

Another option is to use an external file, formatted as YAML, instead of having to create an options object manually each time. This YAML file can be used with `Invoke-PSRule` to quickly execute rules in a repeatable way.

YAML properties are specified using lower camel case, for example:

```yaml
execution:
  languageMode: ConstrainedLanguage
```

By default PSRule will automatically look for a file named `psrule.yml` in the current working directory. Alternatively, you can specify a YAML file in the `-Option` parameter.

For example:

```powershell
Invoke-PSRule -Path . -Option '.\myconfig.yml';
```

### TargetName binding

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_. _TargetName_ is used in output results to identify one object from another. Many objects could be passed down the pipeline at the same time, so using a _TargetName_ that is meaningful is important. _TargetName_ is also used for advanced features such as rule suppression.

The value that PSRule uses for _TargetName_ is configurable. PSRule uses the following logic to determine what _TargetName_ should be used:

- By default PSRule will:
  - Use `TargetName` or `Name` properties on the object.
  - If both `TargetName` and `Name` properties exist, `TargetName` will take precedence over `Name`.
  - If neither `TargetName` or `Name` properties exist, a SHA1 hash of the object will be used as _TargetName_.
- If custom _TargetName_ binding properties are configured, the property names specified will override the defaults.
  - If **none** of the configured property names exist, PSRule will revert back to `TargetName` then `Name`.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
- If a custom _TargetName_ binding function is specified, the function will be evaluated first before any other option.
  - If the function returns `$Null` then custom properties, `TargetName` and `Name` properties will be used.
  - The custom binding function is executed outside the PSRule engine, so PSRule keywords and variables will not be available.
  - Custom binding functions are blocked in constrained language mode is used. See [language mode](#language-mode) for more information.

Custom property names to use for binding can be specified using:

```powershell
# PowerShell: Using the Binding.TargetName hash table key
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' };
```

```yaml
# psrule.yml: Using the binding/targetName YAML property
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

### Language mode

Unless PowerShell has been constrained, full language features of PowerShell are available to use within rule definitions. In locked down environments, a reduced set of language features may be desired.

When PSRule is executed in an environment configured for Device Guard, only constrained language features are available.

The following language modes are available for use in PSRule:

- FullLanguage
- ConstrainedLanguage

This option can be specified using:

```powershell
# PowerShell: Using the Execution.LanguageMode hash table key
$option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
```

```yaml
# psrule.yml: Using the execution/languageMode YAML property
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
# PowerShell: Using the Execution.InconclusiveWarning hash table key
$option = New-PSRuleOption -Option @{ 'Execution.InconclusiveWarning' = $False };
```

```yaml
# psrule.yml: Using the execution/inconclusiveWarning YAML property
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
# PowerShell: Using the Execution.NotProcessedWarning hash table key
$option = New-PSRuleOption -Option @{ 'Execution.NotProcessedWarning' = $False };
```

```yaml
# psrule.yml: Using the execution/notProcessedWarning YAML property
execution:
  notProcessedWarning: false
```

### Rule suppression

In certain circumstances it may be necessary to exclude or suppress rules from processing objects that are in a known failed state.

PSRule allows objects to be suppressed for a rule by TargetName. Objects that are suppressed are not processed by the rule at all, but will continue to be processed by other rules.

Rule suppression complements pre-filtering and pre-conditions.

This option can be specified using:

```powershell
# PowerShell: Using the SuppressTargetName option with a hash table
$option = New-PSRuleOption -SuppressTargetName @{ 'storageAccounts.UseHttps' = 'TestObject1', 'TestObject3' };
```

```yaml
# psrule.yml: Using the suppression YAML property
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
binding:
  targetName:
  - ResourceName
  - AlternateName

# Set execution options
execution:
  languageMode: ConstrainedLanguage
  inconclusiveWarning: false
  notProcessedWarning: false

# Suppress the following target names
suppression:
  storageAccounts.UseHttps:
    targetName:
    - TestObject1
    - TestObject3
```

### Default PSRule.yml

```yaml
# These are the default options.
# Only properties that differ from the default values need to be specified.
binding:
  targetName:
  - TargetName
  - Name

execution:
  languageMode: FullLanguage
  inconclusiveWarning: true
  notProcessedWarning: true

suppression: { }
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Options.md.

## SEE ALSO

- [Invoke-PSRule](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md)
- [New-PSRuleOption](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/New-PSRuleOption.md)

## KEYWORDS

- Options
- PSRule
