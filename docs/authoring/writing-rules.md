---
author: BernieWhite
---

# Writing rules

You can use PSRule to create tests for Infrastructure as Code (IaC).
Each test is called a _rule_.

PSRule allows you to write rules using YAML, JSON, or PowerShell.
Regardless of the format you choose, any combination of YAML, JSON, or PowerShell rules can be used together.

!!! Abstract
    This topic covers how to create a rule using YAML, JSON, and PowerShell by example.
    This example, while fictious is indicative of common testing and validation scenarios for IaC.

## Sample data

To get started authoring a rule, we will be working with a sample file `settings.json`.
This sample configuration file configures an application.

For the purpose of this example, one configuration setting `supportsHttpsTrafficOnly` is set.
This configuration setting can be either `true` or `false`.
When set to `true`, Transport Layer Security (TLS) is enforced.
When set to `false`, the application permits insecure communication with HTTP.

!!! Example "Contents of `settings.json`"

    Create a `settings.json` file in the root of your repository with the following contents.

    ```json
    {
        "type": "app1",
        "version": 1,
        "configure": {
            "supportsHttpsTrafficOnly": false
        }
    }
    ```

## Define a rule

To meet the requirements of our organization we want to write a rule to:

- Enforce secure traffic by requiring `supportsHttpsTrafficOnly` to be `true`.
- Enforce use of TLS 1.2 as a minimum by requiring `minTLSVersion` to be `1.2`.

In this section the same rule will be authored using YAML, JSON, and PowerShell.

!!! Tip
    To make you editing experience even better, consider installing the Visual Studio Code extension.

=== "YAML"

    Create a `.ps-rule/Local.Rule.yaml` file in your repository with the following contents.

    ```yaml
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
    spec:
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name.
    3.  The `condition` block determines the checks PSRule will use to test `settings.json`.
        Specifically, the object path `configures.supportsHttpsTrafficOnly` must exist and be set to `true`.

=== "JSON"

    Create a `.ps-rule/Local.Rule.jsonc` file in your repository with the following contents.

    ```json
    [
        {
            // Synopsis: An example rule to require TLS.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Rule",
            "metadata": {
                "name": "Local.JSON.RequireTLS"
            },
            "spec": {
                "condition": {
                    "field": "configure.supportsHttpsTrafficOnly",
                    "equals": true
                }
            }
        }
    ]
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name.
    3.  The `condition` block determines the checks PSRule will use to test `settings.json`.
        Specifically, the object path `configures.supportsHttpsTrafficOnly` must exist and be set to `true`.

=== "PowerShell"

    Create a `.ps-rule/Local.Rule.ps1` file in your repository with the following contents.

    ```powershell
    # Synopsis: An example rule to require TLS.
    Rule 'Local.PS.RequireTLS' {
        $Assert.HasFieldValue($TargetObject, 'configure.supportsHttpsTrafficOnly', $True)
    }
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name.
    3.  The condition contained within the curly braces `{ }` determines the checks PSRule will use to test `settings.json`.
    4.  The `$Assert.HasFieldValue` method checks the object path `configures.supportsHttpsTrafficOnly` exists and is set to `true`.

!!! Tip
    To learn more about recommended file and naming conventions for rules,
    continue reading [Storing and naming rules][1].

  [1]: storing-rules.md

## Using multiple conditions

Each rule must have at least one condition.
Additional conditions can be combined to check multiple test cases.

In the example a `minTLSVersion` configuration setting does not exist and is not set.

=== "YAML"

    Update `.ps-rule/Local.Rule.yaml` in your repository with the following contents.

    ```yaml hl_lines="9 12-13"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
    spec:
      condition:
        allOf:
        - field: 'configure.supportsHttpsTrafficOnly'
          equals: true
        - field: 'configure.minTLSVersion'
          equals: '1.2'
    ```

    1.  Using the `allOf` expression requires that all conditions be true for the rule to pass.
        This expression allows an array of one or more conditions to be provided.
        Using `anyOf` would pass the rule if any single condition is true.

=== "JSON"

    Update `.ps-rule/Local.Rule.jsonc` in your repository with the following contents.

    ```json hl_lines="11 16-19"
    [
        {
            // Synopsis: An example rule to require TLS.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Rule",
            "metadata": {
                "name": "Local.JSON.RequireTLS"
            },
            "spec": {
                "condition": {
                    "allOf": [
                        {
                            "field": "configure.supportsHttpsTrafficOnly",
                            "equals": true
                        },
                        {
                            "field": "configure.minTLSVersion",
                            "equals": "1.2"
                        }
                    ]
                }
            }
        }
    ]
    ```

    1.  Using the `allOf` expression requires that all conditions be true for the rule to pass.
        This expression allows an array of one or more conditions to be provided.
        Using `anyOf` would pass the rule if any single condition is true.

=== "PowerShell"

    Update `.ps-rule/Local.Rule.ps1` in your repository with the following contents.

    ```powershell hl_lines="4"
    # Synopsis: An example rule to require TLS.
    Rule 'Local.PS.RequireTLS' {
        $Assert.HasFieldValue($TargetObject, 'configure.supportsHttpsTrafficOnly', $True)
        $Assert.HasFieldValue($TargetObject, 'configure.minTLSVersion', '1.2')
    }
    ```

    1.  An additional, `$Assert.HasFieldValue` assertion helper method can be called.
        The rule will pass if all of the conditions return true.

## Testing

### Testing manually

To test the rule manually, run the following command.

```powershell
Assert-PSRule -f ./settings.json
```

## Advanced usage

### Severity level

When defining a rule, you can specify a severity level.
The severity level is used if the rule fails.
By default, the severity level for a rule is `Error`.

- `Error` - A serious problem that must be addressed before going forward.
- `Warning` - A problem that should be addressed.
- `Information` - A minor problem or an opportunity to improve the code.

In a continious integration (CI) pipeline, severity level is particularly important.
If any rule fails with a severity level of `Error` the pipeline will fail.
This helps prevent serious problems from being introduced into the code base or deployed.

The following example shows how to set the severity level to `Warning`.

=== "YAML"

    ```yaml hl_lines="8"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
    spec:
      level: Warning
      condition:
        allOf:
        - field: 'configure.supportsHttpsTrafficOnly'
          equals: true
        - field: 'configure.minTLSVersion'
          equals: '1.2'
    ```

=== "JSON"

    ```json hl_lines="10"
    [
        {
            // Synopsis: An example rule to require TLS.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Rule",
            "metadata": {
                "name": "Local.JSON.RequireTLS"
            },
            "spec": {
                "level": "Warning",
                "condition": {
                    "allOf": [
                        {
                            "field": "configure.supportsHttpsTrafficOnly",
                            "equals": true
                        },
                        {
                            "field": "configure.minTLSVersion",
                            "equals": "1.2"
                        }
                    ]
                }
            }
        }
    ]
    ```

=== "PowerShell"

    Update `.ps-rule/Local.Rule.ps1` in your repository with the following contents.

    ```powershell hl_lines="2"
    # Synopsis: An example rule to require TLS.
    Rule 'Local.PS.RequireTLS' -Level Warning {
        $Assert.HasFieldValue($TargetObject, 'configure.supportsHttpsTrafficOnly', $True)
        $Assert.HasFieldValue($TargetObject, 'configure.minTLSVersion', '1.2')
    }
    ```
