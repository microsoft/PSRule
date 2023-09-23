---
discussion: false
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

What's changed since release v2.9.0:

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
  - **Breaking change:** Remove deprecated object properties by @BernieWhite.
    [#1601](https://github.com/microsoft/PSRule/issues/1601)
  - Expanded support for `FileHeader` assertion by @BernieWhite.
    [#1521](https://github.com/microsoft/PSRule/issues/1521)
    - Added support for `.bicepparam`, `.tsp`, `.tsx`, `.editorconfig`, `.ipynb`, and `.toml` files.
- Engineering:
  - Bump Microsoft.CodeAnalysis.NetAnalyzers to v7.0.4.
    [#1602](https://github.com/microsoft/PSRule/pull/1602)
  - Bump Microsoft.CodeAnalysis.Common to v4.7.0.
    [#1593](https://github.com/microsoft/PSRule/pull/1593)
  - Bump Microsoft.NET.Test.Sdk to v17.7.2.
    [#1608](https://github.com/microsoft/PSRule/pull/1608)
  - Bump YamlDotNet to v13.3.1.
    [#1607](https://github.com/microsoft/PSRule/issues/1607)
  - Bump xunit to v2.5.1.
    [#1619](https://github.com/microsoft/PSRule/pull/1619)
  - Bump xunit.runner.visualstudio to v2.5.1.
    [#1620](https://github.com/microsoft/PSRule/pull/1620)
  - Bump BenchmarkDotNet to v0.13.7.
    [#1587](https://github.com/microsoft/PSRule/pull/1587)
  - Bump BenchmarkDotNet.Diagnostics.Windows to v0.13.7.
    [#1586](https://github.com/microsoft/PSRule/pull/1586)
