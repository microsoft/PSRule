---
title: CLI
---

# PSRule CLI

!!! Abstract
    PSRule provides a command-line interface (CLI) to run rules and analyze results.
    This article describes the commands available in the CLI.

    For details on installing the PSRule CLI, see [Install PSRule](../../setup/index.md#with-cli).

## Commands

The following commands are available in the CLI:

- [run](./run.md) &mdash; Run rules against an input path and output the results.
- [module](./module.md) &mdash; Manage or restore modules tracked by the module lock file and configured options.
- [restore](./restore.md) &mdash; Restore from the module lock file and configured options.
  This is a shortcut for module restore.

## `--version`

Show the version information for PSRule.

For example:

```bash
ps-rule --version
```

## Global options

The following global options can be used with any command:

### `--option`

Specifies the path to an options file.
By default, the CLI will look for a file named `ps-rule.yaml` in the current directory.

### `-?` | `-h` | `--help`

Display help and usage information for the PSRule CLI and commands.
To display help for a specific command, use `--help` with the command name.

For example:

```bash
ps-rule run --help
```

### `--verbose`

Display verbose output for the selected command.

### `--debug`

Display debug output for the selected command.
