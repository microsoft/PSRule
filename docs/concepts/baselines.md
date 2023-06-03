# Baselines

!!! Abstract
    A _baseline_ is a set of rules and configuration options.
    You can define a named baseline to run a set of rules for a specific use case.

Baselines cover two (2) main scenarios:

- **Rules** &mdash; PSRule supports running rules by name or tag.
  However, when working with a large number of rules it is often easier to group and run rules based on a name.
- **Configuration** &mdash; A baseline allows you to run any included rules with a predefined configuration by name.

## Defining baselines

A baseline is defined as a resource within YAML or JSON.
Baselines can be defined side-by-side with rules you create or included separately as a custom baseline.

Continue reading [baseline][1] reference.

  [1]: ./PSRule/en-US/about_PSRule_Baseline.md

## Baseline groups

:octicons-milestone-24: v2.9.0

In addition to regular baselines, you can use a baseline group to provide a friendly name to an existing baseline.
A baseline groups are set by configuring the [Baseline.Group][2] option.

!!! Experimental
    _Baseline groups_ are a work in progress and subject to change.
    Currently, _baseline groups_ allow only a single baseline to be referenced.
    [Join or start a disucssion][3] to let us know how we can improve this feature going forward.

!!! Tip
    You can use baseline groups to reference a baseline.
    If a new baseline is made available in the future, update your baseline group in one place to start using the new baseline.

In the following example, two baseline groups `latest` and `preview` are defined:

```yaml title="ps-rule.yaml"
baseline:
  group:
    latest: PSRule.Rules.Azure\Azure.GA_2023_03
    preview: PSRule.Rules.Azure\Azure.Preview_2023_03
```

- The `latest` baseline group is set to `Azure.GA_2023_03` within the `PSRule.Rules.Azure` module.
- The `preview` baseline group is set to `Azure.Preview_2023_03` within the `PSRule.Rules.Azure` module.

To use the baseline group, prefix the group name with `@` when running PSRule.
For example:

=== "GitHub Actions"

    ```yaml
    - name: Run PSRule
      uses: microsoft/ps-rule@v2.8.1
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
