# PSRule_Variables

## about_PSRule_Variables

## SHORT DESCRIPTION

Describes the automatic variables that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule lets you define rules using PowerShell blocks. A rule is defined within script files by using the `rule` keyword.

Within a rule definition, PSRule exposes a number of automatic variables that can be read to assist with rule execution. Overwriting these variables or variable properties is not supported.

These variables are only available while `Invoke-PSRule` is executing.

The following variables are available for use:

- [$Rule](#rule)
- [$TargetObject](#targetobject)

### Rule

An object representing the current object model of the rule during execution.

The following section properties are available for public read access:

- `RuleName` - The name of the rule.
- `RuleId` - A unique identifier for the rule.
- `TargetObject` - The object currently being processed on the pipeline.
- `TargetName` - The name of the object currently being processed on the pipeline. This property will automatically bind to `TargetName` or `Name` properties of the object if they exist.

Syntax:

```powershell
$Rule
```

### TargetObject

The value of the pipeline object currently being processed. `$TargetObject` is set by using the `-InputObject` parameter of `Invoke-PSRule`.

When more than one input object is set, each object will be processed sequentially.

Syntax:

```powershell
$TargetObject
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Variables.md.

## SEE ALSO

- [Invoke-PSRule](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md)

## KEYWORDS

- Rule
- TargetObject
