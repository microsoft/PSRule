---
title: Notes for upgrading between PSRule versions
author: BernieWhite
---

# Upgrade notes

This document contains notes to help upgrade from previous versions of PSRule.

## Upgrading to v2.0.0

### Resources naming restrictions

When naming resources such as rules or selectors, the following restrictions apply:

- **Use between 3 and 128 characters** &mdash; This is the minimum and maximum length of a resource name.
- **Only use allowed characters** &mdash;
  To preserve consistency between file systems, some characters are not permitted.
  Dots, hyphens, and underscores are not permitted at the start and end of the name.
  Additionally some characters are restricted for future use.
  The following characters are not permitted:
  - `<` (less than)
  - `>` (greater than)
  - `:` (colon)
  - `/` (forward slash)
  - `\` (backslash)
  - `|` (vertical bar or pipe)
  - `?` (question mark)
  - `*` (asterisk)
  - `"` (double quote)
  - `'` (single quote)
  - `` ` `` (backtick)
  - `+` (plus)
  - `@` (at sign)
  - Integer value zero, sometimes referred to as the ASCII NUL character.
  - Characters whose integer representations are in the range from 1 through 31.

Prior to _v2.0.0_, there was no specific naming restriction for resources.
However functionally PSRule and downstream components could not support all resource names.
To avoid confusion, we have decided to restrict resource names to a specific set of characters.

From _v2.0.0_, resource names that do not meet the naming restrictions will generate an error.

```text title="Regular expression for valid resource names"
^[^<>:/\\|?*"'`+@._\-\x00-\x1F][^<>:/\\|?*"'`+@\x00-\x1F]{1,126}[^<>:/\\|?*"'`+@._\-\x00-\x1F]$
```

### Setting default module baseline

When packaging rules in a module, you can set the default baseline.
The default baseline from the module will be automatically used unless overridden.

Prior to _v1.9.0_ the default baseline was set by configuring the module manifest `.psd1` file.
From _v1.9.0_ the default baseline can be configured by within a module configuration.
Using module configuration is the recommended method.
Setting the default baseline from module manifest and has been removed from _v2.0.0_.

A module configuration can be defined in YAML.

!!! Example

    ```yaml hl_lines="8-9"
    ---
    # Synopsis: Example module configuration for Enterprise.Rules module.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: ModuleConfig
    metadata:
      name: Enterprise.Rules
    spec:
      rule:
        baseline: Enterprise.Default
    ```

### Setting resource API version

When creating YAML and JSON resources you define a resource by specifying the `apiVersion` and `kind`.
An `apiVersion` was added as a requirement from _v1.2.0_.
For compatibility, resources without an `apiVersion` were supported however deprecated for removal.
This has now been removed from _v2.0.0_.

When defining resource specify an `apiVersion`.
Currently this must be set to `github.com/microsoft/PSRule/v1`.

=== "YAML"

    ```yaml hl_lines="3-4"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
    spec:
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

=== "JSON"

    ```json hl_lines="4-5"
    [
        {
            // Synopsis: An example rule to require TLS.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Rule",
            "metadata": {
                "name": "Local.JSON.RequireTLS"
            },
            "spec": {
                "condition": {
                    "field": "configure.supportsHttpsTrafficOnly",
                    "equals": true
                }
            }
        }
    ]
    ```

### Change in source file discovery for Get-PSRuleHelp

Previously in PSRule _v1.11.0_ and prior versions, rules would show up twice when running `Get-PSRuleHelp` in the context of a module and in the same working directory of the module.
This behavior has now been removed from _v2.0.0_.

Module files are now preferred over loose files, and rules are only shown once in the output.
Any duplicate rule names from loose files are outputted as a warning instead.

The old behavior:

```powershell
Name                                ModuleName               Synopsis
----                                ----------               --------
M1.Rule1                                                     This is the default
M1.Rule2                                                     This is the default
M1.Rule1                            TestModule               Synopsis en-AU.
M1.Rule2                            TestModule               This is the default
```

The new behavior:

```powershell
WARNING: A rule with the same name 'M1.Rule1' already exists.
WARNING: A rule with the same name 'M1.Rule2' already exists.

Name                                ModuleName               Synopsis
----                                ----------               --------
M1.Rule1                            TestModule               Synopsis en-AU.
M1.Rule2                            TestModule               This is the default
```

### Require source discovery from current working directory to be explicitly included

Previously in PSRule _v1.11.0_ and prior versions, rule sources from the current working directory without the `-Path` and `-Module` parameters were automatically included.
This behavior has now been removed from _v2.0.0_.

Rules sources in the current working directory are only included if `-Path .` or `-Path $PWD` is specified.

The old behavior:

```powershell
Set-Location docs\scenarios\azure-resources
Get-PSRule

RuleName                            ModuleName                 Synopsis
--------                            ----------                 --------
appServicePlan.MinInstanceCount                                App Service Plan has multiple instances
appServicePlan.MinPlan                                         Use at least a Standard App Service Plan
appServiceApp.ARRAffinity                                      Disable client affinity for stateless services
appServiceApp.UseHTTPS                                         Use HTTPS only
storageAccounts.UseHttps                                       Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
storageAccounts.UseEncryption                                  Use at-rest storage encryption
```

The new behavior:

```powershell
Set-Location docs\scenarios\azure-resources
Get-PSRule

# No output, need to specify -Path explicitly

Get-PSRule -Path $PWD

RuleName                            ModuleName                 Synopsis
--------                            ----------                 --------
appServicePlan.MinInstanceCount                                App Service Plan has multiple instances
appServicePlan.MinPlan                                         Use at least a Standard App Service Plan
appServiceApp.ARRAffinity                                      Disable client affinity for stateless services
appServiceApp.UseHTTPS                                         Use HTTPS only
storageAccounts.UseHttps                                       Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
storageAccounts.UseEncryption      
```

## Upgrading to v1.4.0

Follow these notes to upgrade to PSRule _v1.4.0_ from previous versions.

### Change in default output styles

Previously in PSRule _v1.3.0_ and prior the default style when using `Assert-PSRule` was `Client`.
From _v1.4.0_ PSRule now defaults to `Detect`.

The `Detect` output style falls back to `Client` however may detect one of the following styles instead:

- `AzurePipelines` - Output is written for integration Azure Pipelines.
- `GitHubActions` - Output is written for integration GitHub Actions.
- `VisualStudioCode` - Output is written for integration with Visual Studio Code.

Detect uses the following logic:

1. If the `TF_BUILD` environment variable is set to `true`, `AzurePipelines` will be used.
2. If the `GITHUB_ACTIONS` environment variable is set to `true`, `GitHubActions` will be used.
3. If the `TERM_PROGRAM` environment variable is set to `vscode`, `VisualStudioCode` will be used.
4. Use `Client`.

To force usage of the `Client` output style set the `Output.Style` option.
For example:

```yaml
# YAML: Using the output/style property
output:
  style: Client
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_STYLE=Client
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_STYLE: Client
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_STYLE
  value: Client
```

## Upgrading to v1.0.0

Follow these notes to upgrade to PSRule _v1.0.0_ from previous versions.

### Replaced $Rule target properties

Previously in PSRule _v0.22.0_ and prior the `$Rule` automatic variable had the following properties:

- `TargetName`
- `TargetType`
- `TargetObject`

For example:

```powershell
Rule 'Rule1' {
    $Rule.TargetName -eq 'Name1';
    $Rule.TargetType -eq '.json';
    $Rule.TargetObject.someProperty -eq 1;
}
```

In _v1.0.0_ these properties have been removed after being deprecated in _v0.12.0_.
These properties are instead available on the `$PSRule` variable.
Rules referencing the deprecated properties of `$Rule` must be updated.

For example:

```powershell
Rule 'Rule1' {
    $PSRule.TargetName -eq 'Name1';
    $PSRule.TargetType -eq '.json';
    $PSRule.TargetObject.someProperty -eq 1;
}
```
