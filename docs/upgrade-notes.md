# Upgrade notes

This document contains notes to help upgrade from previous versions of PSRule.

## Upgrade to v0.22.0 from v1.0.0

Follow these notes to upgrade from PSRule version _v0.22.0_ to _v1.0.0_.

### Replaced $Rule target properties

Previously in PSRule _v0.22.0_ and prior the `$Rule` automatic variable had the following properties:

- `TargetName`
- `TargetType`
- `TargetObject`

For example:

```powershell
Rule 'Rule1' {
    $Rule.TargetName -eq 'Name1';
    $Rule.TargetType -eq '.json';
    $Rule.TargetObject.someProperty -eq 1;
}
```

In _v1.0.0_ these properties have been removed after being deprecated in _v0.12.0_.
These properties are instead available on the `$PSRule` variable.
Rules referencing the deprecated properties of `$Rule` must be updated.

For example:

```powershell
Rule 'Rule1' {
    $PSRule.TargetName -eq 'Name1';
    $PSRule.TargetType -eq '.json';
    $PSRule.TargetObject.someProperty -eq 1;
}
```
