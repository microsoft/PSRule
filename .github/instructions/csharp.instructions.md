---
applyTo: "**/*.cs"
---

# C# Instructions

Follow the shared workflow in [AGENTS.md](../../AGENTS.md).

## Code and Documentation

- Follow `.editorconfig`: four-space indentation, file-scoped namespaces, and using directives outside namespaces.
- Preserve the Microsoft copyright and MIT license header when creating source files.
- Respect each project's target framework, language version, and nullable settings; do not assume all projects match.
- When implementing logging, use `ILogger` from `src/PSRule.Types/Runtime/ILogger.cs`.
- Use XML documentation comments (`/// <summary>...</summary>`) when documenting a whole method,
  including unit test methods. Place them before any attributes.
- Summarize the method's purpose or the behavior a test validates.
  Use `<remarks>` for additional intent, constraints, or maintenance guidance.
- Add `<param>`, `<returns>`, `<exception>`, and `<see cref="..."/>` where applicable.
  Keep documentation consistent with the signature and behavior.
- Reserve ordinary `//` comments for non-obvious implementation details within a method.
  Do not add redundant documentation to self-explanatory code.

## Tests

- Use the existing xUnit conventions and run the affected test project or filter first.
  See the root instructions for build, formatting, and component validation commands.
- `InputPathBuilderTests` intentionally uses repository files with fixed expected counts.
  Verify added or removed files before adjusting those counts; do not replace them with generated fixtures
  or expectations calculated from the output.
