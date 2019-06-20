
## Unreleased

- Fix circular rule dependency issue. [#190](https://github.com/BernieWhite/PSRule/issues/190)
- Fix rule `DependsOn` parameter allows null. [#191](https://github.com/BernieWhite/PSRule/issues/191)
- Fix error message when attempting to use the rule keyword in a rule definition. [#189](https://github.com/BernieWhite/PSRule/issues/189)
- **Breaking change**: Rule time is recorded in milliseconds instead of seconds. [#192](https://github.com/BernieWhite/PSRule/issues/192)

## v0.7.0-B190624 (pre-release)

- Fix handling of non-boolean results in rules. Rule is failed with more specific error message. [#187](https://github.com/BernieWhite/PSRule/issues/187)
- Include .ps1 files that are specified directly with `-Path`, instead of only .rule.ps1 files. [#182](https://github.com/BernieWhite/PSRule/issues/182)
  - Improved warning message displayed when no Rule.ps1 files are founds.

## v0.7.0-B190613 (pre-release)

- Added support for `Invoke-PSRule` to return CSV formatted results. [#169](https://github.com/BernieWhite/PSRule/issues/169)
  - To generate CSV results use the `-OutputFormat Csv` parameter.
  - Added `Output.Path` option to allow output to be saved directly to file.
  - Added `Output.Encoding` option configure encoding used to write to file.
  - By default, UTF-8 encoding without BOM is used.
  - `Invoke-PSRule` cmdlet also provides a parameter `-OutputPath` to write results to file.
- Reordered cmdlet parameters to improve usage of frequently used parameters. [#175](https://github.com/BernieWhite/PSRule/issues/175)
  - `-Module` parameter will tab-complete with imported rule modules.

## v0.6.0

What's changed since v0.5.0:

- Fix operation is not supported on this platform failure. [#152](https://github.com/BernieWhite/PSRule/issues/152)
- Fix FullName cannot be found on this object error. [#149](https://github.com/BernieWhite/PSRule/issues/149)
- Fix discovery of rules within paths that contain spaces fails. [#168](https://github.com/BernieWhite/PSRule/issues/168)
- Added rule documentation, which allows additional rule information to be stored in markdown files. [#157](https://github.com/BernieWhite/PSRule/issues/157)
  - Rule documentation also adds culture support. [#18](https://github.com/BernieWhite/PSRule/issues/18)
  - Rule documentation can be accessed like help with the `Get-PSRuleHelp` cmdlet.
- Added annotations, which are non-indexed metadata stored in rule documentation. [#148](https://github.com/BernieWhite/PSRule/issues/148)
  - Annotations can contain a link to online version of the documentation. [#147](https://github.com/BernieWhite/PSRule/issues/147)
- **Important change**: Changed `Hint` keyword to `Recommend` to align with rule documentation. [#165](https://github.com/BernieWhite/PSRule/issues/165)
  - Use of `Hint` keyword is deprecated and will be removed in a future release. Currently `Hint` is aliased to `Recommend` for compatibility.
- **Breaking change**: Changed rule properties to align with rule documentation. [#164](https://github.com/BernieWhite/PSRule/issues/164)
  - Rule `Synopsis`, is a brief summary of the rule and `Description` is a detailed purpose of the rule.
  - `Description:` metadata keyword used in comment help is now `Synopsis:`, use of `Description:` will set synopsis. Description metadata keyword is deprecated and will be removed in a future update.
  - Output property `Message` on rule results is now `Recommendation`.

What's changed since pre-release v0.6.0-B190627:

- Fix discovery of rules within paths that contain spaces fails. [#168](https://github.com/BernieWhite/PSRule/issues/168)
- Fix exporting of `Recommend` keyword and `Hint` alias. [#171](https://github.com/BernieWhite/PSRule/issues/171)

## v0.6.0-B190627 (pre-release)

- **Important change**: Changed `Hint` keyword to `Recommend` to align with rule documentation. [#165](https://github.com/BernieWhite/PSRule/issues/165)
  - Use of `Hint` keyword is deprecated and will be removed in a future release. Currently `Hint` is aliased to `Recommend` for compatibility.
- **Breaking change**: Changed rule properties to align with rule documentation. [#164](https://github.com/BernieWhite/PSRule/issues/164)
  - Rule `Synopsis`, is a brief summary of the rule and `Description` is a detailed purpose of the rule.
  - `Description:` metadata keyword used in comment help is now `Synopsis:`, use of `Description:` will set synopsis. Description metadata keyword is deprecated and will be removed in a future update.
  - Output property `Message` on rule results is now `Recommendation`.

## v0.6.0-B190614 (pre-release)

- Added rule documentation, which allows additional rule information to be stored in markdown files. [#157](https://github.com/BernieWhite/PSRule/issues/157)
  - Rule documentation also adds culture support. [#18](https://github.com/BernieWhite/PSRule/issues/18)
  - Rule documentation can be accessed like help with the `Get-PSRuleHelp` cmdlet.
- Added annotations, which are non-indexed metadata stored in rule documentation. [#148](https://github.com/BernieWhite/PSRule/issues/148)
  - Annotations can contain a link to online version of the documentation. [#147](https://github.com/BernieWhite/PSRule/issues/147)

## v0.6.0-B190514 (pre-release)

- Fix operation is not supported on this platform failure. [#152](https://github.com/BernieWhite/PSRule/issues/152)
- Fix FullName cannot be found on this object error. [#149](https://github.com/BernieWhite/PSRule/issues/149)

## v0.5.0

What's changed since v0.4.0:

- Fix PSRule options schema usage of additionalProperties. [#136](https://github.com/BernieWhite/PSRule/issues/136)
- Fix null reference exception when traversing null field. [#123](https://github.com/BernieWhite/PSRule/issues/123)
- Fix missing help topics for options and variables. [#125](https://github.com/BernieWhite/PSRule/issues/125)
- Improved handling of default YAML options file. [#137](https://github.com/BernieWhite/PSRule/issues/137)
- Added support for `Invoke-PSRule` to return NUnit3 formatted results. [#129](https://github.com/BernieWhite/PSRule/issues/129)
  - To generate NUnit3 results use the `-OutputFormat NUnit3` parameter.
- Added `Set-PSRuleOption` cmdlet to configure YAML options file. [#135](https://github.com/BernieWhite/PSRule/issues/135)
- Added parameters to New-PSRuleOption to configure common options. [#134](https://github.com/BernieWhite/PSRule/issues/134)
  - Additional parameters are an alternative to using a `-Option` hashtable.

What's changed since pre-release v0.5.0-B190423:

- Fix schema conformance of `-OutputFormat NUnit3` to NUnit report schema. [#141](https://github.com/BernieWhite/PSRule/issues/141)
- Fix PSRule options schema usage of additionalProperties. [#136](https://github.com/BernieWhite/PSRule/issues/136)

## v0.5.0-B190423 (pre-release)

- Added support for `Invoke-PSRule` to return NUnit3 formatted results. [#129](https://github.com/BernieWhite/PSRule/issues/129)
  - To generate NUnit3 results use the `-OutputFormat NUnit3` parameter.
- Added `Set-PSRuleOption` cmdlet to configure YAML options file. [#135](https://github.com/BernieWhite/PSRule/issues/135)
- Added parameters to New-PSRuleOption to configure common options. [#134](https://github.com/BernieWhite/PSRule/issues/134)
  - Additional parameters are an alternative to using a `-Option` hashtable.
- Improved handling of default YAML options file. [#137](https://github.com/BernieWhite/PSRule/issues/137)

## v0.5.0-B190405 (pre-release)

- Fix null reference exception when traversing null field. [#123](https://github.com/BernieWhite/PSRule/issues/123)
- Fix missing help topics for options and variables. [#125](https://github.com/BernieWhite/PSRule/issues/125)

## v0.4.0

What's changed since v0.3.0:

- Fix incorrect JSON de-serialization. [#109](https://github.com/BernieWhite/PSRule/issues/109) [#111](https://github.com/BernieWhite/PSRule/issues/111)
- Added support for using `-InputPath` instead of using `-InputObject` to handle serialized objects. [#106](https://github.com/BernieWhite/PSRule/issues/106)
  - `-Format` is automatically detected for `.yaml`, `.yml` and `.json` file extensions.
- Added `-OutputFormat` parameter to serialize output from `Invoke-PSRule` as YAML or JSON. [#29](https://github.com/BernieWhite/PSRule/issues/29)
- Added support for logging pass or fail outcomes to a data stream such as Error, Warning or Information. [#97](https://github.com/BernieWhite/PSRule/issues/97)
- **Breaking change**: Deprecated usage of the `-TargetName` parameter on the `Hint` keyword has been removed. [#81](https://github.com/BernieWhite/PSRule/issues/81)

What's changed since pre-release v0.4.0-B190328:

- No additional changes

## v0.4.0-B190328 (pre-release)

- Fix summary is not correctly serialized with JSON or YAML output format. [#116](https://github.com/BernieWhite/PSRule/issues/116)
- Fix missing properties on serialized YAML output. [#115](https://github.com/BernieWhite/PSRule/issues/115)
- Fix incorrect property name case of YAML serialized results. [#114](https://github.com/BernieWhite/PSRule/issues/114)

## v0.4.0-B190320 (pre-release)

- Fix incorrect JSON de-serialization of nested arrays. [#109](https://github.com/BernieWhite/PSRule/issues/109)
- Fix incorrect JSON de-serialization of non-object arrays. [#111](https://github.com/BernieWhite/PSRule/issues/111)

## v0.4.0-B190311 (pre-release)

- Added support for using `-InputPath` instead of using `-InputObject` to handle serialized objects. [#106](https://github.com/BernieWhite/PSRule/issues/106)
  - `-Format` is automatically detected for `.yaml`, `.yml` and `.json` file extensions.
- Added `-OutputFormat` parameter to serialize output from `Invoke-PSRule`. [#29](https://github.com/BernieWhite/PSRule/issues/29)
- Added support for logging pass or fail outcomes to a data stream such as Error, Warning or Information. [#97](https://github.com/BernieWhite/PSRule/issues/97)
- **Breaking change**: Deprecated usage of the `-TargetName` parameter on the `Hint` keyword has been removed. [#81](https://github.com/BernieWhite/PSRule/issues/81)

## v0.3.0

What's changed since v0.2.0:

- Added support for pipelining with `Exists`, `Within`, `Match` and `TypeOf` keywords [#90](https://github.com/BernieWhite/PSRule/issues/90)
- Added support for packaging rules in modules [#16](https://github.com/BernieWhite/PSRule/issues/16)
- Import objects from YAML or JSON format [#75](https://github.com/BernieWhite/PSRule/issues/75)
  - Added support for input de-serialization from FileInfo objects [#95](https://github.com/BernieWhite/PSRule/issues/95)
- Support nested TargetObjects [#77](https://github.com/BernieWhite/PSRule/issues/77)
- Export variables to improve authoring experience [#83](https://github.com/BernieWhite/PSRule/issues/83)
- Binding improvements:
  - Added object type binding and dynamic filtering for rules [#82](https://github.com/BernieWhite/PSRule/issues/82)
  - Added support for indexed and quoted field names [#86](https://github.com/BernieWhite/PSRule/issues/86)
  - Added support for case-sensitive binding operations [#87](https://github.com/BernieWhite/PSRule/issues/87)
    - Binding ignores case by default. Set option `Binding.CaseSensitive` to `true` to enable case-sensitivity.
  - Support TargetName binding of nested properties [#71](https://github.com/BernieWhite/PSRule/issues/71)
- Added online help links to keywords [#72](https://github.com/BernieWhite/PSRule/issues/72)
- Added schema for PSRule options [#74](https://github.com/BernieWhite/PSRule/issues/74)
- **Important change**: The `-TargetName` parameter of the `Hint` keyword has been deprecated [#81](https://github.com/BernieWhite/PSRule/issues/81)
  - `-TargetName` parameter not longer sets the pipeline object _TargetName_ and generates a warning instead.
  - The `-TargetName` will be completely removed in **v0.4.0**, at which time using the parameter will generate an error.
- **Breaking change**: Changed parameter alias for `-Path` from `-f` to `-p` [#99](https://github.com/BernieWhite/PSRule/issues/99)

What's changed since pre-release v0.3.0-B190231:

- Added support for input de-serialization from FileInfo objects [#95](https://github.com/BernieWhite/PSRule/issues/95)
- **Breaking change**: Changed parameter alias for `-Path` from `-f` to `-p` [#99](https://github.com/BernieWhite/PSRule/issues/99)

## v0.3.0-B190231 (pre-release)

- Added support for pipelining with `Exists`, `Within`, `Match` and `TypeOf` keywords [#90](https://github.com/BernieWhite/PSRule/issues/90)
- Fix empty YAML object causes format de-serialize to fail [#92](https://github.com/BernieWhite/PSRule/issues/92)

## v0.3.0-B190224 (pre-release)

- Export variables to improve authoring experience [#83](https://github.com/BernieWhite/PSRule/issues/83)
- Added support for packaging rules in modules [#16](https://github.com/BernieWhite/PSRule/issues/16)
- Added support for indexed and quoted field names [#86](https://github.com/BernieWhite/PSRule/issues/86)
- Added object type binding and dynamic filtering for rules [#82](https://github.com/BernieWhite/PSRule/issues/82)
- Added support for case-sensitive binding operations [#87](https://github.com/BernieWhite/PSRule/issues/87)
  - Binding ignores case by default. Set option `Binding.CaseSensitive` to `true` to enable case-sensitivity.
- **Important change**: The `-TargetName` parameter of the `Hint` keyword has been deprecated [#81](https://github.com/BernieWhite/PSRule/issues/81)
  - `-TargetName` parameter not longer sets the pipeline object _TargetName_ and generates a warning instead.
  - The `-TargetName` will be completely removed in **v0.4.0**, at which time using the parameter will generate an error.

## v0.3.0-B190208 (pre-release)

- Added online help links to keywords [#72](https://github.com/BernieWhite/PSRule/issues/72)
- Added schema for PSRule options [#74](https://github.com/BernieWhite/PSRule/issues/74)
- Import objects from YAML or JSON format [#75](https://github.com/BernieWhite/PSRule/issues/75)
- Support TargetName binding of nested properties [#71](https://github.com/BernieWhite/PSRule/issues/71)
- Support nested TargetObjects [#77](https://github.com/BernieWhite/PSRule/issues/77)

## v0.2.0

What's changed since v0.1.0:

- Added support for cross-platform environments (Windows, Linux and macOS) [#49](https://github.com/BernieWhite/PSRule/issues/49)
- Added support for nested field names with `Exists`, `Within` and `Match` keywords [#60](https://github.com/BernieWhite/PSRule/issues/60)
- Added support for rule configuration using baselines [#17](https://github.com/BernieWhite/PSRule/issues/17)
- Use rule description when hint message not set [#61](https://github.com/BernieWhite/PSRule/issues/61)
- Allow objects to be suppressed by _TargetName_ for individual rules [#13](https://github.com/BernieWhite/PSRule/issues/13)
- Allow binding of _TargetName_ to custom property [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Custom functions can be used to bind _TargetName_ [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Objects that are unable to bind a _TargetName_ will use a SHA1 object hash for _TargetName_ [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Added `Test-PSRuleTarget` command to return an overall `$True` or `$False` after evaluating rules for an object [#30](https://github.com/BernieWhite/PSRule/issues/30)
- Improve reporting of inconclusive results and objects that are not processed by any rule [#46](https://github.com/BernieWhite/PSRule/issues/46)
  - Inconclusive results and objects not processed will return a warning by default.
- Fix propagation of informational messages to host from rule scripts and definitions [#48](https://github.com/BernieWhite/PSRule/issues/48)
- Fix Get-PSRule generates exception when no .rule.ps1 scripts exist in path [#53](https://github.com/BernieWhite/PSRule/issues/53)
- Fix LocalizedData.PathNotFound warning when no .rule.ps1 scripts exist in path [#54](https://github.com/BernieWhite/PSRule/issues/54)

What's changed since pre-release v0.2.0-B190121:

- No additional changes

## v0.2.0-B190121 (pre-release)

- Added support for nested field names with `Exists`, `Within` and `Match` keywords [#60](https://github.com/BernieWhite/PSRule/issues/60)
- Added support for rule configuration using baselines [#17](https://github.com/BernieWhite/PSRule/issues/17)
- Use rule description when hint message not set [#61](https://github.com/BernieWhite/PSRule/issues/61)

## v0.2.0-B190113 (pre-release)

- Fix Get-PSRule generates exception when no .rule.ps1 scripts exist in path [#53](https://github.com/BernieWhite/PSRule/issues/53)
- Fix LocalizedData.PathNotFound warning when no .rule.ps1 scripts exist in path [#54](https://github.com/BernieWhite/PSRule/issues/54)
- **Breaking change**: Renamed `Test-PSRule` cmdlet to `Test-PSRuleTarget` which aligns more closely to the verb-noun naming standard [#57](https://github.com/BernieWhite/PSRule/issues/57)

## v0.2.0-B190105 (pre-release)

- Allow objects to be suppressed by _TargetName_ for individual rules [#13](https://github.com/BernieWhite/PSRule/issues/13)
- Allow binding of _TargetName_ to custom property [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Custom functions can be used to bind _TargetName_ [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Objects that are unable to bind a _TargetName_ will use a SHA1 object hash for _TargetName_ [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Added `Test-PSRule` command to return an overall `$True` or `$False` after evaluating rules for an object [#30](https://github.com/BernieWhite/PSRule/issues/30)
- Improve reporting of inconclusive results and objects that are not processed by any rule [#46](https://github.com/BernieWhite/PSRule/issues/46)
  - Inconclusive results and objects not processed will return a warning by default.
- Fix propagation of informational messages to host from rule scripts and definitions [#48](https://github.com/BernieWhite/PSRule/issues/48)
- Added support for cross-platform environments (Windows, Linux and macOS) [#49](https://github.com/BernieWhite/PSRule/issues/49)

## v0.1.0

- Initial release

What's changed since pre-release v0.1.0-B181235:

- Fix outcome filtering of summary results [#33](https://github.com/BernieWhite/PSRule/issues/33)
- Fix target object counter in verbose logging [#35](https://github.com/BernieWhite/PSRule/issues/35)
- Fix hashtable keys should be handled as fields [#36](https://github.com/BernieWhite/PSRule/issues/36)

## v0.1.0-B181235 (pre-release)

- RuleId and RuleName are now independent. Rules are created with a name, and the RuleId is generated based on rule name and file name
  - Rules with the same name can exist and be cross linked with DependsOn, as long a the script file name is different
- Added `-Not` to `Exists` keyword
- Improved verbose logging of `Exists`, `AllOf`, `AnyOf` keywords and core engine
- **Breaking change**: Renamed outcome filtering parameters to align to type name and increase clarity
  - `Invoke-PSRule` has a `-Outcome` parameter instead of `-Status`
  - `-Outcome` supports values of `Pass`, `Fail`, `Error`, `None`, `Processed` and `All`

## v0.1.0-B181222 (pre-release)

- Added rule tags to results to enable grouping and sorting [#14](https://github.com/BernieWhite/PSRule/issues/14)
- Added support to check for rule tag existence. Use `*` for tag value on `-Tag` parameter with `Invoke-PSRule` and `Get-PSRule`
- Added option to report rule summary using `-As` parameter of `Invoke-PSRule` [#12](https://github.com/BernieWhite/PSRule/issues/12)

## v0.1.0-B181212 (pre-release)

- Initial pre-release
