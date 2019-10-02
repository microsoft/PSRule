# PSRule

A cross-platform PowerShell module (Windows, Linux, and MacOS) with commands to validate objects on the pipeline using PowerShell syntax.

![ci-badge]

Features of PSRule include:

- [Extensible](docs/features.md#extensible) - Use PowerShell, a flexible scripting language.
- [Cross-platform](docs/features.md#cross-platform) - Run on MacOS, Linux and Windows.
- [Reusable](docs/features.md#reusable) - Share rules across teams or organizations.
- [Recommendations](docs/features.md#recommendations) - Include detailed instructions to remediate issues.

## Disclaimer

This project is to be considered a **proof-of-concept** and **not a supported product**.

If you have any problems please check our GitHub [issues](https://github.com/BernieWhite/PSRule/issues) page. If you do not see your problem captured, please file a new issue and follow the provided template.

## Getting the module

You can download and install the PSRule module from the PowerShell Gallery.

Module | Description | Downloads / instructions
------ | ----------- | ------------------------
PSRule | Validate objects using PowerShell rules | [latest][module-psrule] / [instructions][install]

## Getting the extension

A companion extension for Visual Studio Code can be downloaded or installed from the [Visual Studio Marketplace][ext-psrule].

Extension | Description | Downloads / instructions
--------- | ----------- | ------------------------
PSRule    | An extension for IT Pros using the PSRule PowerShell module | [latest][ext-psrule] / [instructions][install]

## Getting started

The following example shows basic PSRule usage. For specific use cases see [scenarios](#scenarios).

### Define a rule

To define a rule, use a `Rule` block saved to a file with the `.Rule.ps1` extension.

```powershell
Rule 'NameOfRule' {
    # Rule conditions
}
```

Within the body of the rule provide one or more conditions. A condition is valid PowerShell that results in `$True` or `$False`.

For example:

```powershell
Rule 'isFruit' {
    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

An optional result message can be added to by using the `Recommend` keyword.

```powershell
Rule 'isFruit' {
    # An recommendation to display in output
    Recommend 'Fruit is only Apple, Orange and Pear'

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

The rule is saved to a file named [`isFruit.Rule.ps1`](docs/scenarios/fruit/isFruit.Rule.ps1) file. One or more rules can be defined within a single file.

### Execute a rule

To execute the rule use `Invoke-PSRule`.

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

RuleName                            Outcome    Recommendation
--------                            -------    --------------
isFruit                             Fail       Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Recommendation
--------                            -------    --------------
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

An optional failure reason can be added to the rule block by using the `Reason` keyword.

```powershell
Rule 'isFruit' {
    # An recommendation to display in output
    Recommend 'Fruit is only Apple, Orange and Pear'

    # An failure reason to display for non-fruit
    Reason "$($Rule.TargetName) is not fruit."

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

To include the reason with output use `Invoke-PSRule -OutputFormat Wide`.

For example:

```powershell
# Show failure reason for failing results
$items | Invoke-PSRule -OutputFormat Wide;
```

The output of this example is:

```text

   TargetName: Fridge

RuleName                            Outcome    Reason                              Recommendation
--------                            -------    ------                              --------------
isFruit                             Fail       Fridge is not fruit.                Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Reason                              Recommendation
--------                            -------    ------                              --------------
isFruit                             Pass                                           Fruit is only Apple, Orange and Pear
```

The final rule is saved to [`isFruit.Rule.ps1`](docs/scenarios/fruit/isFruit.Rule.ps1).

### Scenarios

For walk through examples of PSRule usage see:

- [Validate configuration of Azure resources](docs/scenarios/azure-resources/azure-resources.md)
- [Validate Azure resources tags](docs/scenarios/azure-tags/azure-tags.md)
- [Validate Kubernetes resources](docs/scenarios/kubernetes-resources/kubernetes-resources.md)
- [Using within continuous integration](docs/scenarios/validation-pipeline/validation-pipeline.md)

## Language reference

PSRule extends PowerShell with domain specific language (DSL) keywords, cmdlets and automatic variables.

### Keywords

The following language keywords are used by the `PSRule` module:

- [Rule](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#rule) - A rule definition.
- [Exists](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#exists) - Assert that a field or property must exist.
- [Match](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#match) - Assert that the field must match any of the regular expressions.
- [AnyOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#anyof) - Assert that any of the child expressions must be true.
- [AllOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#allof) - Assert that all of the child expressions must be true.
- [Within](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#within) - Assert that the field must match any of the values.
- [TypeOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#typeof) - Assert that the object must be of a specific type.
- [Reason](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#reason) - Return a reason for why the rule failed.
- [Recommend](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#recommend) - Return a recommendation to resolve the issue and pass the rule.

### Commands

The following commands exist in the `PSRule` module:

- [Get-PSRule](docs/commands/PSRule/en-US/Get-PSRule.md) - Get a list of rule definitions.
- [Get-PSRuleHelp](docs/commands/PSRule/en-US/Get-PSRuleHelp.md) - Get documentation for a rule.
- [Invoke-PSRule](docs/commands/PSRule/en-US/Invoke-PSRule.md) - Evaluate objects against matching rules.
- [New-PSRuleOption](docs/commands/PSRule/en-US/New-PSRuleOption.md) - Create options to configure PSRule execution.
- [Set-PSRuleOption](docs/commands/PSRule/en-US/Set-PSRuleOption.md) - Sets options that configure PSRule execution.
- [Test-PSRuleTarget](docs/commands/PSRule/en-US/Test-PSRuleTarget.md) - Pass or fail objects against matching rules.

### Concepts

The following conceptual topics exist in the `PSRule` module:

- [Assert](docs/concepts/PSRule/en-US/about_PSRule_Assert.md)
  - [HasDefaultValue](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasdefaultvalue)
  - [HasField](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasfield)
  - [HasFieldValue](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasfieldvalue)
  - [JsonSchema](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#jsonschema)
  - [NullOrEmpty](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#nullorempty)
- [Baselines](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md)
  - [Baseline specs](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md#baselinespecs)
  - [Baseline scopes](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md#baselinescopes)
- [Docs](docs/concepts/PSRule/en-US/about_PSRule_Docs.md)
  - [Getting documentation](docs/concepts/PSRule/en-US/about_PSRule_Docs.md#getting-documentation)
  - [Online help](docs/concepts/PSRule/en-US/about_PSRule_Docs.md#online-help)
  - [Creating documentation](docs/concepts/PSRule/en-US/about_PSRule_Docs.md#creating-documentation)
- [Options](docs/concepts/PSRule/en-US/about_PSRule_Options.md)
  - [Binding.IgnoreCase](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingignorecase)
  - [Binding.TargetName](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingtargetname)
  - [Binding.TargetType](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingtargettype)
  - [Configuration](docs/concepts/PSRule/en-US/about_PSRule_Options.md#configuration)
  - [Execution.LanguageMode](docs/concepts/PSRule/en-US/about_PSRule_Options.md#executionlanguagemode)
  - [Execution.InconclusiveWarning](docs/concepts/PSRule/en-US/about_PSRule_Options.md#executioninconclusivewarning)
  - [Execution.NotProcessedWarning](docs/concepts/PSRule/en-US/about_PSRule_Options.md#executionnotprocessedwarning)
  - [Input.Format](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inputformat)
  - [Input.ObjectPath](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inputobjectpath)
  - [Logging.LimitDebug](docs/concepts/PSRule/en-US/about_PSRule_Options.md#logginglimitdebug)
  - [Logging.LimitVerbose](docs/concepts/PSRule/en-US/about_PSRule_Options.md#logginglimitverbose)
  - [Logging.RuleFail](docs/concepts/PSRule/en-US/about_PSRule_Options.md#loggingrulefail)
  - [Logging.RulePass](docs/concepts/PSRule/en-US/about_PSRule_Options.md#loggingrulepass)
  - [Output.As](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputas)
  - [Output.Encoding](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputencoding)
  - [Output.Format](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputformat)
  - [Output.Path](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputpath)
  - [Rule.Include](docs/concepts/PSRule/en-US/about_PSRule_Options.md#ruleinclude)
  - [Rule.Exclude](docs/concepts/PSRule/en-US/about_PSRule_Options.md#ruleexclude)
  - [Rule.Tag](docs/concepts/PSRule/en-US/about_PSRule_Options.md#ruletag)
  - [Suppression](docs/concepts/PSRule/en-US/about_PSRule_Options.md#suppression)
- [Variables](docs/concepts/PSRule/en-US/about_PSRule_Variables.md)
  - [$Assert](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#assert)
  - [$Configuration](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#configuration)
  - [$LocalizedData](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#localizeddata)
  - [$Rule](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#rule)
  - [$TargetObject](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#targetobject)

### Schemas

PSRule uses the following schemas:

- [PSRuleOptions](schemas/PSRule-options.schema.json) - Schema for PSRule YAML configuration file.

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0. Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments. For a list of module changes please see the [change log](CHANGELOG.md).

> Pre-release module versions are created on major commits and can be installed from the PowerShell Gallery. Pre-release versions should be considered experimental. Modules and change log details for pre-releases will be removed as standard releases are made available.

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[install]: docs/scenarios/install-instructions.md
[ci-badge]: https://dev.azure.com/bewhite/PSRule/_apis/build/status/PSRule-CI?branchName=master
[module-psrule]: https://www.powershellgallery.com/packages/PSRule
[ext-psrule]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
