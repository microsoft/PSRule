# PSRule_Rules

## about_PSRule_Rules

## SHORT DESCRIPTION

Describes PSRule rules including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input either from a file or PowerShell pipeline.
The PowerShell pipeline only available when running PSRule directly.
PSRule can also be run from a continuous integration (CI) pipeline or Visual Studio Code.
When using these methods, the PowerShell pipeline is not available.

To evaluate an object PSRule can use rules defined in script or YAML.

When using script rules:

- Each rule is defined PowerShell within a `.Rule.ps1` file by using a `Rule` block.
- PowerShell variables, functions, and cmdlets can be used just like regular PowerShell scripts.
- Built-in assertion helpers can be used to quickly build out rules.
- Pre-conditions can be defined with using a script block, type binding, or YAML-based selector.

To learn more about assertion helpers see [about_PSRule_Assert](about_PSRule_Assert.md).

When using YAML rules:

- Each rule is defined in a `.Rule.yaml` file by using the `Rule` resource.
- YAML-based expressions can be used.
- Pre-conditions can be defined with using a type binding, or YAML-based selector.

To learn more about YAML-based expressions see [about_PSRule_Expressions](about_PSRule_Expressions.md).

### Using pre-conditions

Pre-conditions are used to determine if a rule should be executed.
While pre-conditions are not required for each rule, it is a good practice to define them.
If a rule does not specify a pre-condition it may be executed against an object it does not expect.

Pre-conditions come in three forms:

- Script - A PowerShell script block that is executed and if true will cause the rule to be executed.
  Script block pre-conditions only work with script rules.
  To use a script block pre-condition, specify the `-If` script parameter on the `Rule` block.
- Type - A type string that is compared against the bound object type.
  When the type matches the rule will be executed.
  To use a type pre-conditions, specify the `-Type` script parameter or `type` YAML/JSON property.
- Selector - A YAML/JSON based expression that is evaluated against the object.
  When the expression matches the rule will be executed.
  To use a selector pre-conditions, specify the `-With` script parameter or `with` YAML/JSON property.

Different forms of pre-conditions can be combined.
When combining pre-conditions, different forms must be all true (logical AND).
i.e. Script AND Type AND Selector must be all be true for the rule to be executed.

Multiple Type and Selector pre-conditions can be specified.
If multiple Type and Selector pre-conditions are specified, only one must be true (logical OR).

For example:

```powershell
# Synopsis: An example script rule with pre-conditions.
Rule 'ScriptRule' -If { $True } -Type 'CustomType1', 'CustomType2' -With 'Selector.1', 'Selector.2' {
    # Rule condition
}
```

```yaml
---
# Synopsis: An example YAML rule with pre-conditions.
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'YamlRule'
spec:
  type:
  - 'CustomType1'
  - 'CustomType2'
  with:
  - 'Selector.1'
  - 'Selector.2'
  condition: { }
```

```json
[
  {
    // Synopsis: An example YAML rule with pre-conditions.
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Rule",
    "metadata": {
      "name": "YamlRule"
    },
    "spec": {
      "type": [
        "CustomType1",
        "CustomType2"
      ],
      "with": [
        "Selector.1",
        "Selector.2"
      ],
      "condition": {}
    }
  }
]
```

Pre-conditions are evaluated in the following order: Selector, Type, then Script.

### Defining script rules

To define a script rule use the `Rule` keyword followed by a name and a pair of squiggly brackets `{`.
Within the `{ }` one or more conditions can be used.
Script rule must be defined within `.Rule.ps1` files.
Multiple rules can be defined in a single file by creating multiple `Rule` blocks.
Rule blocks can not be nested within each other.

Within the `Rule` block, define one or more conditions to determine pass or fail of the rule.

Syntax:

```text
Rule [-Name] <string> [-Tag <hashtable>] [-When <string[]>] [-Type <string[]>] [-If <scriptBlock>] [-DependsOn <string[]>] [-Configure <hashtable>] [-ErrorAction <ActionPreference>] [-Body] {
    ...
}
```

Example:

```powershell
# Synopsis: Use a Standard load-balancer with AKS clusters.
Rule 'Azure.AKS.StandardLB' -Type 'Microsoft.ContainerService/managedClusters' -Tag @{ release = 'GA'; ruleSet = '2020_06' } {
    $Assert.HasFieldValue($TargetObject, 'Properties.networkProfile.loadBalancerSku', 'standard');
}
```

### Defining YAML rules

To define a YAML rule use the `Rule` resource in a YAML file.
Each rule must be defined within a `.Rule.yaml` file following a standard schema.
Multiple rules can be defined in a single YAML file by separating each rule with a `---`.

Within the `Rule` resource, the `condition` property specifies conditions to pass or fail the rule.

Syntax:

```yaml
---
# Synopsis: {{ Synopsis }}
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: '{{ Name }}'
  tags: { }
spec:
  type: [ ]
  with: [ ]
  condition: { }
```

Example:

```yaml
---
# Synopsis: Use a Standard load-balancer with AKS clusters.
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'Azure.AKS.StandardLB'
  tags:
    release: 'GA'
    ruleSet: '2020_06'
spec:
  type:
  - Microsoft.ContainerService/managedClusters
  condition:
    field: 'Properties.networkProfile.loadBalancerSku'
    equals: 'standard'
```

### Defining JSON rules

To define a JSON rule use the `Rule` resource in a JSON file.
Each rule must be defined within a `.Rule.jsonc` file following a standard schema.
One or more rules can be defined in a single JSON array separating each rule in a JSON object.

Within the `Rule` resource, the `condition` property specifies conditions to pass or fail the rule.

Use the `.jsonc` extension to view [JSON with Comments](https://code.visualstudio.com/docs/languages/json#_json-with-comments) in Visual Studio Code.

Syntax:

```json
[
  {
    // Synopsis: {{ Synopsis }}
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Rule",
    "metadata": {
      "name": "{{ Name }}",
      "tags": {}
    },
    "spec": {
      "type": [],
      "with": [],
      "condition": {}
    }
  }
]
```

Example:

```json
[
  {
    // Synopsis: Use a Standard load-balancer with AKS clusters.
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Rule",
    "metadata": {
      "name": "Azure.AKS.StandardLB",
      "tags": {
        "release": "GA",
        "ruleSet": "2020_06"
      }
    },
    "spec": {
      "type": [
        "Microsoft.ContainerService/managedClusters"
      ],
      "condition": {
        "field": "Properties.networkProfile.loadBalancerSku",
        "equals": "standard"
      }
    }
  }
]
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/concepts/PSRule/en-US/about_PSRule_Rules.md.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- Rules
- Expressions
- PSRule
