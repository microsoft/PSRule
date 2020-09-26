# PSRule

A cross-platform PowerShell module (Windows, Linux, and MacOS) with commands to validate objects on the pipeline using PowerShell syntax.

![ci-badge]

Features of PSRule include:

- [Extensible](docs/features.md#extensible) - Use PowerShell, a flexible scripting language.
- [Cross-platform](docs/features.md#cross-platform) - Run on MacOS, Linux and Windows.
- [Reusable](docs/features.md#reusable) - Share rules across teams or organizations.
- [Recommendations](docs/features.md#recommendations) - Include detailed instructions to remediate issues.

## Support

This project uses GitHub Issues to track bugs and feature requests.
Please search the existing issues before filing new issues to avoid duplicates.

- For new issues, file your bug or feature request as a new [Issue].
- For help and questions about using this project, we have a Gitter room which you can join below.

[![Join the chat][chat-badge]][chat]

Support for this project/ product is limited to the resources listed above.

## Getting the module

You can download and install the PSRule module from the PowerShell Gallery.

Module | Description | Downloads / instructions
------ | ----------- | ------------------------
PSRule | Validate objects using PowerShell rules | [latest][module-psrule] / [instructions][install]

For rule and integration modules see [related projects](#related-projects).

## Getting extensions

Companion extensions are available for the following platforms.

Extension | Platform           | Description | Downloads / instructions
--------- | --------           | ----------- | ------------------------
PSRule    | Visual Studio Code | An extension for IT Pros using the PSRule PowerShell module. | [latest][extension-vscode] / [instructions][install]
PSRule    | Azure Pipelines    | An Azure DevOps extension for using PSRule within Azure Pipelines. | [latest][extension-pipelines] / [instructions][install]

## Getting started

The following example shows basic PSRule usage.
For specific use cases see [scenarios](#scenarios).

For frequently asked questions, see the [FAQ](docs/features.md#frequently-asked-questions-faq).

### Define a rule

To define a rule, use a `Rule` block saved to a file with the `.Rule.ps1` extension.

```powershell
Rule 'NameOfRule' {
    # Rule conditions
}
```

Within the body of the rule provide one or more conditions.
A condition is valid PowerShell that results in `$True` or `$False`.

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

The rule is saved to a file named [`isFruit.Rule.ps1`](docs/scenarios/fruit/isFruit.Rule.ps1) file.
One or more rules can be defined within a single file.

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

To filter results to only non-fruit results, use `Invoke-PSRule -Outcome Fail`.
Passed, failed and error results are shown by default.

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
    Reason "$($PSRule.TargetName) is not fruit."

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

- [Validate Azure resource configuration](docs/scenarios/azure-resources/azure-resources.md)
- [Validate Azure resources tags](docs/scenarios/azure-tags/azure-tags.md)
- [Validate Kubernetes resources](docs/scenarios/kubernetes-resources/kubernetes-resources.md)
- [Using within continuous integration](docs/scenarios/validation-pipeline/validation-pipeline.md)
- [Packaging rules in a module](docs/scenarios/rule-module/rule-module.md)
- [Writing rule help](docs/scenarios/rule-docs/rule-docs.md)

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

- [Assert-PSRule](docs/commands/PSRule/en-US/Assert-PSRule.md) - Evaluate objects against matching rules and assert any failures.
- [Get-PSRule](docs/commands/PSRule/en-US/Get-PSRule.md) - Get a list of rule definitions.
- [Get-PSRuleBaseline](docs/commands/PSRule/en-US/Get-PSRuleBaseline.md) - Get a list of baselines.
- [Get-PSRuleHelp](docs/commands/PSRule/en-US/Get-PSRuleHelp.md) - Get documentation for a rule.
- [Get-PSRuleTarget](docs/commands/PSRule/en-US/Get-PSRuleTarget.md) - Get a list of target objects.
- [Invoke-PSRule](docs/commands/PSRule/en-US/Invoke-PSRule.md) - Evaluate objects against matching rules and output the results.
- [New-PSRuleOption](docs/commands/PSRule/en-US/New-PSRuleOption.md) - Create options to configure PSRule execution.
- [Set-PSRuleOption](docs/commands/PSRule/en-US/Set-PSRuleOption.md) - Sets options that configure PSRule execution.
- [Test-PSRuleTarget](docs/commands/PSRule/en-US/Test-PSRuleTarget.md) - Pass or fail objects against matching rules.

### Concepts

The following conceptual topics exist in the `PSRule` module:

- [Assert](docs/concepts/PSRule/en-US/about_PSRule_Assert.md)
  - [Contains](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#contains)
  - [EndsWith](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#endswith)
  - [FileHeader](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#fileheader)
  - [FilePath](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#filepath)
  - [Greater](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#greater)
  - [GreaterOrEqual](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#greaterorequal)
  - [HasDefaultValue](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasdefaultvalue)
  - [HasField](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasfield)
  - [HasFieldValue](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasfieldvalue)
  - [HasJsonSchema](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#hasjsonschema)
  - [In](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#in)
  - [IsLower](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#islower)
  - [IsUpper](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#isupper)
  - [JsonSchema](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#jsonschema)
  - [Less](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#less)
  - [Match](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#match)
  - [NotIn](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#notin)
  - [NotMatch](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#notmatch)
  - [LessOrEqual](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#lessorequal)
  - [NullOrEmpty](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#nullorempty)
  - [StartsWith](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#startswith)
  - [Version](docs/concepts/PSRule/en-US/about_PSRule_Assert.md#version)
- [Baselines](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md)
  - [Baseline specs](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md#baseline-specs)
  - [Baseline scopes](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md#baseline-scopes)
- [Docs](docs/concepts/PSRule/en-US/about_PSRule_Docs.md)
  - [Getting documentation](docs/concepts/PSRule/en-US/about_PSRule_Docs.md#getting-documentation)
  - [Online help](docs/concepts/PSRule/en-US/about_PSRule_Docs.md#online-help)
  - [Creating documentation](docs/concepts/PSRule/en-US/about_PSRule_Docs.md#creating-documentation)
- [Options](docs/concepts/PSRule/en-US/about_PSRule_Options.md)
  - [Binding.Field](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingfield)
  - [Binding.IgnoreCase](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingignorecase)
  - [Binding.NameSeparator](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingnameseparator)
  - [Binding.TargetName](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingtargetname)
  - [Binding.TargetType](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingtargettype)
  - [Binding.UseQualifiedName](docs/concepts/PSRule/en-US/about_PSRule_Options.md#bindingusequalifiedname)
  - [Configuration](docs/concepts/PSRule/en-US/about_PSRule_Options.md#configuration)
  - [Execution.LanguageMode](docs/concepts/PSRule/en-US/about_PSRule_Options.md#executionlanguagemode)
  - [Execution.InconclusiveWarning](docs/concepts/PSRule/en-US/about_PSRule_Options.md#executioninconclusivewarning)
  - [Execution.NotProcessedWarning](docs/concepts/PSRule/en-US/about_PSRule_Options.md#executionnotprocessedwarning)
  - [Input.Format](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inputformat)
  - [Input.ObjectPath](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inputobjectpath)
  - [Input.PathIgnore](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inputpathignore)
  - [Input.TargetType](docs/concepts/PSRule/en-US/about_PSRule_Options.md#inputtargettype)
  - [Logging.LimitDebug](docs/concepts/PSRule/en-US/about_PSRule_Options.md#logginglimitdebug)
  - [Logging.LimitVerbose](docs/concepts/PSRule/en-US/about_PSRule_Options.md#logginglimitverbose)
  - [Logging.RuleFail](docs/concepts/PSRule/en-US/about_PSRule_Options.md#loggingrulefail)
  - [Logging.RulePass](docs/concepts/PSRule/en-US/about_PSRule_Options.md#loggingrulepass)
  - [Output.As](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputas)
  - [Output.Culture](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputculture)
  - [Output.Encoding](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputencoding)
  - [Output.Format](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputformat)
  - [Output.Outcome](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputoutcome)
  - [Output.Path](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputpath)
  - [Output.Style](docs/concepts/PSRule/en-US/about_PSRule_Options.md#outputstyle)
  - [Rule.Include](docs/concepts/PSRule/en-US/about_PSRule_Options.md#ruleinclude)
  - [Rule.Exclude](docs/concepts/PSRule/en-US/about_PSRule_Options.md#ruleexclude)
  - [Rule.Tag](docs/concepts/PSRule/en-US/about_PSRule_Options.md#ruletag)
  - [Suppression](docs/concepts/PSRule/en-US/about_PSRule_Options.md#suppression)
- [Variables](docs/concepts/PSRule/en-US/about_PSRule_Variables.md)
  - [$Assert](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#assert)
  - [$Configuration](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#configuration)
  - [$LocalizedData](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#localizeddata)
  - [$PSRule](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#psrule)
  - [$Rule](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#rule)
  - [$TargetObject](docs/concepts/PSRule/en-US/about_PSRule_Variables.md#targetobject)

### Schemas

PSRule uses the following schemas:

- [Options](schemas/PSRule-options.schema.json) - Schema for PSRule YAML options file.
- [Resources](schemas/PSRule-language.schema.json) - Schema for PSRule YAML resources such as baselines.

## Related projects

The following projects use or integrate with PSRule.

Name                      | Description
----                      | -----------
[PSRule.Rules.Azure]      | A suite of rules to validate Azure resources using PSRule.
[PSRule.Rules.Kubernetes] | A suite of rules to validate Kubernetes resources using PSRule.
[PSRule.Rules.CAF]        | A suite of rules to validate Azure resources against the Cloud Adoption Framework (CAF) using PSRule.
[PSRule.Monitor]          | Send and query PSRule analysis results in Azure Monitor.
[PSRule-pipelines]        | An Azure DevOps extension for using PSRule within Azure Pipelines.
[ps-rule]                 | Validate infrastructure as code (IaC) and DevOps repositories using GitHub Actions.

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0.
Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments.
For a list of module changes please see the [change log](CHANGELOG.md).

> Pre-release module versions are created on major commits and can be installed from the PowerShell Gallery.
> Pre-release versions should be considered experimental.
> Modules and change log details for pre-releases will be removed as standard releases are made available.

## Contributing

This project welcomes contributions and suggestions.
If you are ready to contribute, please visit the [contribution guide](CONTRIBUTING.md).

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[issue]: https://github.com/Microsoft/PSRule/issues
[install]: docs/scenarios/install-instructions.md
[ci-badge]: https://dev.azure.com/bewhite/PSRule/_apis/build/status/PSRule-CI?branchName=main
[module-psrule]: https://www.powershellgallery.com/packages/PSRule
[extension-vscode]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
[extension-pipelines]: https://marketplace.visualstudio.com/items?itemName=bewhite.ps-rule
[PSRule.Rules.Azure]: https://github.com/microsoft/PSRule.Rules.Azure
[PSRule.Rules.Kubernetes]: https://github.com/microsoft/PSRule.Rules.Kubernetes
[PSRule.Rules.CAF]: https://github.com/microsoft/PSRule.Rules.CAF
[PSRule.Monitor]: https://github.com/microsoft/PSRule.Monitor
[PSRule-pipelines]: https://github.com/microsoft/PSRule-pipelines
[ps-rule]: https://github.com/microsoft/ps-rule
[chat]: https://gitter.im/PSRule/PSRule.Rules.Azure?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge
[chat-badge]: https://img.shields.io/static/v1.svg?label=chat&message=on%20gitter&color=informational&logo=gitter
