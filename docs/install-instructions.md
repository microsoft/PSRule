# Install instructions

## Prerequisites

- Windows PowerShell 5.1 with .NET Framework 4.7.2+ or
- PowerShell 7.1 or greater on Windows, MacOS, and Linux

For a list of platforms that PowerShell 7.1 is supported on [see][get-powershell].

## Getting the module

Install from [PowerShell Gallery][module] for all users (requires permissions):

```powershell
# Install PSRule module
Install-Module -Name 'PSRule' -Repository PSGallery;
```

Install from [PowerShell Gallery][module] for current user only:

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
> To install pre-release module versions, upgrading to the latest version of _PowerShellGet_ may be required.
To do this use:
>
> `Install-Module -Name PowerShellGet -Repository PSGallery -Scope CurrentUser -Force`

## Getting the extension for Visual Studio Code

Install the [latest release][extension-vscode] from the Visual Studio Marketplace.
Within Visual Studio Code (VSCode) search for `PSRule` in the extensions pane.

Install by the command line:

```text
code --install-extension bewhite.psrule-vscode-preview
```

> NOTE: If you are using the Insiders build, the command will be `code-insiders`.

For detailed instructions, follow the steps in the [Visual Studio Code documentation][vscode-ext-gallery].

## Getting the extension for Azure Pipelines

Install the [latest release][extension-pipelines] from the Visual Studio Marketplace.
For detailed instructions see [Install extensions][pipelines-install].

If you don't have permissions to install extensions within your Azure DevOps organization, you can request it to be installed by an admin instead.

## Getting the GitHub action

Add the [latest version][extension-github] from the GitHub Marketplace to a workflow.

```yaml
- name: Run PSRule analysis
  uses: Microsoft/ps-rule@v1.1.0
```

For detailed instructions and change log see the [action details][extension-github].

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

If you are on a network that does not permit Internet access to the PowerShell Gallery,
download these modules on an alternative device that has access.
The following script can be used to download the required modules to an alternative device.
After downloading the modules copy the module directories to devices with restricted Internet access.

```powershell
# Save modules, in the .\modules directory
Save-Module -Name PlatyPS, Pester, PSScriptAnalyzer, PowerShellGet, PackageManagement, InvokeBuild -Repository PSGallery -Path '.\modules';
```

Additionally .NET Core SDK v3.1 is required.
.NET Core will not be automatically downloaded and installed.
To download and install the latest SDK see [Download .NET Core 3.1][dotnet].

[module]: https://www.powershellgallery.com/packages/PSRule
[get-powershell]: https://github.com/PowerShell/PowerShell#get-powershell
[dotnet]: https://dotnet.microsoft.com/download/dotnet-core/3.1
[extension-vscode]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
[extension-pipelines]: https://marketplace.visualstudio.com/items?itemName=bewhite.ps-rule
[extension-github]: https://github.com/marketplace/actions/psrule
[vscode-ext-gallery]: https://code.visualstudio.com/docs/editor/extension-gallery
[pipelines-install]: https://docs.microsoft.com/en-us/azure/devops/marketplace/install-extension?view=azure-devops&tabs=browser
