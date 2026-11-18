---
name: upgrade-vscode-types
description: >-
  Upgrade or review Dependabot updates to @types/vscode in package.json.
  Keep engines.vscode aligned with the dependency's major and minor version,
  synchronize package-lock.json, and validate VS Code extension packaging.
---

# Upgrade VS Code Types

## When to Use

Use this skill when upgrading `@types/vscode`, completing a Dependabot update for that dependency,
or fixing an extension packaging failure caused by a mismatch with `engines.vscode`.

Follow the repository's [shared instructions](../../../AGENTS.md).
This skill is Markdown-only; do not add helper scripts or automation to perform the upgrade.

## Repository Configuration

- The extension manifest is the root [package.json](../../../package.json), not `packages.json`.
- `devDependencies["@types/vscode"]` is an exact version.
- `engines.vscode` uses a caret range with the same major and minor version and a zero patch.
  For example, types `1.134.0` pair with engine `^1.134.0`.
  This is an example of the convention, not a version to pin future updates to.
- [package-lock.json](../../../package-lock.json) mirrors both fields in `packages[""]`
  and records the installed types version in `packages["node_modules/@types/vscode"]`.
- [Dependabot](../../dependabot.yml) updates root npm dependencies and excludes `@types/vscode`
  from the grouped `dev-common` updates. Check its changes before doing any dependency work again.
- The `package` npm script builds the .NET language server, bundles the extension, and runs `vsce package`.
  Packaging validates the compatibility between the types and the declared VS Code engine.

## Upgrade Procedure

1. Read the current manifest, lockfile, and any existing Dependabot changes.
   Identify the intended target version from the request or dependency update.
   Do not assume the example version above is the target or perform an unrelated latest-version upgrade.
2. Preserve Dependabot's dependency and lockfile updates if they already specify the target.
   Otherwise update the exact `devDependencies["@types/vscode"]` version to the requested release.
3. For stable types version `M.m.p`, set `engines.vscode` to `^M.m.0`.
   Keep the major and minor numbers aligned; do not copy the dependency's patch number into the engine minimum.
   For example, types `1.135.2` require engine `^1.135.0` under this repository's convention.
   A patch-only types update needs no engine change if its major and minor already match.
   If a prerelease or a deliberately different compatibility policy is requested, clarify it before changing the range.
4. Synchronize the lockfile with npm rather than manually editing resolved versions or integrity hashes.
   After a manifest change, use `npm install --package-lock-only --ignore-scripts` from the repository root.
   Confirm the root lockfile metadata matches the manifest and the locked types version matches the target.
   Keep unrelated dependency changes out of the update.
5. Preserve other engine fields, the extension's own version, and unrelated dependencies and settings.
   Raising the engine minimum intentionally raises the minimum supported VS Code release;
   do not downgrade the types or widen the engine range to hide a packaging error.

## Validation and Completion

1. Verify the manifest and root lockfile metadata agree on the types version and engine range.
   Check the actual major and minor numbers, not just whether either field changed.
2. If installed dependencies are missing or stale, run `npm ci` using the synchronized lockfile.
3. Run `npm run package` from the repository root.
   Do not treat successful bundling alone as validation: the command must reach and pass `vsce package`.
4. Confirm the expected VSIX is produced under `out/package/`, following the root validation instructions.
   Report command failures explicitly, distinguishing dependency or environment failures from version mismatches.
5. Summarize the types version, engine range, lockfile changes, and packaging result.
   Do not claim packaging passed if it could not be run.
