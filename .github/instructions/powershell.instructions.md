---
applyTo: "**/*.ps1,**/*.psm1,**/*.psd1"
---

# PowerShell Instructions

Follow the shared workflow in [AGENTS.md](../../AGENTS.md).

- Use four-space indentation and preserve the existing copyright header and surrounding formatting.
- Preserve cross-platform behavior. Use `pwsh` for development commands and `Join-Path` for filesystem paths.
  Do not introduce newer syntax without checking the module's supported PowerShell versions.
- Follow existing advanced-function patterns for parameter attributes, pipeline processing, and output types.
  Preserve public parameter sets, aliases, and pipeline behavior.
- Preserve strict-mode and error-handling conventions; do not suppress unexpected errors.
- Use PowerShell comment-based help for function documentation where appropriate.
  For exported cmdlets using `.ExternalHelp`, keep the corresponding `docs/commands/` documentation consistent.
- Use the existing Pester `Describe`, `Context`, `It`, and setup conventions in `tests/PSRule.Tests/`.
  Tests import the built module from `out/modules/PSRule`; rebuild it after changing module source.
- Use the existing `TestModule` Invoke-Build task for module tests.
  It configures Pester results and accepts the `TestGroup` parameter for tag filtering.
  Refer to `pipeline.build.ps1` rather than duplicating its test setup.
