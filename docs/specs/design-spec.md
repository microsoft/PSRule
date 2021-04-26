# PSRule design specification (draft)

This document is intended as a working technical specification for PSRule.

## What is PSRule?

PSRule is an engine, shipped as a PowerShell module designed to validate infrastructure as code (IaC).
Additionally, PSRule can validate any PowerShell object, allowing almost any custom scenario to be supported.

PSRule natively supports common infrastructure code artifacts with the following file formats:

- YAML (`.yaml` or `.yml`).
- JSON (`.json`).
- PowerShell Data Files (`.psd1`).
- Markdown front matter (`.md` or `.markdown`).

While some infrastructure as code languages implement their own custom language,
many support output into a standard artifact format.
i.e. `terraform show -json`

## Project objectives

1. **Extensible**:
   - Provide an execution environment (tools and language) to validate infrastructure code.
   - Handling of common concerns such as input/ output/ reporting should be handled by the engine.
   - Language must be flexible enough to support a wide range of use cases.
2. **DevOps**:
   - Validation should support and enhance DevOps workflows by providing fast feedback in pull requests.
   - Allow quality gates to be implemented between environments such development, test, and production.
3. **Cross-platform**:
   - A wide range of platforms can be used to author and deploy infrastructure code.
PSRule must support rule validation and authoring on Linux, MacOS, and Windows.
   - Runs in a Linux container.
For continuous integration (CI) systems that do not support PowerShell, run in a container.
4. **Reusable**:
   - Validation should plug and play, reusable across teams and organizations.
   - Any reusable validation will have exceptions.
Rules must be able to be disabled where they are not applicable.

## Language specification

PSRule is rooted in PowerShell.
This provides significant benefits and flexibility including:

- Reuses existing skills within Microsoft and customers who already know how to author PowerShell scripts.
- Builds on existing PowerShell community; allowing existing integrations and cmdlets to be used.
- PowerShell already has an established model for distributing packages (modules).
This includes options for trust and hosting (publicly or privately).

To ensure these benefits remain, the following must be true:

- Rules can be written using standard PowerShell operators and conventions.
Minimal knowledge of PSRule should be required to author rules.
- Rules validate an object graph.
Whether an object originates from a YAML or JSON file should be abstract.

### Future cases

PowerShell offers complete flexibility to build simple to complex rules.
However, rule authors may be unfamiliar with PowerShell.
Authoring rules in YAML or JSON with a defined schema will allow additional options for basic rules.

## Concepts

### Rule definitions

Rule definitions or _rules_ are defined using PowerShell.
Rules can be created in a PowerShell script file with the `.Rule.ps1` extension.
Rule files can be created and used individually or bundled as a module.

Each rule:

- Implements a test for one or more conditions against an object.
When all conditions return true the rule passes, if not the rule fails.
- Is evaluated by executing the rule within a sandbox that provides context to each rule.
Such as the deserialized object being processed and configuration.
- Can specify preconditions which determine if a rule should be evaluated based on the object being processed.
Rules only run against the objects they are designed to test.

For example:

```powershell
Rule 'NameOfRule' {
    # <Rule conditions>
}
```

```powershell
# Synopsis: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'StorageAccounts.UseHttps' -Type 'Microsoft.Storage/storageAccounts' {
    $TargetObject.Properties.supportsHttpsTrafficOnly -eq $True
}
```

### PSRule engine

Distributed as a PowerShell module, all source code is included within this repository.
This included cmdlets and an execution sandbox where rules are evaluated.
PSRule is designed to be self contained, requiring only PowerShell to run.

By itself PSRule doesn't implement any specific rules.
Custom rules can be defined and evaluated from PowerShell script files.
Reusable rules can be distributed using PowerShell modules.
Use _PowerShellGet_ to install modules from public and private sources.

PSRule extends PowerShell with domain specific language (DSL) keywords, cmdlets, and automatic variables.
These language features are only available within the sandbox.
Stubs are exported from PSRule module to provide command completion during authoring.

In additional to rules, a number of resources can be used within PSRule.
Resources are defined in YAML files with the `.Rule.yaml` file extension.
You can create one or many resource within a single `.Rule.yaml` file.

PSRule supports the following resources:

- `Baseline` - A reusable group of rules and configuration defaults for a given scenario.
- `Selector` - A reusable filter to determine which objects a rule should be run against.

A special `ModuleConfig` resource can also be defined to configure defaults for a module.

### Keywords and variables

TBA

### Baselines

A baseline is a resource defined in YAML that determines which rules to run for a given scenario.
One or more baselines can be included in a `.Rule.yaml` file.
Baselines can be created individually or bundled in a module.

Common use cases where baselines are helpful include:

- Separation of rules or features in development.
For infrastructure code or rules early in their lifecycle, a recommend practice may not be fully ratified.
Baselines allow rules to be distributed but not executed by default.
- Progressive adoption.
If validation has been added for a new use case, it may not be possible to adopt all rules at once.
Baselines act as checkpoints to allow validation of a subset of rules.

## Execution

Execution within PSRule occurs within a pipeline.
The PSRule pipeline is similar to PowerShell and contains a `begin`, `process`, and `end` stage.

- **Begin**: TBA
- **Process**: TBA
- **End**: TBA

Three execution pipelines exist, `Invoke`, `Assert`, or `Test`.
Differences between each of these pipeline is minimal and mostly deals with how output is presented.

- **Invoke**: Returns output as pipeline objects so they can be natively processed by PowerShell code.
- **Assert**: Returns output as styled text to provide readable results within a CI pipeline.
- **Test**: Returns true or false based on pass or fail of each object.
Use this option to use filter objects from a PowerShell pipeline.

### Execution sandbox

The execution sandbox is implemented using PowerShell runspaces.
Runspaces are a PowerShell feature which enable partial isolation within a PowerShell process.

PSRule uses two discrete runspaces:

- In the _parent_ runspace where PSRule is called using `Invoke-PSRule`, `Assert-PSRule`, or `Test-PSRule`.
The _parent_ runspace is responsible for all input and output.
- The _sandbox_ runspace is where rules execute.
PSRule keywords and automatic variables are implemented in the sandbox.
Flow control within the PSRule pipeline maintains context for each object as it is processed by rules.

Input and output are proxied between the two discrete runspaces to maintain runspace separation.
This separation allows rules to be executed without polluting the state of the _parent_ runspace.

### Rule evaluation

TBA

## Configuration

PSRule has built-in support for configuration of the engine and rules.
Configuration can be set by:

- Configuring the default `ps-rule.yaml` file.
- Setting at runtime by passing a `-Option` parameter to PSRule cmdlets.

### Engine options

Configuration of the PSRule engine is referred to as options.
Each option changes the default that PSRule uses during execution.
The supported options that can be configured for PSRule are described in the [about_PSRule_Options] topic.

### Rule configuration

Separately, rules can optionally define configuration that can skip or change the rule conditions.
Rule configuration is a key/ value pair.

## Integration

TBA

[about_PSRule_Options]: ../concpets/PSRule/en-US/about_PSRule_Options.md
