---
description: This article contains notes on breaking changes in PSRule v3 to help you upgrade from a previous version.
author: BernieWhite
discussion: false
---

# Deprecations and breaking changes

This article contains notes on breaking changes in PSRule v3 to help you upgrade from a previous version.

## Execution options

PSRule provides a number of execution options that control logging of certain events.
In many cases these options turn a warning on or off.

These options are deprecated but replaced to provide more choice to when configuring logging options.
Now you can configure the following:

- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

The following execution options have been deprecated and will be removed from _v3_.

- `Execution.SuppressedRuleWarning` is replaced with `Execution.RuleSuppressed`.
  Set `Execution.RuleSuppressed` to `Warn` to log a warning from _v2.8.0_.
  If both options are set, `Execution.SuppressedRuleWarning` takes precedence until _v3_.
- `Execution.AliasReferenceWarning` is replaced with `Execution.AliasReference`.
  Set `Execution.AliasReference` to `Warn` to log a warning from _v2.9.0_.
  If both options are set, `Execution.AliasReferenceWarning` takes precedence until _v3_.
- `Execution.InconclusiveWarning` is replaced with `Execution.RuleInconclusive`.
  Set `Execution.RuleInconclusive` to `Warn` to log a warning from _v2.9.0_.
  If both options are set, `Execution.InconclusiveWarning` takes precedence until _v3_.
- `Execution.InvariantCultureWarning` is replaced with `Execution.InvariantCulture`.
  Set `Execution.InvariantCulture` to `Warn` to log a warning from _v2.9.0_.
  If both options are set, `Execution.InvariantCultureWarning` takes precedence until _v3_.
- `Execution.NotProcessedWarning` is replaced with `Execution.UnprocessedObject`.
  Set `Execution.UnprocessedObject` to `Warn` to log a warning from _v2.9.0_.
  If both options are set, `Execution.NotProcessedWarning` takes precedence until _v3_.

!!! Tip
    You do not need to configure both options.
    If you have the deprecated option configured, switch to the new option.

## Git Head input object

Previously when the `Input.Format` option was set to `File` the `.git/HEAD` file was emitted as an input object.
The original purpose of this feature was to allow conventions to run once against the root of the repository.
Subsequent changes to PSRule have made this feature redundant by adding support for the `-Initialize` block.

From _v3_ the `.git/HEAD` file will no longer be emitted as an input object.

Consider adding or updating a convention that uses the `-Initialize` block to emit run initialization logic.
Yon can also use the `-Initialize` block to emit a custom object to the pipeline by using the `$PSRule.ImportWithType` method.

## Rule output object

Several properties of the rule object have been renamed to improve consistency with other objects.
Previously rules returned by `Get-PSRule` returned a rule object which included the following properties:

- `RuleId`
- `RuleName`
- `Description`
- `ModuleName`
- `SourcePath`

These have been replaced with the following properties:

- `Id` instead of `RuleId`.
- `Name` instead of `RuleName`.
- `Synopsis` instead of `Description`.
- `Source.Module` instead of `ModuleName`.
- `Source.Path` instead of `SourcePath`.

The changes apply from _v2.1.0_, however the old properties are still available for backwards compatibility.
From _v3_ these properties will be removed.
These changes do not affect normal usage of PSRule.
Supporting scripts that directly use the old names may not work correctly until you update these names.

## Language block interface

Several properties of Baselines and Selectors have been renamed to improve consistency.

- `ModuleName`
- `SourcePath`

These have been replaced with the following properties:

- `Source.Module` instead of `ModuleName`.
- `Source.Path` instead of `SourcePath`.

The changes apply from _v2.1.0_, however the old properties are still available for backwards compatibility.
From _v3_ these properties will be removed.
These changes do not affect normal usage of PSRule.
Supporting scripts that directly use the old names may not work correctly until you update these names.

## Binding configuration in baselines

Prior to v3, a baseline could configure a binding configuration to modify how objects are recognized by name, type, and scope.
This existed to support scenarios before a module configuration and language scopes where core to how PSRule operates.

- Rules within a module will automatically use binding configuration from the module configuration.
  If no binding configuration is set, the configuration of the workspace will be used.
- Rules within the workspace will automatically use the binding configuration from options (`ps-rule.yaml`).

Configuring binding configuration on a baseline is removed from PSRule v3.

## Binding hooks

Prior to v3, a custom binding PowerShell script block could be used to perform custom binding inline.
This feature was hard to use and obsolete for most common use cases.

Alternatively, configure `Binding.TargetName` and `Binding.TargetType` options to use the built-in binder.

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
apiVersion: github.com/microsoft/PSRule/2025-01-01
kind: Selector
metadata:
  name: AnyVersion
spec:
  if:
    field: dateVersion
    apiVersion: ''
    includePrerelease: true
```
