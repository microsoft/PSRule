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
- [Binding.PreferTargetInfo](about_PSRule_Options.md#bindingprefertargetinfo)
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
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: <name>
  annotations: { }
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
apiVersion: github.com/microsoft/PSRule/v1
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
apiVersion: github.com/microsoft/PSRule/v1
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

### Annotations

Additional baseline annotations can be provided as key/ value pairs.
Annotations can be used to provide additional information that is available in `Get-PSRuleBaseline` output.

The following reserved annotation exists:

- `obsolete` - Marks the baseline as obsolete when set to `true`.
PSRule will generate a warning when an obsolete baseline is used.

For example:

```yaml
---
# Synopsis: This is an example baseline that is obsolete
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: ObsoleteBaseline
  annotations:
    obsolete: true
spec: { }
```

## EXAMPLES

### Example Baseline.Rule.yaml

```yaml
# Example Baseline.Rule.yaml

---
# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline1
spec:
  binding:
    targetName:
    - AlternateName
    targetType:
    - kind
  rule:
    include:
    - 'WithBaseline'
  configuration:
    key1: value1

---
# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline2
spec:
  binding:
    targetName:
    - AlternateName
    targetType:
    - kind
  rule:
    include:
    - 'WithBaseline'
  configuration:
    key1: value1
```

## KEYWORDS

- Options
- PSRule
- Baseline
- Binding
