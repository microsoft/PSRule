---
title: Notes for upgrading to PSRule v3.0.0
author: BernieWhite
discussion: false
---

# Upgrade notes

This article contains notes to help upgrade to PSRule v3.0.0 and later.

## Unbound object names

When an object is processed by PSRule, it is assigned a name.
This name is used to identify the object in the output and to suppress the object from future processing.

Prior to _v3.0.0_, the name was generated using a SHA-1 hash of the object.
The SHA-1 algorithm is no longer considered secure and has been replaced with SHA-512.

From _v3.0.0_, if the name of an object can not be determined, the SHA-512 hash of the object will be used.
Any objects that have previously been suppressed with a name based on a SHA-1 hash will no longer be suppressed.

To resolve any issue caused by this change, you can:

1. Configure binding by setting the [Binding.TargetName][1] option to set an alternative property to use as the name. _OR_
2. Update any existing keys set with the [Suppression][2] option to use the new SHA-512 hash.

  [1]: https://aka.ms/ps-rule/options#bindingtargetname
  [2]: https://aka.ms/ps-rule/options#suppression

## Using PowerShell 7.4 or later

From _v3.0.0_, PSRule requires:

- Windows PowerShell 5.1 for running as a PowerShell module. _OR_
- PowerShell 7.4 or later for development, building locally, or running as a PowerShell module.

Support for Windows PowerShell 5.1 is deprecated and will be removed in a future release of PSRule (v4).
We recommend upgrading to PowerShell 7.4 or later.

## Changes to CLI commands

From _v3.0.0_, the CLI command names have been renamed to simplify usage.
The following changes have been made:

- To run rules, use `run` instead of `analyze`. i.e. `ps-rule run`.
- To restore modules for a workspace, use `module restore` instead of `restore`. i.e. `ps-rule module restore`.

The `run` command provides similar output to the `Assert-PSRule` cmdlet in PowerShell.

Previously the `restore` command installed modules based on the configuration of the [Requires][3] option.
From _v3.0.0_, the `module restore` command installs modules based on:

- The module lock file `ps-rule.lock.json` if set.
  Use `module` [CLI commands][5] to manage the [lock file][6]. _AND_
- Modules defined in the [Include.Module][4] option, if set.
  Additionally the [Requires][3] option is used to constrain the version of modules installed.

  [3]: concepts/PSRule/en-US/about_PSRule_Options.md#requires
  [4]: concepts/PSRule/en-US/about_PSRule_Options.md#includemodule
  [5]: concepts/cli/module.md
  [6]: concepts/lockfile.md

## Version and APIVersion accept stable

Prior to _v3.0.0_, some usage of `version` and `apiVersion` accepted pre-release versions by default.
For example:

```yaml
---
# Synopsis: Any version example
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: PreviousAnyVersionExample
spec:
  if:
    field: dateVersion
    apiVersion: ''
```

When `apiVersion` is empty any version is accepted including pre-releases.

From _v3.0.0_ pre-release versions are not accepted by default.
Set the `includePrerelease` property to `true`.

```yaml
---
# Synopsis: Test comparison with apiVersion.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: AnyVersion
spec:
  if:
    field: dateVersion
    apiVersion: ''
    includePrerelease: true
```
