# PSRule features

The following sections describe key features of PSRule.

- [Extensible](#extensible)
- [Cross-platform](#cross-platform)
- [Reusable](#reusable)
- [Recommendations](#recommendations)

## Extensible

Authors define rules using PowerShell, a flexible scripting language.
If you or your team already can write a basic PowerShell script, you can already define a rule.
What's more, you can tap into a large world-wide community of PowerShell users with scripts and cmdlets to help you build out rules quickly.

In addition to PowerShell, PSRule adds domain specific language (DSL) keywords, cmdlets, and variables.
These features can optionally be used to reduce the code you have to write for common scenarios.
For example: checking that a property exists, or providing a reason why the validation failed.

## Cross-platform

PSRule uses modern PowerShell libraries at its core, allowing it to go anywhere PowerShell can go.
The companion extension for Visual Studio Code provides snippets for authoring rules and documentation.
PSRule runs on MacOS, Linux, and Windows.

PowerShell makes it easy to integrate PSRule into popular continuous integration (CI) systems.
Additionally, PSRule has extensions for:

- [Azure Pipeline (Azure DevOps)][extension-pipelines]
- [GitHub Actions (GitHub)][extension-github]

To install, use the `Install-Module` cmdlet within PowerShell.
For installation options see [install instructions](install-instructions.md).

## Reusable

Define rules once then reuse and share rules across teams or organizations.
Rules can be packaged up into a module then distributed.

PSRule uses PowerShell modules as the standard way to distribute rules.
Modules containing rules can be published on the PowerShell Gallery or network share using the same process as regular PowerShell modules.

For a walk through see [Packaging rules in a module](scenarios/rule-module/rule-module.md).

## Recommendations

PSRule allows rule authors to define recommendations in markdown.
This allows not only the cause of the issue to be identified but detailed instructions to be included to remediate issues.

For more information see [about_PSRule_Docs].

## Frequently Asked Questions (FAQ)

### How is PSRule different to Pester?

PSRule is a framework for preforming validation against objects using PowerShell.

Pester is a framework for running unit tests to execute and validate PowerShell commands.

While this may be a subtle difference, for PSRule this means:

- Rules understand which objects they apply to.
- Rules can be reused between projects and optionally packaged into a module.
- PowerShell objects can be validated on the pipeline.
- Objects that originate from outside PowerShell can be imported.
- Optimized keywords and variables make authoring rules faster.
- Built-in assertions, automatically traverse object properties.

These features make PSRule ideal for validating:

- Infrastructure as code, including:
  - Kubernetes manifests.
  - Azure Resource Manager templates.
  - Configuration files.
  - Pipeline files.
- Deployments or configurations against a baseline.

If you want to test PowerShell code, use Pester.

### What pre-built modules are available for PSRule?

PSRule rules modules can be found on the [PowerShell Gallery][PSRule-rules] using the tag `PSRule-rules`.

### How do I configure PSRule?

PSRule can be configured at runtime by passing the `-Option` parameter to cmdlets.
Additionally, PSRule uses the `ps-rule.yaml` option file to load configuration from disk.
The `ps-rule.yaml` option file is read automatically from the current working path by default.
When checking into source control, store this file in the root directory of the repository.

For a list of configuration options and usage see [about_PSRule_Options].

### How do I ignore a rule?

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

### Why should I use PSRule keywords and assertions?

Except for the `Rule` keyword, using the built-in language features are optional.

The built-in keywords and assertions accelerate rule creation.
They do this by providing a condition and a set of reasons in a single command.

Reasons are also optional; however, they provide additional context as to why the rule failed.
Alternatively, you can provide your own reasons to complement standard PowerShell with the `Reason` keyword.

[about_PSRule_Docs]: concepts/PSRule/en-US/about_PSRule_Docs.md
[about_PSRule_Options]: concepts/PSRule/en-US/about_PSRule_Options.md
[extension-pipelines]: https://marketplace.visualstudio.com/items?itemName=bewhite.ps-rule
[extension-github]: https://github.com/marketplace/actions/psrule
[PSRule-rules]: https://www.powershellgallery.com/packages?q=Tags%3A%22PSRule-rules%22
