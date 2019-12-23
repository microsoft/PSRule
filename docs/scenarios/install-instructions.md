# Install instructions

## Prerequisites

- Windows PowerShell 5.1 with .NET Framework 4.7.2+ or
- PowerShell Core 6.2 or greater on Windows, MacOS and Linux

For a list of platforms that PowerShell Core is supported on [see](https://github.com/PowerShell/PowerShell#get-powershell).

## Getting the module

Install from [PowerShell Gallery][module-psrule] for all users (requires permissions):

```powershell
# Install PSRule module
Install-Module -Name 'PSRule' -Repository PSGallery;
```

Install from [PowerShell Gallery][module-psrule] for current user only:

```powershell
# Install PSRule module
Install-Module -Name 'PSRule' -Repository PSGallery -Scope CurrentUser;
```

Save for offline use from PowerShell Gallery:

```powershell
# Save PSRule module, in the .\modules directory
Save-Module -Name 'PSRule' -Repository PSGallery -Path '.\modules';
```

> For pre-release versions the `-AllowPrerelease` switch must be added when calling `Install-Module` or `Save-Module`.
>
> To install pre-release module versions, upgrading to the latest version of _PowerShellGet_ may be required. To do this use:
>
> `Install-Module -Name PowerShellGet -Repository PSGallery -Scope CurrentUser -Force`

## Getting the extension

You can install the [latest release][ext-psrule] of the Visual Studio Code (VSCode) companion extension by searching for `PSRule` in the extensions pane within VSCode and installing it.

Install by the command line:

```text
code --install-extension bewhite.psrule-vscode-preview
```

> NOTE: If you are using VS Code Insiders, the command will be `code-insiders`.

For detailed instructions, follow the steps in the [Visual Studio Code documentation][vscode-ext-gallery].

## Building from source

To build this module from source run `./build.ps1`.
This build script will compile the module and documentation then output the result into `out/modules/PSRule`.

The following PowerShell modules will be automatically downloaded if the required versions are not present:

- PlatyPS
- Pester
- PSScriptAnalyzer
- PowerShellGet
- PackageManagement
- InvokeBuild

These additional modules are only required for building PSRule and are not required for running PSRule.

If you are on a network that does not permit Internet access to the PowerShell Gallery, download these modules on an alternative device that has access.
The following script can be used to download the required modules to an alternative device.
After downloading the modules copy the module directories to devices with restricted Internet access.

```powershell
# Save modules, in the .\modules directory
Save-Module -Name PlatyPS, Pester, PSScriptAnalyzer, PowerShellGet, PackageManagement, InvokeBuild -Repository PSGallery -Path '.\modules';
```

Additionally .NET Core SDK v2.1 is required. .NET Core will not be automatically downloaded and installed.
To download and install .NET Core SDK see [Download .NET Core 2.1](https://dotnet.microsoft.com/download/dotnet-core/2.1).

[module-psrule]: https://www.powershellgallery.com/packages/PSRule
[ext-psrule]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
[vscode-ext-gallery]: https://code.visualstudio.com/docs/editor/extension-gallery
