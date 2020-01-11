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

## Cross-platform

PSRule uses modern PowerShell libraries at its core, allowing it to go anywhere Windows PowerShell 5.1 or PowerShell Core 6.2 can go.
PSRule runs on MacOS, Linux and Windows.

To install PSRule use the `Install-Module` cmdlet within Windows PowerShell or PowerShell Core.

```powershell
Install-Module -Name PSRule -Scope CurrentUser;
```

PSRule also has editor support for Visual Studio Code with the companion extension.
The extension is available for installation on MacOS, Linux and Windows.

To install the extension:

```text
code --install-extension bewhite.psrule-vscode-preview
```

For additional installation options, see [install instructions](scenarios/install-instructions.md).

## Reusable

Define rules once then reuse and share rules across teams or organizations.
Rules can be packaged up into a module then distributed.

PSRule uses PowerShell modules as the standard way to distribute rules.
Modules containing rules can be published on the PowerShell Gallery or network share using the same process as regular PowerShell modules.

For a walk through see [Packaging rules in a module](scenarios/rule-module/rule-module.md).

## Recommendations

PSRule allows rule authors to define recommendations in markdown.
This allows not only the cause of the issue to be identified but detailed instructions to be included to remediate issues.

For more information see [about_PSRule_Docs](concepts/PSRule/en-US/about_PSRule_Docs.md).

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

- Infrastructure code templates and manifests, such as Kubernetes manifests.
- Deployments or configurations against a baseline.

If you want to test PowerShell code, use Pester.

### Why should I use PSRule keywords and assertions?

With the exception of the `Rule` keyword, using the built-in language features are optional.

The built-in keywords and assertions accelerate rule creation.
They do this by providing a condition and a set of reasons in a single command.

Reasons are also optional, however they provide additional context as to why the rule failed.
Alternatively, you can provide your own reasons to complement standard PowerShell with the `Reason` keyword.
