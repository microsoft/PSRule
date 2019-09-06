# PSRule_Baseline

## about_PSRule_Baseline

## SHORT DESCRIPTION

Describes usage of baselines within PSRule.

## LONG DESCRIPTION

PSRule lets you define a baseline.
A baseline includes a set of rule and configuration options that are used for evaluating objects.

The following baseline options can be configured:

- [Binding.IgnoreCase](#bindingignorecase)
- [Binding.TargetName](#bindingtargetname)
- [Binding.TargetType](#bindingtargettype)
- [Configuration](#configuration)
- [Rule.Include](#ruleinclude)
- [Rule.Exclude](#ruleexclude)
- [Rule.Tag](#ruletag)

Baseline options can be:

- Included as a baseline spec within a YAML file.
  - When using this method, multiple baseline specs can be defined within the same YAML file.
  - Each baseline spec is separated using `---`.
- Set within a workspace options file like `ps-rule.yaml`.
  - Only a single baseline can be specified using the `binding`, `configuration` and `rule` options.
  - See [about_PSRule_Options](about_PSRule_Options.md) for details on using this method.

### Baseline specs

A baseline spec is defined and saved within a YAML file with a `.rule.yaml` extension, for example `Baseline.rule.yaml`.

To define a baseline spec use the following structure:

```yaml
---
# Synopsis: <synopsis>
kind: Baseline
metadata:
  name: <name>
spec:
  # One or more baseline options
  binding: { }
  rule: { }
  configuration: { }
```

For example:

```yaml
---
# Synopsis: This is an example baseline
kind: Baseline
metadata:
  name: Baseline1
spec:
  binding:
    targetName:
    - Name
    - ResourceName
    - ResourceGroupName
    targetType:
    - ResourceType
  rule:
    include:
    - Rule1
    - Rule2
  configuration:
    allowedLocations:
    - 'Australia East'
    - 'Australia South East'

---
# Synopsis: This is an example baseline
kind: Baseline
metadata:
  name: Baseline2
spec:
  binding:
    targetName:
    - Name
    - ResourceName
    - ResourceGroupName
    targetType:
    - ResourceType
  rule:
    include:
    - Rule1
    - Rule3
  configuration:
    allowedLocations:
    - 'Australia East'
```

### Baseline scopes

When more then one baseline is available during execution, PSRule uses the following order to
determine precedence.

1. Parameter - `-Name` and `-Tag`.
2. Explicit - A named baseline called out with `-Baseline`.
3. Workspace - Included in `ps-rule.yaml` or specified on the command line with `-Option`.
4. Module - A baseline object included in a `.rule.yaml` file.

After precedence is determined, baselines are merged and null values are ignored, such that:

### Creating a module baseline

zzz

## EXAMPLES

### Example PSRule.yml

```yaml
#
# PSRule example configuration
#

# Configures binding
binding:
  ignoreCase: false
  targetName:
  - ResourceName
  - AlternateName
  targetType:
  - ResourceType
  - kind

# Adds rule configuration
configuration:
  appServiceMinInstanceCount: 2

# Configure rule filtering
rule:
  include:
  - rule1
  - rule2
  exclude:
  - rule3
  - rule4

```
