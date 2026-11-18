---
applyTo: "**/*.ts"
---

# TypeScript Instructions

Follow the shared workflow in [AGENTS.md](../../AGENTS.md).

- The VS Code extension lives in `src/vscode-ps-rule/`.
  Follow nearby code and the existing TypeScript and ESLint configuration rather than reformatting unrelated code.
- Preserve strict typing from `tsconfig.json`; prefer explicit types and guards over `any` or unchecked assertions.
- Use JSDoc when documenting a whole function or public API, with `@param` and `@returns` where useful.
  Reserve inline comments for non-obvious implementation details.
- Reuse the extension's existing configuration, workspace, command, and logging helpers.
  Use its `logger` from `src/vscode-ps-rule/logger.ts` instead of adding console logging.
- Preserve VS Code resource disposal and workspace trust behavior when changing extension features.
- Keep command identifiers and configuration keys consistent with their declarations in `package.json`.
- Follow existing tests in `src/vscode-ps-rule/test/`.
  Check `package.json` for current scripts; `npm test` builds and launches the VS Code test runner.
- Use the root build and package validation commands for extension changes.
  Do not assume the esbuild bundle also performs TypeScript type-checking.
