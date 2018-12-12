
## Unreleased

- **Breaking change** - Renamed outcome filtering parameters to align to type name and increase clarity
  - `Invoke-PSRule` has a `-Outcome` parameter instead of `-Status`
  - `-Outcome` supports values of `Pass`, `Fail`, `Error`, `None`, `Processed` and `All`

## v0.1.0-B181222

- Added rule tags to results to enable grouping and sorting [#14](https://github.com/BernieWhite/PSRule/issues/14)
- Added support to check for rule tag existence. Use `*` for tag value on `-Tag` parameter with `Invoke-PSRule` and `Get-PSRule`
- Added option to report rule summary using `-As` parameter of `Invoke-PSRule` [#12](https://github.com/BernieWhite/PSRule/issues/12)

## v0.1.0-B181212

- Initial pre-release
