# PSRule_Baseline

## about_PSRule_Baseline

## SHORT DESCRIPTION

Describes usage of baselines within PSRule.

## LONG DESCRIPTION

PSRule lets you define a baseline.
A baseline can include a set of rule and configuration options that are used for evaluating objects.

The following baseline options can be configured:

- [Binding](#binding)
- [Configuration](#configuration)
- [Rule](#rule)

Baseline options can be:

- Included in a YAML file within a baseline spec.
- Set within a workspace options file like `ps-rule.yaml`.

### Baseline specs

A baseline spec is defined and saved within a YAML file with a `.rule.yaml` extension, for example `Baseline.rule.yaml`.

To define a baseline spec use the following structure:

```yaml

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
