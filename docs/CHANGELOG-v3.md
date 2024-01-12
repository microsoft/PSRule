---
discussion: false
link_users: true
---

# Change log

See [upgrade notes][1] for helpful information when upgrading from previous versions.

  [1]: https://aka.ms/ps-rule/upgrade

**Experimental features**:

- Baseline groups allow you to use a friendly name to reference baselines.
  See [baselines][6] for more information.
- Functions within YAML and JSON expressions can be used to perform manipulation prior to testing a condition.
  See [functions][3] for more information.
- Sub-selectors within YAML and JSON expressions can be used to filter rules and list properties.
  See [sub-selectors][4] for more information.
- Processing of changes files only within a pipeline.
  See [creating your pipeline][5] for more information.

  [3]: expressions/functions.md
  [4]: expressions/sub-selectors.md
  [5]: creating-your-pipeline.md#processing-changed-files-only
  [6]: concepts/baselines.md

## Unreleased

What's changed since pre-release v3.0.0-B0122:

- Engineering:
  - Bump BenchmarkDotNet to v0.13.12.
    [#1725](https://github.com/microsoft/PSRule/pull/1725)
  - Bump xunit to v2.6.5.
    [#1724](https://github.com/microsoft/PSRule/pull/1724)
  - Bump xunit.runner.visualstudio to v2.5.6.
    [#1717](https://github.com/microsoft/PSRule/pull/1717)

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
  - Bump Microsoft.NET.Test.Sdk to v17.8.0.
    [#1659](https://github.com/microsoft/PSRule/pull/1659)
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
  - Bump Microsoft.NET.Test.Sdk to v17.7.2.
    [#1608](https://github.com/microsoft/PSRule/pull/1608)
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
