---
discussion: false
link_users: true
---

# Change log

See [upgrade notes][1] for helpful information when upgrading from previous versions.

  [1]: https://aka.ms/ps-rule/upgrade

**Experimental features**:

- Functions within YAML and JSON expressions can be used to perform manipulation prior to testing a condition.
  See [functions][3] for more information.
- Sub-selectors within YAML and JSON expressions can be used to filter rules and list properties.
  See [sub-selectors][4] for more information.
- Processing of changes files only within a pipeline.
  See [creating your pipeline][5] for more information.

  [3]: expressions/functions.md
  [4]: expressions/sub-selectors.md
  [5]: creating-your-pipeline.md#processing-changed-files-only

## Unreleased

## v3.0.0-B0486 (pre-release)

What's changed since pre-release v3.0.0-B0453:

- New features:
  - **Experimental**: Added support for target scanning in Visual Studio Code by @BernieWhite.
    [#2641](https://github.com/microsoft/PSRule/issues/2641)
    - To scan a single file, right-click on the file in explorer or an open editor tab and select `Run scan on path`.
    - To scan a folder, right-click on the folder in explorer and select `Run scan on path`.
  - Added support and by default error if rules or input is not found during a run by @BernieWhite.
    [#1778](https://github.com/microsoft/PSRule/issues/1778)
    - Added options for `Execution.NoMatchingRules`, `Execution.NoValidInput`, and `Execution.NoValidSources`.
- Engineering:
  - Added GitHub Actions support to CLI by @BernieWhite.
    [#2824](https://github.com/microsoft/PSRule/issues/2824)
  - Bump vscode engine to v1.99.1.
    [#2858](https://github.com/microsoft/PSRule/pull/2858)

## v3.0.0-B0453 (pre-release)

What's changed since pre-release v3.0.0-B0416:

- Engineering:
  - Bump System.Drawing.Common to v9.0.3.
    [#2808](https://github.com/microsoft/PSRule/pull/2808)
  - Bump NuGet.Protocol to v6.13.2.
    [#2788](https://github.com/microsoft/PSRule/pull/2788)
  - Bump vscode engine to v1.98.0.
    [#2796](https://github.com/microsoft/PSRule/pull/2796)
- Bug fixes:
  - Fixed custom emitter loaded too late by @BernieWhite.
    [#2775](https://github.com/microsoft/PSRule/issues/2775)
  - Fixed changed files includes excluded paths by @BernieWhite.
    [#1465](https://github.com/microsoft/PSRule/issues/1465)
  - Fixed branch ref for working with changes files only examples by @BernieWhite.
    [#2777](https://github.com/microsoft/PSRule/issues/2777)

## v3.0.0-B0416 (pre-release)

What's changed since pre-release v3.0.0-B0390:

- New features:
  - Added support dependency management in VSCode by @BernieWhite.
    [#2734](https://github.com/microsoft/PSRule/issues/2734)
    - Code lens in `ps-rule.lock.json` allows you to upgrade all or specific modules to the latest version.
    - The command `Upgrade dependency` allows you to upgrade all or specific modules to the latest version.
  - Added support for enabling/ disabling emitters by @BernieWhite.
    [#2752](https://github.com/microsoft/PSRule/issues/2752)
    - Emitters can be enabled or disabled by setting the `enabled` property on each format.
    - Additionally, the `formats` parameter/ input can be set on the command-line and CI to enable emitters for a run.
  - Added support for configuring replacement string for each format by @BernieWhite.
    [#2753](https://github.com/microsoft/PSRule/issues/2753)
    - Replacement strings allow common literal tokens to be replaced when processed by PSRule.
      i.e. `{{environment}}` replaced with `dev`.
    - All built-in emitters now support replacement strings, by configuring the `replace` property on each format.
- Bug fixes:
  - Fixed upgrade dependency could use pre-release version by @BernieWhite.
    [#2726](https://github.com/microsoft/PSRule/issues/2726)

## v3.0.0-B0390 (pre-release)

What's changed since pre-release v3.0.0-B0351:

- New features:
  - Simplify type conditions for selectors and suppression groups with 2025-01-01 API by @BernieWhite.
    [#2702](https://github.com/microsoft/PSRule/issues/2702)
    - A precondition `type` property has been added to selectors and suppression groups.
    - This simplifies type conditions that are common used in selectors and suppression groups.
    - To use this feature, set the `apiVersion` to `github.com/microsoft/PSRule/2025-01-01`.
  - Add support for declaring required capabilities in workspaces and modules by @BernieWhite.
    [#2707](https://github.com/microsoft/PSRule/issues/2707)
    - A module or workspace can declare required capabilities that must be supported by the runtime.
    - When a capability is not supported or disabled, the runtime will fail with a specific error.
    - This provides a way to ensure that rules execute consistently across environments.
- General improvements:
  - Added support for registering custom emitters by @BernieWhite.
    [#2681](https://github.com/microsoft/PSRule/issues/2681)
- Engineering:
  - Migrate samples into PSRule repository by @BernieWhite.
    [#2614](https://github.com/microsoft/PSRule/issues/2614)
- Bug fixes:
  - Fixed string formatting of semantic version and constraints by @BernieWhite.
    [#1828](https://github.com/microsoft/PSRule/issues/1828)
  - Fixed directory handling of input paths without trailing slash by @BernieWhite.
    [#1842](https://github.com/microsoft/PSRule/issues/1842)
  - Fixed duplicate reasons are reported for the same rule by @BernieWhite.
    [#2553](https://github.com/microsoft/PSRule/issues/2553)
  - Fixed JSON output format returns exception when no results are produced by @BernieWhite.
    [#1832](https://github.com/microsoft/PSRule/issues/1832)
  - Fixed path navigation with XML nodes by @BernieWhite.
    [#1518](https://github.com/microsoft/PSRule/issues/1518)
  - Fixed CLI output format argument not working by @BernieWhite.
    [#2699](https://github.com/microsoft/PSRule/issues/2699)

## v3.0.0-B0351 (pre-release)

What's changed since pre-release v3.0.0-B0342:

- General improvements:
  - Added an integrity hash to lock file by @BernieWhite.
    [#2664](https://github.com/microsoft/PSRule/issues/2664)
    - The lock file now includes an integrity hash to ensures the restored module matches originally added module.

## v3.0.0-B0342 (pre-release)

<!-- vscode:version 2024.12.2 -->

What's changed since pre-release v3.0.0-B0340:

- New features:
  - VSCode extension includes PSRule runtime by @BernieWhite.
    [#1755](https://github.com/microsoft/PSRule/issues/1755)
    - The PSRule runtime is bundled with the VSCode extension.
    - Separate installation of the PSRule PowerShell module is no longer required.
  - VSCode extension asks to automatically restore modules by @BernieWhite.
    [#2642](https://github.com/microsoft/PSRule/issues/2642)
    - When opening a workspace, the extension will ask to restore any modules from the lock file.
    - Alternatively, running the `PSRule: Restore modules` command manually will restore modules.

## v3.0.0-B0340 (pre-release)

What's changed since pre-release v3.0.0-B0315:

- New features:
  - VSCode extension set to use Microsoft verified publisher name by @BernieWhite.
    [#2636](https://github.com/microsoft/PSRule/issues/2636)
- General improvements:
  - Expose format options to emitters by @BernieWhite.
    [#1838](https://github.com/microsoft/PSRule/issues/1838)
  - Added support for overriding options path from the default in VSCode by @BernieWhite.
    [#2635](https://github.com/microsoft/PSRule/issues/2635)
- Engineering:
  - Migrated VSCode extension into PSRule repository by @BernieWhite.
    [#2615](https://github.com/microsoft/PSRule/issues/2615)
    - VSCode extension will now sit side-by-side with the other core PSRule components.
- Bug fixes:
  - Fixes path filtering of ignored files includes prefixed files by @BernieWhite.
    [#2624](https://github.com/microsoft/PSRule/issues/2624)

## v3.0.0-B0315 (pre-release)

What's changed since pre-release v3.0.0-B0275:

- New features:
  - Added support for overriding rule severity level by @BernieWhite.
    [#1180](https://github.com/microsoft/PSRule/issues/1180)
    - Baselines now accept a new `spec.overrides.level` property which configures severity level overrides.
    - Options now accept a new `overrides.level` properties which configures severity level overrides.
    - For example, a rule that generates an `Error` can be overridden to `Warning`.
- General improvements:
  - Automatically restore missing modules when running CLI by @BernieWhite.
    [#2552](https://github.com/microsoft/PSRule/issues/2552)
    - Modules are automatically restored unless `--no-restore` is used with the `run` command.
- Engineering:
  - Bump YamlDotNet to v16.2.0.
    [#2596](https://github.com/microsoft/PSRule/pull/2596)

## v3.0.0-B0275 (pre-release)

What's changed since pre-release v3.0.0-B0267:

- New features:
  - Allow CLI upgrade command to upgrade a single module by @BernieWhite.
    [#2551](https://github.com/microsoft/PSRule/issues/2551)
    - A single or specific modules can be upgraded by name when using `module upgrade`.
    - By default, all modules are upgraded.
  - Allow CLI to install pre-release modules by @BernieWhite.
    [#2550](https://github.com/microsoft/PSRule/issues/2550)
    - Add and upgrade pre-release modules with `--prerelease`.
    - Pre-release modules will be restored from the lock file with `module restore`.
- General improvements:
  - **Breaking change**: Empty version comparison only accepts stable versions by default by @BernieWhite.
    [#2557](https://github.com/microsoft/PSRule/issues/2557)
    - `version` and `apiVersion` assertions only accept stable versions by default for all cases.
    - Pre-release versions can be accepted by setting `includePrerelease` to `true`.
- Bug fixes:
  - Fixed CLI upgrade uses pre-release module by @BernieWhite.
    [#2549](https://github.com/microsoft/PSRule/issues/2549)

## v3.0.0-B0267 (pre-release)

What's changed since pre-release v3.0.0-B0203:

- New features:
  - Added option to configure the severity level that PSRule will break the pipeline at by @BernieWhite.
    [#1508](https://github.com/microsoft/PSRule/issues/1508)
    - Previously only rules with the severity level `Error` would break the pipeline.
    - With this update rules with the severity level `Error` that fail will break the pipeline by default.
    - The `Execution.Break` option can be set to `Never`, `OnError`, `OnWarning`, or `OnInformation`.
    - If a rule fails with a severity level equal or higher than the configured level the pipeline will break.
- General improvements:
  - **Breaking change**: Improve scope handling for correctly handling cases with multiple module by @BernieWhite.
    [#1215](https://github.com/microsoft/PSRule/issues/1215)
    - As a result of this change:
      - The `binding` property can no longer be used within baselines.
      - Custom inline script blocks can no longer be used for custom binding.
    - Use module configuration or workspace to configure binding options instead.
  - Added support for native logging within emitters by @BernieWhite.
    [#1837](https://github.com/microsoft/PSRule/issues/1837)
- Engineering:
  - Bump xunit to v2.9.0.
    [#1869](https://github.com/microsoft/PSRule/pull/1869)
  - Bump xunit.runner.visualstudio to v2.8.2.
    [#1869](https://github.com/microsoft/PSRule/pull/1869)
  - Bump System.Drawing.Common to v8.0.8.
    [#1887](https://github.com/microsoft/PSRule/pull/1887)
  - Bump YamlDotNet to v15.3.0.
    [#1856](https://github.com/microsoft/PSRule/pull/1856)
  - Bump Microsoft.CodeAnalysis.Common to v4.10.0.
    [#1854](https://github.com/microsoft/PSRule/pull/1854)
  - Bump Pester to v5.6.1.
    [#1872](https://github.com/microsoft/PSRule/pull/1872)
  - Bump PSScriptAnalyzer to v1.22.0.
    [#1858](https://github.com/microsoft/PSRule/pull/1858)
  - Bump BenchmarkDotNet from 0.13.12 to 0.14.0.
    [#1886](https://github.com/microsoft/PSRule/pull/1886)
- Bug fixes:
  - Fixed CLI exception the term Find-Module is not recognized by @BernieWhite.
    [#1860](https://github.com/microsoft/PSRule/issues/1860)
  - Fixed aggregation of reasons with `$Assert.AnyOf()` by @BernieWhite.
    [#1829](https://github.com/microsoft/PSRule/issues/1829)
  - Added `Problem` to validate sets of `OutputOutcome` by @nightroman
    [#2542](https://github.com/microsoft/PSRule/issues/2542)

## v3.0.0-B0203 (pre-release)

What's changed since pre-release v3.0.0-B0198:

- New features:
  - **Breaking change**: Simplify handling of inputs from files using emitters by @BernieWhite.
    [#1179](https://github.com/microsoft/PSRule/issues/1179)
    - Files are automatically read from input paths and emitted as objects to the pipeline.
    - Emitter interface can be used to implement custom file readers and expansion of custom file types.
    - The `File` and `Detect` input formats are no longer required and have been removed.
    - Processing files and objects with rules is no longer recommended, and disabled by default.
    - The `Input.FileObjects` can be set to `true` to enable processing of files as objects with rules.
- Bug fixes:
  - Fixed reason reported for `startsWith` by @BernieWhite.
    [#1818](https://github.com/microsoft/PSRule/issues/1818)
  - Fixes CSV output of multiple lines by @BernieWhite.
    [#1627](https://github.com/microsoft/PSRule/issues/1627)

## v3.0.0-B0198 (pre-release)

What's changed since pre-release v3.0.0-B0153:

- Engineering:
  - Bump System.Drawing.Common to v8.0.5.
    [#1817](https://github.com/microsoft/PSRule/pull/1817)
  - Bump xunit to v2.8.0.
    [#1809](https://github.com/microsoft/PSRule/pull/1809)
  - Bump xunit.runner.visualstudio to v2.8.0.
    [#1808](https://github.com/microsoft/PSRule/pull/1808)
  - Bump YamlDotNet to v15.1.4.
    [#1816](https://github.com/microsoft/PSRule/pull/1816)
  - Bump Microsoft.CodeAnalysis.Common to v4.9.2.
    [#1773](https://github.com/microsoft/PSRule/pull/1773)
- Bug fixes:
  - Fixed discovery of installed modules in CLI by @BernieWhite.
    [#1779](https://github.com/microsoft/PSRule/issues/1779)
  - Fixed for git head in tests by @BernieWhite.
    [#1801](https://github.com/microsoft/PSRule/issues/1801)

## v3.0.0-B0153 (pre-release)

What's changed since pre-release v3.0.0-B0151:

- Bug fixes:
  - Fixes null references for CLI module handling by @BernieWhite.
    [#1746](https://github.com/microsoft/PSRule/issues/1746)

## v3.0.0-B0151 (pre-release)

What's changed since pre-release v3.0.0-B0141:

- General improvements:
  - Improved support for packaging with Visual Studio Code by @BernieWhite.
    [#1755](https://github.com/microsoft/PSRule/issues/1755)
- Engineering:
  - **Breaking change:** Bump development tools to .NET 8.0 SDK by @BernieWhite.
    [#1673](https://github.com/microsoft/PSRule/pull/1673)
    - Running PSRule from PowerShell 7.x is supported on 7.4 and above.
    - Running PSRule from Windows PowerShell 5.1 is still supported but deprecated and will be removed in PSRule v4.
- Bug fixes:
  - Fixed CLI null reference when include module is undefined by @BernieWhite.
    [#1746](https://github.com/microsoft/PSRule/issues/1746)

## v3.0.0-B0141 (pre-release)

What's changed since pre-release v3.0.0-B0137:

- General improvements:
  - SARIF output has been improved to include effective configuration from a run by @BernieWhite.
    [#1739](https://github.com/microsoft/PSRule/issues/1739)
  - SARIF output has been improved to include file hashes for source files from a run by @BernieWhite.
    [#1740](https://github.com/microsoft/PSRule/issues/1740)
  - Added support to allow disabling PowerShell features that can be run from a repository by @BernieWhite.
    [#1742](https://github.com/microsoft/PSRule/issues/1742)
    - Added the `Execution.RestrictScriptSource` option to disable running scripts from a repository.
- Engineering:
  - Bump YamlDotNet to v15.1.0.
    [#1737](https://github.com/microsoft/PSRule/pull/1737)

## v3.0.0-B0137 (pre-release)

What's changed since pre-release v3.0.0-B0122:

- General improvements:
  - **Breaking change:** Moved the `restore` command to a sub-command of `module` by @BernieWhite.
    [#1730](https://github.com/microsoft/PSRule/issues/1730)
    - The functionality of the `restore` command is now available as `module restore`.
  - Added CLI commands to list and report status of locked modules by @BernieWhite.
    [#1729](https://github.com/microsoft/PSRule/issues/1729)
    - Added `module init` sub-command to initialize the lock file from configured options.
    - Added `module list` sub-command to list locked and unlocked modules associated with the workspace.
    - Added `version` property to the lock file schema to support versioning of the lock file.
- Engineering:
  - Bump BenchmarkDotNet to v0.13.12.
    [#1725](https://github.com/microsoft/PSRule/pull/1725)
  - Bump BenchmarkDotNet.Diagnostics.Windows to v0.13.12.
    [#1728](https://github.com/microsoft/PSRule/pull/1728)
  - Bump xunit to v2.6.6.
    [#1732](https://github.com/microsoft/PSRule/pull/1732)
  - Bump xunit.runner.visualstudio to v2.5.6.
    [#1717](https://github.com/microsoft/PSRule/pull/1717)
  - Bump System.Drawing.Common to v8.0.1.
    [#1727](https://github.com/microsoft/PSRule/pull/1727)

## v3.0.0-B0122 (pre-release)

What's changed since pre-release v3.0.0-B0093:

- General improvements:
  - **Breaking change:** Renamed `analyze` CLI command to `run` by @BernieWhite.
    [#1713](https://github.com/microsoft/PSRule/issues/1713)
  - Added `--outcome` argument for CLI to support filtering output by @bernieWhite.
    [#1706](https://github.com/microsoft/PSRule/issues/1706)
- Engineering:
  - Bump xunit to v2.6.3.
    [#1699](https://github.com/microsoft/PSRule/pull/1699)
  - Bump xunit.runner.visualstudio to v2.5.5.
    [#1700](https://github.com/microsoft/PSRule/pull/1700)
  - Bump Microsoft.CodeAnalysis.NetAnalyzers to v8.0.0.
    [#1674](https://github.com/microsoft/PSRule/pull/1674)
  - Bump Microsoft.CodeAnalysis.Common to v4.8.0.
    [#1686](https://github.com/microsoft/PSRule/pull/1686)
  - Bump BenchmarkDotNet to v0.13.11.
    [#1694](https://github.com/microsoft/PSRule/pull/1694)
  - Bump BenchmarkDotNet.Diagnostics.Windows to v0.13.11.
    [#1697](https://github.com/microsoft/PSRule/pull/1697)

## v3.0.0-B0093 (pre-release)

What's changed since pre-release v3.0.0-B0084:

- Engineering:
  - Bump xunit to v2.6.1.
    [#1656](https://github.com/microsoft/PSRule/pull/1656)
  - Bump System.Drawing.Common to v8.0.0.
    [#1669](https://github.com/microsoft/PSRule/pull/1669)
- Bug fixes:
  - Fixed CLI IndexOutOfRangeException with lock file by @BernieWhite.
    [#1676](https://github.com/microsoft/PSRule/issues/1676)

## v3.0.0-B0084 (pre-release)

What's changed since release v2.9.0:

- New features:
  - Added lock file support when using CLI and related tools by @BernieWhite.
    [#1660](https://github.com/microsoft/PSRule/issues/1660)
    - The lock file used used during analysis and when installing modules to select a specific version.
- General improvements:
  - **Breaking change:** Switch to use SHA-512 for generating unbound objects by @BernieWhite.
    [#1155](https://github.com/microsoft/PSRule/issues/1155)
    - Objects that have no bound name will automatically be assigned a name based on the SHA-512 hash of the object.
    - Previously a SHA-1 hash was used, however this is no longer considered secure.
    - The name for unbound objects that are suppressed will change as a result.
    - Additionally the hash can be changed by setting the `Execution.HashAlgorithm` option.
    - See [upgrade notes][1] for details.
  - **Breaking change:** Removed deprecated execution options by @BernieWhite.
    [#1457](https://github.com/microsoft/PSRule/issues/1457)
  - **Breaking change:** Removed deprecated object properties by @BernieWhite.
    [#1601](https://github.com/microsoft/PSRule/issues/1601)
  - Expanded support for `FileHeader` assertion by @BernieWhite.
    [#1521](https://github.com/microsoft/PSRule/issues/1521)
    - Added support for `.bicepparam`, `.tsp`, `.tsx`, `.editorconfig`, `.ipynb`, and `.toml` files.
- Engineering:
  - **Breaking change:** Bump development tools to .NET 7.0 SDK by @BernieWhite.
    [#1631](https://github.com/microsoft/PSRule/issues/1631)
    - Running PSRule from PowerShell 7.x is supported on 7.3 and above.
    - Running PSRule from Windows PowerShell 5.1 is still supported but deprecated and will be removed in PSRule v4.
  - Bump Microsoft.CodeAnalysis.NetAnalyzers to v7.0.4.
    [#1602](https://github.com/microsoft/PSRule/pull/1602)
  - Bump Microsoft.CodeAnalysis.Common to v4.7.0.
    [#1593](https://github.com/microsoft/PSRule/pull/1593)
  - Bump YamlDotNet to v13.7.1.
    [#1647](https://github.com/microsoft/PSRule/issues/1647)
  - Bump xunit to v2.5.3.
    [#1648](https://github.com/microsoft/PSRule/pull/1648)
  - Bump xunit.runner.visualstudio to v2.5.3.
    [#1644](https://github.com/microsoft/PSRule/pull/1644)
  - Bump BenchmarkDotNet to v0.13.10.
    [#1654](https://github.com/microsoft/PSRule/pull/1654)
  - Bump BenchmarkDotNet.Diagnostics.Windows to v0.13.10.
    [#1654](https://github.com/microsoft/PSRule/pull/1654)
