---
author: BernieWhite
discussion: false
---

# Changes and versioning

This article briefly covers the conventions/ terminology used for versioning and describing changes in PSRule.

## Semantic versioning

PSRule generally uses [semantic versioning][1] for each release **stable** and **pre-release**.
However not all platforms we publish to support semantic versioning.
Continue reading to understand how we use semantic versioning in PSRule.

You can read more about semantic versioning at [semver.org][1].

  [1]: https://semver.org/

For a list of changes please see the [change log][2].

  [2]: https://aka.ms/ps-rule/changelog

## Stable releases

Stable releases are released periodically and are considered production ready.
These releases are generally feature complete and have been tested by the PSRule team.

Each stable release uses a version number in the format `major.minor.patch`.
For example, `3.0.0`.

Generally we aim to fix any issues or bugs in the next stable release, however that is not always possible.
If you experience an issue with a stable release please let us know by logging an issue as a [bug][3].
Also, consider voting up (👍) an issue that is important to you to help us prioritize work.

To install a stable release see [Setting up PSRule][4].

  [3]: https://github.com/microsoft/PSRule/issues
  [4]: setup.md

## Pre-releases

Pre-release versions are created on major commits leading up to a stable release.

Each pre-release uses a version number in the format `major.minor.patch-prerelease`.
For example, `3.0.0-B0100`.

However, Visual Studio Marketplace does not support adding a pre-release identifier `-prerelease`.
Instead, a pre-release version is published in the format `nnnn.nnnn.nnnn` and marked as a pre-release.

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
