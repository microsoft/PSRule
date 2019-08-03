# PSRule_Variables

## about_PSRule_Variables

## SHORT DESCRIPTION

Describes the automatic variables that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule lets you define rules using PowerShell blocks. A rule is defined within script files by using the `rule` keyword.

Within a rule definition, PSRule exposes a number of automatic variables that can be read to assist with rule execution. Overwriting these variables or variable properties is not supported.

These variables are only available while `Invoke-PSRule` is executing.

The following variables are available for use:

- [$Assert](#assert)
- [$Configuration](#configuration)
- [$LocalizedData](#localizeddata)
- [$Rule](#rule)
- [$TargetObject](#targetobject)

### Assert

An assertion helper with methods to evaluate objects. The `$Assert` object provides a set of built-in methods and provides a consistent variable for extension.

Each `$Assert` method returns an `AssertResult` object that contains the result of the condition.

The following built-in assertion methods are provided:

- `HasField` - Asserts that the object must have the specified field.
- `HasFieldValue` - Asserts that the object must have the specified field and that field is not empty.
- `JsonSchema` - Asserts that the object must validate successfully against a JSON schema.
- `NullOrEmpty` - Asserts that the object must not have the specified field or it must be empty.

The `$Assert` variable can only be used within a rule definition block.

For detailed information on the assertion helper see [about_PSRule_Assert](about_PSRule_Assert.md).

Syntax:

```powershell
$Assert
```

Examples:

```powershell
# Synopsis: Determine if $TargetObject is valid against the provided schema
Rule 'UseJsonSchema' {
    $Assert.JsonSchema($TargetObject, 'schemas/PSRule-options.schema.json')
}
```

### Configuration

A dynamic object with properties names that map to configuration values set in the baseline.

When accessing configuration:

- Configuration keys are case sensitive.
- Configuration values are read only.

Syntax:

```powershell
$Configuration.<configurationKey>
```

Examples:

```powershell
# Synopsis: This rule uses a threshold stored as $Configuration.appServiceMinInstanceCount
Rule 'appServicePlan.MinInstanceCount' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/serverfarms' } {
    $TargetObject.Sku.capacity -ge $Configuration.appServiceMinInstanceCount
} -Configure @{ appServiceMinInstanceCount = 2 }
```

### LocalizedData

A dynamic object with properties names that map to localized data messages in a `.psd1` file.

When using localized data, PSRule loads localized strings as a hashtable from `PSRule-rules.psd1`.

The following logic is used to locate `PSRule-rules.psd1`:

- If the rules are loose (not part of a module), PSRule will search for `PSRule-rules.psd1` in the `.\<culture>\` subdirectory relative to where the rule script _.ps1_ file is located.
- When the rules are shipped as part of a module, PSRule will search for `PSRule-rules.psd1` in the `.\<culture>\` subdirectory relative to where the module manifest _.psd1_ file is located.

When accessing localized data:

- Message names are case sensitive.
- Message values are read only.

Syntax:

```powershell
$LocalizedData.<messageName>
```

Examples:

```powershell
# Data for rules stored in PSRule-rules.psd1
@{
    WithLocalizedDataMessage = 'LocalizedMessage for en-ZZ. Format={0}.'
}
```

```powershell
# Synopsis: Use -f to generate a formatted localized warning
Rule 'WithLocalizedData' {
    Write-Warning -Message ($LocalizedData.WithLocalizedDataMessage -f $TargetObject.Type)
}
```

This rule returns a warning message similar to:

```text
LocalizedMessage for en-ZZ. Format=TestType.
```

### Rule

An object representing the current object model of the rule during execution.

The following section properties are available for public read access:

- `RuleName` - The name of the rule.
- `RuleId` - A unique identifier for the rule.
- `TargetObject` - The object currently being processed on the pipeline.
- `TargetName` - The name of the object currently being processed on the pipeline. This property will automatically default to `TargetName` or `Name` properties of the object if they exist.
- `TargetType` - The type of the object currently being processed on the pipeline. This property will automatically bind to `PSObject.TypeNames[0]` by default.

Syntax:

```powershell
$Rule
```

Examples:

```powershell
# Synopsis: This rule determines if the target object matches the naming convention
Rule 'resource.NamingConvention' {
    $Rule.TargetName.ToLower() -ceq $Rule.TargetName
}
```

### TargetObject

The value of the pipeline object currently being processed. `$TargetObject` is set by using the `-InputObject` parameter of `Invoke-PSRule`.

When more than one input object is set, each object will be processed sequentially.

Syntax:

```powershell
$TargetObject
```

Examples:

```powershell
# Synopsis: Check that sku capacity is set to at least 2
Rule 'HasMinInstances' {
    $TargetObject.Sku.capacity -ge 2
}
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Variables.md.

## SEE ALSO

- [Invoke-PSRule](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md)

## KEYWORDS

- Assert
- Configuration
- LocalizedData
- Rule
- TargetObject
