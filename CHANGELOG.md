# Change log

**Important notes**:

- Removal of deprecated `$Rule` properties are scheduled for PSRule v1.0.0. [#495](https://github.com/microsoft/PSRule/issues/495)

## Unreleased

## v0.20.0

What's changed since v0.19.0:

- Engine features:
  - Added support for scanning repository files. [#524](https://github.com/microsoft/PSRule/issues/524)
    - Added `File` input type (`-InputType File`) to scan for files without deserializing them.
    - Added `Input.PathIgnore` option to ignore files.
    - When using the `File` input type path specs in `.gitignore` are ignored.
  - Added `Get-PSRuleTarget` cmdlet to read input files and return raw objects. [#525](https://github.com/microsoft/PSRule/issues/525)
    - This cmdlet can be used to troubleshoot PSRule input issues.
  - Baselines can now be flagged as obsolete. [#499](https://github.com/microsoft/PSRule/issues/499)
    - Set the `metadata.annotations.obsolete` property to `true` to flag a baseline as obsolete.
    - When an obsolete baseline is used, a warning will be generated.
  - Added file assertion helpers `FileHeader`, and `FilePath`. [#534](https://github.com/microsoft/PSRule/issues/534)
    - `FileHeader` checks for a comment header in the file.
    - `FilePath` checks that a file path (optionally with suffixes) exist.
- General improvements:
  - Added automatic binding for Rule object. [#542](https://github.com/microsoft/PSRule/issues/542)
- Engineering:
  - Warn when deprecated `$Rule` properties are used. [#536](https://github.com/microsoft/PSRule/issues/536) [#545](https://github.com/microsoft/PSRule/issues/545)
    - First usage of deprecated property generates a warning.
    - Rule using deprecated property is flagged in debug output.
  - Bump YamlDotNet dependency to v8.1.2. [#439](https://github.com/microsoft/PSRule/issues/439)
- Bug fixes:
  - Fixed out of bounds exception when empty markdown documentation is used. [#516](https://github.com/microsoft/PSRule/issues/516)

What's changed since pre-release v0.20.0-B2009013:

- Bug fixes:
  - Fixed excessive obsolete property warnings. [#545](https://github.com/microsoft/PSRule/issues/545)

## v0.20.0-B2009013 (pre-release)

What's changed since pre-release v0.20.0-B2009007:

- General improvements:
  - Added automatic binding for Rule object. [#542](https://github.com/microsoft/PSRule/issues/542)
- Bug fixes:
  - Fixed `InputFileInfo` `Type` property causes downstream binding issues. [#541](https://github.com/microsoft/PSRule/issues/541)

## v0.20.0-B2009007 (pre-release)

What's changed since pre-release v0.20.0-B2008010:

- Engine features:
  - Added file assertion helpers `FileHeader`, and `FilePath`. [#534](https://github.com/microsoft/PSRule/issues/534)
    - `FileHeader` checks for a comment header in the file.
    - `FilePath` checks that a file path (optionally with suffixes) exist.
- Engineering:
  - Warn when deprecated `$Rule` properties are used. [#536](https://github.com/microsoft/PSRule/issues/536)
- Bug fixes:
  - Fixed out of bounds exception when empty markdown documentation is used. [#516](https://github.com/microsoft/PSRule/issues/516)
  - Fixed lines breaks in `RepositoryInfo` target name with git ref. [#538](https://github.com/microsoft/PSRule/issues/538)

## v0.20.0-B2008010 (pre-release)

What's changed since pre-release v0.20.0-B2008002:

- Engine features:
  - Baselines can now be flagged as obsolete. [#499](https://github.com/microsoft/PSRule/issues/499)
    - Set the `metadata.annotations.obsolete` property to `true` to flag a baseline as obsolete.
    - When an obsolete baseline is used, a warning will be generated.
- Engineering:
  - Bump YamlDotNet dependency to v8.1.2. [#439](https://github.com/microsoft/PSRule/issues/439)

## v0.20.0-B2008002 (pre-release)

What's changed since v0.19.0:

- Engine features:
  - Added support for scanning repository files. [#524](https://github.com/microsoft/PSRule/issues/524)
    - Added `File` input type (`-InputType File`) to scan for files without deserializing them.
    - Added `Input.PathIgnore` option to ignore files.
    - When using the `File` input type path specs in `.gitignore` are ignored.
  - Added `Get-PSRuleTarget` cmdlet to read input files and return raw objects. [#525](https://github.com/microsoft/PSRule/issues/525)
    - This cmdlet can be used to troubleshoot PSRule input issues.

## v0.19.0

What's changed since v0.18.1:

- Engine features:
  - Added `Reason` method to assertion results. [#500](https://github.com/microsoft/PSRule/issues/500)
    - This new method, streamlines setting custom reasons particularly with formatted strings.
    - The `Reason` method replaces any previously set reasons with a custom string.
    - Optional arguments can be provided to be included in string formatting.
  - Improvements to assertion methods.
    - Added regular expression assertion helpers `Match`, and `NotMatch`. [#502](https://github.com/microsoft/PSRule/issues/502)
    - Added collection assertion helpers `In`, and `NotIn`. [#501](https://github.com/microsoft/PSRule/issues/501)
  - Added module version constraints. [#498](https://github.com/microsoft/PSRule/issues/498)
    - The module versions that PSRule uses can be constrained.
- Bug fixes:
  - Fixed styling for no rule files warning with `Assert-PSRule`. [#484](https://github.com/microsoft/PSRule/issues/484)
  - Fixed actual value in reason for numeric comparison assertion method. [#505](https://github.com/microsoft/PSRule/issues/505)

What's changed since pre-release v0.19.0-B2007030:

- No additional changes.

## v0.19.0-B2007030 (pre-release)

- Bug fixes:
  - Fixed `Assert.In` unable to compare PSObject wrapped array items. [#512](https://github.com/microsoft/PSRule/issues/512)

## v0.19.0-B2007023 (pre-release)

- Engine features:
  - Added `Reason` method to assertion results. [#500](https://github.com/microsoft/PSRule/issues/500)
    - This new method, streamlines setting custom reasons particularly with formatted strings.
    - The `Reason` method replaces any previously set reasons with a custom string.
    - Optional arguments can be provided to be included in string formatting.
  - Improvements to assertion methods.
    - Added regular expression assertion helpers `Match`, and `NotMatch`. [#502](https://github.com/microsoft/PSRule/issues/502)
    - Added collection assertion helpers `In`, and `NotIn`. [#501](https://github.com/microsoft/PSRule/issues/501)
  - Added module version constraints. [#498](https://github.com/microsoft/PSRule/issues/498)
    - The module versions that PSRule uses can be constrained.
- Bug fixes:
  - Fixed styling for no rule files warning with `Assert-PSRule`. [#484](https://github.com/microsoft/PSRule/issues/484)
  - Fixed actual value in reason for numeric comparison assertion method. [#505](https://github.com/microsoft/PSRule/issues/505)

## v0.18.1

What's changed since v0.18.0:

- Bug fixes:
  - Fixed unable to read properties for .NET `DynamicObject`. [#491](https://github.com/Microsoft/PSRule/issues/491)
  - Fixed read of JSON input format with null array item. [#490](https://github.com/Microsoft/PSRule/issues/490)
  - Fixed `Csv` output format with summary for `Invoke-PSRule`. [#486](https://github.com/Microsoft/PSRule/issues/486)

## v0.19.0-B2006027 (pre-release)

- Bug fixes:
  - Fixed unable to read properties for .NET `DynamicObject`. [#491](https://github.com/Microsoft/PSRule/issues/491)
  - Fixed read of JSON input format with null array item. [#490](https://github.com/Microsoft/PSRule/issues/490)

## v0.19.0-B2006018 (pre-release)

- Bug fixes:
  - Fixed `Csv` output format with summary for `Invoke-PSRule`. [#486](https://github.com/Microsoft/PSRule/issues/486)

## v0.18.0

What's changed since v0.17.0:

- General improvements:
  - Improved `Assert-PSRule` output formatting. [#472](https://github.com/Microsoft/PSRule/issues/472)
    - Added recommendation and reasons for `AzurePipelines` and `GitHubActions` styles.
    - Summary line is has been updated to include synopsis instead of reasons.
- Bug fixes:
  - Fixed binding with `ModuleConfig`. [#468](https://github.com/Microsoft/PSRule/issues/468)
  - Fixed recommendation output with client style. [#467](https://github.com/Microsoft/PSRule/issues/467)

What's changed since pre-release v0.18.0-B2005015:

- No additional changes.

## v0.18.0-B2005015 (pre-release)

- General improvements:
  - Improved `Assert-PSRule` output formatting. [#472](https://github.com/Microsoft/PSRule/issues/472)
    - Added recommendation and reasons for `AzurePipelines` and `GitHubActions` styles.
    - Summary line is has been updated to include synopsis instead of reasons.
- Bug fixes:
  - Fixed binding with `ModuleConfig`. [#468](https://github.com/Microsoft/PSRule/issues/468)
  - Fixed recommendation output with client style. [#467](https://github.com/Microsoft/PSRule/issues/467)

## v0.17.0

What's changed since v0.16.0:

- General improvements:
  - Improved `Assert-PSRule` output formatting.
    - Added recommendation and reasons for `Client` and `Plain` styles. [#456](https://github.com/Microsoft/PSRule/issues/456)
  - Added support for configuration of default module options. [#459](https://github.com/Microsoft/PSRule/issues/459)
    - `binding` and `configuration` options can be set to a default value.
    - Updated `New-PSRuleOption` parameter sets and help based on updates to module config.
  - Added support for module fallback culture. [#441](https://github.com/Microsoft/PSRule/issues/441)
- Bug fixes:
  - Fixed resource schema to include `useQualifiedName` and `nameSeparator` option. [#458](https://github.com/Microsoft/PSRule/issues/458)

What's changed since pre-release v0.17.0-B2005010:

- No additional changes.

## v0.17.0-B2005010 (pre-release)

- General improvements:
  - Improved `Assert-PSRule` output formatting.
    - Added recommendation and reasons for `Client` and `Plain` styles. [#456](https://github.com/Microsoft/PSRule/issues/456)
  - Added support for configuration of default module options. [#459](https://github.com/Microsoft/PSRule/issues/459)
    - `binding` and `configuration` options can be set to a default value.
    - Updated `New-PSRuleOption` parameter sets and help based on updates to module config.
  - Added support for module fallback culture. [#441](https://github.com/Microsoft/PSRule/issues/441)
- Bug fixes:
  - Fixed resource schema to include `useQualifiedName` and `nameSeparator` option. [#458](https://github.com/Microsoft/PSRule/issues/458)

## v0.16.0

What's changed since v0.15.0:

- General improvements:
  - Added configuration option `Output.Culture` for setting culture. [#442](https://github.com/Microsoft/PSRule/issues/442)
  - Improved handling of fields to allow the input object to be referenced with `.`. [#437](https://github.com/Microsoft/PSRule/issues/437)
- Bug fixes:
  - Fixed numeric comparison assertion with non-int types. [#436](https://github.com/Microsoft/PSRule/issues/436)
  - Fixed output culture option ignored. [#449](https://github.com/Microsoft/PSRule/issues/449)

What's changed since pre-release v0.16.0-B2003027:

- No additional changes.

## v0.16.0-B2003027 (pre-release)

- Bug fixes:
  - Fixed output culture option ignored. [#449](https://github.com/Microsoft/PSRule/issues/449)

## v0.16.0-B2003022 (pre-release)

- General improvements:
  - Added configuration option `Output.Culture` for setting culture. [#442](https://github.com/Microsoft/PSRule/issues/442)
  - Improved handling of fields to allow the input object to be referenced with `.`. [#437](https://github.com/Microsoft/PSRule/issues/437)
- Bug fixes:
  - Fixed numeric comparison assertion with non-int types. [#436](https://github.com/Microsoft/PSRule/issues/436)

## v0.15.0

What's changed since v0.14.0:

- Engine features:
  - Added `-ResultVariable` to store results from Assert-PSRule into a variable. [#412](https://github.com/Microsoft/PSRule/issues/412)
- General improvements:
  - Added recommendation to failure message of NUnit results. [#421](https://github.com/Microsoft/PSRule/issues/421)
- Bug fixes:
  - Fixed handling of `v` in field value with `$Assert.Version`. [#429](https://github.com/Microsoft/PSRule/issues/429)
  - Fixed handling of warning action preference with `Assert-PSRule`. [#428](https://github.com/Microsoft/PSRule/issues/428)
  - Fixed parent culture unwind with POSIX. [#414](https://github.com/Microsoft/PSRule/issues/414)
  - Fixed output of warning with `Assert-PSRule`. [#417](https://github.com/Microsoft/PSRule/issues/417)
  - Fixed NUnit report to include a failure element when reason is not specified. [#420](https://github.com/Microsoft/PSRule/issues/420)

What's changed since pre-release v0.15.0-B2002031:

- No additional changes.

## v0.15.0-B2002031 (pre-release)

- Fixed handling of `v` in field value with `$Assert.Version`. [#429](https://github.com/Microsoft/PSRule/issues/429)
- Fixed handling of warning action preference with `Assert-PSRule`. [#428](https://github.com/Microsoft/PSRule/issues/428)

## v0.15.0-B2002019 (pre-release)

- Added `-ResultVariable` to store results from Assert-PSRule into a variable. [#412](https://github.com/Microsoft/PSRule/issues/412)

## v0.15.0-B2002012 (pre-release)

- Fixed output of warning with `Assert-PSRule`. [#417](https://github.com/Microsoft/PSRule/issues/417)
- Fixed NUnit report to include a failure element when reason is not specified. [#420](https://github.com/Microsoft/PSRule/issues/420)
- Added recommendation to failure message of NUnit results. [#421](https://github.com/Microsoft/PSRule/issues/421)

## v0.15.0-B2002005 (pre-release)

- Fixed parent culture unwind with POSIX. [#414](https://github.com/Microsoft/PSRule/issues/414)

## v0.14.0

What's changed since v0.13.0:

- Engine features:
  - Added support for qualified target names. [#395](https://github.com/Microsoft/PSRule/issues/395)
    - Added options `Binding.UseQualifiedName` and `Binding.NameSeparator`.
    - See `about_PSRule_Options` for details.
  - Added assertion method `HasJsonSchema` to check if a JSON schema is referenced. [#398](https://github.com/Microsoft/PSRule/issues/398)
    - See `about_PSRule_Assert` for usage details.
  - Added file content helper for reading objects from files. [#399](https://github.com/Microsoft/PSRule/issues/399)
    - The method `GetContent` of `$PSRule` can be used to read files as objects.
    - See `about_PSRule_Variables` for usage details.
- General improvements:
  - Improved reporting on runtime errors in rule blocks. [#239](https://github.com/Microsoft/PSRule/issues/239)
  - Improved NUnit results to include a failure message based on reported reasons. [#404](https://github.com/Microsoft/PSRule/issues/404)
- Bug fixes:
  - Fixed wide formatting of rules with `Get-PSRule`. [#407](https://github.com/Microsoft/PSRule/issues/407)
  - Fixed TargetName hash serialization for base types. [#406](https://github.com/Microsoft/PSRule/issues/406)
  - Fixed output not generated with Assert-PSRule and Stop. [#405](https://github.com/Microsoft/PSRule/issues/405)
  - Fixed NUnit results incorrectly reporting that the test had not executed. [#403](https://github.com/Microsoft/PSRule/issues/403)

What's changed since pre-release v0.14.0-B2002003:

- No additional changes

## v0.14.0-B2002003 (pre-release)

- Fixed wide formatting of rules with `Get-PSRule`. [#407](https://github.com/Microsoft/PSRule/issues/407)
- Fixed TargetName hash serialization for base types. [#406](https://github.com/Microsoft/PSRule/issues/406)
- Fixed output not generated with Assert-PSRule and Stop. [#405](https://github.com/Microsoft/PSRule/issues/405)
- Fixed NUnit results incorrectly reporting that the test had not executed. [#403](https://github.com/Microsoft/PSRule/issues/403)
- Improved NUnit results to include a failure message based on reported reasons. [#404](https://github.com/Microsoft/PSRule/issues/404)
- Improved reporting on runtime errors in rule blocks. [#239](https://github.com/Microsoft/PSRule/issues/239)

## v0.14.0-B2001020 (pre-release)

- Added support for qualified target names. [#395](https://github.com/Microsoft/PSRule/issues/395)
  - Added options `Binding.UseQualifiedName` and `Binding.NameSeparator`.
  - See `about_PSRule_Options` for details.
- Added assertion method `HasJsonSchema` to check if a JSON schema is referenced. [#398](https://github.com/Microsoft/PSRule/issues/398)
  - See `about_PSRule_Assert` for usage details.
- Added file content helper for reading objects from files. [#399](https://github.com/Microsoft/PSRule/issues/399)
  - The method `GetContent` of `$PSRule` can be used to read files as objects.
  - See `about_PSRule_Variables` for usage details.

## v0.13.0

What's changed since v0.12.0:

- Engine features:
  - Improvements to rule help and documentation. [#382](https://github.com/Microsoft/PSRule/issues/382) [#316](https://github.com/Microsoft/PSRule/issues/316)
    - Added links and notes sections to help.
    - Added `-Full` switch to `Get-PSRuleHelp` to display links and notes sections.
    - Added support for using a parent culture in rule help.
    - Rule help will use parent culture when a more specific culture is not available.
  - Added input format for reading PowerShell data `.psd1` files. [#368](https://github.com/Microsoft/PSRule/issues/368)
    - `PowerShellData` has been added to `Input.Format`.
    - See `about_PSRule_Options` for details.
  - Added custom rule data to results. [#322](https://github.com/Microsoft/PSRule/issues/322)
    - `$PSRule.Data` can be used to set custom data during rule execution that is included in output.
    - See `about_PSRule_Variables` for usage details.
  - Improvements to assertion methods. [#386](https://github.com/Microsoft/PSRule/issues/386) [#374](https://github.com/Microsoft/PSRule/issues/374) [#387](https://github.com/Microsoft/PSRule/issues/387) [#344](https://github.com/Microsoft/PSRule/issues/344) [#353](https://github.com/Microsoft/PSRule/issues/353) [#357](https://github.com/Microsoft/PSRule/issues/357)
    - Added support for assertion methods to be used within script pre-conditions.
    - Added numeric comparison assertion helpers `Greater`, `GreaterOrEqual`, `Less` and `LessOrEqual`.
    - Added semantic version assertion helper `Version`.
    - Added string affix assertion helpers `StartsWith`, `EndsWith` and `Contains`.
    - See `about_PSRule_Assert` for usage details.
  - Improvements to output logging and formatting for `Assert-PSRule`.
    - Formatting now includes errors and warnings using style.
    - Added PSRule banner with module information.
    - Added rule success summary.
- General improvements:
  - Added aliases for `-OutputFormat` (`-o`) and `-Module` (`-m`) parameters. [#384](https://github.com/Microsoft/PSRule/issues/384)
  - Added `WithReason` to append/ replace reasons from assertion result. [#354](https://github.com/Microsoft/PSRule/issues/354)
  - Added configuration helper for strings arrays. [#363](https://github.com/Microsoft/PSRule/issues/363)
- Bug fixes:
  - Fixed JSON de-serialization fails with single object. [#379](https://github.com/Microsoft/PSRule/issues/379)
  - Fixed stack overflow when parsing malformed JSON. [#380](https://github.com/Microsoft/PSRule/issues/380)

What's changed since pre-release v0.13.0-B2001013:

- No additional changes.

## v0.13.0-B2001013 (pre-release)

- Fixed JSON de-serialization fails with single object. [#379](https://github.com/Microsoft/PSRule/issues/379)
- Fixed stack overflow when parsing malformed JSON. [#380](https://github.com/Microsoft/PSRule/issues/380)
- Added rule documentation links and notes to help. [#382](https://github.com/Microsoft/PSRule/issues/382)
  - Added `-Full` switch to `Get-PSRuleHelp` to display links and notes sections.
- Added aliases for `-OutputFormat` (`-o`) and `-Module` (`-m`) parameters. [#384](https://github.com/Microsoft/PSRule/issues/384)
- Improved numeric comparison assertion helpers to support strings. [#387](https://github.com/Microsoft/PSRule/issues/387)
  - Methods `Greater`, `GreaterOrEqual`, `Less` and `LessOrEqual` now also check string length.
- Added support for assertion methods to be used within script pre-conditions. [#386](https://github.com/Microsoft/PSRule/issues/386)

## v0.13.0-B1912043 (pre-release)

- Added input format for reading PowerShell data `.psd1` files. [#368](https://github.com/Microsoft/PSRule/issues/368)
  - `PowerShellData` has been added to `Input.Format`.
  - See `about_PSRule_Options` for details.
- Added numeric comparison assertion helpers. [#374](https://github.com/Microsoft/PSRule/issues/374)
  - Added methods `Greater`, `GreaterOrEqual`, `Less` and `LessOrEqual`.
  - See `about_PSRule_Assert` for usage details.

## v0.13.0-B1912027 (pre-release)

- Added configuration helper for strings arrays. [#363](https://github.com/Microsoft/PSRule/issues/363)
- Added support for using a parent culture in rule help. [#316](https://github.com/Microsoft/PSRule/issues/316)
  - Rule help will use parent culture when a more specific culture is not available.
- Added custom rule data to results. [#322](https://github.com/Microsoft/PSRule/issues/322)
  - `$PSRule.Data` can be used to set custom data during rule execution that is included in output.
  - See `about_PSRule_Variables` for usage details.

## v0.13.0-B1912012 (pre-release)

- Improves output logging and formatting for Assert-PSRule. [#357](https://github.com/Microsoft/PSRule/issues/357)
  - Formatting now includes errors and warnings using style.
  - Added PSRule banner with module information.
  - Added rule success summary.

## v0.13.0-B1912005 (pre-release)

- Added semantic version assertion helper `Version`. [#344](https://github.com/Microsoft/PSRule/issues/344)
- Added string affix assertion helpers. [#353](https://github.com/Microsoft/PSRule/issues/353)
  - Added methods `StartsWith`, `EndsWith` and `Contains`. See `about_PSRule_Assert` for usage details.
- Added `WithReason` to append/ replace reasons from assertion result. [#354](https://github.com/Microsoft/PSRule/issues/354)

## v0.12.0

What's changed since v0.11.0:

- Engine features:
  - Added `-All` option to `Exists` keyword. [#331](https://github.com/Microsoft/PSRule/issues/331)
  - Added custom field binding. [#321](https://github.com/Microsoft/PSRule/issues/321)
    - Added new option `Binding.Field` available in baselines to configure binding.
- General improvements:
  - Added filtering for rules against a baseline with `Get-PSRule`. [#345](https://github.com/Microsoft/PSRule/issues/345)
  - Added parameter alias `-f` for `-InputPath`. [#340](https://github.com/Microsoft/PSRule/issues/340)
    - `-f` was added to `Invoke-PSRule`, `Assert-PSRule` and `Test-PSRuleTarget` cmdlets.
- **Important change**: Added `$PSRule` generic context variable. [#341](https://github.com/Microsoft/PSRule/issues/341)
  - Deprecated `TargetName`, `TargetType` and `TargetObject` properties on `$Rule`.
  - Use `TargetName`, `TargetType` and `TargetObject` on `$PSRule` instead.
  - Properties `TargetName`, `TargetType` and `TargetObject` on `$Rule` will be removed in the future.
  - Going forward `$Rule` will only contain properties that relate to the current rule context.
- Bug fixes:
  - Fixed key has already been added for default baseline. [#349](https://github.com/Microsoft/PSRule/issues/349)
  - Fixed multiple value tag filtering. [#346](https://github.com/Microsoft/PSRule/issues/346)
  - Fixed TargetType fall back to type name. [#339](https://github.com/Microsoft/PSRule/issues/339)
  - Fixed NUnit serialization issue for unprocessed rules. [#332](https://github.com/Microsoft/PSRule/issues/332)

What's changed since pre-release v0.12.0-B1912007:

- Fixed key has already been added for default baseline. [#349](https://github.com/Microsoft/PSRule/issues/349)

## v0.12.0-B1912007 (pre-release)

- Fixed multiple value tag filtering. [#346](https://github.com/Microsoft/PSRule/issues/346)
- Added filtering for rules against a baseline with `Get-PSRule`. [#345](https://github.com/Microsoft/PSRule/issues/345)

## v0.12.0-B1912002 (pre-release)

- Fixed TargetType fall back to type name. [#339](https://github.com/Microsoft/PSRule/issues/339)
- Added custom field binding. [#321](https://github.com/Microsoft/PSRule/issues/321)
  - Added new option `Binding.Field` available in baselines to configure binding.
- Added parameter alias `-f` for `-InputPath`. [#340](https://github.com/Microsoft/PSRule/issues/340)
  - `-f` was added to `Invoke-PSRule`, `Assert-PSRule` and `Test-PSRuleTarget` cmdlets.
- **Important change**: Added `$PSRule` generic context variable. [#341](https://github.com/Microsoft/PSRule/issues/341)
  - Deprecated `TargetName`, `TargetType` and `TargetObject` properties on `$Rule`.
  - Use `TargetName`, `TargetType` and `TargetObject` on `$PSRule` instead.
  - Properties `TargetName`, `TargetType` and `TargetObject` on `$Rule` will be removed in the future.
  - Going forward `$Rule` will only contain properties that relate to the current rule context.

## v0.12.0-B1911013 (pre-release)

- Fixed NUnit serialization issue for unprocessed rules. [#332](https://github.com/Microsoft/PSRule/issues/332)
- Added `-All` option to `Exists` keyword. [#331](https://github.com/Microsoft/PSRule/issues/331)

## v0.11.0

What's changed since v0.10.0:

- General improvements:
  - Added `-TargetType` parameter to filter input objects by target type. [#176](https://github.com/Microsoft/PSRule/issues/176)
    - This parameter applies to `Invoke-PSRule`, `Assert-PSRule` and `Test-PSRuleTarget`.
- Bug fixes:
  - Fixed null reference exception when bound property is null. [#323](https://github.com/Microsoft/PSRule/issues/323)
  - Fixed missing `Markdown` input format in options schema. [#315](https://github.com/Microsoft/PSRule/issues/315)
- **Breaking change**: Unprocessed object results are not returned from `Test-PSRuleTarget` by default. [#318](https://github.com/Microsoft/PSRule/issues/318)
  - Previously unprocessed objects returned `$True`, now unprocessed objects return no result.
  - Use `-Outcome All` to return `$True` for unprocessed objects the same as <= v0.10.0.

What's changed since pre-release v0.11.0-B1911002:

- No additional changes.

## v0.11.0-B1911002 (pre-release)

- Fixed null reference exception when bound property is null. [#323](https://github.com/Microsoft/PSRule/issues/323)

## v0.11.0-B1910014 (pre-release)

- Fixed missing `Markdown` input format in options schema. [#315](https://github.com/Microsoft/PSRule/issues/315)
- Added `-TargetType` parameter to filter input objects by target type. [#176](https://github.com/Microsoft/PSRule/issues/176)
  - This parameter applies to `Invoke-PSRule`, `Assert-PSRule` and `Test-PSRuleTarget`.
- **Breaking change**: Unprocessed object results are not returned from `Test-PSRuleTarget` by default. [#318](https://github.com/Microsoft/PSRule/issues/318)
  - Previously unprocessed objects returned `$True`, now unprocessed objects return no result.
  - Use `-Outcome All` to return `$True` for unprocessed objects the same as <= v0.10.0.

## v0.10.0

What's changed since v0.9.0:

- General improvements:
  - Added source note properties to input objects read from disk with `-InputPath`. [#302](https://github.com/Microsoft/PSRule/issues/302)
- Engine features:
  - Added assertion helper for checking field default value. [#289](https://github.com/Microsoft/PSRule/issues/289)
  - Added dependency `DependsOn` information to results from `Get-PSRule`. [#210](https://github.com/Microsoft/PSRule/issues/210)
    - To include dependencies that would normally be filtered out use `-IncludeDependencies`.
  - Added input format for reading markdown front matter. [#301](https://github.com/Microsoft/PSRule/issues/301)
    - Markdown front matter is deserialized and evaluated as an object.
  - Added `Assert-PSRule` cmdlet to improve integration into CI processes. [#290](https://github.com/Microsoft/PSRule/issues/290)
    - Added `Output.Style` option to support output in the following styles:
      - Client/ Plain - Output returns easy to read log of rule pass/ fail.
      - Azure Pipelines - Report rule failures as errors collected by Azure Pipelines.
      - GitHub Actions - Reports rule failures as errors collected by GitHub Actions.
- Bug fixes:
  - Fix `Get-PSRuleHelp` -Online in constrained language mode. [#296](https://github.com/Microsoft/PSRule/issues/296)
- **Breaking change**: Removed previously deprecated alias `Hint` for `Recommend`. [#165](https://github.com/Microsoft/PSRule/issues/165)
  - Use the `Recommend` keyword instead.

What's changed since pre-release v0.10.0-B1910036:

- No additional changes.

## v0.10.0-B1910036 (pre-release)

- Added dependency `DependsOn` information to results from `Get-PSRule`. [#210](https://github.com/Microsoft/PSRule/issues/210)
  - To include dependencies that would normally be filtered out use `-IncludeDependencies`.
- Added input format for reading markdown front matter. [#301](https://github.com/Microsoft/PSRule/issues/301)
  - Markdown front matter is deserialized and evaluated as an object.
- Added source note properties to input objects read from disk with `-InputPath`. [#302](https://github.com/Microsoft/PSRule/issues/302)
- **Breaking change**: Removed previously deprecated alias `Hint` for `Recommend`. [#165](https://github.com/Microsoft/PSRule/issues/165)
  - Use the `Recommend` keyword instead.

## v0.10.0-B1910025 (pre-release)

- Fix `Get-PSRuleHelp` -Online in constrained language mode. [#296](https://github.com/Microsoft/PSRule/issues/296)
- Added `Assert-PSRule` cmdlet to improve integration into CI processes. [#290](https://github.com/Microsoft/PSRule/issues/290)
  - Added `Output.Style` option to support output in the following styles:
    - Client/ Plain - Output returns easy to read log of rule pass/ fail.
    - Azure Pipelines - Report rule failures as errors collected by Azure Pipelines.
    - GitHub Actions - Reports rule failures as errors collected by GitHub Actions.

## v0.10.0-B1910011 (pre-release)

- Added assertion helper for checking field default value. [#289](https://github.com/Microsoft/PSRule/issues/289)

## v0.9.0

What's changed since v0.8.0:

- General improvements:
  - Improve feedback of parsing errors. [#185](https://github.com/Microsoft/PSRule/issues/185)
  - Updated `Get-PSRuleHelp` to include help within the current path by default. [#197](https://github.com/Microsoft/PSRule/issues/197)
- Engine features:
  - Added support for a wildcard match using the `Within` keyword. [#272](https://github.com/Microsoft/PSRule/issues/272)
  - Added rule info display name. [#276](https://github.com/Microsoft/PSRule/issues/276)
  - Added support for matching an array of tag values. [#282](https://github.com/Microsoft/PSRule/issues/282)
  - Added named baselines. Now baselines are a separate resource that can be individually used.
    - Baselines can be packaged within module.
    - Modules can specify a default baseline in module manifest.
    - Target binding options (`Binding`) are now part of baselines.
    - See [about_PSRule_Baseline](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md) for more information.
- Bug fixes:
  - Fix can not serialize nested System.IO.DirectoryInfo property. [#281](https://github.com/Microsoft/PSRule/issues/281)
  - Fix ModuleName not displayed in Get-PSRuleHelp list. [#275](https://github.com/Microsoft/PSRule/issues/275)
  - Fix outcome reported when error or exception is raised. [#211](https://github.com/Microsoft/PSRule/issues/211)
- **Breaking change**: Baseline improvements, fundamentally changes how baselines work. [#274](https://github.com/Microsoft/PSRule/issues/274)
  - Previously, baselines were specified as workspace options.
  - The previous `baseline` options property has been renamed to `rule`.
  - The previous `configuration` property is now a top level option.

What's changed since pre-release v0.9.0-B190905:

- No additional changes

## v0.9.0-B190905 (pre-release)

- Added support for matching an array of tag values. [#282](https://github.com/Microsoft/PSRule/issues/282)
- Updated `Get-PSRuleHelp` to include help within the current path by default. [#197](https://github.com/Microsoft/PSRule/issues/197)
- Fix can not serialize nested System.IO.DirectoryInfo property. [#281](https://github.com/Microsoft/PSRule/issues/281)
- Fix export of `Like` parameter for `Within` keyword. [#279](https://github.com/Microsoft/PSRule/issues/279)
- **Breaking change**: Added named baselines. This changes how baselines work. [#274](https://github.com/Microsoft/PSRule/issues/274)
  - Previously, baselines were specified as workspace options.
  - Now, baselines are a separate resource that can be individually used. Additionally:
    - Baselines can be packaged within module.
    - Modules can specify a default baseline in module manifest.
    - Target binding options (`Binding`) are now part of baselines.
  - The previous `baseline` options property has been renamed to `rule`.
  - The previous `configuration` property is now a top level option.
  - See [about_PSRule_Baseline](docs/concepts/PSRule/en-US/about_PSRule_Baseline.md) for more information.

## v0.9.0-B190819 (pre-release)

- Added support for a wildcard match using the `Within` keyword. [#272](https://github.com/Microsoft/PSRule/issues/272)
- Added rule info display name. [#276](https://github.com/Microsoft/PSRule/issues/276)
- Fix ModuleName not displayed in Get-PSRuleHelp list. [#275](https://github.com/Microsoft/PSRule/issues/275)

## v0.9.0-B190810 (pre-release)

- Improve feedback of parsing errors. [#185](https://github.com/Microsoft/PSRule/issues/185)
- Fix outcome reported when error or exception is raised. [#211](https://github.com/Microsoft/PSRule/issues/211)

## v0.8.0

What's changed since v0.7.0:

- General improvements:
  - PSRule options are now displayed as YAML instead of a complex object. [#233](https://github.com/Microsoft/PSRule/issues/233)
  - Add detection for improper keyword use. [#203](https://github.com/Microsoft/PSRule/issues/203)
  - Automatically load rule modules. [#218](https://github.com/Microsoft/PSRule/issues/218)
  - Added support for debug messages and `Write-Debug` in rule definitions. [#146](https://github.com/Microsoft/PSRule/issues/146)
  - Added `Logging.LimitDebug` and `Logging.LimitVerbose` options to limit logging to named scopes. [#235](https://github.com/Microsoft/PSRule/issues/235)
- Engine features:
  - Added per object reason for failing rules. [#200](https://github.com/Microsoft/PSRule/issues/200)
    - Keywords `Exists`, `Match`, `Within` and `TypeOf` automatically add a reason when they fail.
    - Custom reason can be set for keywords `Exists`, `Match`, `Within` and `TypeOf` with `-Reason`.
    - Added `Reason` keyword to add to reason for custom logic.
    - Added wide output display for `Invoke-PSRule` which include the reason why rule failed.
      - To use wide output use the `-OutputFormat Wide` parameter.
    - Renamed `-Message` parameter to `-Text` on the `Recommend` keyword.
      - The `-Message` is an alias of `-Text` and will be deprecated in the future.
  - Added assertion helper `$Assert` for extensibility. [#250](https://github.com/Microsoft/PSRule/issues/250)
    - Add built-in assertions for `HasField`, `HasFieldValue` and `NullOrEmpty`.
    - Add JSON schema assertion method `JsonSchema`. [#42](https://github.com/Microsoft/PSRule/issues/42)
- Bug fixes:
  - Fix rule synopsis comment capture. [#214](https://github.com/Microsoft/PSRule/issues/214)
  - Fix YAML options file discovery issue in dotted directory. [#232](https://github.com/Microsoft/PSRule/issues/232)
  - Fix comparison of wrapped types and null with `Within`. [#237](https://github.com/Microsoft/PSRule/issues/237)
- **Breaking change**: Use rule references consistent with cmdlet fully qualified syntax. [#217](https://github.com/Microsoft/PSRule/issues/217)
  - Rule names have to be unique within the current execution path or within a module.
    - Previously rule names only had to be unique within a single file.
  - Previously the `filename.rule.ps1/RuleName` was required to reference rules across files.
    - This is no longer required because rule names are unique.
  - You can reference a rule from a loaded module by using the syntax `ModuleName\RuleName`.

What's changed since pre-release v0.8.0-B190806:

- Fix export of assertion helper variable `$Assert`. [#262](https://github.com/Microsoft/PSRule/issues/262)

## v0.8.0-B190806 (pre-release)

- Fix module reloading with different versions. [#254](https://github.com/Microsoft/PSRule/issues/254)
- Fix not finding rules in current path by default. [#256](https://github.com/Microsoft/PSRule/issues/256)
- Fix rule synopsis comment capture. [#214](https://github.com/Microsoft/PSRule/issues/214)

## v0.8.0-B190742 (pre-release)

- Fix inconsistent handling of `$PWD`. [#249](https://github.com/Microsoft/PSRule/issues/249)
- Add detection for improper keyword use. [#203](https://github.com/Microsoft/PSRule/issues/203)
- Automatically load rule modules. [#218](https://github.com/Microsoft/PSRule/issues/218)
- Added assertion helper `$Assert` for extensibility. [#250](https://github.com/Microsoft/PSRule/issues/250)
  - Add built-in assertions for `HasField`, `HasFieldValue` and `NullOrEmpty`.
  - Add JSON schema assertion method `JsonSchema`. [#42](https://github.com/Microsoft/PSRule/issues/42)
- **Breaking change**: Use rule references consistent with cmdlet fully qualified syntax. [#217](https://github.com/Microsoft/PSRule/issues/217)
  - Rule names have to be unique within the current execution path or within a module.
    - Previously rule names only had to be unique within a single file.
  - Previously the `filename.rule.ps1/RuleName` was required to reference rules across files.
    - This is no longer required because rule names are unique.
  - You can reference a rule from a loaded module by using the syntax `ModuleName\RuleName`.

## v0.8.0-B190716 (pre-release)

- Added per object reason for failing rules. [#200](https://github.com/Microsoft/PSRule/issues/200)
  - The keywords `Exists`, `Match`, `Within` and `TypeOf` automatically add a reason when they fail.
  - Added `-Reason` parameter to `Exists`, `Match`, `Within` and `TypeOf` keywords to allow a custom reason to be set.
  - Added `Reason` keyword to add to reason for custom logic.
  - Added wide output display for `Invoke-PSRule` which include the reason why rule failed.
    - To use wide output use the `-OutputFormat Wide` parameter.
  - Renamed `-Message` parameter to `-Text` on the `Recommend` keyword.
    - The `-Message` is an alias of `-Text` and will be deprecated in the future.

## v0.8.0-B190708 (pre-release)

- Fix YAML options file discovery issue in dotted directory. [#232](https://github.com/Microsoft/PSRule/issues/232)
- Fix comparison of wrapped types and null with `Within`. [#237](https://github.com/Microsoft/PSRule/issues/237)
- PSRule options are now displayed as YAML instead of a complex object. [#233](https://github.com/Microsoft/PSRule/issues/233)
- Added support for debug messages and `Write-Debug` in rule definitions. [#146](https://github.com/Microsoft/PSRule/issues/146)
- Added `Logging.LimitDebug` and `Logging.LimitVerbose` options to limit logging to named scopes. [#235](https://github.com/Microsoft/PSRule/issues/235)

## v0.7.0

What's changed since v0.6.0:

- Fix reading nested arrays from JSON input. [#223](https://github.com/Microsoft/PSRule/issues/223)
- Fix comparison of non-string types with `Within`. [#226](https://github.com/Microsoft/PSRule/issues/226)
- Fix circular rule dependency issue. [#190](https://github.com/Microsoft/PSRule/issues/190)
- Fix rule `DependsOn` parameter allows null. [#191](https://github.com/Microsoft/PSRule/issues/191)
- Fix error message when attempting to use the rule keyword in a rule definition. [#189](https://github.com/Microsoft/PSRule/issues/189)
- Fix TargetName binding when TargetName or Name property is null. [#202](https://github.com/Microsoft/PSRule/issues/202)
- Fix handling of non-boolean results in rules. Rule is failed with more specific error message. [#187](https://github.com/Microsoft/PSRule/issues/187) [#224](https://github.com/Microsoft/PSRule/issues/224)
- Include .ps1 files that are specified directly with `-Path`, instead of only .Rule.ps1 files. [#182](https://github.com/Microsoft/PSRule/issues/182)
  - Improved warning message displayed when no Rule.ps1 files are founds.
- Added support for `Invoke-PSRule` to return CSV formatted results. [#169](https://github.com/Microsoft/PSRule/issues/169)
  - To generate CSV results use the `-OutputFormat Csv` parameter.
  - Added `Output.Path` option to allow output to be saved directly to file.
  - Added `Output.Encoding` option configure encoding used to write to file.
  - By default, UTF-8 encoding without BOM is used.
  - `Invoke-PSRule` cmdlet also provides a parameter `-OutputPath` to write results to file.
- Reordered cmdlet parameters to improve usage of frequently used parameters. [#175](https://github.com/Microsoft/PSRule/issues/175)
  - `-Module` parameter will tab-complete with imported rule modules.
- Added culture support for PowerShell informational messages. [#158](https://github.com/Microsoft/PSRule/issues/158)
  - A new `$LocalizedData` variable can be used within rule definitions.
- Added `-Not` switch to `Within` and `Match` keywords to allow negative comparison. [#208](https://github.com/Microsoft/PSRule/issues/208)
- Improve discovery of rule tags. [#209](https://github.com/Microsoft/PSRule/issues/209)
  - Add wide format `-OutputFormat Wide` to `Get-PSRule` to allow output of rule tags.
- **Breaking change**: Changed rule filtering by tag to be case-insensitive. [#204](https://github.com/Microsoft/PSRule/issues/204)
  - Previously tag value was case-sensitive, however this is confusing since PowerShell is case-insensitive by default.
- **Breaking change**: Rule time is recorded in milliseconds instead of seconds. [#192](https://github.com/Microsoft/PSRule/issues/192)

What's changed since pre-release v0.7.0-B190664:

- No additional changes.

## v0.7.0-B190664 (pre-release)

- Fix reading nested arrays from JSON input. [#223](https://github.com/Microsoft/PSRule/issues/223)
- Fix comparison of non-string types with `Within`. [#226](https://github.com/Microsoft/PSRule/issues/226)
- Improve handling of null rule result. [#224](https://github.com/Microsoft/PSRule/issues/224)

## v0.7.0-B190652 (pre-release)

- Fix TargetName binding when TargetName or Name property is null. [#202](https://github.com/Microsoft/PSRule/issues/202)
- Add culture support for PowerShell informational messages. [#158](https://github.com/Microsoft/PSRule/issues/158)
  - A new `$LocalizedData` variable can be used within rule definitions.
- Add `-Not` switch to `Within` and `Match` keywords to allow negative comparison. [#208](https://github.com/Microsoft/PSRule/issues/208)
- Improve discovery of rule tags. [#209](https://github.com/Microsoft/PSRule/issues/209)
  - Add wide format `-OutputFormat Wide` to `Get-PSRule` to allow output of rule tags.
- **Breaking change**: Change rule filtering by tag to be case-insensitive. [#204](https://github.com/Microsoft/PSRule/issues/204)
  - Previously tag value was case-sensitive, however this is confusing since PowerShell is case-insensitive by default.

## v0.7.0-B190633 (pre-release)

- Fix circular rule dependency issue. [#190](https://github.com/Microsoft/PSRule/issues/190)
- Fix rule `DependsOn` parameter allows null. [#191](https://github.com/Microsoft/PSRule/issues/191)
- Fix error message when attempting to use the rule keyword in a rule definition. [#189](https://github.com/Microsoft/PSRule/issues/189)
- **Breaking change**: Rule time is recorded in milliseconds instead of seconds. [#192](https://github.com/Microsoft/PSRule/issues/192)

## v0.7.0-B190624 (pre-release)

- Fix handling of non-boolean results in rules. Rule is failed with more specific error message. [#187](https://github.com/Microsoft/PSRule/issues/187)
- Include .ps1 files that are specified directly with `-Path`, instead of only .rule.ps1 files. [#182](https://github.com/Microsoft/PSRule/issues/182)
  - Improved warning message displayed when no Rule.ps1 files are founds.

## v0.7.0-B190613 (pre-release)

- Added support for `Invoke-PSRule` to return CSV formatted results. [#169](https://github.com/Microsoft/PSRule/issues/169)
  - To generate CSV results use the `-OutputFormat Csv` parameter.
  - Added `Output.Path` option to allow output to be saved directly to file.
  - Added `Output.Encoding` option configure encoding used to write to file.
  - By default, UTF-8 encoding without BOM is used.
  - `Invoke-PSRule` cmdlet also provides a parameter `-OutputPath` to write results to file.
- Reordered cmdlet parameters to improve usage of frequently used parameters. [#175](https://github.com/Microsoft/PSRule/issues/175)
  - `-Module` parameter will tab-complete with imported rule modules.

## v0.6.0

What's changed since v0.5.0:

- Fix operation is not supported on this platform failure. [#152](https://github.com/Microsoft/PSRule/issues/152)
- Fix FullName cannot be found on this object error. [#149](https://github.com/Microsoft/PSRule/issues/149)
- Fix discovery of rules within paths that contain spaces fails. [#168](https://github.com/Microsoft/PSRule/issues/168)
- Added rule documentation, which allows additional rule information to be stored in markdown files. [#157](https://github.com/Microsoft/PSRule/issues/157)
  - Rule documentation also adds culture support. [#18](https://github.com/Microsoft/PSRule/issues/18)
  - Rule documentation can be accessed like help with the `Get-PSRuleHelp` cmdlet.
- Added annotations, which are non-indexed metadata stored in rule documentation. [#148](https://github.com/Microsoft/PSRule/issues/148)
  - Annotations can contain a link to online version of the documentation. [#147](https://github.com/Microsoft/PSRule/issues/147)
- **Important change**: Changed `Hint` keyword to `Recommend` to align with rule documentation. [#165](https://github.com/Microsoft/PSRule/issues/165)
  - Use of `Hint` keyword is deprecated and will be removed in a future release. Currently `Hint` is aliased to `Recommend` for compatibility.
- **Breaking change**: Changed rule properties to align with rule documentation. [#164](https://github.com/Microsoft/PSRule/issues/164)
  - Rule `Synopsis`, is a brief summary of the rule and `Description` is a detailed purpose of the rule.
  - `Description:` metadata keyword used in comment help is now `Synopsis:`, use of `Description:` will set synopsis. Description metadata keyword is deprecated and will be removed in a future update.
  - Output property `Message` on rule results is now `Recommendation`.

What's changed since pre-release v0.6.0-B190627:

- Fix discovery of rules within paths that contain spaces fails. [#168](https://github.com/Microsoft/PSRule/issues/168)
- Fix exporting of `Recommend` keyword and `Hint` alias. [#171](https://github.com/Microsoft/PSRule/issues/171)

## v0.6.0-B190627 (pre-release)

- **Important change**: Changed `Hint` keyword to `Recommend` to align with rule documentation. [#165](https://github.com/Microsoft/PSRule/issues/165)
  - Use of `Hint` keyword is deprecated and will be removed in a future release. Currently `Hint` is aliased to `Recommend` for compatibility.
- **Breaking change**: Changed rule properties to align with rule documentation. [#164](https://github.com/Microsoft/PSRule/issues/164)
  - Rule `Synopsis`, is a brief summary of the rule and `Description` is a detailed purpose of the rule.
  - `Description:` metadata keyword used in comment help is now `Synopsis:`, use of `Description:` will set synopsis. Description metadata keyword is deprecated and will be removed in a future update.
  - Output property `Message` on rule results is now `Recommendation`.

## v0.6.0-B190614 (pre-release)

- Added rule documentation, which allows additional rule information to be stored in markdown files. [#157](https://github.com/Microsoft/PSRule/issues/157)
  - Rule documentation also adds culture support. [#18](https://github.com/Microsoft/PSRule/issues/18)
  - Rule documentation can be accessed like help with the `Get-PSRuleHelp` cmdlet.
- Added annotations, which are non-indexed metadata stored in rule documentation. [#148](https://github.com/Microsoft/PSRule/issues/148)
  - Annotations can contain a link to online version of the documentation. [#147](https://github.com/Microsoft/PSRule/issues/147)

## v0.6.0-B190514 (pre-release)

- Fix operation is not supported on this platform failure. [#152](https://github.com/Microsoft/PSRule/issues/152)
- Fix FullName cannot be found on this object error. [#149](https://github.com/Microsoft/PSRule/issues/149)

## v0.5.0

What's changed since v0.4.0:

- Fix PSRule options schema usage of additionalProperties. [#136](https://github.com/Microsoft/PSRule/issues/136)
- Fix null reference exception when traversing null field. [#123](https://github.com/Microsoft/PSRule/issues/123)
- Fix missing help topics for options and variables. [#125](https://github.com/Microsoft/PSRule/issues/125)
- Improved handling of default YAML options file. [#137](https://github.com/Microsoft/PSRule/issues/137)
- Added support for `Invoke-PSRule` to return NUnit3 formatted results. [#129](https://github.com/Microsoft/PSRule/issues/129)
  - To generate NUnit3 results use the `-OutputFormat NUnit3` parameter.
- Added `Set-PSRuleOption` cmdlet to configure YAML options file. [#135](https://github.com/Microsoft/PSRule/issues/135)
- Added parameters to New-PSRuleOption to configure common options. [#134](https://github.com/Microsoft/PSRule/issues/134)
  - Additional parameters are an alternative to using a `-Option` hashtable.

What's changed since pre-release v0.5.0-B190423:

- Fix schema conformance of `-OutputFormat NUnit3` to NUnit report schema. [#141](https://github.com/Microsoft/PSRule/issues/141)
- Fix PSRule options schema usage of additionalProperties. [#136](https://github.com/Microsoft/PSRule/issues/136)

## v0.5.0-B190423 (pre-release)

- Added support for `Invoke-PSRule` to return NUnit3 formatted results. [#129](https://github.com/Microsoft/PSRule/issues/129)
  - To generate NUnit3 results use the `-OutputFormat NUnit3` parameter.
- Added `Set-PSRuleOption` cmdlet to configure YAML options file. [#135](https://github.com/Microsoft/PSRule/issues/135)
- Added parameters to New-PSRuleOption to configure common options. [#134](https://github.com/Microsoft/PSRule/issues/134)
  - Additional parameters are an alternative to using a `-Option` hashtable.
- Improved handling of default YAML options file. [#137](https://github.com/Microsoft/PSRule/issues/137)

## v0.5.0-B190405 (pre-release)

- Fix null reference exception when traversing null field. [#123](https://github.com/Microsoft/PSRule/issues/123)
- Fix missing help topics for options and variables. [#125](https://github.com/Microsoft/PSRule/issues/125)

## v0.4.0

What's changed since v0.3.0:

- Fix incorrect JSON de-serialization. [#109](https://github.com/Microsoft/PSRule/issues/109) [#111](https://github.com/Microsoft/PSRule/issues/111)
- Added support for using `-InputPath` instead of using `-InputObject` to handle serialized objects. [#106](https://github.com/Microsoft/PSRule/issues/106)
  - `-Format` is automatically detected for `.yaml`, `.yml` and `.json` file extensions.
- Added `-OutputFormat` parameter to serialize output from `Invoke-PSRule` as YAML or JSON. [#29](https://github.com/Microsoft/PSRule/issues/29)
- Added support for logging pass or fail outcomes to a data stream such as Error, Warning or Information. [#97](https://github.com/Microsoft/PSRule/issues/97)
- **Breaking change**: Deprecated usage of the `-TargetName` parameter on the `Hint` keyword has been removed. [#81](https://github.com/Microsoft/PSRule/issues/81)

What's changed since pre-release v0.4.0-B190328:

- No additional changes.

## v0.4.0-B190328 (pre-release)

- Fix summary is not correctly serialized with JSON or YAML output format. [#116](https://github.com/Microsoft/PSRule/issues/116)
- Fix missing properties on serialized YAML output. [#115](https://github.com/Microsoft/PSRule/issues/115)
- Fix incorrect property name case of YAML serialized results. [#114](https://github.com/Microsoft/PSRule/issues/114)

## v0.4.0-B190320 (pre-release)

- Fix incorrect JSON de-serialization of nested arrays. [#109](https://github.com/Microsoft/PSRule/issues/109)
- Fix incorrect JSON de-serialization of non-object arrays. [#111](https://github.com/Microsoft/PSRule/issues/111)

## v0.4.0-B190311 (pre-release)

- Added support for using `-InputPath` instead of using `-InputObject` to handle serialized objects. [#106](https://github.com/Microsoft/PSRule/issues/106)
  - `-Format` is automatically detected for `.yaml`, `.yml` and `.json` file extensions.
- Added `-OutputFormat` parameter to serialize output from `Invoke-PSRule`. [#29](https://github.com/Microsoft/PSRule/issues/29)
- Added support for logging pass or fail outcomes to a data stream such as Error, Warning or Information. [#97](https://github.com/Microsoft/PSRule/issues/97)
- **Breaking change**: Deprecated usage of the `-TargetName` parameter on the `Hint` keyword has been removed. [#81](https://github.com/Microsoft/PSRule/issues/81)

## v0.3.0

What's changed since v0.2.0:

- Added support for pipelining with `Exists`, `Within`, `Match` and `TypeOf` keywords [#90](https://github.com/Microsoft/PSRule/issues/90)
- Added support for packaging rules in modules [#16](https://github.com/Microsoft/PSRule/issues/16)
- Import objects from YAML or JSON format [#75](https://github.com/Microsoft/PSRule/issues/75)
  - Added support for input de-serialization from FileInfo objects [#95](https://github.com/Microsoft/PSRule/issues/95)
- Support nested TargetObjects [#77](https://github.com/Microsoft/PSRule/issues/77)
- Export variables to improve authoring experience [#83](https://github.com/Microsoft/PSRule/issues/83)
- Binding improvements:
  - Added object type binding and dynamic filtering for rules [#82](https://github.com/Microsoft/PSRule/issues/82)
  - Added support for indexed and quoted field names [#86](https://github.com/Microsoft/PSRule/issues/86)
  - Added support for case-sensitive binding operations [#87](https://github.com/Microsoft/PSRule/issues/87)
    - Binding ignores case by default. Set option `Binding.CaseSensitive` to `true` to enable case-sensitivity.
  - Support TargetName binding of nested properties [#71](https://github.com/Microsoft/PSRule/issues/71)
- Added online help links to keywords [#72](https://github.com/Microsoft/PSRule/issues/72)
- Added schema for PSRule options [#74](https://github.com/Microsoft/PSRule/issues/74)
- **Important change**: The `-TargetName` parameter of the `Hint` keyword has been deprecated [#81](https://github.com/Microsoft/PSRule/issues/81)
  - `-TargetName` parameter not longer sets the pipeline object _TargetName_ and generates a warning instead.
  - The `-TargetName` will be completely removed in **v0.4.0**, at which time using the parameter will generate an error.
- **Breaking change**: Changed parameter alias for `-Path` from `-f` to `-p` [#99](https://github.com/Microsoft/PSRule/issues/99)

What's changed since pre-release v0.3.0-B190231:

- Added support for input de-serialization from FileInfo objects [#95](https://github.com/Microsoft/PSRule/issues/95)
- **Breaking change**: Changed parameter alias for `-Path` from `-f` to `-p` [#99](https://github.com/Microsoft/PSRule/issues/99)

## v0.3.0-B190231 (pre-release)

- Added support for pipelining with `Exists`, `Within`, `Match` and `TypeOf` keywords [#90](https://github.com/Microsoft/PSRule/issues/90)
- Fix empty YAML object causes format de-serialize to fail [#92](https://github.com/Microsoft/PSRule/issues/92)

## v0.3.0-B190224 (pre-release)

- Export variables to improve authoring experience [#83](https://github.com/Microsoft/PSRule/issues/83)
- Added support for packaging rules in modules [#16](https://github.com/Microsoft/PSRule/issues/16)
- Added support for indexed and quoted field names [#86](https://github.com/Microsoft/PSRule/issues/86)
- Added object type binding and dynamic filtering for rules [#82](https://github.com/Microsoft/PSRule/issues/82)
- Added support for case-sensitive binding operations [#87](https://github.com/Microsoft/PSRule/issues/87)
  - Binding ignores case by default. Set option `Binding.CaseSensitive` to `true` to enable case-sensitivity.
- **Important change**: The `-TargetName` parameter of the `Hint` keyword has been deprecated [#81](https://github.com/Microsoft/PSRule/issues/81)
  - `-TargetName` parameter not longer sets the pipeline object _TargetName_ and generates a warning instead.
  - The `-TargetName` will be completely removed in **v0.4.0**, at which time using the parameter will generate an error.

## v0.3.0-B190208 (pre-release)

- Added online help links to keywords [#72](https://github.com/Microsoft/PSRule/issues/72)
- Added schema for PSRule options [#74](https://github.com/Microsoft/PSRule/issues/74)
- Import objects from YAML or JSON format [#75](https://github.com/Microsoft/PSRule/issues/75)
- Support TargetName binding of nested properties [#71](https://github.com/Microsoft/PSRule/issues/71)
- Support nested TargetObjects [#77](https://github.com/Microsoft/PSRule/issues/77)

## v0.2.0

What's changed since v0.1.0:

- Added support for cross-platform environments (Windows, Linux and macOS) [#49](https://github.com/Microsoft/PSRule/issues/49)
- Added support for nested field names with `Exists`, `Within` and `Match` keywords [#60](https://github.com/Microsoft/PSRule/issues/60)
- Added support for rule configuration using baselines [#17](https://github.com/Microsoft/PSRule/issues/17)
- Use rule description when hint message not set [#61](https://github.com/Microsoft/PSRule/issues/61)
- Allow objects to be suppressed by _TargetName_ for individual rules [#13](https://github.com/Microsoft/PSRule/issues/13)
- Allow binding of _TargetName_ to custom property [#44](https://github.com/Microsoft/PSRule/issues/44)
- Custom functions can be used to bind _TargetName_ [#44](https://github.com/Microsoft/PSRule/issues/44)
- Objects that are unable to bind a _TargetName_ will use a SHA1 object hash for _TargetName_ [#44](https://github.com/Microsoft/PSRule/issues/44)
- Added `Test-PSRuleTarget` command to return an overall `$True` or `$False` after evaluating rules for an object [#30](https://github.com/Microsoft/PSRule/issues/30)
- Improve reporting of inconclusive results and objects that are not processed by any rule [#46](https://github.com/Microsoft/PSRule/issues/46)
  - Inconclusive results and objects not processed will return a warning by default.
- Fix propagation of informational messages to host from rule scripts and definitions [#48](https://github.com/Microsoft/PSRule/issues/48)
- Fix Get-PSRule generates exception when no .rule.ps1 scripts exist in path [#53](https://github.com/Microsoft/PSRule/issues/53)
- Fix LocalizedData.PathNotFound warning when no .rule.ps1 scripts exist in path [#54](https://github.com/Microsoft/PSRule/issues/54)

What's changed since pre-release v0.2.0-B190121:

- No additional changes.

## v0.2.0-B190121 (pre-release)

- Added support for nested field names with `Exists`, `Within` and `Match` keywords [#60](https://github.com/Microsoft/PSRule/issues/60)
- Added support for rule configuration using baselines [#17](https://github.com/Microsoft/PSRule/issues/17)
- Use rule description when hint message not set [#61](https://github.com/Microsoft/PSRule/issues/61)

## v0.2.0-B190113 (pre-release)

- Fix Get-PSRule generates exception when no .rule.ps1 scripts exist in path [#53](https://github.com/Microsoft/PSRule/issues/53)
- Fix LocalizedData.PathNotFound warning when no .rule.ps1 scripts exist in path [#54](https://github.com/Microsoft/PSRule/issues/54)
- **Breaking change**: Renamed `Test-PSRule` cmdlet to `Test-PSRuleTarget` which aligns more closely to the verb-noun naming standard [#57](https://github.com/Microsoft/PSRule/issues/57)

## v0.2.0-B190105 (pre-release)

- Allow objects to be suppressed by _TargetName_ for individual rules [#13](https://github.com/Microsoft/PSRule/issues/13)
- Allow binding of _TargetName_ to custom property [#44](https://github.com/Microsoft/PSRule/issues/44)
- Custom functions can be used to bind _TargetName_ [#44](https://github.com/Microsoft/PSRule/issues/44)
- Objects that are unable to bind a _TargetName_ will use a SHA1 object hash for _TargetName_ [#44](https://github.com/Microsoft/PSRule/issues/44)
- Added `Test-PSRule` command to return an overall `$True` or `$False` after evaluating rules for an object [#30](https://github.com/Microsoft/PSRule/issues/30)
- Improve reporting of inconclusive results and objects that are not processed by any rule [#46](https://github.com/Microsoft/PSRule/issues/46)
  - Inconclusive results and objects not processed will return a warning by default.
- Fix propagation of informational messages to host from rule scripts and definitions [#48](https://github.com/Microsoft/PSRule/issues/48)
- Added support for cross-platform environments (Windows, Linux and macOS) [#49](https://github.com/Microsoft/PSRule/issues/49)

## v0.1.0

- Initial release

What's changed since pre-release v0.1.0-B181235:

- Fix outcome filtering of summary results [#33](https://github.com/Microsoft/PSRule/issues/33)
- Fix target object counter in verbose logging [#35](https://github.com/Microsoft/PSRule/issues/35)
- Fix hashtable keys should be handled as fields [#36](https://github.com/Microsoft/PSRule/issues/36)

## v0.1.0-B181235 (pre-release)

- RuleId and RuleName are now independent. Rules are created with a name, and the RuleId is generated based on rule name and file name
  - Rules with the same name can exist and be cross linked with DependsOn, as long a the script file name is different
- Added `-Not` to `Exists` keyword
- Improved verbose logging of `Exists`, `AllOf`, `AnyOf` keywords and core engine
- **Breaking change**: Renamed outcome filtering parameters to align to type name and increase clarity
  - `Invoke-PSRule` has a `-Outcome` parameter instead of `-Status`
  - `-Outcome` supports values of `Pass`, `Fail`, `Error`, `None`, `Processed` and `All`

## v0.1.0-B181222 (pre-release)

- Added rule tags to results to enable grouping and sorting [#14](https://github.com/Microsoft/PSRule/issues/14)
- Added support to check for rule tag existence. Use `*` for tag value on `-Tag` parameter with `Invoke-PSRule` and `Get-PSRule`
- Added option to report rule summary using `-As` parameter of `Invoke-PSRule` [#12](https://github.com/Microsoft/PSRule/issues/12)

## v0.1.0-B181212 (pre-release)

- Initial pre-release.
