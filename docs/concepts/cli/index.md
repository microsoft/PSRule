# PSRule CLI

!!! Abstract
    PSRule provides a command-line interface (CLI) to run rules and analyze results.
    This article describes the commands available in the CLI.

## Commands

The following commands are available in the PSRule CLI:

- [run](./run.md) &mdash; Run rules against an input path and output the results.
- [module](./module.md) &mdash; Manage or restore modules tracked by the module lock file and configured options.

## Global options

The following options are available in the PSRule CLI:

### `--option`

Specifies the path to an options file.
By default, the CLI will look for a file named `ps-rule.yaml` in the current directory.

### `--version`

Show the version information for the PSRule CLI.

For example:

```bash
ps-rule --version
```

### `--help`

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
