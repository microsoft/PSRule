---
title: ps-rule run command
---

# ps-rule run

!!! Abstract
    Use the `run` command to run rules against an input path and output the results.

## Usage

```bash title="PSRule CLI command-line"
ps-rule run [options]
```

## Options

### `--input-path` | `-f`

The file or directory path to search for input file to use during a run.
By default, this is the current working path.

### `--module` | `-m`

The name of one or more modules that contain rules or resources to use during a run.

### `--baseline`

The name of a specific baseline to use.
Currently, only a single baseline can be used during a run.

### `--outcome`

Specifies the rule results to show in output.
By default, `Pass`/ `Fail`/ `Error` results are shown.

Allows filtering of results by outcome.
The supported values are:

- `Pass` - Results for rules that passed.
- `Fail` - Results for rules that did not pass.
- `Error` - Results for rules that raised an error are returned.
- `Processed` - All results that were processed.
  This aggregated outcome includes `Pass`, `Fail`, or `Error` results.
- `Problem` - Processed results that did not pass.
  This aggregated outcome includes `Fail`, or `Error` results.

To specify multiple values, specify the parameter multiple times.
For example: `--outcome Pass --Outcome Fail`.

### `--output` | `-o`

Specifies the format to use when outputting results.

### `--output-path`

Specifies a path to write results to.

## Next steps

To find out more about the commands available with the PSRule CLI, see [PSRule CLI](./index.md).
