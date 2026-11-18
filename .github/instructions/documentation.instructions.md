---
applyTo: "**/*.md"
---

# Markdown and Documentation Instructions

Follow the shared workflow in [AGENTS.md](../../AGENTS.md).

- Follow `.markdownlint.json` and surrounding Markdown conventions:
  ATX headings, dash lists, language-tagged code fences, and descriptive link text.
- Use relative links for repository files and documentation pages.
  Preserve existing reference-link conventions and verify changed link targets and anchors.
- Keep examples consistent with the actual commands, options, schemas, and supported platforms.
- For pages under `docs/`, preserve applicable YAML front matter, MkDocs admonitions, and code-block titles.
  Check `mkdocs.yml` when adding, moving, or renaming navigable pages.
- Keep cmdlet documentation under `docs/commands/` consistent with PowerShell parameters and external help.
- For documentation-site changes, run `mkdocs build` without `--strict`, as described in the root instructions.
  Instruction-only Markdown changes do not require rebuilding the site.
- Adding or removing files under `.github/` affects repository-based `InputPathBuilderTests`.
  Verify the matching file counts and update fixed expectations when needed; preserve the test's design.
