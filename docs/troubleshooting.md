---
author: BernieWhite
discussion: false
---

# Troubleshooting

!!! Abstract
    This article provides troubleshooting instructions for common errors generic to PSRule or core functionality.

!!! Tip
    See [troubleshooting specific to PSRule for Azure][1] for common errors when testing Azure resources using the `PSRule.Rules.Azure` module.

  [1]: https://azure.github.io/PSRule.Rules.Azure/troubleshooting/

## Custom rules are not running

There is a few common causes of this issue including:

- **Check rule path** &mdash; By default, PSRule will look for rules in the `.ps-rule/` directory.
  This directory is the root for your repository or the current working path by default.
  On case-sensitive file systems such as Linux, this directory name is case-sensitive.
  See [Storing and naming rules][2] for more information.
- **Check file name suffix** &mdash; PSRule only looks for files with the `.Rule.ps1`, `.Rule.yaml`, or `.Rule.jsonc` suffix.
  On case-sensitive file systems such as Linux, this file suffix is case-sensitive.
  See [Storing and naming rules][2] for more information.
- **Check binding configuration** &mdash; PSRule uses _binding_ to work out which property to use for a resource type.
  To be able to use the `-Type` parameter or `type` properties in rules definitions, binding must be set.
  This is automatically configured for modules such as PSRule for Azure, however must be set in `ps-rule.yaml` for custom rules.
- **Check modules** &mdash; Check if your custom rules have a dependency on another module such as `PSRule.Rules.Azure`.
  If your rules have a dependency, be sure to add the module in your command-line.

!!! Tip
    You may be able to use `git mv` to change the case of a file if it is committed to the repository incorrectly.

  [2]: authoring/storing-rules.md#naming-rules

## Windows PowerShell is in NonInteractive mode

When running PSRule on a Windows self-hosted agent/ private runner you may encounter an error similar to the following:

!!! Error

    Exception calling "ShouldContinue" with "2" argument(s): "Windows PowerShell is in NonInteractive mode. Read and Prompt functionality is not available."

This error may be caused by the PowerShell NuGet package provider not being installed.
By default, Windows PowerShell does not have these components installed.
These components are required for installing and checking versions of PSRule modules.

To resolve this issue, install the NuGet package provider during setup the agent/ runner by using the following script:

```powershell
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}
```

Additionally consider installing the latest version of `PowerShellGet` by using the following script:

```powershell
if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion 2.2.1 -ErrorAction Ignore)) {
    Install-Module PowerShellGet -MinimumVersion 2.2.1 -Scope CurrentUser -Force -AllowClobber;
}
```

## PSR0001 - Unable to read options file

When running PSRule you may encounter an error similar to the following:

!!! Error

    PSR0001: Unable to read options file 'ps-rule.yaml'.

This error typically indicates a problem with the YAML syntax in the `ps-rule.yaml` file.
Double check the file for incorrect indentation or missing punctuation such as `-` and `:` characters.

If you still have an issue, try resaving the file as UTF-8 in an editor such as Visual Studio Code.

## PSR0002 - Summary results are not supported with Job Summaries

!!! Error

    PSR0002: Summary results are not supported with Job Summaries.

Currently using the `Output.As` with the `Summary` option is not supported with job summaries.
Choose to use one or the other.

If you have a specific use case your would like to enable, please start a [discussion][3].

  [3]: https://github.com/microsoft/PSRule/discussions
