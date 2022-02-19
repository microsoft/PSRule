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

The following conditions are available:

- [Contains](about_PSRule_Expressions.md#contains)
- [Count](about_PSRule_Expressions.md#count)
- [Equals](about_PSRule_Expressions.md#equals)
- [EndsWith](about_PSRule_Expressions.md#endswith)
- [Exists](about_PSRule_Expressions.md#exists)
- [Greater](about_PSRule_Expressions.md#greater)
- [GreaterOrEquals](about_PSRule_Expressions.md#greaterorequals)
- [HasDefault](about_PSRule_Expressions.md#hasdefault)
- [HasSchema](about_PSRule_Expressions.md#hasschema)
- [HasValue](about_PSRule_Expressions.md#hasvalue)
- [In](about_PSRule_Expressions.md#in)
- [IsLower](about_PSRule_Expressions.md#islower)
- [IsString](about_PSRule_Expressions.md#isstring)
- [IsUpper](about_PSRule_Expressions.md#isupper)
- [Less](about_PSRule_Expressions.md#less)
- [LessOrEquals](about_PSRule_Expressions.md#lessorequals)
- [Match](about_PSRule_Expressions.md#match)
- [NotEquals](about_PSRule_Expressions.md#notequals)
- [NotIn](about_PSRule_Expressions.md#notin)
- [NotMatch](about_PSRule_Expressions.md#notmatch)
- [SetOf](about_PSRule_Expressions.md#setof)
- [StartsWith](about_PSRule_Expressions.md#startswith)
- [Subset](about_PSRule_Expressions.md#subset)
- [Version](about_PSRule_Expressions.md#version)

The following operators are available:

- [AllOf](about_PSRule_Expressions.md#allof)
- [AnyOf](about_PSRule_Expressions.md#anyof)
- [Not](about_PSRule_Expressions.md#not)

The following comparison properties are available:

- [Field](about_PSRule_Expressions.md#field)
- [Name](about_PSRule_Expressions.md#name)
- [Type](about_PSRule_Expressions.md#type)

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

Selectors can be defined with either YAML or JSON format, and can be included with a module or standalone `.Rule.yaml` or `.Rule.jsonc` file.
In either case, define a selector within a file ending with the `.Rule.yaml` or `.Rule.jsonc` extension.
A selector can be defined side-by-side with other resources such as baselines or module configurations.

Selectors can also be defined within `.json` files.
We recommend using `.jsonc` to view [JSON with Comments](https://code.visualstudio.com/docs/languages/json#_json-with-comments) in Visual Studio Code.

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

```jsonc
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

### Example Selectors.Rule.jsonc

```jsonc
// Example Selectors.Rule.jsonc
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

An online version of this document is available at https://microsoft.github.io/PSRule/v1/concepts/PSRule/en-US/about_PSRule_Selectors/.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/v1/commands/PSRule/en-US/Invoke-PSRule/)

## KEYWORDS

- Selectors
- Expressions
- PSRule
