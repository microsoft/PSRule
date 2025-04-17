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

## Format-Default error when running Invoke-PSRule

When running PSRule you may encounter an error similar to the following:

!!! Error

    Format-Default: Cannot process argument because the value of argument "name" is not valid. Change the value of the "name" argument and run the operation again.

This error is caused by a known issue in PowerShell 7.4.0 and 7.4.1.
To resolve this issue, upgrade to PowerShell 7.4.2 or later.

For more details see [#1723][5].

  [5]: https://github.com/microsoft/PSRule/issues/1723

## Engine error messages

### PSR0001 - Unable to read options file

When running PSRule you may encounter an error similar to the following:

!!! Error

    PSR0001: Unable to read options file 'ps-rule.yaml'.

This error typically indicates a problem with the YAML syntax in the `ps-rule.yaml` file.
Double check the file for incorrect indentation or missing punctuation such as `-` and `:` characters.

If you still have an issue, try re-saving the file as UTF-8 in an editor such as Visual Studio Code.

### PSR0002 - Summary results are not supported with Job Summaries

!!! Error

    PSR0002: Summary results are not supported with Job Summaries.

Currently using the `Output.As` with the `Summary` option is not supported with job summaries.
Choose to use one or the other.

If you have a specific use case your would like to enable, please start a [discussion][3].

  [3]: https://github.com/microsoft/PSRule/discussions

### PSR0003 - The specified baseline group is not known

!!! Error

    PSR0003: The specified baseline group 'latest' is not known.

This error is caused by attempting to reference a baseline group which has not been defined.
To define a baseline group, see [Baseline.Group][4] option.

  [4]: https://aka.ms/ps-rule/options#baselinegroup

### PSR0004 - The specified resource is not known

!!! Error

    PSR0004: The specified Baseline resource 'TestModule4\Module4' is not known.

This error is caused when you attempt to reference a resource such as a baseline, rule, or selector which has not been defined.

### PSR0015 - No valid sources where found

!!! Error

    PSR0015: No valid sources were found. Please check your working path and configured options.

When this message occurs, PSRule didn't find any `*.Rule.*` files in the specified path or module.
These files contain the rules to be evaluated.

If no sources are found this is probably a configuration error, since PSRule requires at least one rule to execute.

The `Path` and `Module` arguments are used to specify the location of the rules.
By default, PSRule will look for rules in the `.ps-rule/` directory when the `Path` arguments is not set.

This may occur when:

- You are running PSRule from a different working directory than expected.
  For example, if you are running from a sub-directory of the repository, the `.ps-rule/` directory may not be found.
- Using the `Path` argument to specify a path that does not exist or is empty.
- Using the `Module` argument to specify a module that does not exist or is empty.
- The rule files are not named correctly.
  PSRule only looks for files with the `.Rule.ps1`, `.Rule.yaml`, or `.Rule.jsonc` suffix.
  On case-sensitive file systems such as Linux, this file suffix is case-sensitive.

### PSR0016 - Could not find a matching rule

!!! Error

    PSR0016: Could not find a matching rule. Please check that Path, Name and Tag parameters are correct.

When this message occurs, PSRule loaded sources but didn't find any rules that matched that should be evaluated.
If no rules are found this is probably a configuration error, since PSRule requires at least one rule to execute.

This may occur when:

- The `Path` or `Module` arguments are configured to a path or module that does not exist or does not contain any rules.
- A baseline or `ps-rule.yaml` file is configured with `name`, `tag`, or `label` properties that do not match any rules.

### PSR0017 - No valid input

!!! Error

    PSR0017: No valid input objects or files were found. Please check your working path and configured options.

When this message occurs, PSRule didn't find any input objects or files in the specified path or module.
if no input is found this is probably a configuration error, since PSRule requires at least one input to evaluate any rules.

This may occur when:

- You are running PSRule from a different working directory than expected and the input files are not found.
- The input files your are expecting to evaluate are in a path that has been excluded by `Input.PathIgnore` or `.gitignore`.

## CLI exit codes

The following table lists exit codes that may be returned by the PSRule CLI.

Exit code | Description | Notes
--------- | ----------- | -----
0         | Success | The CLI completed the operation successfully. This may occur during normal operation.
1         | Generic error. | An unexpected error occurred. Please report this issue.
100       | Break because one or more rules failed. | This may occur during normal operation when one or more rules fail. Use the `Execution.Break` option to control this behavior.
501       | Unable to manage or restore a module. | This may occur when attempting to restoring a module that is not available.
502       | Failed to find a module. | A specified module could not be found in PowerShell Gallery.
503       | The module version does not meet configured version constraint requirements. | The module version that was specified on the command line does not meet the configured `Requires` option.

## Language server exit codes

The following table lists exit codes that may be returned by the PSRule language server.

Exit code | Description | Notes
--------- | ----------- | -----
0         | Success | The language server exited during normal operation.
901       | The language server was unable to start due to missing or invalid configuration. | Unexpected. Please report this issue.
902       | The language server encountered an unexpected exception and stopped. | Unexpected. Please report this issue.
903       | A debugger failed to attach to the language server. | When debugging the language server, ensure the debugger is attached within 5 minutes.
