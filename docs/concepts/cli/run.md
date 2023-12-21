# `ps-rule run`

Run rule analysis.

## Optional parameters

### `--baseline`

The name of a specific baseline to use.

### `--outcome`

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
