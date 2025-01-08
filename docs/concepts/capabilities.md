---
description: Capabilities are a way to declare or require a minimum set of features or functionality.
module: 3.0.0
---

# Capabilities

Capabilities are a way to declare or require a minimum set of features or functionality.
As PSRule evolves, new features or behaviors are added that may not be supported by previous versions.
By using capabilities, workspace and module authors can declare capabilities that are required.
Declaring capabilities provides a way to ensure consistent behavior across different configurations and versions of PSRule.

Each capability is a unique identifier that represents a feature or behavior of the PSRule engine.

!!! Note
    Capabilities are a new feature in PSRule v3 and are not supported in previous versions.
    Earlier versions of PSRule will not check for missing capabilities before executing rules or resources.

## Supported capabilities

The following capabilities are currently supported:

Identifier            | Description
----------            | -----------
`api-v1`              | YAML or JSON resource types that use the API version `github.com/microsoft/PSRule/v1`. This capability is always enabled in PSRule v3.
`api-2025-01-01`      | YAML or JSON resource types that use the API version `github.com/microsoft/PSRule/2025-01-01`. This capability is always enabled in PSRule v3.
`powershell-language` | Determines if the workspace or module uses language features such as rules and conventions that use PowerShell script. This capability is enabled unless the `Execution.RestrictScriptSource` option restricts the use of PowerShell script.

## Declaring capabilities

Declaring capabilities is optional.
When you do not declare capabilities, an unrecognized or disabled features is ignored.
However, please note the execution behavior may not be as expected if rules or resources depend on these features.

For example, if a you define a rule in PowerShell but PowerShell script is disabled, the rule will not be executed.

Workspace capabilities are declared in a `ps-rule.yaml` file using the `capabilities` key.

```yaml title="ps-rule.yaml"
capabilities:
  - api-v1
  - powershell-language
```

Module capabilities are declared in the `ModuleConfig` resource for the module using the `capabilities` key.

```yaml title="ModuleConfig.Rule.yaml"
apiVersion: github.com/microsoft/PSRule/2025-01-01
kind: ModuleConfig
metadata:
  name: MyModule
spec:
  capabilities:
    - api-v1
    - powershell-language
```
