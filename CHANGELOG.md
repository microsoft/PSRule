
## Unreleased

## v0.2.0-B190113 (pre-release)

- Fix Get-PSRule generates exception when no .rule.ps1 scripts exist in path [#53](https://github.com/BernieWhite/PSRule/issues/53)
- Fix LocalizedData.PathNotFound warning when no .rule.ps1 scripts exist in path [#54](https://github.com/BernieWhite/PSRule/issues/54)
- **Breaking change** - Renamed `Test-PSRule` cmdlet to `Test-PSRuleTarget` which aligns more closely to the verb-noun naming standard [#57](https://github.com/BernieWhite/PSRule/issues/57)

## v0.2.0-B190105 (pre-release)

- Allow objects to be suppressed by _TargetName_ for individual rules [#13](https://github.com/BernieWhite/PSRule/issues/13)
- Allow binding of _TargetName_ to custom property [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Custom functions can be used to bind _TargetName_ [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Objects that are unable to bind a _TargetName_ will use a SHA1 object hash for _TargetName_ [#44](https://github.com/BernieWhite/PSRule/issues/44)
- Add `Test-PSRule` command to return an overall `$True` or `$False` after evaluating rules for an object [#30](https://github.com/BernieWhite/PSRule/issues/30)
- Improve reporting of inconclusive results and objects that are not processed by any rule [#46](https://github.com/BernieWhite/PSRule/issues/46)
  - Inconclusive results and objects not processed will return a warning
- Fix propagation of informational messages to host from rule scripts and definitions [#48](https://github.com/BernieWhite/PSRule/issues/48)
- Add support for cross-platform environments (Windows, Linux, and macOS) [#49](https://github.com/BernieWhite/PSRule/issues/49)

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
- **Breaking change** - Renamed outcome filtering parameters to align to type name and increase clarity
  - `Invoke-PSRule` has a `-Outcome` parameter instead of `-Status`
  - `-Outcome` supports values of `Pass`, `Fail`, `Error`, `None`, `Processed` and `All`

## v0.1.0-B181222 (pre-release)

- Added rule tags to results to enable grouping and sorting [#14](https://github.com/BernieWhite/PSRule/issues/14)
- Added support to check for rule tag existence. Use `*` for tag value on `-Tag` parameter with `Invoke-PSRule` and `Get-PSRule`
- Added option to report rule summary using `-As` parameter of `Invoke-PSRule` [#12](https://github.com/BernieWhite/PSRule/issues/12)

## v0.1.0-B181212 (pre-release)

- Initial pre-release
