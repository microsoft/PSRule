# PSRule

A PowerShell module with commands to validate objects on the pipeline.

| AppVeyor (Windows) | Codecov (Windows) |
| --- | --- |
| [![av-image][]][av-site] | [![cc-image][]][cc-site] |

[av-image]: https://ci.appveyor.com/api/projects/status/pl7tu7ktue388n7s
[av-site]: https://ci.appveyor.com/project/BernieWhite/PSRule
[cc-image]: https://codecov.io/gh/BernieWhite/PSRule/branch/master/graph/badge.svg
[cc-site]: https://codecov.io/gh/BernieWhite/PSRule

## Disclaimer

This project is to be considered a **proof-of-concept** and **not a supported Microsoft product**.

## Modules

The following modules are included in this repository.

| Module     | Description | Latest version |
| ------     | ----------- | -------------- |
| PSRule     | A PowerShell rules engine. | Unreleased |

## Getting started

### Prerequsits

- Windows PowerShell 5.1 or PowerShell Core 6.0

## Language reference

PSRule extends PowerShell with domain specific language (DSL) keywords and cmdlets.

### Keywords

The following language keywords are used by the `PSRule` module:

- [Rule](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#Rule) -
- [Exists](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#Exists) -
- [Match](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#Match) -
- [AnyOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#AnyOf) -
- [AllOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#AllOf) -
- [Within](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#Within) -
- [When](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#When) -

### Commands

The following commands exist in the `PSRule` module:

- [Invoke-RuleEngine](docs/commands/PSRule/en-US/Invoke-RuleEngine.md)

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0. Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments. For a list of module changes please see the [change log](CHANGELOG.md).

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[psg-psrule]: https://www.powershellgallery.com/packages/PSRule