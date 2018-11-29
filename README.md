# PSRule

A PowerShell module with commands to validate objects on the pipeline.

![ci-badge]

## Disclaimer

This project is to be considered a **proof-of-concept** and **not a supported product**.

If you have any problems please check our GitHub [issues](https://github.com/BernieWhite/PSRule/issues) page. If you do not see your problem captured, please file a new issue and follow the provided template.

## Modules

## Getting the modules

You can download and install these PowerShell modules from the PowerShell Gallery.

| Module     | Description | Downloads / instructions |
| ------     | ----------- | ------------------------ |
| PSRule     | A PowerShell rules engine | Unreleased |

## Getting started

### Prerequisites

- Windows PowerShell 5.1 or PowerShell Core 6.0

## Language reference

PSRule extends PowerShell with domain specific language (DSL) keywords and cmdlets.

### Keywords

The following language keywords are used by the `PSRule` module:

- [Rule](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#rule) - A rule definition
- [Exists](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#exists) - Assert that a field or property must exist
- [Match](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#match) - Assert that the field must match any of the regular expressions
- [AnyOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#anyof) - Assert that any of the child expressions must be true
- [AllOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#allof) -Assert that all of the child expressions must be true
- [Within](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#within) - Assert that the field must match any of the values
- [TypeOf](docs/keywords/PSRule/en-US/about_PSRule_Keywords.md#typeof) - Assert that the object must be of a specific type

### Commands

The following commands exist in the `PSRule` module:

- [Invoke-PSRule](docs/commands/PSRule/en-US/Invoke-PSRule.md)
- [Get-PSRule](docs/commands/PSRule/en-US/Get-PSRule.md)
- [New-PSRuleOption](docs/commands/PSRule/en-US/New-PSRuleOption.md)

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0. Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments. For a list of module changes please see the [change log](CHANGELOG.md).

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[install]: docs/scenarios/install-instructions.md
[ci-badge]: https://bewhite.visualstudio.com/PSRule/_apis/build/status/PSRule-CI?branchName=master
[psg-psrule]: https://www.powershellgallery.com/packages/PSRule
