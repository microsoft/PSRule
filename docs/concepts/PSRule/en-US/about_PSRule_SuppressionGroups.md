# PSRule_SuppressionGroups

## about_PSRule_SuppressionGroups

## SHORT DESCRIPTION

Describes PSRule Suppression Groups including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When an evaluating each object, PSRule can use suppression groups to suppress rules based on a condition.
Suppression groups use a [Selector](about_PSRule_Selectors.md) to determine if the rule is suppressed.

### Defining suppression groups

Suppression groups can be defined using either YAML or JSON format.
A suppression group can be in a standalone file or included in a module.
Define suppression groups in `.Rule.yaml` or `.Rule.jsonc` files.
Each suppression group may be defined individually or side-by-side with resources such as rules or baselines.

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
  expiresOn: null
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
      "expiresOn": null,
      "rule": [],
      "if": {}
    }
  }
]
```

Set the `synopsis` to describe the justification for the suppression.
Within the `rule` array, one or more rule names can be used.
If no rules are specified, suppression will occur for all rules.
Within the `if` object, one or more conditions or logical operators can be used.
When the `if` condition is `true` the object will be suppressed for the current rule.

Optionally, an expiry can be set using the `expiresOn` property.
When the expiry date is reached, the suppression will no longer be applied.
To configure an expiry, set a RFC3339 (ISO 8601) formatted date time using the format `yyyy-MM-ddTHH:mm:ssZ`.

### Documentation

Suppression groups can be configured with a synopsis.
When set, the synopsis will be included in output for any suppression warnings that are shown.
The synopsis helps provide justification for the suppression, in a short single line message.
To set the synopsis, include a comment above the suppression group `apiVersion` property.

Alternatively, a localized synopsis can be provided in a separate markdown file.
See [about_PSRule_Docs](about_PSRule_Docs.md) for details.

Some examples of a suppression group synopsis include:

- _Ignore test objects by name._
- _Ignore test objects by type._
- _Ignore objects with non-production tag._

## EXAMPLES

### Example SuppressionGroups.Rule.yaml

```yaml
# Example SuppressionGroups.Rule.yaml

---
# Synopsis: Ignore test objects by name.
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
# Synopsis: Ignore test objects by type.
apiVersion: github.com/microsoft/PSRule/v1
kind: SuppressionGroup
metadata:
  name: SuppressWithTestType
spec:
  expiresOn: '2030-01-01T00:00:00Z'
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
    // Synopsis: Ignore test objects by name.
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
    // Synopsis: Ignore test objects by type.
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "SuppressionGroup",
    "metadata": {
      "name": "SuppressWithTestType"
    },
    "spec": {
      "expiresOn": "2030-01-01T00:00:00Z",
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

An online version of this document is available at <https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_SuppressionGroups/>.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- SuppressionGroups
- Selectors
- PSRule
