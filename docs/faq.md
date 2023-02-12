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
  - Objects can be [imported][1] directly from `JSON`, `YAML`, or `.psd1`.
  - Each object is automatically bound to a _target type_ for use with pre-conditions.
  - Rule results are orientated to validating an object.
  - Built-in [assertions][2], automatically traverse object properties.
- **Pre-conditions** - Rules understand which objects they apply to.
Objects are bound to a type as they are processed using object properties.
Dissimilar objects can be processed quickly.
  - Objects that match no rules are flagged with a warning by default.
- **Packaging** - Rules can be reused between projects and optionally packaged into a module.
  - Portable rules, configuration, baselines, and documentation allow greater reuse and distribution.
  - [Documentation][3] with detailed guidance or next steps can be included.
  - Standalone or rules from modules can be combined together with `-Module` and `-Path`.
- **Configuration** - [Configuration][4] of rules is handled by PSRule.
  - Rules can be configured at runtime, from YAML configuration, or environment variables.
  - [Baselines][5] can be used to pair rules and configuration for a specific scenario.
- **Exceptions** - Exceptions to a rule can be ignored for a single object using [suppression][6].
  - Exclusion can be used additionally to ignore a rule entirely.

These features make PSRule ideal for validating:

- Infrastructure as code, including:
  - Kubernetes manifests.
  - Azure Resource Manager (ARM) templates.
  - Configuration files.
  - Pipeline files.
- Deployments or configurations against a baseline.

If you want to test PowerShell code, consider using Pester, we do!

  [1]: concepts/PSRule/en-US/about_PSRule_Options.md#inputformat
  [2]: concepts/PSRule/en-US/about_PSRule_Assert.md
  [3]: concepts/PSRule/en-US/about_PSRule_Docs.md
  [4]: concepts/PSRule/en-US/about_PSRule_Options.md#configuration
  [5]: concepts/PSRule/en-US/about_PSRule_Baseline.md
  [6]: concepts/PSRule/en-US/about_PSRule_Options.md#suppression

## What pre-built modules are available for PSRule?

PSRule rules modules can be found on the [PowerShell Gallery][7] using the tag `PSRule-rules`.

  [7]: https://www.powershellgallery.com/packages?q=Tags%3A%22PSRule-rules%22

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

For a list of configuration options and usage see [about_PSRule_Options][8].

  [8]: concepts/PSRule/en-US/about_PSRule_Options.md

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

See [about_PSRule_Options][8] for additional usage for both of these options.

## How do exclude or ignore files from being processed?

To exclude or ignore files from being processed, configure the [Input.PathIgnore][9] option.
This option allows you to ignore files using a path spec.

For example:

```yaml
input:
  pathIgnore:
  # Exclude files with these extensions
  - '*.md'
  - '*.png'
  # Exclude specific configuration files
  - 'bicepconfig.json'
```

Or:

```yaml
input:
  pathIgnore:
  # Exclude all files
  - '*'
  # Only process deploy.bicep files
  - '!**/deploy.bicep'
```

  [9]: concepts/PSRule/en-US/about_PSRule_Options.md#inputpathignore

## How do I disable or suppress the not processed warning?

You may recieve a warning message suggesting a file or object _has not been processed_.
If there are no rules that apply to the file or object this warning will be displayed.

!!! Note
    This warning is intended as a verification so that you are able to confirm your configuration is correct.

After you have tuned your configuration, you may wish to disable this warning to reduce output noise.
To do this you have two options:

1. Exclude files from analysis &mdash; Configure the [Input.PathIgnore][9] option.
2. Disable the warning entirely &mdash; Set the [Execution.NotProcessedWarning][10] option to `false`.

  [10]: concepts/PSRule/en-US/about_PSRule_Options/#executionnotprocessedwarning

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

## Collection of telemetry

PSRule and PSRule for Azure currently do not collect any telemetry during installation or execution.

PowerShell (used by PSRule for Azure) does collect basic telemetry by default.
Collection of telemetry in PowerShell and how to opt-out is explained in [about_Telemetry][11].

  [11]: https://docs.microsoft.com/powershell/module/microsoft.powershell.core/about/about_telemetry

*[IaC]: Infrastructure as Code
*[CI]: Continuous Integration
*[PRs]: Pull Requests
