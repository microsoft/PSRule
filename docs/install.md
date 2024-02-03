---
author: BernieWhite
---

# Install PSRule tools

PSRule supports running within continuous integration (CI) systems or locally.
It is shipped as a PowerShell module which makes it easy to install and distribute updates.

!!! Tip
    PSRule provides native integration to popular CI systems such as GitHub Actions and Azure Pipelines.
    If you are using a different CI system you can use the local install to run on MacOS,
    Linux, and Windows worker nodes.

## With GitHub Actions

[:octicons-workflow-24: GitHub Action][1]

Install and use PSRule with GitHub Actions by referencing the `microsoft/ps-rule` action.

=== "Specific version"

    ```yaml title="GitHub Actions"
    - name: Analyze with PSRule
      uses: microsoft/ps-rule@v2.9.0
    ```

=== "Latest stable v2"

    ```yaml title="GitHub Actions"
    - name: Analyze with PSRule
      uses: microsoft/ps-rule@v2
    ```

=== "Latest stable"

    ```yaml title="GitHub Actions"
    - name: Analyze with PSRule
      uses: microsoft/ps-rule@latest
    ```

This will automatically install compatible versions of all dependencies.

  [1]: https://github.com/marketplace/actions/psrule

### Working with Dependabot

You can use [Dependabot][7] to automatically upgrade your PSRule action if you use a specific version.
When new versions a released Dependabot will automatically add a pull request (PR) for you to review and merge.

```yaml title=".github/dependabot.yaml"
#
# Dependabot configuration
#
version: 2
updates:

  # Maintain GitHub Actions
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "daily"
```

  [7]: https://docs.github.com/code-security/dependabot/working-with-dependabot

## With Azure Pipelines

[:octicons-workflow-24: Extension][2]

Install and use PSRule with Azure Pipeline by using extension tasks.
Install the extension from the marketplace, then use the `ps-rule-assert` task in pipeline steps.

```yaml
- task: ps-rule-assert@2
  displayName: Analyze Azure template files
  inputs:
    inputType: repository
```

This will automatically install compatible versions of all dependencies.

  [2]: https://marketplace.visualstudio.com/items?itemName=bewhite.ps-rule

## Installing locally

PSRule can be installed locally from the PowerShell Gallery using PowerShell.
You can also use this option to install on CI workers that are not natively supported.

The following platforms are supported:

- Windows PowerShell 5.1 with .NET Framework 4.7.2 or greater.
- PowerShell 7.3 or greater on MacOS, Linux, and Windows.

### Installing PowerShell

PowerShell 7.x can be installed on MacOS, Linux, and Windows but is not installed by default.
For a list of platforms that PowerShell 7.3 is supported on and install instructions see [Get PowerShell][3].

!!! Note
    If you are using Windows PowerShell you may need to bootstrap NuGet before you can install modules.
    The NuGet package provider is not installed in Windows PowerShell be default.
    For instructions see [Bootstrapping NuGet][6].

  [3]: https://github.com/PowerShell/PowerShell#get-powershell
  [6]: https://learn.microsoft.com/powershell/scripting/gallery/how-to/getting-support/bootstrapping-nuget

### Getting the modules

[:octicons-download-24: Module][module]

PSRule can be installed or updated from the PowerShell Gallery.
Use the following command line examples from a PowerShell terminal to install or update PSRule.

=== "For the current user"
    To install PSRule for the current user use:

    ```powershell
    Install-Module -Name 'PSRule' -Repository PSGallery -Scope CurrentUser
    ```

    To update PSRule for the current user use:

    ```powershell
    Update-Module -Name 'PSRule' -Scope CurrentUser
    ```

=== "For all users"
    Open PowerShell with _Run as administrator_ on Windows or `sudo pwsh` on Linux.

    To install PSRule for all users (requires admin/ root permissions) use:

    ```powershell
    Install-Module -Name 'PSRule' -Repository PSGallery -Scope AllUsers
    ```

    To update PSRule for all users (requires admin/ root permissions) use:

    ```powershell
    Update-Module -Name 'PSRule' -Scope AllUsers
    ```

### Pre-release versions

To use a pre-release version of PSRule add the `-AllowPrerelease` switch when calling `Install-Module`,
`Update-Module`, or `Save-Module` cmdlets.

!!! Tip
    To install pre-release module versions, the latest version of _PowerShellGet_ may be required.

    ```powershell
    # Install the latest PowerShellGet version
    Install-Module -Name PowerShellGet -Repository PSGallery -Scope CurrentUser -Force
    ```

=== "For the current user"
    To install PSRule for the current user use:

    ```powershell
    Install-Module -Name PowerShellGet -Repository PSGallery -Scope CurrentUser -Force
    Install-Module -Name 'PSRule' -Repository PSGallery -Scope CurrentUser -AllowPrerelease
    ```

=== "For all users"
    Open PowerShell with _Run as administrator_ on Windows or `sudo pwsh` on Linux.

    To install PSRule for all users (requires admin/ root permissions) use:

    ```powershell
    Install-Module -Name PowerShellGet -Repository PSGallery -Scope CurrentUser -Force
    Install-Module -Name 'PSRule' -Repository PSGallery -Scope AllUsers -AllowPrerelease
    ```

### Building from source

[:octicons-file-code-24: Source][5]

PSRule is provided as open source on GitHub.
To build PSRule from source code:

1. Clone the GitHub [repository][5].
2. Run `./build.ps1` from a PowerShell terminal in the cloned path.

This build script will compile the module and documentation then output the result into `out/modules/PSRule`.

  [5]: https://github.com/microsoft/PSRule.git

#### Development dependencies

The following PowerShell modules will be automatically install if the required versions are not present:

- PlatyPS
- Pester
- PSScriptAnalyzer
- PowerShellGet
- PackageManagement
- InvokeBuild

These additional modules are only required for building PSRule.

Additionally .NET SDK v7 is required.
.NET will not be automatically downloaded and installed.
To download and install the latest SDK see [Download .NET 7.0][dotnet].

### Limited access networks

If you are on a network that does not permit Internet access to the PowerShell Gallery,
download the required PowerShell modules on an alternative device that has access.
PowerShell provides the `Save-Module` cmdlet that can be run from a PowerShell terminal to do this.

The following command lines can be used to download the required modules using a PowerShell terminal.
After downloading the modules, copy the module directories to devices with restricted Internet access.

=== "Runtime modules"
    To save PSRule for offline use:

    ```powershell
    Save-Module -Name 'PSRule' -Path '.\modules'
    ```

    This will save PSRule into the `modules` sub-directory.

=== "Development modules"
    To save PSRule development module dependencies for offline use:

    ```powershell
    $modules = @('PlatyPS', 'Pester', 'PSScriptAnalyzer', 'PowerShellGet',
    'PackageManagement', 'InvokeBuild')
    Save-Module -Name $modules -Repository PSGallery -Path '.\modules';
    ```

    This will save required developments dependencies into the `modules` sub-directory.

!!! Tip
    If you use additional rules modules such as PSRule for Azure you should also save these for offline use.

!!! Note
    If you are using Windows PowerShell you may need to bootstrap NuGet before you can install modules.
    The NuGet package provider is not installed in Windows PowerShell be default.
    For instructions see [Bootstrapping NuGet][6].

*[CI]: continuous integration

[module]: https://www.powershellgallery.com/packages/PSRule
[dotnet]: https://dotnet.microsoft.com/download/dotnet/7.0
