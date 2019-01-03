# PSRule

A cross-platform PowerShell module (Windows, Linux, and macOS) with commands to validate objects on the pipeline using PowerShell syntax.

![ci-badge]

## Disclaimer

This project is to be considered a **proof-of-concept** and **not a supported product**.

If you have any problems please check our GitHub [issues](https://github.com/BernieWhite/PSRule/issues) page. If you do not see your problem captured, please file a new issue and follow the provided template.

## Getting the modules

You can download and install the PSRule module from the PowerShell Gallery.

Module | Description | Downloads / instructions
------ | ----------- | ------------------------
PSRule | Validate objects using PowerShell rules | [latest][psg-psrule] / [instructions][install]

## Getting started

### Define a rule

To define a rule use the `Rule` keyword.

```powershell
Rule 'NameOfRule' {
    # Rule conditions
}
```

Within the body of the rule provide one or more conditions. A condition is valid PowerShell that results in `$True` or `$False`.

For example:

```powershell
# Saved to isFruit.Rule.ps1
Rule 'isFruit' {
    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

An optional result message can be added to by using the `Hint` keyword.

```powershell
Rule 'isFruit' {
    # An additional message to display in output
    Hint 'Fruit is only Apple, Orange and Pear'

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

### Execute a rule

To execute with rule use `Invoke-PSRule`.

For example:

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item using rules saved in current working path
$items | Invoke-PSRule;
```

The output of this example is:

```text
   TargetName: Fridge

RuleName                            Outcome    Message
--------                            -------    -------
isFruit                             Fail       Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Message
--------                            -------    -------
isFruit                             Pass       Fruit is only Apple, Orange and Pear
```

### Additional options

To filter results to only non-fruit results, use `Invoke-PSRule -Outcome Fail`. Passed, failed and error results are shown by default.

```powershell
# Only show non-fruit results
$items | Invoke-PSRule -Outcome Fail;
```

For a summary of results for each rule use `Invoke-PSRule -As Summary`.

For example:

```powershell
# Show rule summary
$items | Invoke-PSRule -As Summary;
```

The output of this example is:

```text
RuleName                            Pass  Fail  Outcome
--------                            ----  ----  -------
isFruit                             1     1     Fail
```

### Scenarios

For practical examples of PSRule see:

- [Validate configuration of Azure resources](docs/scenarios/azure-resources/azure-resources.md)

## Language reference

PSRule extends PowerShell with domain specific language (DSL) keywords, cmdlets and automatic variables.

### Keywords

The following language keywords are used by the `PSRule` module:

- [Rule](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#rule) - A rule definition
- [Exists](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#exists) - Assert that a field or property must exist
- [Match](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#match) - Assert that the field must match any of the regular expressions
- [AnyOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#anyof) - Assert that any of the child expressions must be true
- [AllOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#allof) - Assert that all of the child expressions must be true
- [Within](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#within) - Assert that the field must match any of the values
- [TypeOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#typeof) - Assert that the object must be of a specific type

### Commands

The following commands exist in the `PSRule` module:

- [Invoke-PSRule](docs/commands/PSRule/en-US/Invoke-PSRule.md)
- [Get-PSRule](docs/commands/PSRule/en-US/Get-PSRule.md)
- [New-PSRuleOption](docs/commands/PSRule/en-US/New-PSRuleOption.md)
- [Test-PSRule](docs/commands/PSRule/en-US/Test-PSRule.md)

### Concepts

The following conceptual topics exist in the `PSRule` module:

- [Options](docs/concepts/PSRule/en-US/about_PSRule_Options.md)
  - [Binding.TargetName](docs/concepts/PSRule/en-US/about_PSRule_Options.md#targetname-binding)
  - [Execution.LanguageMode](docs/concepts/PSRule/en-US/about_PSRule_Options.md#language-mode)
  - [Execution.InconclusiveWarning](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inconclusive-warning)
  - [Execution.NotProcessedWarning](docs/concepts/PSRule/en-US/about_PSRule_Options.md#not-processed-warning)
  - [Suppression](docs/concepts/PSRule/en-US/about_PSRule_Options.md#rule-suppression)
- [Variables](docs/concepts/PSRule/en-US/about_PSRule_Variables.md)
  - [$Rule](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#rule)
  - [$TargetObject](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#targetobject)

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0. Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments. For a list of module changes please see the [change log](CHANGELOG.md).

> Pre-release module versions are created on major commits and can be installed from the PowerShell Gallery. Pre-release versions should be considered experimental. Modules and change log details for pre-releases will be removed as standard releases are made available.

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[install]: docs/scenarios/install-instructions.md
[ci-badge]: https://bewhite.visualstudio.com/PSRule/_apis/build/status/PSRule-CI?branchName=master
[psg-psrule]: https://www.powershellgallery.com/packages/PSRule
