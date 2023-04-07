---
author: BernieWhite
discussion: false
---

# Deprecations

## Deprecations for v3

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

!!! Tip
    You do not need to configure both options.
    If you have the deprecated option configured, switch to the new option.

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

## Deprecations for v2

### Default baseline by module manifest

When packaging baselines in a module, you may want to specify a default baseline.
PSRule _v1.9.0_ added support for setting the default baseline in a module configuration.

Previously a default baseline could be set by specifying the baseline in the module manifest.
From _v1.9.0_ this is deprecated and will be removed from _v2_.

For details on how to migrate to the new default baseline option, continue reading the [upgrade notes][1].

  [1]: upgrade-notes.md#setting-default-module-baseline

### Resources without an API version

When creating YAML and JSON resources you define a resource by specifying the `apiVersion` and `kind`.
To allow new schema versions for resources to be introduced in the future, an `apiVersion` was introduced.
For backwards compatibility, resources without an `apiVersion` deprecated but supported.
From _v2_ resources without an `apiVersion` will be ignored.

For details on how to add an `apiVersion` to a resource, continue reading the [upgrade notes][2].

  [2]: upgrade-notes.md#setting-resource-api-version
