---
title: ps-rule restore command
---

# ps-rule restore

!!! Abstract
    Use the `restore` command restore modules tracked by the module lock file and configured options. (`ps-rule.lock.json`).
    This command is an alias for the `module restore` command.
    The module lock file, provides consistent module versions across multiple machines and environments.
    For more information, see [Lock file](../lockfile.md).

## Usage

```bash title="PSRule CLI command-line"
ps-rule restore [options]
```

## Options

### `--force`

Restore modules even when an existing version that meets constraints is already installed locally.

For example:

```bash title="PSRule CLI command-line"
ps-rule restore --force
```
