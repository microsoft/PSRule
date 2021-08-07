# PSRule_Selectors

## about_PSRule_Selectors

## SHORT DESCRIPTION

Describes PSRule Selectors including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When evaluating an object from input, PSRule can use selectors to perform complex matches of an object.

- A selector is a YAML-based expression that evaluates an object.
- Each selector is comprised of nested conditions, operators, and comparison properties.
- Selectors must use one or more available conditions with a comparison property to evaluate the object.
- Optionally a condition can be nested in an operator.
- Operators can be nested within other operators.

Conditions and operators available for use include:

- AllOf
- AnyOf
- Contains
- Equals
- EndsWith
- Exists
- Greater
- GreaterOrEquals
- HasValue
- In
- IsLower
- IsString
- IsUpper
- Less
- LessOrEquals
- Match
- Not
- NotEquals
- NotIn
- NotMatch
- StartsWith

The following comparison properties are available:

- Field

To learn more about conditions, operators, and properties see [about_PSRule_Expressions](about_PSRule_Expressions.md).

Currently the following limitations apply:

- Selectors can only evaluate a field of the target object.
The following examples can not be evaluated by selectors:
  - Bound properties such as `TargetName`, `TargetType`, and `Field`.
  - State variables such as `$PSRule`.

### Using selectors as pre-conditions

Selectors can be referenced by name as a rule pre-condition by using the `-With` parameter.
For example:

```powershell
Rule 'RuleWithSelector' -With 'BasicSelector' {
    # Rule condition
}
```

Selector pre-conditions can be used together with type and script block pre-conditions.
If one or more selector pre-conditions are used, they are evaluated before type or script block pre-conditions.

### Defining selectors

Selectors are defined in YAML and can be included within a module or standalone `.Rule.yaml` file.
In either case, define a selector within a file ending with the `.Rule.yaml` extension.
A selector can be defined side-by-side with other resources such as baselines or module configurations.

Use the following template to define a selector:

```yaml
---
# Synopsis: {{ Synopsis }}
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: '{{ Name }}'
spec:
  if: { }
```

Within the `if` object, one or more conditions or logical operators can be used.

## EXAMPLES

### Example Selectors.Rule.yaml

```yaml
# Example Selectors.Rule.yaml
---
# Synopsis: Require the CustomValue field.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: RequireCustomValue
spec:
  if:
    field: 'CustomValue'
    exists: true

---
# Synopsis: Require a Name or AlternativeName.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: RequireName
spec:
  if:
    anyOf:
    - field: 'AlternateName'
      exists: true
    - field: 'Name'
      exists: true

---
# Synopsis: Require a specific CustomValue
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: RequireSpecificCustomValue
spec:
  if:
    field: 'CustomValue'
    in:
    - 'Value1'
    - 'Value2'
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/concepts/PSRule/en-US/about_PSRule_Selectors.md.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- Selectors
- Expressions
- PSRule
