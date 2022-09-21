# Grouping rules

!!! Abstract
    _Taxa_ are additional metadata that can be used to classify rules.
    Together with tags they can be used to group or filter rules.

## Using taxa

When defining a rule you can specify taxa to classify or link rules using a framework or standard.
A single rule can be can linked to multiple taxa.
For example:

- The Azure Well-Architected Framework (WAF) defines pillars such as _Security_ and _Reliability_.
- The CIS Benchmarks define a number of control IDs such as _3.12_ and _13.4_.

=== "YAML"

    To specify taxa in YAML, use the `taxa` property:

    ```yaml
    ---
    # Synopsis: A rule with taxa defined.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: WithTaxa
      taxa:
        Azure.WAF/pillar: Security
        Azure.ASB.v3/control: [ 'ID-1', 'ID-2' ]
    spec: { }
    ```

=== "JSON"

    To specify taxa in JSON, use the `taxa` property:

    ```json
    {
      // Synopsis: A rule with taxa defined.
      "apiVersion": "github.com/microsoft/PSRule/v1",
      "kind": "Rule",
      "metadata": {
        "name": "WithTaxa",
        "taxa": {
          "Azure.WAF/pillar": "Security",
          "Azure.ASB.v3/control": [ "ID-1", "ID-2" ]
        }
      },
      "spec": { }
    }
    ```

=== "PowerShell"

    To specify taxa in PowerShell, use the `-Taxa` parameter:

    ```powershell
    # Synopsis: A rule with taxa defined.
    Rule 'WithTaxa' -Taxa @{ 'Azure.WAF/pillar' = 'Security'; 'Azure.ASB.v3/control' = @('ID-1', 'ID-2') } {
        # Define conditions here
    } 
    ```

## Filtering with taxa

A reason for assigning taxa to rules is to perform filtering of rules to a specific subset.
This can be accomplished using baselines and the `spec.rule.taxa` property.
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
    taxa:
      Azure.WAF/pillar: [ 'Security' ]

---
# Synopsis: A baseline which returns any rules that are classified to Azure.WAF/pillar.
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline6
spec:
  rule:
    taxa:
      Azure.WAF/pillar: '*'
```
