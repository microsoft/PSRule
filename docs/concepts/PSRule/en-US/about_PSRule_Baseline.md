# PSRule_Baseline

## about_PSRule_Baseline

## SHORT DESCRIPTION

Describes usage of baselines within PSRule.

## LONG DESCRIPTION

PSRule lets you define a baseline.
A baseline includes a set of rule and configuration options that are used for evaluating objects.

The following baseline options can be configured:

- [Binding.Field](about_PSRule_Options.md#bindingfield)
- [Binding.IgnoreCase](about_PSRule_Options.md#bindingignorecase)
- [Binding.NameSeparator](about_PSRule_Options.md#bindingnameseparator)
- [Binding.TargetName](about_PSRule_Options.md#bindingtargetname)
- [Binding.TargetType](about_PSRule_Options.md#bindingtargettype)
- [Binding.UseQualifiedName](about_PSRule_Options.md#bindingusequalifiedname)
- [Configuration](about_PSRule_Options.md#configuration)
- [Rule.Include](about_PSRule_Options.md#ruleinclude)
- [Rule.Exclude](about_PSRule_Options.md#ruleexclude)
- [Rule.Tag](about_PSRule_Options.md#ruletag)

Baseline options can be:

- Included as a baseline spec within a YAML file.
  - When using this method, multiple baseline specs can be defined within the same YAML file.
  - Each baseline spec is separated using `---`.
- Set within a workspace options file like `ps-rule.yaml`.
  - Only a single baseline can be specified.
  - See [about_PSRule_Options](about_PSRule_Options.md) for details on using this method.

### Baseline specs

A baseline spec is defined and saved within a YAML file with a `.Rule.yaml` extension, for example `Baseline.Rule.yaml`.

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
    field:
      id:
      - ResourceId
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

When baseline options are set, PSRule uses the following order to determine precedence.

1. Parameter - `-Name` and `-Tag`.
2. Explicit - A named baseline specified with `-Baseline`.
3. Workspace - Included in `ps-rule.yaml` or specified on the command line with `-Option`.
4. Module - A baseline object included in a `.Rule.yaml` file.

After precedence is determined, baselines are merged and null values are ignored, such that:

## EXAMPLES

### Example ps-rule.yaml

```yaml
#
# PSRule example configuration
#

# Configures binding
binding:
  ignoreCase: false
  field:
    id:
    - ResourceId
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
