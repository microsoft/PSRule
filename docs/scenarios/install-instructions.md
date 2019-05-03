# Install instructions

## Prerequisites

- Windows PowerShell 5.1 with .NET Framework 4.7.2+ or
- PowerShell Core 6.0 or greater on Windows, macOS and Linux

For a list of platforms that PowerShell Core is supported on [see](https://github.com/PowerShell/PowerShell#get-powershell).

## Getting the module

Install from [PowerShell Gallery][module-psrule] for all users (requires permissions):

```powershell
# Install PSRule module
Install-Module -Name 'PSRule';
```

Install from [PowerShell Gallery][module-psrule] for current user only:

```powershell
# Install PSRule module
Install-Module -Name 'PSRule' -Scope CurrentUser;
```

Save for offline use from PowerShell Gallery:

```powershell
# Save PSRule module, in the .\modules directory
Save-Module -Name 'PSRule' -Path '.\modules';
```

> For pre-release versions the `-AllowPrerelease` switch must be added when calling `Install-Module` or `Save-Module`.

## Getting the extension

You can install the [latest release][ext-psrule] of the Visual Studio Code (VSCode) companion extension by searching for `PSRule` in the extensions pane within VSCode and installing it.

Install by the command line:

```text
code --install-extension bewhite.psrule-vscode-preview
```

> NOTE: If you are using VS Code Insiders, the command will be `code-insiders`.

For detailed instructions, follow the steps in the [Visual Studio Code documentation][vscode-ext-gallery].

[module-psrule]: https://www.powershellgallery.com/packages/PSRule
[ext-psrule]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
[vscode-ext-gallery]: https://code.visualstudio.com/docs/editor/extension-gallery
