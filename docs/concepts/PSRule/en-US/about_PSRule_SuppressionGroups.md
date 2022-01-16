# PSRule_SuppressionGroups

## about_PSRule_SuppressionGroups

## SHORT DESCRIPTION

Describes PSRule Suppression Groups including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When an evaluating an object from input, PSRule can use suppression groups to suppress rules based on a [Selector](about_PSRule_Selectors.md).

## Defining suppression groups

Suppression groups can be defined with either YAML or JSON format, and can be included with a module or a standalone `.Rule.yaml` or `.Rule.jsonc` file.
In either case, define a suppression group within a file ending with the `.Rule.yaml` or `.Rule.jsonc` extension.
A suppression group can be defined side-by-side with other resources such as rules, baselines or module configurations.

Suppression groups can also be defined within `.json` files.
We recommend using `.jsonc` to view [JSON with Comments](https://code.visualstudio.com/docs/languages/json#_json-with-comments) in Visual Studio Code.

Use the following template to define a suppression group:

```yaml
---
# Synopsis: {{ Synopsis }}
apiVersion: github.com/microsoft/PSRule/v1
kind: SuppressionGroup
metadata:
  name: '{{ Name }}'
spec:
  rule: []
  if: { }
```

```jsonc
[
  {
    // Synopsis: {{ Synopsis }}
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "SuppressionGroup",
    "metadata": {
      "name": "{{ Name }}"
    },
    "spec": {
      "rule": [],
      "if": {}
    }
  }
]
```

Within the `rule` array, one or more rule names can be used.
If no rules are specified, suppression will occur for all rules.
Within the `if` object, one or more conditions or logical operators can be used.
When the `if` condition is `true` the object will be suppressed for the current rule.

## EXAMPLES

### Example SuppressionGroups.Rule.yaml

```yaml
# Example SuppressionGroups.Rule.yaml

---
# Synopsis: Suppress with target name
apiVersion: github.com/microsoft/PSRule/v1
kind: SuppressionGroup
metadata:
  name: SuppressWithTargetName
spec:
  rule:
  - 'FromFile1'
  - 'FromFile2'
  if:
    name: '.'
    in:
    - 'TestObject1'
    - 'TestObject2'

---
# Synopsis: Suppress with target type
apiVersion: github.com/microsoft/PSRule/v1
kind: SuppressionGroup
metadata:
  name: SuppressWithTestType
spec:
  rule:
  - 'FromFile3'
  - 'FromFile5'
  if:
    type: '.'
    equals: 'TestType'
```

### Example SuppressionGroups.Rule.jsonc

```jsonc
// Example SuppressionGroups.Rule.jsonc
[
  {
    // Synopsis: Suppress with target name
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "SuppressionGroup",
    "metadata": {
      "name": "SuppressWithTargetName"
    },
    "spec": {
      "rule": [
        "FromFile1",
        "FromFile2"
      ],
      "if": {
        "name": ".",
        "in": [
          "TestObject1",
          "TestObject2"
        ]
      }
    }
  },
  {
    // Synopsis: Suppress with target type
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "SuppressionGroup",
    "metadata": {
      "name": "SuppressWithTestType"
    },
    "spec": {
      "rule": [
        "FromFile3",
        "FromFile5"
      ],
      "if": {
        "type": ".",
        "equals": "TestType"
      }
    }
  }
]
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/concepts/PSRule/en-US/about_PSRule_SuppressionGroups.md.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- SuppressionGroups
- Selectors
- PSRule