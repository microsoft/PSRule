# Change log

See [upgrade notes][1] for helpful information when upgrading from previous versions.

  [1]: https://microsoft.github.io/PSRule/latest/upgrade-notes/

**Important notes**:

- YAML resources will require an `apiVersion` from PSRule v2. [#648](https://github.com/microsoft/PSRule/issues/648)
- Setting the default module baseline requires a module configuration from PSRule v2. [#809](https://github.com/microsoft/PSRule/issues/809)

## Unreleased

## v2.0.0-B2202017 (pre-release)

What's changed since pre-release v2.0.0-B2202006:

- Bug fixes:
  - Fixed GitHub code scanning alerts treats pass as problems. [#955](https://github.com/microsoft/PSRule/issues/955)
    - By default, SARIF output will only include fail or error outcomes.
    - Added `Output.SarifProblemsOnly` option to include pass outcomes.
  - Fixed SARIF output includes rule property for default tool component. [#956](https://github.com/microsoft/PSRule/issues/956)

## v2.0.0-B2202006 (pre-release)

What's changed since pre-release v2.0.0-B2201161:

- General improvements:
  - Added new properties for module lookup to SARIF results. [#951](https://github.com/microsoft/PSRule/issues/951)
- Bug fixes:
  - Fixed rule references in SARIF format for extensions need a toolComponent reference. [#949](https://github.com/microsoft/PSRule/issues/949)
  - Fixed file objects processed with file input format have no source location. [#950](https://github.com/microsoft/PSRule/issues/950)

## v2.0.0-B2201161 (pre-release)

What's changed since pre-release v2.0.0-B2201146:

- New features:
  - Added support for rule severity level. [#880](https://github.com/microsoft/PSRule/issues/880)
    - Rules can be configured to be `Error`, `Warning`, or `Information`.
    - Failing rules with the `Error` severity level will cause the pipeline to fail.
    - Rules with the `Warning` severity level will be reported as warnings.
    - Rules with the `Information` severity level will be reported as informational messages.
    - By default, the severity level for a rule is `Error`.
  - Added expression support for type based assertions. [#908](https://github.com/microsoft/PSRule/issues/908)
    - Included the following expressions:
      - `IsArray`
      - `IsBoolean`
      - `IsDateTime`
      - `IsInteger`
      - `IsNumeric`
  - Added support for formatting results as SARIF. [#878](https://github.com/microsoft/PSRule/issues/878)
    - Set `Output.Format` to `Sarif` to output results in the SARIF format.
    - See [about_PSRule_Options] for details.

## v2.0.0-B2201146 (pre-release)

What's changed since pre-release v2.0.0-B2201135:

- Engineering:
  - **Breaking change:** Require rule sources from current working directory to be explicitly included. [#760](https://github.com/microsoft/PSRule/issues/760)
    - From v2 onwards, `$PWD` is not included by default unless `-Path .` or `-Path $PWD` is explicitly specified.
    - See [upgrade notes][1] for details.
- Bug fixes:
  - Fixed rule source loading twice from `$PWD` and `.ps-rule/`. [#939](https://github.com/microsoft/PSRule/issues/939)

## v2.0.0-B2201135 (pre-release)

What's changed since pre-release v2.0.0-B2201117:

- Engineering:
  - **Breaking change:** Prefer module sources over loose files. [#610](https://github.com/microsoft/PSRule/issues/610)
    - Module sources are discovered before loose files.
    - Warning is shown for duplicate rule names, and exception is thrown for duplicate rule Ids.
    - See [upgrade notes][1] for details.
  - Added more tests for JSON resources. [#929](https://github.com/microsoft/PSRule/issues/929)
- Bug fixes:
  - Fixed empty suppression group rules property applies to no rules. [#931](https://github.com/microsoft/PSRule/issues/931)
  - Fixed object reference for suppression group will rule not defined. [#932](https://github.com/microsoft/PSRule/issues/932)

## v2.0.0-B2201117 (pre-release)

What's changed since pre-release v2.0.0-B2201093:

- General improvements:
  - Add option to disable invariant culture warning. [#899](https://github.com/microsoft/PSRule/issues/899)
    - Added `Execution.InvariantCultureWarning` option.
    - See [about_PSRule_Options] for details.

## v2.0.0-B2201093 (pre-release)

What's changed since pre-release v2.0.0-B2201075:

- New features:
  - Add support for suppression groups. [#793](https://github.com/microsoft/PSRule/issues/793)
    - New `SuppressionGroup` resource has been included.
    - See [about_PSRule_SuppressionGroups] for details.

## v2.0.0-B2201075 (pre-release)

What's changed since pre-release v2.0.0-B2201054:

- General improvements:
  - Added support for rule aliases. [#792](https://github.com/microsoft/PSRule/issues/792)
    - Aliases allow rules to be references by an alternative name.
    - When renaming rules, add a rule alias to avoid breaking references to the old rule name.
    - To specify an alias use the `-Alias` parameter or `alias` metadata property in YAML or JSON.
  - Added support for stable identifiers with rule refs. [#881](https://github.com/microsoft/PSRule/issues/881)
    - A rule ref may be optionally be used to reference a rule.
    - Rule refs should be:
      stable, not changing between releases;
      opaque, as opposed to being a human-readable string.
      Stable and opaque refs ease web lookup and to help to avoid language difficulties.
    - To specify a rule ref use the `-Ref` parameter or `ref` metadata property in YAML or JSON.
- Bug fixes:
  - Fixed object path handling with dash. [#902](https://github.com/microsoft/PSRule/issues/902)

## v2.0.0-B2201054 (pre-release)

What's changed since v1.11.0:

- General improvements:
  - Added support for object path expressions. [#808](https://github.com/microsoft/PSRule/issues/808) [#693](https://github.com/microsoft/PSRule/issues/693)
    - Inspired by JSONPath, object path expressions can be used to access nested objects.
    - Array members can be filtered and enumerated using object path expressions.
    - Object path expressions can be used in YAML, JSON, and PowerShell rules and selectors.
    - See [about_PSRule_Assert] for details.
  - Improve tracking of suppressed objects. [#794](https://github.com/microsoft/PSRule/issues/794)
    - Added `Execution.SuppressedRuleWarning` option to output warning for suppressed rules.
- Engineering:
  - **Breaking change:** Removal of deprecated default baseline from module manifest. [#755](https://github.com/microsoft/PSRule/issues/755)
    - Set the default module baseline using module configuration.
    - See [upgrade notes][1] for details.
  - **Breaking change:** Require `apiVersion` on YAML and JSON to be specified. [#648](https://github.com/microsoft/PSRule/issues/648)
    - Resources should use `github.com/microsoft/PSRule/v1` as the `apiVersion`.
    - Resources that do not specify an `apiVersion` will be ignored.
    - See [upgrade notes][1] for details.

[Assert-PSRule]: commands/PSRule/en-US/Assert-PSRule.md
[about_PSRule_Assert]: concepts/PSRule/en-US/about_PSRule_Assert.md
[about_PSRule_Options]: concepts/PSRule/en-US/about_PSRule_Options.md
[about_PSRule_Variables]: concepts/PSRule/en-US/about_PSRule_Variables.md
[about_PSRule_Conventions]: concepts/PSRule/en-US/about_PSRule_Conventions.md
[about_PSRule_Selectors]: concepts/PSRule/en-US/about_PSRule_Selectors.md
[about_PSRule_Rules]: concepts/PSRule/en-US/about_PSRule_Rules.md
[about_PSRule_Badges]: concepts/PSRule/en-US/about_PSRule_Badges.md
[about_PSRule_Expressions]: concepts/PSRule/en-US/about_PSRule_Expressions.md
[about_PSRule_SuppressionGroups]: concepts/PSRule/en-US/about_PSRule_SuppressionGroups.md
