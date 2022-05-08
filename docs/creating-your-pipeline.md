---
author: BernieWhite
---

# Creating your pipeline

You can use PSRule to test Infrastructure as Code (IaC) artifacts throughout their lifecycle.
By using validation within a continuous integration (CI) pipeline, any issues provide fast feedback.

Within the root directory of your IaC repository:

=== "GitHub Actions"

    Create a new GitHub Actions workflow by creating `.github/workflows/analyze-arm.yaml`.

    ```yaml
    name: Analyze templates
    on:
    - pull_request
    jobs:
      analyze_arm:
        name: Analyze templates
        runs-on: ubuntu-latest
        steps:

        - name: Checkout
          uses: actions/checkout@v3

        # Analyze Azure resources using PSRule for Azure
        - name: Analyze Azure template files
          uses: microsoft/ps-rule@v2.1.0
          with:
            modules: 'PSRule.Rules.Azure'
    ```

    This will automatically install compatible versions of all dependencies.

=== "Azure Pipelines"

    Create a new Azure DevOps YAML pipeline by creating `.azure-pipelines/analyze-arm.yaml`.

    ```yaml
    steps:

    # Analyze Azure resources using PSRule for Azure
    - task: ps-rule-assert@1
      displayName: Analyze Azure template files
      inputs:
        inputType: repository
        modules: 'PSRule.Rules.Azure'
    ```

    This will automatically install compatible versions of all dependencies.

=== "Generic with PowerShell"

    Create a pipeline in any CI environment by using PowerShell.

    ```powershell
    Install-Module -Name 'PSRule.Rules.Azure' -Scope CurrentUser -Force -ErrorAction Stop;
    Assert-PSRule -InputPath '.' -Module 'PSRule.Rules.Azure' -Format File -ErrorAction Stop;
    ```

!!! Tip
    This example demonstrates using PSRule for Azure, a populate module for testing Azure IaC.
    Instead, you can [write your own module][7] or use one of our [pre-built modules][6].

## Configuration

Configuration options for PSRule are set within the `ps-rule.yaml` file.

### Ignoring rules

To prevent a rule executing you can either:

- **Exclude rules by name** &mdash; The rule is not executed for any object.
- **Suppress rules by name** &mdash; The rule is not executed for a specific object by name.
- **Suppress rules by condition** &mdash; The rule is not executed for matching objects.

=== "Exclude by name"

    To exclude a rule, set `Rule.Exclude` option within the `ps-rule.yaml` file.

    [:octicons-book-24: Docs][3]

    ```yaml
    rule:
      exclude:
      # Ignore the following rules for all objects
      - Azure.VM.UseHybridUseBenefit
      - Azure.VM.Standalone
    ```

=== "Suppression by name"

    To suppress an individual rule, set `Suppression` option within the `ps-rule.yaml` file.

    [:octicons-book-24: Docs][4]

    ```yaml
    suppression:
      Azure.AKS.AuthorizedIPs:
      # Exclude the following externally managed AKS clusters
      - aks-cluster-prod-eus-001
      Azure.Storage.SoftDelete:
      # Exclude the following non-production storage accounts
      - storagedeveus6jo36t
      - storagedeveus1df278
    ```

=== "Suppression by condition"

    To suppress an rules by condition, create a suppression group.

    [:octicons-book-24: Docs][5]

    ```yaml
    ---
    # Synopsis: Ignore test objects by name.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: SuppressionGroup
    metadata:
      name: SuppressWithTargetName
    spec:
      rule:
      - 'FromFile1'
      - 'FromFile2'
      if:
        name: '.'
        in:
        - 'TestObject1'
        - 'TestObject2'
    ```

!!! tip
    Use comments within `ps-rule.yaml` to describe the reason why rules are excluded or suppressed.
    Meaningful comments help during peer review within a Pull Request (PR).
    Also consider including a date if the exclusions or suppressions are temporary.

  [3]: concepts/PSRule/en-US/about_PSRule_Options.md#ruleexclude
  [4]: concepts/PSRule/en-US/about_PSRule_Options.md#suppression
  [5]: concepts/PSRule/en-US/about_PSRule_SuppressionGroups.md
  [6]: addon-modules.md
  [7]: authoring/packaging-rules.md
