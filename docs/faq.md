---
title: Frequently Asked Questions
author: BernieWhite
---

# Frequently Asked Questions (FAQ)

## How is PSRule different to Pester?

PSRule is a framework for testing infrastructure as code (IaC) and objects using rules.
Rules can be written in PowerShell, YAML, or JSON.
Some features include:

- **Objects** - PowerShell objects can be validated on the pipeline or imported.
  - Objects can be [imported][format] directly from `JSON`, `YAML`, or `.psd1`.
  - Each object is automatically bound to a _target type_ for use with pre-conditions.
  - Rule results are orientated to validating an object.
  - Built-in [assertions][about_PSRule_Assert], automatically traverse object properties.
- **Pre-conditions** - Rules understand which objects they apply to.
Objects are bound to a type as they are processed using object properties.
Dissimilar objects can be processed quickly.
  - Objects that match no rules are flagged with a warning by default.
- **Packaging** - Rules can be reused between projects and optionally packaged into a module.
  - Portable rules, configuration, baselines, and documentation allow greater reuse and distribution.
  - [Documentation][about_PSRule_Docs] with detailed guidance or next steps can be included.
  - Standalone or rules from modules can be combined together with `-Module` and `-Path`.
- **Configuration** - [Configuration] of rules is handled by PSRule.
  - Rules can be configured at runtime, from YAML configuration, or environment variables.
  - [Baselines][about_PSRule_Baseline] can be used to pair rules and configuration for a specific scenario.
- **Exceptions** - Exceptions to a rule can be ignored for a single object using [suppression].
  - Exclusion can be used additionally to ignore a rule entirely.

These features make PSRule ideal for validating:

- Infrastructure as code, including:
  - Kubernetes manifests.
  - Azure Resource Manager (ARM) templates.
  - Configuration files.
  - Pipeline files.
- Deployments or configurations against a baseline.

If you want to test PowerShell code, consider using Pester, we do!

## What pre-built modules are available for PSRule?

PSRule rules modules can be found on the [PowerShell Gallery][PSRule-rules] using the tag `PSRule-rules`.

## How do I configure PSRule?

PSRule and rules can be configured by:

- **Parameter** - PSRule can be configured at runtime by passing the `-Option` parameter to cmdlets.
- **Options file** - Options stored in YAML are load configuration from file.
The default `ps-rule.yaml` option file is read automatically from the current working path by default.
When checking into source control, store this file in the root directory of the repository.
- **Environment variables** - Configuration can be specified using environment variables.

For example:

```powershell
# With cmdlet
$option = New-PSRuleOption -OutputAs Summary -OutputCulture 'en-AU' -NotProcessedWarning $False -Configuration @{
  CUSTOM_VALUE = 'example'
}
$items | Assert-PSRule -Option $option

# With hashtable
$items | Assert-PSRule -Option @{
  'Output.As' = 'Summary'
  'Output.Culture' = 'en-AU'
  'Execution.NotProcessedWarning' = $False
  'Configuration.CUSTOM_VALUE' = 'Example'
}
```

```yaml
# With YAML
output:
  as: Summary
  culture: [ 'en-AU' ]

execution:
  notProcessedWarning: false

configuration:
  CUSTOM_VALUE: Example
```

```bash
# With environment variable in bash
export PSRULE_EXECUTION_NOTPROCESSEDWARNING=false
export PSRULE_OUTPUT_AS=Summary
export PSRULE_OUTPUT_CULTURE=en-AU
export PSRULE_CONFIGURATION_CUSTOM_VALUE=Example
```

For a list of configuration options and usage see [about_PSRule_Options].

## How do I ignore a rule?

To prevent a rule executing you can either:

- Exclude the rule - The rule is not executed for any object.
- Suppress the rule - The rule is not executed for a specific object by name.

To exclude a rule use the `Rule.Exclude` option.
To do this in YAML, add the following to the `ps-rule.yaml` options file.

```yaml
# YAML: Using the rule/exclude property
rule:
  exclude:
  - 'My.FirstRule'  # The name of the first rule to exclude.
  - 'My.SecondRule' # The name of the second rule to exclude.
```

To suppress a rule use the `Suppression` option.
To do this in YAML, add the following to the `ps-rule.yaml` options file.

```yaml
# YAML: Using the suppression property
suppression:
  My.FirstRule:    # The name of the rule being suppressed
  - TestObject1    # The name of the first object to suppress
  - TestObject3    # The name of the second object to suppress
  My.SecondRule:   # An additional rule to suppress
  - TestObject2
```

The name of the object is reported by PSRule in output results.

See [about_PSRule_Options] for additional usage for both of these options.

## How do I layer on custom rules on top of an existing module?

PSRule allows rules from modules and standalone _(loose)_ rules to be run together.

To run rules from a standalone path use:

```powershell
# Note: .ps-rule/ is a standard path to include standalone rules.

# With input from the pipeline
$items | Assert-PSRule -Path '.ps-rule/'

# With input from file
Assert-PSRule -Path '.ps-rule/' -InputPath 'src/'
```

To run rules from an installed module use:

```powershell
# With input from the pipeline
$items | Assert-PSRule -Module 'PSRule.Rules.Azure'

# With input from file
Assert-PSRule -Module 'PSRule.Rules.Azure' -InputPath 'src/'
```

Combining both:

```powershell
Assert-PSRule -Module 'PSRule.Rules.Azure', 'PSRule.Rules.CAF' -Path '.ps-rule/' -InputPath 'src/'
```

## Why should I use PSRule keywords and assertions?

Except for the `Rule` keyword, using the built-in language features are optional.

The built-in keywords and assertions accelerate rule creation.
They do this by providing a condition and a set of reasons in a single command.

Reasons are also optional; however, they provide additional context as to why the rule failed.
Alternatively, you can provide your own reasons to complement standard PowerShell with the `Reason` keyword.

*[IaC]: Infrastructure as Code
*[CI]: Continuous Integration
*[PRs]: Pull Requests

[about_PSRule_Assert]: concepts/PSRule/en-US/about_PSRule_Assert.md
[about_PSRule_Baseline]: concepts/PSRule/en-US/about_PSRule_Baseline.md
[about_PSRule_Docs]: concepts/PSRule/en-US/about_PSRule_Docs.md
[about_PSRule_Options]: concepts/PSRule/en-US/about_PSRule_Options.md
[extension-pipelines]: https://marketplace.visualstudio.com/items?itemName=bewhite.ps-rule
[extension-github]: https://github.com/marketplace/actions/psrule
[PSRule-rules]: https://www.powershellgallery.com/packages?q=Tags%3A%22PSRule-rules%22
[configuration]: concepts/PSRule/en-US/about_PSRule_Options.md#configuration
[suppression]: concepts/PSRule/en-US/about_PSRule_Options.md#suppression
[format]: concepts/PSRule/en-US/about_PSRule_Options.md#inputformat
