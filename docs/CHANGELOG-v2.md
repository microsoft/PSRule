# Change log

See [upgrade notes][1] for helpful information when upgrading from previous versions.

  [1]: https://microsoft.github.io/PSRule/latest/upgrade-notes/

**Important notes**:

- Several properties of rule and language block elements will be removed from v3.
  See [deprecations][2] for details.

  [2]: https://microsoft.github.io/PSRule/latest/deprecations/#deprecations-for-v3

## Unreleased

## v2.2.0-B0175 (pre-release)

What's changed since pre-release v2.2.0-B0131:

- Bug fixes:
  - Fixed issue building a PSRule project by removing PSRule.psd1 from compile target.
    [#1140](https://github.com/microsoft/PSRule/issues/1140)

## v2.2.0-B0131 (pre-release)

What's changed since pre-release v2.2.0-B0089:

- General improvements:
  - Added descendant selector to object path syntax.
    [#1133](https://github.com/microsoft/PSRule/issues/1133)
    - Use `..` to traverse into child objects, for example `$..name` finds names for all nested objects.
- Engineering:
  - Bump Newtonsoft.Json to 13.0.1.
    [#1137](https://github.com/microsoft/PSRule/pull/1137)

## v2.2.0-B0089 (pre-release)

What's changed since pre-release v2.2.0-B0052:

- General improvements:
  - Improved reporting of the object path that caused rule failures by @BernieWhite.
    [#1092](https://github.com/microsoft/PSRule/issues/1092)
    - Output include a new `Detail` property with details of the reason and the object path.
    - Custom methods `ReasonFrom` and `ReasonIf` accept a `path` parameter to specify the object path.

## v2.2.0-B0052 (pre-release)

What's changed since pre-release v2.2.0-B0021:

- General improvements:
  - Added informational message when output has been written to disk by @BernieWhite.
    [#1074](https://github.com/microsoft/PSRule/issues/1074)
    - The `Output.Footer` option now supports `OutputFile` which reports the output file path.
      This is enabled by default.
- Engineering:
  - Added more object path tests by @ArmaanMcleod.
    [#1110](https://github.com/microsoft/PSRule/issues/1110)
- Bug fixes:
  - Fixed output of reason with wide format by @BernieWhite.
    [#1117](https://github.com/microsoft/PSRule/issues/1117)
  - Fixed piped input does not respect excluded paths by @BernieWhite.
    [#1114](https://github.com/microsoft/PSRule/issues/1114)
    - By default, objects are not excluded by source.
    - To exclude piped input based on source configure the `Input.IgnoreObjectSource` option.

## v2.2.0-B0021 (pre-release)

What's changed since v2.1.0:

- New features:
  - Added `notCount` expression and assertion helper by @ArmaanMcleod.
    [#1091](https://github.com/microsoft/PSRule/issues/1091)
- Engineering:
  - Bump xunit.runner.visualstudio to 2.4.5.
    [#1084](https://github.com/microsoft/PSRule/pull/1084)
  - Bump Pester to 5.3.3.
    [#1079](https://github.com/microsoft/PSRule/pull/1079)
  - Bump Microsoft.NET.Test.Sdk to 17.2.0.
    [#1089](https://github.com/microsoft/PSRule/pull/1089)
  - Added NuGet packaging publishing by @BernieWhite.
    [#1093](https://github.com/microsoft/PSRule/issues/1093)
  - Updated NuGet packaging metadata by @BernieWhite.
    [#1093](https://github.com/microsoft/PSRule/issues/1093)
- Bug fixes:
  - Fixed grouping of logical operators in object path by @BernieWhite.
    [#1101](https://github.com/microsoft/PSRule/issues/1101)

## v2.1.0

What's changed since v2.0.1:

- General improvements:
  - Added `notStartsWith`, `notEndsWith`, and `notContains` expressions and assertion helpers. [#1047](https://github.com/microsoft/PSRule/issues/1047)
  - Added `like`, `notLike` expressions and assertion helpers. [#1048](https://github.com/microsoft/PSRule/issues/1048)
  - Added additional repository paths to ignore by default. [#1043](https://github.com/microsoft/PSRule/issues/1043)
  - Added custom suppression message during PSRule runs. [#1046](https://github.com/microsoft/PSRule/issues/1046)
    - When a rule is suppressed using a suppression group the synopsis is shown in the suppression warning.
    - Configure the suppression group synopsis to display a custom message.
    - Suppression groups synopsis can be localized using markdown documentation.
    - Use markdown to set a culture specific synopsis.
    - Custom suppression messages are not supported when suppressing individual rules using `ps-rule.yaml`.
    - See [about_PSRule_SuppressionGroups] for details.
  - Added source support for string conditions. [#1068](https://github.com/microsoft/PSRule/issues/1068)
- Engineering:
  - Added code signing of module. [#1049](https://github.com/microsoft/PSRule/issues/1049)
  - Added SBOM manifests to module. [#1050](https://github.com/microsoft/PSRule/issues/1050)
  - Bump Sarif.Sdk to 2.4.15. [#1075](https://github.com/microsoft/PSRule/pull/1075)
  - Bump Pester to 5.3.2. [#1062](https://github.com/microsoft/PSRule/pull/1062)
- Bug fixes:
  - **Important change:** Fixed source scope not updated in multi-module runs. [#1053](https://github.com/microsoft/PSRule/issues/1053)
    - Several properties of rule and language block elements have been renamed to improve consistency.
    - From _v3_ custom scripts may not work correctly until you update these names.
    - For details on the updated property names see [deprecations][2].

What's changed since pre-release v2.1.0-B0069:

- No additional changes.

## v2.1.0-B0069 (pre-release)

What's changed since pre-release v2.1.0-B0040:

- General improvements:
  - Added `notStartsWith`, `notEndsWith`, and `notContains` expressions and assertion helpers. [#1047](https://github.com/microsoft/PSRule/issues/1047)
  - Added `like`, `notLike` expressions and assertion helpers. [#1048](https://github.com/microsoft/PSRule/issues/1048)
  - Added additional repository paths to ignore by default. [#1043](https://github.com/microsoft/PSRule/issues/1043)
- Engineering:
  - Bump Sarif.Sdk to 2.4.15. [#1075](https://github.com/microsoft/PSRule/pull/1075)

## v2.1.0-B0040 (pre-release)

What's changed since pre-release v2.1.0-B0015:

- General improvements:
  - Added custom suppression message during PSRule runs. [#1046](https://github.com/microsoft/PSRule/issues/1046)
    - When a rule is suppressed using a suppression group the synopsis is shown in the suppression warning.
    - Configure the suppression group synopsis to display a custom message.
    - Suppression groups synopsis can be localized using markdown documentation.
    - Use markdown to set a culture specific synopsis.
    - Custom suppression messages are not supported when suppressing individual rules using `ps-rule.yaml`.
    - See [about_PSRule_SuppressionGroups] for details.
  - Added source support for string conditions. [#1068](https://github.com/microsoft/PSRule/issues/1068)
- Engineering:
  - Bump Sarif.Sdk to 2.4.14. [#1064](https://github.com/microsoft/PSRule/pull/1064)
  - Bump Pester to 5.3.2. [#1062](https://github.com/microsoft/PSRule/pull/1062)
- Bug fixes:
  - **Important change:** Fixed source scope not updated in multi-module runs. [#1053](https://github.com/microsoft/PSRule/issues/1053)
    - Several properties of rule and language block elements have been renamed to improve consistency.
    - From _v3_ custom scripts may not work correctly until you update these names.
    - For details on the updated property names see [deprecations][2].

## v2.1.0-B0015 (pre-release)

What's changed since v2.0.1:

- Engineering:
  - Added code signing of module. [#1049](https://github.com/microsoft/PSRule/issues/1049)
  - Added SBOM manifests to module. [#1050](https://github.com/microsoft/PSRule/issues/1050)

## v2.0.1

What's changed since v2.0.0:

- Bug fixes:
  - Fixed read JSON failed with comments. [#1051](https://github.com/microsoft/PSRule/issues/1051)
  - Fixed null reference on elapsed time when required module check fails. [#1054](https://github.com/microsoft/PSRule/issues/1054)
  - Fixed failed to read JSON objects with a empty property name. [#1052](https://github.com/microsoft/PSRule/issues/1052)

## v2.0.0

What's changed since v1.11.1:

- New features:
  - Add support for suppression groups. [#793](https://github.com/microsoft/PSRule/issues/793)
    - New `SuppressionGroup` resource has been included.
    - See [about_PSRule_SuppressionGroups] for details.
  - Added source expression property. [#933](https://github.com/microsoft/PSRule/issues/933)
    - Included the following expressions:
      - `source`
      - `withinPath`
      - `notWithinPath`
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
- General improvements:
  - Add option to disable invariant culture warning. [#899](https://github.com/microsoft/PSRule/issues/899)
    - Added `Execution.InvariantCultureWarning` option.
    - See [about_PSRule_Options] for details.
  - Added support for object path expressions. [#808](https://github.com/microsoft/PSRule/issues/808) [#693](https://github.com/microsoft/PSRule/issues/693)
    - Inspired by JSONPath, object path expressions can be used to access nested objects.
    - Array members can be filtered and enumerated using object path expressions.
    - Object path expressions can be used in YAML, JSON, and PowerShell rules and selectors.
    - See [about_PSRule_Assert] for details.
  - Improve tracking of suppressed objects. [#794](https://github.com/microsoft/PSRule/issues/794)
    - Added `Execution.SuppressedRuleWarning` option to output warning for suppressed rules.
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
  - Added new properties for module lookup to SARIF results. [#951](https://github.com/microsoft/PSRule/issues/951)
  - Capture and output repository info in Assert-PSRule runs. [#978](https://github.com/microsoft/PSRule/issues/978)
    - Added `Repository.Url` option set repository URL reported in output.
    - Repository URL is detected automatically for GitHub Actions and Azure Pipelines.
    - Added `RepositoryInfo` to `Output.Banner` option.
    - Repository info is shown by default.
  - Added `convert` and `caseSensitive` to string comparison expressions. [#1001](https://github.com/microsoft/PSRule/issues/1001)
    - The following expressions support type conversion and case-sensitive comparison.
      - `startsWith`, `contains`, and `endsWith`.
      - `equals` and `notEquals`.
  - Added `convert` to numeric comparison expressions. [#943](https://github.com/microsoft/PSRule/issues/943)
    - Type conversion is now supported for `less`, `lessOrEquals`, `greater`, and `greaterOrEquals`.
  - Added `Extent` property on rules reported by `Get-PSRule`. [#990](https://github.com/microsoft/PSRule/issues/990)
    - Extent provides the line and position of the rule in the source code.
  - **Breaking change:** Added validation of resource names. [#1012](https://github.com/microsoft/PSRule/issues/1012)
    - Invalid rules names will now produce a specific error.
    - See [upgrade notes][1] for more information.
- Engineering:
  - **Breaking change:** Removal of deprecated default baseline from module manifest. [#755](https://github.com/microsoft/PSRule/issues/755)
    - Set the default module baseline using module configuration.
    - See [upgrade notes][1] for details.
  - **Breaking change:** Require `apiVersion` on YAML and JSON to be specified. [#648](https://github.com/microsoft/PSRule/issues/648)
    - Resources should use `github.com/microsoft/PSRule/v1` as the `apiVersion`.
    - Resources that do not specify an `apiVersion` will be ignored.
    - See [upgrade notes][1] for details.
  - **Breaking change:** Prefer module sources over loose files. [#610](https://github.com/microsoft/PSRule/issues/610)
    - Module sources are discovered before loose files.
    - Warning is shown for duplicate rule names, and exception is thrown for duplicate rule Ids.
    - See [upgrade notes][1] for details.
  - **Breaking change:** Require rule sources from current working directory to be explicitly included. [#760](https://github.com/microsoft/PSRule/issues/760)
    - From v2 onwards, `$PWD` is not included by default unless `-Path .` or `-Path $PWD` is explicitly specified.
    - See [upgrade notes][1] for details.
  - Added more tests for JSON resources. [#929](https://github.com/microsoft/PSRule/issues/929)
  - Bump Sarif.Sdk to 2.4.13. [#1007](https://github.com/microsoft/PSRule/pull/1007)
  - Bump PowerShellStandard.Library to 5.1.1. [#999](https://github.com/microsoft/PSRule/pull/999)
- Bug fixes:
  - Fixed object path handling with dash. [#902](https://github.com/microsoft/PSRule/issues/902)
  - Fixed empty suppression group rules property applies to no rules. [#931](https://github.com/microsoft/PSRule/issues/931)
  - Fixed object reference for suppression group will rule not defined. [#932](https://github.com/microsoft/PSRule/issues/932)
  - Fixed rule source loading twice from `$PWD` and `.ps-rule/`. [#939](https://github.com/microsoft/PSRule/issues/939)
  - Fixed rule references in SARIF format for extensions need a toolComponent reference. [#949](https://github.com/microsoft/PSRule/issues/949)
  - Fixed file objects processed with file input format have no source location. [#950](https://github.com/microsoft/PSRule/issues/950)
  - Fixed GitHub code scanning alerts treats pass as problems. [#955](https://github.com/microsoft/PSRule/issues/955)
    - By default, SARIF output will only include fail or error outcomes.
    - Added `Output.SarifProblemsOnly` option to include pass outcomes.
  - Fixed SARIF output includes rule property for default tool component. [#956](https://github.com/microsoft/PSRule/issues/956)
  - Fixed Invoke-PSRule hanging if JSON rule file is empty. [#969](https://github.com/microsoft/PSRule/issues/969)
  - Fixed SARIF should report base branch. [#964](https://github.com/microsoft/PSRule/issues/964)
  - Fixed unclear error message on invalid rule names. [#1012](https://github.com/microsoft/PSRule/issues/1012)

What's changed since pre-release v2.0.0-B2203045:

- No additional changes.

## v2.0.0-B2203045 (pre-release)

What's changed since pre-release v2.0.0-B2203033:

- General improvements:
  - Added `convert` to numeric comparison expressions. [#943](https://github.com/microsoft/PSRule/issues/943)
    - Type conversion is now supported for `less`, `lessOrEquals`, `greater`, and `greaterOrEquals`.
  - **Breaking change:** Added validation of resource names. [#1012](https://github.com/microsoft/PSRule/issues/1012)
    - Invalid rules names will now produce a specific error.
    - See [upgrade notes][1] for more information.
- Bug fixes:
  - Fixed unclear error message on invalid rule names. [#1012](https://github.com/microsoft/PSRule/issues/1012)

## v2.0.0-B2203033 (pre-release)

What's changed since pre-release v2.0.0-B2203019:

- General improvements:
  - Added `Extent` property on rules reported by `Get-PSRule`. [#990](https://github.com/microsoft/PSRule/issues/990)
    - Extent provides the line and position of the rule in the source code.
- Engineering:
  - Bump Sarif.Sdk to 2.4.13. [#1007](https://github.com/microsoft/PSRule/pull/1007)
  - Bump PowerShellStandard.Library to 5.1.1. [#999](https://github.com/microsoft/PSRule/pull/999)

## v2.0.0-B2203019 (pre-release)

What's changed since pre-release v2.0.0-B2202072:

- General improvements:
  - Added `convert` and `caseSensitive` to string comparison expressions. [#1001](https://github.com/microsoft/PSRule/issues/1001)
    - The following expressions support type conversion and case-sensitive comparison.
      - `startsWith`, `contains`, and `endsWith`.
      - `equals` and `notEquals`.

## v2.0.0-B2202072 (pre-release)

What's changed since pre-release v2.0.0-B2202065:

- General improvements:
  - Capture and output repository info in Assert-PSRule runs. [#978](https://github.com/microsoft/PSRule/issues/978)
    - Added `Repository.Url` option set repository URL reported in output.
    - Repository URL is detected automatically for GitHub Actions and Azure Pipelines.
    - Added `RepositoryInfo` to `Output.Banner` option.
    - Repository info is shown by default.
- Bug fixes:
  - Fixed SARIF should report base branch. [#964](https://github.com/microsoft/PSRule/issues/964)

## v2.0.0-B2202065 (pre-release)

What's changed since pre-release v2.0.0-B2202056:

- Bug fixes:
  - Fixed broken documentation links. [#980](https://github.com/microsoft/PSRule/issues/980)

## v2.0.0-B2202056 (pre-release)

What's changed since pre-release v2.0.0-B2202024:

- Bug fixes:
  - Fixed Invoke-PSRule hanging if JSON rule file is empty. [#969](https://github.com/microsoft/PSRule/issues/969)

## v2.0.0-B2202024 (pre-release)

What's changed since pre-release v2.0.0-B2202017:

- New features:
  - Added source expression property. [#933](https://github.com/microsoft/PSRule/issues/933)
    - Included the following expressions:
      - `source`
      - `withinPath`
      - `notWithinPath`

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
