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

          # Checkout the repository
          - name: Checkout
            uses: actions/checkout@v4

          # Run PSRule
          - name: Analyze with PSRule
            uses: microsoft/ps-rule@v3.0.0
            with:
              modules: PSRule.Rules.Azure
    ```

    This will automatically install compatible versions of all dependencies.

=== "Azure Pipelines"

    Create a new Azure DevOps YAML pipeline by creating `.azure-pipelines/analyze-arm.yaml`.

    ```yaml
    steps:

      # Checkout the repository
      - checkout: self

      # Run PSRule
      - task: ps-rule-assert@3
        displayName: Analyze with PSRule
        inputs:
          modules: PSRule.Rules.Azure
    ```

    This will automatically install compatible versions of all dependencies.

=== "Generic with PowerShell"

    Create a pipeline in any CI environment by using PowerShell.

    ```powershell
    $modules = @('PSRule.Rules.Azure')
    Install-Module -Name $modules -Scope CurrentUser -Force -ErrorAction Stop;
    Assert-PSRule -InputPath '.' -Module $modules -Format File -ErrorAction Stop;
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

    ```yaml title="ps-rule.yaml"
    rule:
      exclude:
      # Ignore the following rules for all objects
      - Azure.VM.UseHybridUseBenefit
      - Azure.VM.Standalone
    ```

=== "Suppression by name"

    To suppress an individual rule, set `Suppression` option within the `ps-rule.yaml` file.

    [:octicons-book-24: Docs][4]

    ```yaml title="ps-rule.yaml"
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
    apiVersion: github.com/microsoft/PSRule/2025-01-01
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

!!! Tip
    Use comments within `ps-rule.yaml` to describe the reason why rules are excluded or suppressed.
    Meaningful comments help during peer review within a Pull Request (PR).
    Also consider including a date if the exclusions or suppressions are temporary.

  [3]: concepts/PSRule/en-US/about_PSRule_Options.md#ruleexclude
  [4]: concepts/PSRule/en-US/about_PSRule_Options.md#suppression
  [5]: concepts/PSRule/en-US/about_PSRule_SuppressionGroups.md
  [6]: addon-modules.md
  [7]: authoring/packaging-rules.md

### Processing changed files only

:octicons-milestone-24: v2.5.0 · [:octicons-book-24: Docs][8]

To only process files that have changed within a pull request, set the `Input.IgnoreUnchangedPath` option.
This option does not work with a shallow or detached checkout, full git history is required for comparison.

=== "GitHub Actions"

    Update your GitHub Actions workflow by setting the `PSRULE_INPUT_IGNOREUNCHANGEDPATH` environment variable.

    ```yaml title=".github/workflows/analyze-arm.yaml"
    name: Analyze templates
    on:
    - pull_request
    jobs:
      analyze_arm:
        name: Analyze templates
        runs-on: ubuntu-latest
        steps:

          # Checkout the repository
          - name: Checkout
            uses: actions/checkout@v4
            with:
              fetch-depth: 0 # (1)

          # Run PSRule
          - name: Analyze with PSRule
            uses: microsoft/ps-rule@v3.0.0
            with:
              modules: PSRule.Rules.Azure
            env:
              PSRULE_INPUT_IGNOREUNCHANGEDPATH: true # (2)
    ```

    <div class="result" markdown>
    1.  Checkout the repository with full history. By default, GitHub Actions will only fetch the latest commit.
    2.  Enable processing of changed files only.

    </div>

=== "Azure Pipelines"

    Update your Azure DevOps YAML pipeline by setting the `PSRULE_INPUT_IGNOREUNCHANGEDPATH` environment variable.

    ```yaml title=".azure-pipelines/analyze-arm.yaml"
    steps:

      # Checkout the repository
      - checkout: self
        fetchDepth: 0 # (1)

      # Run PSRule
      - task: ps-rule-assert@3
        displayName: Analyze with PSRule
        inputs:
          modules: PSRule.Rules.Azure
        env:
          PSRULE_INPUT_IGNOREUNCHANGEDPATH: true # (2)
    ```

    <div class="result" markdown>
    1.  Checkout the repository with full history. By default, Azure Pipelines will only fetch the latest commit.
    2.  Enable processing of changed files only.

    </div>

=== "Generic with PowerShell"

    Update your PowerShell command-line to include the `Input.IgnoreUnchangedPath` option.

    ```powershell title="PowerShell"
    $modules = @('PSRule.Rules.Azure')
    $options = @{
        'Input.IgnoreUnchangedPath' = $True
    }
    Install-Module -Name $modules -Scope CurrentUser -Force -ErrorAction Stop;
    Assert-PSRule -Options $options -InputPath '.' -Module $modules -Format File -ErrorAction Stop;
    ```

!!! Tip
    In some cases it may be necessary to set `Repository.BaseRef` to the default branch of your repository.
    By default, PSRule will detect the default branch of the repository from the build system environment variables.

  [8]: concepts/PSRule/en-US/about_PSRule_Options.md#inputignoreunchangedpath
