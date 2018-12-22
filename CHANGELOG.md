
## Unreleased

- Allow objects to be suppressed by TargetName for individual rules [#13](https://github.com/BernieWhite/PSRule/issues/13)
- Allow binding of TargetName to custom property [#44](https://github.com/BernieWhite/PSRule/issues/44)

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
