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

### `--formats`

Enables one or more formats by name to process files and deserialized objects.
All formats are disabled by default.

For example, to enable JSON and YAML formats:

```bash
--formats json yaml
```

### `--baseline`

The name of a specific baseline to use.
Currently, only a single baseline can be used during a run.

### `--name`

The name of one or more specific rules to run instead of all rules.
By default, all rules are evaluated.

### `--no-restore`

Do not restore modules before running rules.
By default, modules are restored automatically before running rules.

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
For example: `--outcome Pass --outcome Fail`.

### `--output` | `-o`

Specifies the format to use when outputting results to file in addition to the console.
By default, results are not written to a file.

The supported values are:

- `Yaml` - Output results in YAML format.
- `Json` - Output results in JSON format.
- `Markdown` - Output results in Markdown format.
- `NUnit3` - Output results in NUnit format.
- `Csv` - Output results in CSV format.
- `Sarif` - Output results in SARIF format.

### `--output-path`

Specifies a path to write results to.
Use this argument in conjunction with the `--output` to set the output format.
By default, results are not written to a file.

## Next steps

To find out more about the commands available with the PSRule CLI, see [PSRule CLI](./index.md).
