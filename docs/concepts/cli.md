# PSRule CLI

!!! Abstract
    PSRule provides a command-line interface (CLI) to run rules and analyze results.
    This article describes the commands available in the CLI.

## `analyze`

Run rule analysis.

### `--outcome`

Allows filtering of results by outcome.
The supported values are:

- `Pass` - Results that passed.
- `Fail` - Results that did not pass.
- `Error` - Results that failed to be evaluted correctly due to an error.
- `Processed` - All results that were processed.
  This aggregated outcome includes `Pass`, `Fail`, or `Error` results.
- `Problem` - Processed results that did not pass.
  This aggregated outcome includes `Fail`, or `Error` results.

To specify multiple values, specify the parameter multiple times.
For example: `--outcome Pass --Outcome Fail`.

## `module add`

Add one or more modules to the lock file.

## `module remove`

Remove one or more modules from the lock file.

## `module upgrade`

Upgrade to the latest versions any modules within the lock file.

## `restore`

Restore modules defined in configuration locally.
