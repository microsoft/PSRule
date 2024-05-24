# PSRule options spec (draft)

This is a spec for implementing options in PSRule v2.

## Synopsis

When executing resources options is often required to control the specific behavior.
In additional, many of these cases are scoped to the module being used.

The following scopes exist:

- Parameter
- Local options
- Baseline
- Module configuration
