# Sub-selectors

!!! Abstract
    This topic covers _sub-selectors_ which are a PSRule language feature specific to YAML and JSON expressions.
    They are useful for filtering out objects that you do not want to evaluate.
    Sub-selectors don't apply to script expressions because PowerShell already has rich support for filtering.

!!! Experimental
    _Sub-selectors_ are a work in progress and subject to change.
    We hope to add broader support, and more detailed documentation in the future.
    [Join or start a disucssion][1] to let us know how we can improve this feature going forward.

  [1]: https://github.com/microsoft/PSRule/discussions

Sub-selectors cover two (2) main scenarios:

- **Pre-conditions** &mdash; you want to filtering out objects before a rule is run.
- **Object filtering** &mdash; you want to limit a condition to specific elements in a list of items.

## Pre-conditions

PSRule can process many different types of objects.
Rules however, are normally written to test a specific property or type of object.
So it is important that rules only run on objects that you want to evaluate.
Pre-condition sub-selectors are one way you can determine if a rule should be run.

To use a sub-selector as a pre-condition, use the `where` property, directly under the `spec`.
The expressions in the sub-selector follow the same form that you can use in rules.

For example:

=== "YAML"

    ```yaml hl_lines="8-10"
    ---
    # Synopsis: A rule with a sub-selector precondition.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: Yaml.Subselector.Precondition
    spec:
      where:
        field: 'kind'
        equals: 'api'
      condition:
        field: resources
        count: 10
    ```

=== "JSON"

    ```json hl_lines="9-12"
    {
      // Synopsis: A rule with a sub-selector precondition.
      "apiVersion": "github.com/microsoft/PSRule/v1",
      "kind": "Rule",
      "metadata": {
        "name": "Json.Subselector.Precondition"
      },
      "spec": {
        "where": {
          "field": "kind",
          "equals": "api"
        },
        "condition": {
          "field": "resources",
          "count": 10
        }
      }
    }
    ```

In the example:

1. The `where` property is the start of a sub-selector.
2. The sub-selector checks if the `kind` property equals `api`.

The rule does not run if the:

- The object does not have a `kind` property. **OR**
- The value of the `kind` property is not `api`.

!!! Tip
    Other types of pre-conditions also exist that allow you to filter based on type or by a shared selector.

## Object filter

When you are evaluating an object, you can use sub-selectors to limit the condition.
This is helpful when dealing with properties that are a list of items.
Properties that contain a list of items may contain a sub-set of items that you want to evaluate.

For example, the object may look like this:

=== "YAML"

    ```yaml
    name: app1
    type: Microsoft.Web/sites
    resources:
    - name: web
      type: Microsoft.Web/sites/config
      properties:
        detailedErrorLoggingEnabled: true
    ```

=== "JSON"

    ```json
    {
      "name": "app1",
      "type": "Microsoft.Web/sites",
      "resources": [
        {
          "name": "web",
          "type": "Microsoft.Web/sites/config",
          "properties": {
            "detailedErrorLoggingEnabled": true
          }
        }
      ]
    }
    ```

A rule to test if any sub-resources with the `detailedErrorLoggingEnabled` set to `true` exist might look like this:

=== "YAML"

    ```yaml hl_lines="10-12"
    ---
    # Synopsis: A rule with a sub-selector filter.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: Yaml.Subselector.Filter
    spec:
      condition:
        field: resources
        where:
          type: '.'
          equals: 'Microsoft.Web/sites/config'
        allOf:
        - field: properties.detailedErrorLoggingEnabled
          equals: true
    ```

=== "JSON"

    ```json
    {
      // Synopsis: A rule with a sub-selector filter.
      "apiVersion": "github.com/microsoft/PSRule/v1",
      "kind": "Rule",
      "metadata": {
        "name": "Json.Subselector.Filter"
      },
      "spec": {
        "condition": {
          "field": "resources",
          "where": {
            "type": ".",
            "equals": "Microsoft.Web/sites/config"
          },
          "allOf": [
            {
              "field": "properties.detailedErrorLoggingEnabled",
              "equals": true
            }
          ]
        }
      }
    }
    ```

In the example:

- If the array property `resources` exists, any items with a type of `Microsoft.Web/sites/config` are evaluated.
  - Each item must have the `properties.detailedErrorLoggingEnabled` property set to `true` to pass.
  - Items without the `properties.detailedErrorLoggingEnabled` property fail.
  - Items with the `properties.detailedErrorLoggingEnabled` property set to a value other then `true` fail.
- If the `resources` property does not exist, the rule fails.
- If the `resources` property exists but has 0 items of type `Microsoft.Web/sites/config`, the rule fails.
- If the `resources` property exists and has any items of type `Microsoft.Web/sites/config` but any fail, the rule fails.
- If the `resources` property exists and has any items of type `Microsoft.Web/sites/config` and all pass, the rule passes.

### When there are no results

Given the example, is important to understand what happens if:

- The `resources` property doesn't exist.
- The `resources` property doesn't contain any items that match the sub-selector condition.

In either of these two cases, the sub-selector will return `false` and fail the rule.
The rule fails because there is no secondary conditions that could be used instead.

If this was not the desired behavior, you could:

- Use a pre-condition to avoid running the rule.
- Group the sub-selector into a `anyOf`, and provide a secondary condition.

For example:

=== "YAML"

    ```yaml hl_lines="9 11-14 19-20"
    ---
    # Synopsis: A rule with a sub-selector filter.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: Yaml.Subselector.FilterOr
    spec:
      condition:
        anyOf:

        - field: resources
          where:
            type: '.'
            equals: 'Microsoft.Web/sites/config'
          allOf:
          - field: properties.detailedErrorLoggingEnabled
            equals: true

        - field: resources
          exists: false
    ```

=== "JSON"

    ```json hl_lines="10 12-16 25-26"
    {
      // Synopsis: A rule with a sub-selector filter.
      "apiVersion": "github.com/microsoft/PSRule/v1",
      "kind": "Rule",
      "metadata": {
        "name": "Json.Subselector.FilterOr"
      },
      "spec": {
        "condition": {
          "anyOf": [
            {
              "field": "resources",
              "where": {
                "type": ".",
                "equals": "Microsoft.Web/sites/config"
              },
              "allOf": [
                {
                  "field": "properties.detailedErrorLoggingEnabled",
                  "equals": true
                }
              ]
            },
            {
              "field": "resources",
              "exists": false
            }
          ]
        }
      }
    }
    ```

In the example:

- If the array property `resources` exists, any items with a type of `Microsoft.Web/sites/config` are evaluated.
  - Each item must have the `properties.detailedErrorLoggingEnabled` property set to `true` to pass.
  - Items without the `properties.detailedErrorLoggingEnabled` property fail.
  - Items with the `properties.detailedErrorLoggingEnabled` property set to a value other then `true` fail.
- If the `resources` property does not exist, the rule passes.
- If the `resources` property exists but has 0 items of type `Microsoft.Web/sites/config`, the rule fails.
- If the `resources` property exists and has any items of type `Microsoft.Web/sites/config` but any fail, the rule fails.
- If the `resources` property exists and has any items of type `Microsoft.Web/sites/config` and all pass, the rule passes.
