# Change log

See [upgrade notes][upgrade-notes] for helpful information when upgrading from previous versions.

[upgrade-notes]: upgrade-notes.md

**Important notes**:

- YAML resources will require an `apiVersion` from PSRule v2. [#648](https://github.com/microsoft/PSRule/issues/648)
- Setting the default module baseline requires a module configuration from PSRule v2. [#809](https://github.com/microsoft/PSRule/issues/809)

## Unreleased

What's changed since v1.11.0:

- General improvements:
  - Added support for object path expressions. [#808](https://github.com/microsoft/PSRule/issues/808) [#693](https://github.com/microsoft/PSRule/issues/693)
    - Inspired by JSONPath, object path expressions can be used to access nested objects.
    - Array members can be filtered and enumerated using object path expressions.
    - Object path expressions can be used in YAML, JSON, and PowerShell rules and selectors.
    - See [about_PSRule_Assert] for details.
  - Improve tracking of suppressed objects. [#794](https://github.com/microsoft/PSRule/issues/794)
    - Added `Execution.SuppressedRuleWarning` option to output warning for suppressed rules.

## v1.11.0

What's changed since v1.10.0:

- General improvements:
  - Added `version` expression to check semantic version constraints. [#861](https://github.com/microsoft/PSRule/issues/861)
    - See [about_PSRule_Expressions] for details.
  - Added `hasDefault` expression to check field default value. [#870](https://github.com/microsoft/PSRule/issues/870)
    - See [about_PSRule_Expressions] for details.
- Bug fixes:
  - Fixed `GetReason()` not returning results for a failed assertion. [#874](https://github.com/microsoft/PSRule/issues/874)

What's changed since pre-release v1.11.0-B2112016:

- No additional changes.

## v1.11.0-B2112016 (pre-release)

What's changed since v1.10.0:

- General improvements:
  - Added `version` expression to check semantic version constraints. [#861](https://github.com/microsoft/PSRule/issues/861)
    - See [about_PSRule_Expressions] for details.
  - Added `hasDefault` expression to check field default value. [#870](https://github.com/microsoft/PSRule/issues/870)
    - See [about_PSRule_Expressions] for details.
- Bug fixes:
  - Fixed `GetReason()` not returning results for a failed assertion. [#874](https://github.com/microsoft/PSRule/issues/874)

## v1.10.0

What's changed since v1.9.0:

- General improvements:
  - Added JSON support for reading rules and selectors from pipeline. [#857](https://github.com/microsoft/PSRule/issues/857)
  - Added `HasSchema` expression to check the schema of an object. [#860](https://github.com/microsoft/PSRule/issues/860)
    - See [about_PSRule_Expressions] for details.
- Engineering:
  - Bump Microsoft.SourceLink.GitHub to 1.1.1. [#856](https://github.com/microsoft/PSRule/pull/856)
- Bug fixes:
  - Fixed `$Assert.HasJsonSchema` accepts empty value. [#859](https://github.com/microsoft/PSRule/issues/859)
  - Fixed module configuration is not loaded when case does not match. [#864](https://github.com/microsoft/PSRule/issues/864)

What's changed since pre-release v1.10.0-B2112002:

- No additional changes.

## v1.10.0-B2112002 (pre-release)

What's changed since pre-release v1.10.0-B2111024:

- Bug fixes:
  - Fixed module configuration is not loaded when case does not match. [#864](https://github.com/microsoft/PSRule/issues/864)

## v1.10.0-B2111024 (pre-release)

What's changed since v1.9.0:

- General improvements:
  - Added JSON support for reading rules and selectors from pipeline. [#857](https://github.com/microsoft/PSRule/issues/857)
  - Added `HasSchema` expression to check the schema of an object. [#860](https://github.com/microsoft/PSRule/issues/860)
    - See [about_PSRule_Expressions] for details.
- Engineering:
  - Bump Microsoft.SourceLink.GitHub to 1.1.1. [#856](https://github.com/microsoft/PSRule/pull/856)
- Bug fixes:
  - Fixed `$Assert.HasJsonSchema` accepts empty value. [#859](https://github.com/microsoft/PSRule/issues/859)

## v1.9.0

What's changed since v1.8.0:

- General improvements:
  - Added improvements to YAML output for `Get-PSRuleBaseline`. [#829](https://github.com/microsoft/PSRule/issues/829)
  - Added `-Initialize` convention block. [#826](https://github.com/microsoft/PSRule/issues/826)
    - Use this block to perform any initialization that is required before any rules are run.
    - This block is only run once instead of `-Begin` which is run once per object.
    - See [about_PSRule_Conventions] for details.
  - Allow lifetime services to be used. [#827](https://github.com/microsoft/PSRule/issues/827)
    - Use `$PSRule.AddService` and `$PSRule.GetService` to add a service.
    - Services allows a singleton instance to be used and shared across multiple rules.
    - PSRule will automatically dispose the service when all rules have run.
    - See [about_PSRule_Variables] for details.
  - Added `Export-PSRuleBaseline` cmdlet to export baseline. [#622](https://github.com/microsoft/PSRule/issues/622)
  - Added JSON output format for Baseline cmdlets. [#839](https://github.com/microsoft/PSRule/issues/839)
  - Allow downstream issues to be consumed. [#843](https://github.com/microsoft/PSRule/issues/843)
    - Objects can be flagged with issues that have been generated externally.
    - See [about_PSRule_Assert] for details.
  - Migrated default baseline to module configuration. [#809](https://github.com/microsoft/PSRule/issues/809)
    - This enables configuration of the default baseline for a module with a module configuration.
    - This depreciate configuring the default baseline within the module manifest.
    - Modules using manifest configuration will start warning from v1.9.0.
    - See [about_PSRule_Options] for details.
  - Added JSON support to read baselines from pipeline. [#845](https://github.com/microsoft/PSRule/issues/845)
- Engineering:
  - Bump System.Drawing.Common dependency to v6.0.0. [#848](https://github.com/microsoft/PSRule/pull/848)
- Bug fixes:
  - Fixed convention execution is out of order. [#835](https://github.com/microsoft/PSRule/issues/835)

What's changed since pre-release v1.9.0-B2111024:

- Engineering:
  - Bump Microsoft.CodeAnalysis.NetAnalyzers to v6.0.0. [#851](https://github.com/microsoft/PSRule/pull/851)

## v1.9.0-B2111024 (pre-release)

What's changed since pre-release v1.9.0-B2111009:

- General improvements:
  - Allow downstream issues to be consumed. [#843](https://github.com/microsoft/PSRule/issues/843)
    - Objects can be flagged with issues that have been generated externally.
    - See [about_PSRule_Assert] for details.
  - Migrated default baseline to module configuration. [#809](https://github.com/microsoft/PSRule/issues/809)
    - This enables configuration of the default baseline for a module with a module configuration.
    - This depreciate configuring the default baseline within the module manifest.
    - Modules using manifest configuration will start warning from v1.9.0.
    - See [about_PSRule_Options] for details.
  - Added JSON support to read baselines from pipeline. [#845](https://github.com/microsoft/PSRule/issues/845)
- Engineering:
  - Bump System.Drawing.Common dependency to v6.0.0. [#848](https://github.com/microsoft/PSRule/pull/848)

## v1.9.0-B2111009 (pre-release)

What's changed since pre-release v1.9.0-B2110027:

- General improvements:
  - Added JSON output format for Baseline cmdlets. [#839](https://github.com/microsoft/PSRule/issues/839)
- Bug fixes:
  - Fixed convention execution is out of order. [#835](https://github.com/microsoft/PSRule/issues/835)

## v1.9.0-B2110027 (pre-release)

What's changed since pre-release v1.9.0-B2110015:

- General improvements:
  - Added `Export-PSRuleBaseline` cmdlet to export baseline. [#622](https://github.com/microsoft/PSRule/issues/622)

## v1.9.0-B2110015 (pre-release)

What's changed since v1.8.0:

- General improvements:
  - Added improvements to YAML output for `Get-PSRuleBaseline`. [#829](https://github.com/microsoft/PSRule/issues/829)
  - Added `-Initialize` convention block. [#826](https://github.com/microsoft/PSRule/issues/826)
    - Use this block to perform any initialization that is required before any rules are run.
    - This block is only run once instead of `-Begin` which is run once per object.
    - See [about_PSRule_Conventions] for details.
  - Allow lifetime services to be used. [#827](https://github.com/microsoft/PSRule/issues/827)
    - Use `$PSRule.AddService` and `$PSRule.GetService` to add a service.
    - Services allows a singleton instance to be used and shared across multiple rules.
    - PSRule will automatically dispose the service when all rules have run.
    - See [about_PSRule_Variables] for details.

## v1.8.0

What's changed since v1.7.2:

- General improvements:
  - Added YAML output format support for `Get-PSRuleBaseline`. [#326](https://github.com/microsoft/PSRule/issues/326)
  - Added YAML/JSON output format support for `Get-PSRule`. [#128](https://github.com/microsoft/PSRule/issues/128)
  - Added `Output.JsonIndent` option for JSON output format. [#817](https://github.com/microsoft/PSRule/issues/817)
  - Added assertion helpers and expressions for improving intersection checks. [#795](https://github.com/microsoft/PSRule/issues/795)
    - Added `Count` to determine of the field has a specific number of elements.
    - Added `SetOf` to determine if a collection is another collection.
    - Added `Subset` to determine if a collection is includes another collection.
    - See [about_PSRule_Assert] and [about_PSRule_Expressions] for details.
  - Added support for conditional reason messages with `ReasonIf`. [#804](https://github.com/microsoft/PSRule/issues/804)
    - See [about_PSRule_Assert] for details.
  - Added support for `type` and `name` expression properties. [#810](https://github.com/microsoft/PSRule/issues/810)
    - Use `type` to compare the bound type of the current object.
    - Use `name` to compare the bound name of the current object.
    - See [about_PSRule_Expressions] for details.
- Engineering:
  - Migration of Pester v4 tests to Pester v5. [#478](https://github.com/microsoft/PSRule/issues/478)

What's changed since pre-release v1.8.0-B2110030:

- No additional changes.

## v1.8.0-B2110030 (pre-release)

What's changed since pre-release v1.8.0-B2110020:

- General improvements:
  - Added `Output.JsonIndent` option for JSON output format. [#817](https://github.com/microsoft/PSRule/issues/817)

## v1.8.0-B2110020 (pre-release)

What's changed since pre-release v1.8.0-B2110006:

- General improvements:
  - Added YAML/JSON output format support for `Get-PSRule`. [#128](https://github.com/microsoft/PSRule/issues/128)
- Engineering:
  - Migration of Pester v4 tests to Pester v5. [#478](https://github.com/microsoft/PSRule/issues/478)

## v1.8.0-B2110006 (pre-release)

What's changed since pre-release v1.8.0-B2109022:

- General improvements:
  - Added YAML output format support for `Get-PSRuleBaseline`. [#326](https://github.com/microsoft/PSRule/issues/326)

## v1.8.0-B2109022 (pre-release)

What's changed since pre-release v1.8.0-B2109015:

- General improvements:
  - Added support for conditional reason messages with `ReasonIf`. [#804](https://github.com/microsoft/PSRule/issues/804)
    - See [about_PSRule_Assert] for details.
  - Added support for `type` and `name` expression properties. [#810](https://github.com/microsoft/PSRule/issues/810)
    - Use `type` to compare the bound type of the current object.
    - Use `name` to compare the bound name of the current object.
    - See [about_PSRule_Expressions] for details.

## v1.8.0-B2109015 (pre-release)

What's changed since v1.7.2:

- General improvements:
  - Added assertion helpers and expressions for improving intersection checks. [#795](https://github.com/microsoft/PSRule/issues/795)
    - Added `Count` to determine of the field has a specific number of elements.
    - Added `SetOf` to determine if a collection is another collection.
    - Added `Subset` to determine if a collection is includes another collection.
    - See [about_PSRule_Assert] and [about_PSRule_Expressions] for details.

## v1.7.2

What's changed since v1.7.1:

- Bug fixes:
  - Fixed `Get-PSRuleBaseline` does not return any results from module. [#801](https://github.com/microsoft/PSRule/issues/801)

## v1.7.1

What's changed since v1.7.0:

- Bug fixes:
  - Fixed ResourceTags does not contain a method named ToHashtable. [#798](https://github.com/microsoft/PSRule/issues/798)

## v1.7.0

What's changed since v1.6.0:

- Engine features:
  - Added support for generating badges from rule results. [#623](https://github.com/microsoft/PSRule/issues/623)
    - Standard or custom badges can be generated using a convention and the badge API.
    - See [about_PSRule_Badges] for details.
- General improvements:
  - Rule results now include a run ID or each run. [#774](https://github.com/microsoft/PSRule/issues/774)
    - Run ID is returned in `Assert-PSRule` output at the end of each run by default.
    - By default a unique `runId` is generated when the rule is run.
    - The `Output.Footer`option was added to configure the output footer.
    - See [about_PSRule_Options] for details.
  - Automatically exclude common repository files from input files. [#721](https://github.com/microsoft/PSRule/issues/721)
    - Added `Input.IgnoreRepositoryCommon` option to change default behavior.
    - See [about_PSRule_Options] for details.
  - Added aggregation assertion methods for `AnyOf` and `AllOf`. [#776](https://github.com/microsoft/PSRule/issues/776)
    - See [about_PSRule_Assert] for details.
  - Allow baselines to include local rules. [#756](https://github.com/microsoft/PSRule/issues/756)
    - The `Rule.IncludeLocal`option was automatically include local/ standalone rules not in a module.
    - This option is useful when you want to include local rules not included in a baseline.
    - See [about_PSRule_Options] for details.
- Bug fixes:
  - Fixed configuration array deserializes as dictionary from YAML options. [#779](https://github.com/microsoft/PSRule/issues/779)

What's changed since pre-release v1.7.0-B2109002:

- No additional changes.

## v1.7.0-B2109002 (pre-release)

What's changed since pre-release v1.7.0-B2108032:

- General improvements:
  - Allow baselines to include local rules. [#756](https://github.com/microsoft/PSRule/issues/756)
    - The `Rule.IncludeLocal`option was automatically include local/ standalone rules not in a module.
    - This option is useful when you want to include local rules not included in a baseline.
    - See [about_PSRule_Options] for details.

## v1.7.0-B2108032 (pre-release)

What's changed since pre-release v1.7.0-B2108021:

- Engine features:
  - Added support for generating badges from rule results. [#623](https://github.com/microsoft/PSRule/issues/623)
    - Standard or custom badges can be generated using a convention and the badge API.
    - See [about_PSRule_Badges] for details.
- General improvements:
  - Rule results now include a run ID or each run. [#774](https://github.com/microsoft/PSRule/issues/774)
    - Run ID is returned in `Assert-PSRule` output at the end of each run by default.
    - By default a unique `runId` is generated when the rule is run.
    - The `Output.Footer`option was added to configure the output footer.
    - See [about_PSRule_Options] for details.

## v1.7.0-B2108021 (pre-release)

What's changed since pre-release v1.7.0-B2108016:

- Bug fixes:
  - Fixed configuration array deserializes as dictionary from YAML options. [#779](https://github.com/microsoft/PSRule/issues/779)

## v1.7.0-B2108016 (pre-release)

What's changed since v1.6.0:

- General improvements:
  - Automatically exclude common repository files from input files. [#721](https://github.com/microsoft/PSRule/issues/721)
    - Added `Input.IgnoreRepositoryCommon` option to change default behavior.
    - See [about_PSRule_Options] for details.
  - Added aggregation assertion methods for `AnyOf` and `AllOf`. [#776](https://github.com/microsoft/PSRule/issues/776)
    - See [about_PSRule_Assert] for details.

## v1.6.1

What's changed since v1.6.0:

- Bug fixes:
  - Fixed configuration array deserializes as dictionary from YAML options. [#779](https://github.com/microsoft/PSRule/issues/779)

## v1.6.0

What's changed since v1.5.0:

- Engine features:
  - Added support for YAML rules. [#603](https://github.com/microsoft/PSRule/issues/603)
    - YAML rules evaluate an expression tree and return a result for each object.
    - YAML provides an additional option for defining rules in addition to PowerShell script rules.
    - Type and selector pre-conditions are supported.
    - See [about_PSRule_Rules] for details.
- General improvements:
  - Added support for object source location in validation. [#757](https://github.com/microsoft/PSRule/issues/757)
  - Default rule source location `.ps-rule/` is automatically included. [#742](https://github.com/microsoft/PSRule/issues/742)
    - Added `Include.Path` and `Include.Module` options to automatically include rule sources.
    - See [about_PSRule_Options] for details.
- Bug fixes:
  - Fixed target binding across multiple scopes. [#762](https://github.com/microsoft/PSRule/issues/762)

What's changed since pre-release v1.6.0-B2108009:

- No additional changes.

## v1.6.0-B2108009 (pre-release)

What's changed since pre-release v1.6.0-B2108003:

- Engine features:
  - Added support for YAML rules. [#603](https://github.com/microsoft/PSRule/issues/603)
    - YAML rules evaluate an expression tree and return a result for each object.
    - YAML provides an additional option for defining rules in addition to PowerShell script rules.
    - Type and selector pre-conditions are supported.
    - See [about_PSRule_Rules] for details.

## v1.6.0-B2108003 (pre-release)

What's changed since pre-release v1.6.0-B2107008:

- Bug fixes:
  - Fixed target binding across multiple scopes. [#762](https://github.com/microsoft/PSRule/issues/762)

## v1.6.0-B2107008 (pre-release)

What's changed since v1.5.0:

- General improvements:
  - Added support for object source location in validation. [#757](https://github.com/microsoft/PSRule/issues/757)
  - Default rule source location `.ps-rule/` is automatically included. [#742](https://github.com/microsoft/PSRule/issues/742)
    - Added `Include.Path` and `Include.Module` options to automatically include rule sources.
    - See [about_PSRule_Options] for details.

## v1.5.0

What's changed since v1.4.0:

- General improvements:
  - Added string selector conditions. [#747](https://github.com/microsoft/PSRule/issues/747)
    - Use `startWith`, `contains`, and `endsWith` to check for a sub-string.
    - Use `isString`, `isLower`, and `isUpper` to check for string type and casing.
    - See [about_PSRule_Selectors] for details.
- Engineering:
  - Bump YamlDotNet dependency to 11.2.1. [#740](https://github.com/microsoft/PSRule/pull/740)
- Bug fixes:
  - Fixed options schema should allow spacing after `@pre`. [#743](https://github.com/microsoft/PSRule/issues/743)
  - Fixed match selector expression passing on missing field. [#745](https://github.com/microsoft/PSRule/issues/745)

What's changed since pre-release v1.5.0-B2107009:

- No additional changes.

## v1.5.0-B2107009 (pre-release)

What's changed since pre-release v1.5.0-B2106006:

- General improvements:
  - Added string selector conditions. [#747](https://github.com/microsoft/PSRule/issues/747)
    - Use `startWith`, `contains`, and `endsWith` to check for a sub-string.
    - Use `isString`, `isLower`, and `isUpper` to check for string type and casing.
    - See [about_PSRule_Selectors] for details.
- Engineering:
  - Bump YamlDotNet dependency to 11.2.1. [#740](https://github.com/microsoft/PSRule/pull/740)
- Bug fixes:
  - Fixed options schema should allow spacing after `@pre`. [#743](https://github.com/microsoft/PSRule/issues/743)
  - Fixed match selector expression passing on missing field. [#745](https://github.com/microsoft/PSRule/issues/745)

## v1.5.0-B2106006 (pre-release)

What's changed since v1.4.0:

- Engineering:
  - Bump YamlDotNet dependency to 11.2.0. [#736](https://github.com/microsoft/PSRule/pull/736)

## v1.4.0

What's changed since v1.3.0:

- General improvements:
  - PSRule banner can be configured in output when using `Assert-PSRule`. [#708](https://github.com/microsoft/PSRule/issues/708)
  - Input source location of objects are included in results.
    - Input source location of objects from JSON and YAML input files are read automatically. [#624](https://github.com/microsoft/PSRule/issues/624)
    - Input source location of objects from the pipeline are read from properties. [#729](https://github.com/microsoft/PSRule/issues/729)
  - Assert output improvements:
    - Added support for Visual Studio Code with `VisualStudioCode` style. [#731](https://github.com/microsoft/PSRule/issues/731)
      - Updated output format provides support for problem matchers in task output.
    - Automatically detect output style from environment variables. [#732](https://github.com/microsoft/PSRule/issues/732)
      - _Assert-PSRule_ now defaults to `Detect` instead of `Client`.
    - See [about_PSRule_Options] for details.
  - Improved support for version constraints by:
    - Constraints can include prerelease versions of other matching versions. [#714](https://github.com/microsoft/PSRule/issues/714)
    - Constraints support using a `@prerelease` or `@pre` to include prerelease versions. [#717](https://github.com/microsoft/PSRule/issues/717)
    - Constraint sets allow multiple constraints to be joined together. [#715](https://github.com/microsoft/PSRule/issues/715)
    - See [about_PSRule_Assert] for details.
- Bug fixes:
  - Fixed prerelease constraint handling for prerelease versions. [#712](https://github.com/microsoft/PSRule/issues/712)
  - Fixed null reference in convention for nested exceptions. [#725](https://github.com/microsoft/PSRule/issues/725)

What's changed since pre-release v1.4.0-B2105041:

- No additional changes.

## v1.4.0-B2105041 (pre-release)

What's changed since pre-release v1.4.0-B2105032:

- General improvements:
  - Source location of objects from the pipeline are read from properties. [#729](https://github.com/microsoft/PSRule/issues/729)
  - Assert output improvements:
    - Added support for Visual Studio Code with `VisualStudioCode` style. [#731](https://github.com/microsoft/PSRule/issues/731)
      - Updated output format provides support for problem matchers in task output.
    - Automatically detect output style from environment variables. [#732](https://github.com/microsoft/PSRule/issues/732)
      - _Assert-PSRule_ now defaults to `Detect` instead of `Client`.
    - See [about_PSRule_Options] for details.

## v1.4.0-B2105032 (pre-release)

What's changed since pre-release v1.4.0-B2105019:

- Bug fixes:
  - Fixed null reference in convention for nested exceptions. [#725](https://github.com/microsoft/PSRule/issues/725)

## v1.4.0-B2105019 (pre-release)

What's changed since pre-release v1.4.0-B2105004:

- General improvements:
  - Source location of objects are included in results.
    - Source location of objects from JSON and YAML input files are read automatically. [#624](https://github.com/microsoft/PSRule/issues/624)
  - Improved support for version constraints by:
    - Constraints can include prerelease versions of other matching versions. [#714](https://github.com/microsoft/PSRule/issues/714)
    - Constraints support using a `@prerelease` or `@pre` to include prerelease versions. [#717](https://github.com/microsoft/PSRule/issues/717)
    - Constraint sets allow multiple constraints to be joined together. [#715](https://github.com/microsoft/PSRule/issues/715)
    - See [about_PSRule_Assert] for details.
- Bug fixes:
  - Fixed prerelease constraint handling for prerelease versions. [#712](https://github.com/microsoft/PSRule/issues/712)

## v1.4.0-B2105004 (pre-release)

What's changed since v1.3.0:

- General improvements:
  - PSRule banner can be configured in output when using `Assert-PSRule`. [#708](https://github.com/microsoft/PSRule/issues/708)

## v1.3.0

What's changed since v1.2.0:

- Engine features:
  - Options can be configured with environment variables. [#691](https://github.com/microsoft/PSRule/issues/691)
    - See [about_PSRule_Options] for details.
- General improvements:
  - Exclude `.git` sub-directory by default for recursive scans. [#697](https://github.com/microsoft/PSRule/issues/697)
    - Added `Input.IgnoreGitPath` option to configure inclusion of `.git` path.
    - See [about_PSRule_Options] for details.
  - Added file path assertion helpers. [#679](https://github.com/microsoft/PSRule/issues/679)
    - Added `WithinPath` to check the file path field is within a specified path.
    - Added `NotWithinPath` to check the file path field is not within a specified path
    - See [about_PSRule_Assert] for details.
  - Added DateTime type assertion helper. [#680](https://github.com/microsoft/PSRule/issues/680)
    - Added `IsDateTime` to check of object field is `[DateTime]`.
    - See [about_PSRule_Assert] for details.
  - Improved numeric comparison assertion helpers to compare `[DateTime]` fields. [#685](https://github.com/microsoft/PSRule/issues/685)
    - `Less`, `LessOrEqual`, `Greater`, and `GreaterOrEqual` compare the number of days from the current time.
    - See [about_PSRule_Assert] for details.
  - Improved handling of field names for objects implementing `IList`, `IEnumerable`, and index properties. [#692](https://github.com/microsoft/PSRule/issues/692)
- Engineering:
  - Bump YamlDotNet dependency to 11.1.1. [#690](https://github.com/microsoft/PSRule/pull/690)
- Bug fixes:
  - Fixed expected DocumentEnd got SequenceEnd. [#698](https://github.com/microsoft/PSRule/issues/698)

What's changed since pre-release v1.3.0-B2105004:

- No additional changes.

## v1.3.0-B2105004 (pre-release)

What's changed since pre-release v1.3.0-B2104042:

- Engine features:
  - Options can be configured with environment variables. [#691](https://github.com/microsoft/PSRule/issues/691)
    - See [about_PSRule_Options] for details.
- General improvements:
  - Exclude `.git` sub-directory by default for recursive scans. [#697](https://github.com/microsoft/PSRule/issues/697)
    - Added `Input.IgnoreGitPath` option to configure inclusion of `.git` path.
    - See [about_PSRule_Options] for details.

## v1.3.0-B2104042 (pre-release)

What's changed since pre-release v1.3.0-B2104030:

- Bug fixes:
  - Fixed expected DocumentEnd got SequenceEnd. [#698](https://github.com/microsoft/PSRule/issues/698)

## v1.3.0-B2104030 (pre-release)

What's changed since pre-release v1.3.0-B2104021:

- General improvements:
  - Improved handling of field names for objects implementing `IList`, `IEnumerable`, and index properties. [#692](https://github.com/microsoft/PSRule/issues/692)
- Engineering:
  - Bump YamlDotNet dependency to 11.1.1. [#690](https://github.com/microsoft/PSRule/pull/690)

## v1.3.0-B2104021 (pre-release)

What's changed since v1.2.0:

- General improvements:
  - Added file path assertion helpers. [#679](https://github.com/microsoft/PSRule/issues/679)
    - Added `WithinPath` to check the file path field is within a specified path.
    - Added `NotWithinPath` to check the file path field is not within a specified path
    - See [about_PSRule_Assert] for details.
  - Added DateTime type assertion helper. [#680](https://github.com/microsoft/PSRule/issues/680)
    - Added `IsDateTime` to check of object field is `[DateTime]`.
    - See [about_PSRule_Assert] for details.
  - Improved numeric comparison assertion helpers to compare `[DateTime]` fields. [#685](https://github.com/microsoft/PSRule/issues/685)
    - `Less`, `LessOrEqual`, `Greater`, and `GreaterOrEqual` compare the number of days from the current time.
    - See [about_PSRule_Assert] for details.

## v1.2.0

What's changed since v1.1.0:

- Engine features:
  - Added support for extensibility with conventions. [#650](https://github.com/microsoft/PSRule/issues/650)
    - Conventions provide an extensibility point within PSRule to execute actions within the pipeline.
    - A convention can expose `Begin`, `Process`, and `End` blocks.
    - In additional to within rules `$PSRule.Data` can be accessed from `Begin` and `Process` blocks.
    - See [about_PSRule_Conventions] for details.
  - Added support for object expansion with conventions. [#661](https://github.com/microsoft/PSRule/issues/661)
    - Use the `$PSRule.Import` method to import child source objects into the pipeline.
    - See [about_PSRule_Variables] for details.
  - Added support for complex pre-conditions with selectors. [#649](https://github.com/microsoft/PSRule/issues/649)
    - See [about_PSRule_Selectors] for details.
- General improvements:
  - Added support for preferring automatic binding over custom binding configurations. [#670](https://github.com/microsoft/PSRule/issues/670)
    - Added the `Binding.PreferTargetInfo` option to prefer target info specified by the object.
    - See [about_PSRule_Options] for details.
  - Added strong apiVersion to resource types. [#647](https://github.com/microsoft/PSRule/issues/647)
    - Resource schemas now support an `apiVersion` field.
    - The `apiVersion` field is optional but recommended.
    - Resources without a `apiVersion` field will not be supported from PSRule v2.
    - Added warning to flag baseline without `apiVersion` set.
  - Added support for detecting files headers from additional file extensions using `FileHeader`. [#664](https://github.com/microsoft/PSRule/issues/664)
    - Added `.bicep`, `.csx`, `.jsx`, `.groovy`, `.java`, `.json`, `.jsonc`,
    `.scala`, `.rb`, `.bat`, `.cmd`.
    - Added support for `Jenkinsfile` and `Dockerfile` without an extension.
    - See [about_PSRule_Assert] for details.
  - Added support for automatic type binding with files that do not have a file extension. [#665](https://github.com/microsoft/PSRule/issues/665)
- Bug fixes:
  - Fixed dependent rule execution is skipped for consequent input objects. [#657](https://github.com/microsoft/PSRule/issues/657)

What's changed since pre-release v1.2.0-B2103043:

- No additional changes.

## v1.2.0-B2103043 (pre-release)

What's changed since pre-release v1.2.0-B2103031:

- Engine features:
  - Added support for complex pre-conditions with selectors. [#649](https://github.com/microsoft/PSRule/issues/649)
- General improvements:
  - Added support for preferring automatic binding over custom binding configurations. [#670](https://github.com/microsoft/PSRule/issues/670)
    - Added the `Binding.PreferTargetInfo` option to prefer target info specified by the object.
    - See [about_PSRule_Options] for details.
  - Added strong apiVersion to resource types. [#647](https://github.com/microsoft/PSRule/issues/647)
    - Resource schemas now support an `apiVersion` field.
    - The `apiVersion` field is optional but recommended.
    - Resources without a `apiVersion` field will not be supported from PSRule v2.
    - Added warning to flag baseline without `apiVersion` set.

## v1.2.0-B2103031 (pre-release)

What's changed since pre-release v1.2.0-B2103023:

- General improvements:
  - Added support for detecting files headers from additional file extensions. [#664](https://github.com/microsoft/PSRule/issues/664)
    - Added `.bicep`, `.csx`, `.jsx`, `.groovy`, `.java`, `.json`, `.jsonc`,
    `.scala`, `.rb`, `.bat`, `.cmd`.
    - Added support for `Jenkinsfile` and `Dockerfile` without an extension.
    - See [about_PSRule_Assert] for details.
  - Added support for automatic type binding with files that do not have a file extension. [#665](https://github.com/microsoft/PSRule/issues/665)

## v1.2.0-B2103023 (pre-release)

What's changed since pre-release v1.2.0-B2103016:

- Engine features:
  - Added support for object expansion with conventions. [#661](https://github.com/microsoft/PSRule/issues/661)
    - Use the `$PSRule.Import` method to import child source objects into the pipeline.
    - See [about_PSRule_Variables] for details.

## v1.2.0-B2103016 (pre-release)

What's changed since pre-release v1.2.0-B2103008:

- Bug fixes:
  - Fixed dependent rule execution is skipped for consequent input objects. [#657](https://github.com/microsoft/PSRule/issues/657)

## v1.2.0-B2103008 (pre-release)

What's changed since v1.1.0:

- Engine features:
  - Added support for extensibility with conventions. [#650](https://github.com/microsoft/PSRule/issues/650)
    - Conventions provide an extensibility point within PSRule to execute actions within the pipeline.
    - A convention can expose `Begin`, `Process`, and `End` blocks.
    - In additional to within rules `$PSRule.Data` can be accessed from `Begin` and `Process` blocks.
    - See [about_PSRule_Conventions] for details.

## v1.1.0

What's changed since v1.0.3:

- Engine features:
  - Added assertion helpers. [#640](https://github.com/microsoft/PSRule/issues/640)
    - Added `NotHasField` to check object does not have any of the specified fields.
    - Added `Null` to check field value is null.
    - Added `NotNull` to check field value is not null.
    - See [about_PSRule_Assert] for details.
  - Added type assertion helpers. [#635](https://github.com/microsoft/PSRule/issues/635)
    - Added `IsNumeric` to check field value is a numeric types.
    - Added `IsInteger` to check field value is an integer types.
    - Added `IsBoolean` to check field value is a boolean.
    - Added `IsArray` to check field value is an array.
    - Added `IsString` to check field value is a string.
    - Added `TypeOf` to check field value is a specified type.
    - See [about_PSRule_Assert] for details.
  - Added content helpers. [#637](https://github.com/microsoft/PSRule/issues/637)
    - Added `$PSRule.GetContentFirstOrDefault` to get content and return the first object.
    - Added `$PSRule.GetContentField` to get the field from content objects.
    - See [about_PSRule_Variables] for details.
- General improvements:
  - Updated `HasJsonSchema` assertion helper. [#636](https://github.com/microsoft/PSRule/issues/636)
    - The URI scheme can optionally be ignored for `http://` or `https://` URIs.
    - The fragment `#` is ignored.
    - See [about_PSRule_Assert] for details.
  - Added support for `-Outcome` and `-As` to produce filtered output from `Assert-PSRule`. [#643](https://github.com/microsoft/PSRule/issues/643)
    - Configure `Output.As` with `Summary` to produce summarized results per object.
    - Configure `Output.Outcome` to limit output to `Fail` or `Error`.
    - See [Assert-PSRule] for details.

What's changed since pre-release v1.1.0-B2102029:

- No additional changes.

## v1.1.0-B2102029 (pre-release)

What's changed since pre-release v1.1.0-B2102024:

- General improvements:
  - Added support for `-Outcome` and `-As` to produce filtered output from `Assert-PSRule`. [#643](https://github.com/microsoft/PSRule/issues/643)
    - Configure `Output.As` with `Summary` to produce summarized results per object.
    - Configure `Output.Outcome` to limit output to `Fail` or `Error`.
    - See [Assert-PSRule] for details.

## v1.1.0-B2102024 (pre-release)

What's changed since pre-release v1.1.0-B2102019:

- Engine features:
  - Added assertion helpers. [#640](https://github.com/microsoft/PSRule/issues/640)
    - Added `NotHasField` to check object does not have any of the specified fields.
    - Added `Null` to check field value is null.
    - Added `NotNull` to check field value is not null.
    - See [about_PSRule_Assert] for details.

## v1.1.0-B2102019 (pre-release)

What's changed since v1.0.3:

- Engine features:
  - Added type assertion helpers. [#635](https://github.com/microsoft/PSRule/issues/635)
    - Added `IsNumeric` to check field value is a numeric types.
    - Added `IsInteger` to check field value is an integer types.
    - Added `IsBoolean` to check field value is a boolean.
    - Added `IsArray` to check field value is an array.
    - Added `IsString` to check field value is a string.
    - Added `TypeOf` to check field value is a specified type.
    - See [about_PSRule_Assert] for details.
  - Added content helpers. [#637](https://github.com/microsoft/PSRule/issues/637)
    - Added `$PSRule.GetContentFirstOrDefault` to get content and return the first object.
    - Added `$PSRule.GetContentField` to get the field from content objects.
    - See [about_PSRule_Variables] for details.
- General improvements:
  - Updated `HasJsonSchema` assertion helper. [#636](https://github.com/microsoft/PSRule/issues/636)
    - The URI scheme can optionally be ignored for `http://` or `https://` URIs.
    - The fragment `#` is ignored.
    - See [about_PSRule_Assert] for details.

## v1.0.3

What's changed since v1.0.2:

- Bug fixes:
  - Fixed reason reported fields for `HasField` and `HasFields` assertion helpers. [#632](https://github.com/microsoft/PSRule/issues/632)

## v1.0.2

What's changed since v1.0.1:

- Engineering:
  - Bump Manatee.Json dependency to 13.0.5. [#619](https://github.com/microsoft/PSRule/pull/619)
- Bug fixes:
  - Fixed `GetContent` processing of `InputFileInfo`. [#625](https://github.com/microsoft/PSRule/issues/625)
  - Fixed null reference of rule reason with wide output. [#626](https://github.com/microsoft/PSRule/issues/626)
  - Fixed markdown help handling of inline code blocks with `[`. [#627](https://github.com/microsoft/PSRule/issues/627)
  - Fixed markdown help inclusion of fenced code blocks in notes and description. [#628](https://github.com/microsoft/PSRule/issues/628)

## v1.0.1

What's changed since v1.0.0:

- Bug fixes:
  - Fixed module source key has already been added. [#608](https://github.com/microsoft/PSRule/issues/608)

## v1.0.0

What's changed since v0.22.0:

- General improvements:
  - Added rule help link in failed `Assert-PSRule` output. [#595](https://github.com/microsoft/PSRule/issues/595)
- Engineering:
  - **Breaking change**: Removed deprecated `$Rule` properties. [#495](https://github.com/microsoft/PSRule/pull/495)
  - Bump Manatee.Json dependency to 13.0.4. [#591](https://github.com/microsoft/PSRule/pull/591)

What's changed since pre-release v1.0.0-B2011028:

- No additional changes.

## v1.0.0-B2011028 (pre-release)

What's changed since v0.22.0:

- General improvements:
  - Added rule help link in failed `Assert-PSRule` output. [#595](https://github.com/microsoft/PSRule/issues/595)
- Engineering:
  - **Breaking change**: Removed deprecated `$Rule` properties. [#495](https://github.com/microsoft/PSRule/pull/495)
  - Bump Manatee.Json dependency to 13.0.4. [#591](https://github.com/microsoft/PSRule/pull/591)

[Assert-PSRule]: commands/PSRule/en-US/Assert-PSRule.md
[about_PSRule_Assert]: concepts/PSRule/en-US/about_PSRule_Assert.md
[about_PSRule_Options]: concepts/PSRule/en-US/about_PSRule_Options.md
[about_PSRule_Variables]: concepts/PSRule/en-US/about_PSRule_Variables.md
[about_PSRule_Conventions]: concepts/PSRule/en-US/about_PSRule_Conventions.md
[about_PSRule_Selectors]: concepts/PSRule/en-US/about_PSRule_Selectors.md
[about_PSRule_Rules]: concepts/PSRule/en-US/about_PSRule_Rules.md
[about_PSRule_Badges]: concepts/PSRule/en-US/about_PSRule_Badges.md
[about_PSRule_Expressions]: concepts/PSRule/en-US/about_PSRule_Expressions.md
