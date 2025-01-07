---
description: A baseline is a set of rules and configuration options. You can define a named baseline to run a set of rules for a specific use case.
---

# Baselines

A _baseline_ is a set of rules and configuration options.
You can define a named baseline to run a set of rules for a specific use case.

Baselines cover two (2) main scenarios:

- **A collection of rules** &mdash; Baselines allow you to define a set of rules that will run each time the baseline is specified.
- **Paired with configuration** &mdash; Baselines allow you to pair configuration so rules run consistently.

## Using baselines

To use a baseline, specify it by name when you run PSRule.
For example:

=== "GitHub Actions"

    ```yaml
    - name: Run PSRule
      uses: microsoft/ps-rule@v2.9.0
      with:
        modules: 'PSRule.Rules.Azure'
        baseline: 'Azure.GA_2024_09'
    ```

=== "Azure Pipelines"

    ```yaml
    - task: ps-rule-assert@2
      displayName: Run PSRule
      inputs:
        modules: 'PSRule.Rules.Azure'
        baseline: 'Azure.GA_2024_09'
    ```

=== "Generic with PowerShell"

    ```powershell
    Assert-PSRule -InputPath '.' -Baseline 'Azure.GA_2024_09' -Module PSRule.Rules.Azure;
    ```

### Baseline groups

:octicons-milestone-24: v2.9.0

In addition to specifying a baseline by name you can use a baseline group.
A baseline group provides an alternative name to an existing baseline.
Baseline groups allowing you to decouple pipeline configuration from the baseline name when it changes often.

A baseline groups are set by configuring the [Baseline.Group][2] option.

!!! Experimental
    _Baseline groups_ are a work in progress and subject to change.
    Currently, _baseline groups_ allow only a single baseline to be referenced.
    [Join or start a discussion][3] to let us know how we can improve this feature going forward.

!!! Tip
    You can use baseline groups to reference a baseline.
    If a new baseline is made available in the future, update your baseline group in one place to start using the new baseline.

In the following example, two baseline groups `latest` and `preview` are defined:

```yaml title="ps-rule.yaml"
baseline:
  group:
    latest: PSRule.Rules.Azure\Azure.GA_2024_09
    preview: PSRule.Rules.Azure\Azure.Preview_2024_09
```

- The `latest` baseline group is set to `Azure.GA_2024_09` within the `PSRule.Rules.Azure` module.
- The `preview` baseline group is set to `Azure.Preview_2024_09` within the `PSRule.Rules.Azure` module.

To use the baseline group, prefix the group name with `@` when running PSRule.
For example:

=== "GitHub Actions"

    ```yaml
    - name: Run PSRule
      uses: microsoft/ps-rule@v2.9.0
      with:
        modules: 'PSRule.Rules.Azure'
        baseline: '@latest'
    ```

=== "Azure Pipelines"

    ```yaml
    - task: ps-rule-assert@2
      displayName: Run PSRule
      inputs:
        modules: 'PSRule.Rules.Azure'
        baseline: '@latest'
    ```

=== "Generic with PowerShell"

    ```powershell
    Assert-PSRule -InputPath '.' -Baseline '@latest' -Module PSRule.Rules.Azure -Format File;
    ```

  [2]: ./PSRule/en-US/about_PSRule_Options.md#baselinegroup
  [3]: https://github.com/microsoft/PSRule/discussions

## Defining baselines

A baseline is defined as a resource within YAML or JSON.
Custom baselines can be defined side-by-side with rules you create or included separately.

Continue reading [baseline][1] reference.

  [1]: ./PSRule/en-US/about_PSRule_Baseline.md
