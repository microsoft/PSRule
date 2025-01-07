---
description: Labels are additional metadata that can be used to classify rules.
---

# Grouping rules

_Labels_ are additional metadata that can be used to classify rules.
Together with tags they can be used to group or filter rules.

## Using labels

When defining a rule you can specify labels to classify or link rules using a framework or standard.
A single rule can be can linked to multiple labels.
For example:

- The Azure Well-Architected Framework (WAF) defines pillars such as _Security_ and _Reliability_.
- The CIS Benchmarks define a number of control IDs such as _3.12_ and _13.4_.

=== "YAML"

    To specify labels in YAML, use the `labels` property:

    ```yaml
    ---
    # Synopsis: A rule with labels defined.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: WithLabels
      labels:
        Azure.WAF/pillar: Security
        Azure.ASB.v3/control: [ 'ID-1', 'ID-2' ]
    spec: { }
    ```

=== "JSON"

    To specify labels in JSON, use the `labels` property:

    ```json
    {
      // Synopsis: A rule with labels defined.
      "apiVersion": "github.com/microsoft/PSRule/v1",
      "kind": "Rule",
      "metadata": {
        "name": "WithLabels",
        "labels": {
          "Azure.WAF/pillar": "Security",
          "Azure.ASB.v3/control": [ "ID-1", "ID-2" ]
        }
      },
      "spec": { }
    }
    ```

=== "PowerShell"

    To specify labels in PowerShell, use the `-Labels` parameter:

    ```powershell
    # Synopsis: A rule with labels defined.
    Rule 'WithLabels' -Labels @{ 'Azure.WAF/pillar' = 'Security'; 'Azure.ASB.v3/control' = @('ID-1', 'ID-2') } {
        # Define conditions here
    } 
    ```

## Filtering with labels

A reason for assigning labels to rules is to perform filtering of rules to a specific subset.
This can be accomplished using baselines and the `spec.rule.labels` property.
For example:

```yaml
---
# Synopsis: A baseline which returns only security rules.
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline6
spec:
  rule:
    labels:
      Azure.WAF/pillar: [ 'Security' ]

---
# Synopsis: A baseline which returns any rules that are classified to Azure.WAF/pillar.
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline6
spec:
  rule:
    labels:
      Azure.WAF/pillar: '*'
```
