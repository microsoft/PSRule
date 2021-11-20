# PSRule_Selectors

## about_PSRule_Selectors

## SHORT DESCRIPTION

Describes PSRule Selectors including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When evaluating an object from input, PSRule can use selectors to perform complex matches of an object.

- A selector is a YAML/JSON based expression that evaluates an object.
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
- Name
- Type

To learn more about conditions, operators, and properties see [about_PSRule_Expressions](about_PSRule_Expressions.md).

Currently the following limitations apply:

- Selectors can evaluate:
  - Fields of the target object.
  - Type and name binding of the target object by using `name` and `type` comparison properties.
- State variables such has `$PSRule` can not be evaluated.
- Bound fields can not be evaluated.

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

Selectors can be defined with either YAML or JSON format, and can be included with a module or standalone `.Rule.yaml` or `.Rule.json` file.
In either case, define a selector within a file ending with the `.Rule.yaml` or `.Rule.json` extension.
A selector can be defined side-by-side with other resources such as baselines or module configurations.

JSON selectors can also be saved with the `.Rule.jsonc` extension, for example `Selectors.Rule.jsonc`.
Use `.jsonc` to view [JSON with Comments](https://code.visualstudio.com/docs/languages/json#_json-with-comments) in Visual Studio Code.

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

```json
[
  {
    // Synopsis: {{ Synopsis }}
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Selector",
    "metadata": {
      "name": "{{ Name }}"
    },
    "spec": {
      "if": {}
    }
  }
]
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

### Example Selectors.Rule.json

```json
// Example Selectors.Rule.json
[
  {
    // Synopsis: Require the CustomValue field.
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Selector",
    "metadata": {
      "name": "RequireCustomValue"
    },
    "spec": {
      "if": {
        "field": "CustomValue",
        "exists": true
      }
    }
  },
  {
    // Synopsis: Require a Name or AlternativeName.
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Selector",
    "metadata": {
      "name": "RequireName"
    },
    "spec": {
      "if": {
        "anyOf": [
          {
            "field": "AlternateName",
            "exists": true
          },
          {
            "field": "Name",
            "exists": true
          }
        ]
      }
    }
  },
  {
    // Synopsis: Require a specific CustomValue
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Selector",
    "metadata": {
      "name": "RequireSpecificCustomValue"
    },
    "spec": {
      "if": {
        "field": "CustomValue",
        "in": [
          "Value1",
          "Value2"
        ]
      }
    }
  }
]
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/concepts/PSRule/en-US/about_PSRule_Selectors.md.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- Selectors
- Expressions
- PSRule
