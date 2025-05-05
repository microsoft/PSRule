---
description: This article contains notes on breaking changes in PSRule v3 to help you upgrade from v2.
author: BernieWhite
discussion: false
---

# Deprecations and breaking changes

The following sections describe specific breaking changes that may affect your use of PSRule when upgrading from v2.x.x.

## Changes to behavior

### Input format of files and strings

How input files and string objects are deserialized has had a major refactor.
As part of the change, the `Input.Format` option has been removed.

This option previously had a dual purpose of setting the deserialization files and string objects.

As is option is not longer applicable, the `-Format` parameter has also been removed from the following PowerShell cmdlets:
`Invoke-PSRule`, `Assert-PSRule`, `Get-PSRuleTarget`, `Test-PSRuleTarget`, `New-PSRuleOption`, and `Set-PSRuleOption`.

Additionally the `format:` input in GitHub Actions and Azure Pipelines has also been removed.

The replacement for the `Input.Format` option is emitters.
Emitters can be enabled and configured with the [`Format` option][7].
This allows for more flexibility over how input files are deserialized, and allows multiple emitters to be used simultaneously.
For string objects, the `Input.StringFormat` option has been added to configure specify which emitter is used.

Additionally, to simplify enabling one or more emitters for a specific run similar to the previous behavior:

- **CLI** &mdash; Use the `--formats` argument with one or more formats.
- **GitHub Actions/ Azure Pipelines** &mdash; Use the `formats` input with one or more formats.
- **PowerShell** &mdash; Use the `-Formats` parameter with one or more formats.
  This applies to `Invoke-PSRule`, `Assert-PSRule`, `Get-PSRuleTarget`, and `Test-PSRuleTarget` cmdlets.

  [7]: https://microsoft.github.io/PSRule/v3/concepts/PSRule/en-US/about_PSRule_Options/#format

For example, to run the CLI with YAML and JSON formats:

```bash
ps-rule run -f . --formats yaml json
```

Or in PowerShell:

```powershell
Invoke-PSRule -InputPath . -Formats yaml,json
```

Or in GitHub Actions:

```yaml
- name: Analyze with PSRule
  uses: microsoft/ps-rule@v3.0.0
  with:
    formats: yaml,json
```

Or in Azure Pipelines:

```yaml
- task: PSRule@3
  displayName: Analyze with PSRule
  inputs:
    formats: yaml,json
```

### Version and APIVersion accept stable

Prior to _v3.0.0_, some usage of `version` and `apiVersion` accepted pre-release versions by default.
For example:

```yaml
---
# Synopsis: Any version example.
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

### Git Head input object

Previously when the `Input.Format` option was set to `File` the `.git/HEAD` file was emitted as an input object.
The original purpose of this feature was to allow conventions to run once against the root of the repository.
Subsequent changes to PSRule have made this feature redundant by adding support for the `-Initialize` block.

From _v3_ the `.git/HEAD` file will no longer be emitted as an input object.

Consider adding or updating a convention that uses the `-Initialize` block to emit run initialization logic.
Yon can also use the `-Initialize` block to emit a custom object to the pipeline by using the `$PSRule.ImportWithType` method.

### Binding configuration in baselines

Prior to v3, a baseline could configure a binding configuration to modify how objects are recognized by name, type, and scope.
This existed to support scenarios before a module configuration and language scopes where core to how PSRule operates.

- Rules within a module will automatically use binding configuration from the module configuration.
  If no binding configuration is set, the configuration of the workspace will be used.
- Rules within the workspace will automatically use the binding configuration from options (`ps-rule.yaml`).

Configuring binding configuration on a baseline is removed from PSRule v3.

### Binding hooks

Prior to v3, a custom binding PowerShell script block could be used to perform custom binding inline.
This feature was hard to use and obsolete for most common use cases.

Alternatively, configure `Binding.TargetName` and `Binding.TargetType` options to use the built-in binder.

### Unbound object names

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

## Changes to interface

### Changes to CLI commands

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

## Changes to configuration options

### Execution options

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

### Logging options

The following legacy logging options have been removed because they are no longer effective for their intended purpose:

- `Logging.RuleFail`
- `Logging.RulePass`
- `Logging.LimitDebug`
- `Logging.LimitVerbose`

## Changes to API

### Rule output object

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

### Language block interface

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

## Changes to supported platforms

### Using PowerShell 7.4 or later

From _v3.0.0_, PSRule requires:

- Windows PowerShell 5.1 for running as a PowerShell module. _OR_
- PowerShell 7.4 or later for development, building locally, or running as a PowerShell module.

Support for Windows PowerShell 5.1 is deprecated and will be removed in a future release of PSRule (v4).
We recommend upgrading to PowerShell 7.4 or later.
