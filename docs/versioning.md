---
author: BernieWhite
discussion: false
---

# Changes and versioning

PSRule uses [semantic versioning][1] to declare breaking changes.
The latest module version can be installed from the PowerShell Gallery.
For a list of module changes please see the [change log][2].

  [1]: https://semver.org/
  [2]: https://aka.ms/ps-rule/changelog

## Pre-releases

Pre-release module versions are created on major commits and can be installed from the PowerShell Gallery.
Module versions and change log details for pre-releases will be removed as stable releases are made available.

!!! Important
    Pre-release versions should be considered work in progress.
    These releases should not be used in production.
    We may introduce breaking changes between a pre-release as we work towards a stable version release.

## Experimental features

From time to time we may ship experiential features.
These features are generally marked experiential in the change log as these features ship.
Experimental features may ship in stable releases, however to use them you may need to:

- Enable or explicitly reference them.

!!! Important
    Experimental features should be considered work in progress.
    These features may be incomplete and should not be used in production.
    We may introduce breaking changes for experimental features as we work towards a general release for the feature.

## Reporting bugs

If you experience an issue with an pre-release or experimental feature please let us know by logging an issue as a [bug][3].

  [3]: https://github.com/microsoft/PSRule/issues
