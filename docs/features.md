# PSRule features

The following sections describe key features of PSRule.

- [Extensible](#extensible)
- [Cross-platform](#cross-platform)
- [Reusable](#reusable)
- [Recommendations](#recommendations)

## Extensible

Authors define rules using PowerShell, a flexible scripting language. If you or your team already can write a basic PowerShell script, you can already define a rule. What's more, you can tap into a large world-wide community of PowerShell users with scripts and cmdlets to help you build out rules quickly.

## Cross-platform

PSRule uses modern PowerShell libraries at its core, allowing it to go anywhere Windows PowerShell 5.1 or PowerShell Core 6.2 can go. PSRule runs on MacOS, Linux and Windows.

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

For additional installation options see [install instructions](scenarios/install-instructions.md).

## Reusable

Define rules once then reuse and share rules across teams or organizations.
Rules can be packaged up into a module then distributed.

PSRule uses PowerShell modules as the standard way to distribute rules.
Modules containing rules can be published on the PowerShell Gallery or network share using the same process as regular PowerShell modules.

## Recommendations

PSRule allows rule authors to define recommendations in markdown.
This allows not only the cause of the issue to be identified but detailed instructions to be included to remediate issues.

For more information see [about_PSRule_docs](concepts/PSRule/en-US/about_PSRule_Docs.md).
