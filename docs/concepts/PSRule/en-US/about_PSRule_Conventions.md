# PSRule_Conventions

## about_PSRule_Conventions

## SHORT DESCRIPTION

Describes PSRule Conventions including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When processing input it may be necessary to perform custom actions before or after rules execute.
Conventions provide an extensibility point that can be shipped with or external to standard rules.
Each convention, hooks into one or more places within the pipeline.

### Execution flow

Before defining a convention it is important to understand the high execution level flow of PSRule.
Which can be described as:

- Pipeline
  - Initialize:
    - Call convention initialize.
  - Begin.
  - Process:
    - Each object:
      - Each run:
        - Call convention begin.
        - Each rule.
        - Call convention process.
  - End:
    - Call convention end.

### Using conventions

A convention can be included by using the `-Convention` parameter when executing a PSRule cmdlet.
Alternatively, conventions can be included with options.
To use a convention specify the name of the convention by name.
For example:

```powershell
Invoke-PSRule -Convention 'ExampleConvention';
```

If multiple conventions are specified in an array, all are executed in they are specified.
As a result, the convention specified last may override state set by earlier conventions.

```powershell
Assert-PSRule -Convention 'ExampleConvention1', 'ExampleConvention2';
```

### Defining conventions

To define a convention, add a `Export-PSRuleConvention` block within a `.Rule.ps1` file.
The `.Rule.ps1` must be in an included path or module with `-Path` or `-Module`.

The `Export-PSRuleConvention` block works similar to the `Rule` block.
Each convention must have a unique name.
For example:

```powershell
# Synopsis: An example convention.
Export-PSRuleConvention 'ExampleConvention' {
    # Add code here
}
```

### Initialize Begin Process End blocks

Conventions define four executable blocks `Initialize`, `Begin`, `Process`, `End` similar to a PowerShell function.
Each block is injected in a different part of the pipeline as follows:

- `Initialize` occurs once globally at the beginning of the pipeline outside of a run.
  Use `Initialize` to perform any initialization, such as creating global objects.
- `Begin` occurs once per object per run before the any rules are executed.
  Use `Begin` blocks to perform expansion, set data, or alter the object before rules are processed.
- `Process` occurs once per object per run after all rules are executed.
  Use `Process` blocks to perform per object tasks such as generate badges.
- `End` occurs once globally at the end of the pipeline outside of a run.
  Use `End` to perform any finalization, such as upload results to an external service.

Convention block limitations:

- `Initialize` can not use automatic variables except `$PSRule`.
  Most methods and properties of `$PSRule` are not available in `Initialize`.
- `Begin` and `Process` can not use rule specific variables such as `$Rule`.
  These blocks are executed outside of the context of a single rule.
- `End` can not use automatic variables except `$PSRule`.
  Most methods and properties of `$PSRule` are not available in `End`.

By default, the `Process` block is used.
For example:

```powershell
# Synopsis: The default { } executes the process block
Export-PSRuleConvention 'ExampleConvention' {
    # Process block
}

# Synopsis: With optional -Process parameter name
Export-PSRuleConvention 'ExampleConvention' -Process {
    # Process block
}
```

To use `Initialize`, `Begin`, or `End` explicitly add these blocks.
For example:

```powershell
Export-PSRuleConvention 'ExampleConvention' -Process {
    # Process block
} -Begin {
    # Begin block
} -End {
    # End block
} -Initialize {
    # Initialize block
}
```

### Including with options

Conventions can be included by name within options in addition to using the `-Convention` parameter.
To specify a convention within YAML options use the following:

```yaml title="ps-rule.yaml"
convention:
  include:
  - 'ExampleConvention1'
  - 'ExampleConvention2'
```

### Using within modules

Conventions can be shipped within a module using the same packaging and distribution process as rules.
Additionally, conventions shipped within a module can be automatically included.
By default, PSRule does not include conventions shipped within a module.
To use a convention included in a module use the `-Convention` parameter or options configuration.

A module can automatically include a convention by specifying the convention by name in module configuration.
For example:

```yaml title="Config.Rule.yaml"
---
apiVersion: github.com/microsoft/PSRule/2025-01-01
kind: ModuleConfig
metadata:
  name: ExampleModule
spec:
  convention:
    include:
    - 'ExampleConvention1'
    - 'ExampleConvention2'
```

### Execution order

Conventions are executed in the order they are specified.
This is true for `Initialize`, `Begin`, `Process`, and `End` blocks.
i.e. In the following example `ExampleConvention1` is execute before `ExampleConvention2`.

```powershell
Assert-PSRule -Convention 'ExampleConvention1', 'ExampleConvention2';
```

When conventions are specified from multiple locations PSRule orders conventions as follows:

1. Using `-Convention` parameter.
2. PSRule options.
3. Module configuration.

## NOTE

An online version of this document is available at <https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Conventions/>.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Invoke-PSRule/)

## KEYWORDS

- Conventions
- PSRule
