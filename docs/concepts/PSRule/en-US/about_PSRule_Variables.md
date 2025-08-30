# PSRule_Variables

## about_PSRule_Variables

## SHORT DESCRIPTION

Describes the automatic variables that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule lets you define rules using PowerShell blocks.
A rule is defined within script files by using the `rule` keyword.

Within a rule definition, PSRule exposes a number of automatic variables that can be read to assist with rule execution.
Overwriting these variables or variable properties is not supported.

These variables are only available while `Invoke-PSRule` is executing.

The following variables are available for use:

- [$Assert](#assert)
- [$Configuration](#configuration)
- [$LocalizedData](#localizeddata)
- [$PSRule](#psrule)
- [$Rule](#rule)
- [$TargetObject](#targetobject)

### Assert

An assertion helper with methods to evaluate objects.
The `$Assert` object provides a set of built-in methods and provides a consistent variable for extension.

Each `$Assert` method returns an `AssertResult` object that contains the result of the condition.

The following built-in assertion methods are provided:

- `Contains` - The field value must contain at least one of the strings.
- `EndsWith` - The field value must match at least one suffix.
- `FileHeader` - The file must contain a comment header.
- `FilePath` - The file path must exist.
- `Greater` - The field value must be greater.
- `GreaterOrEqual` - The field value must be greater or equal to.
- `HasDefaultValue` - The object should not have the field or the field value is set to the default value.
- `HasField` - The object must have any of the specified fields.
- `HasFields` - The object must have all of the specified fields.
- `HasFieldValue` - The object must have the specified field and that field is not empty.
- `HasJsonSchema` - The object must reference a JSON schema with the `$schema` field.
- `In` - The field value must be included in the set.
- `IsArray` - The field value must be an array.
- `IsBoolean` - The field value must be a boolean.
- `IsInteger` - The field value must be an integer.
- `IsLower` - The field value must include only lowercase characters.
- `IsNumeric` - The field value must be a numeric type.
- `IsString` - The field value must be a string.
- `IsUpper` - The field value must include only uppercase characters.
- `JsonSchema` - The object must validate successfully against a JSON schema.
- `Less` - The field value must be less.
- `LessOrEqual` - The field value must be less or equal to.
- `Match` - The field value matches a regular expression pattern.
- `NotIn` - The field value must not be included in the set.
- `NotMatch` - The field value does not match a regular expression pattern.
- `NullOrEmpty` - The object must not have the specified field or it must be empty.
- `TypeOf` - The field value must be of the specified type.
- `StartsWith` - The field value must match at least one prefix.
- `Version` - The field value must be a semantic version string.

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
- Configuration values can be accessed through helper methods.

The following helper methods are available:

- `GetStringValues(string configurationKey)` - Returns an array of strings, based on `configurationKey`.

Syntax:

```powershell
$Configuration.<configurationKey>
```

```powershell
$Configuration.GetStringValues(<configurationKey>)
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

### PSRule

An object representing the current context during execution.

The following properties are available for read access:

- `Badges` - A helper to generate badges within PSRule.
  This property can only be called within the `-End` block of a convention.
- `Field` - A hashtable of custom bound fields.
  See option `Binding.Field` for more information.
- `Input` - Allows adding additional input paths to the pipeline.
- `Repository` - Provides access to information about the current repository.
- `Scope` - Any scopes assigned to the object currently being processed by the pipeline.
- `Source` - A collection of sources for the object currently being processed on the pipeline.
- `TargetObject` - The object currently being processed on the pipeline.
- `TargetName` - The name of the object currently being processed on the pipeline.
  This property will automatically default to `TargetName` or `Name` properties of the object if they exist.
- `TargetType` - The type of the object currently being processed on the pipeline.
  This property will automatically bind to `PSObject.TypeNames[0]` by default.
- `Output` - The output of all rules.
  This property can only be called within the `-End` block of a convention.

The following properties are available for read/ write access:

- `Data` - A hashtable of custom data.
This property can be populated during rule or begin/ process convention execution.
Custom data is not used by PSRule directly, and is intended to be used by downstream processes that need to interpret PSRule results.

To bind fields that already exist on the target object use custom binding and `Binding.Field`.
Use custom data to store data that must be calculated during rule execution.

The following helper methods are available:

- `GetContent(PSObject sourceObject)` - Returns the content of a file as one or more objects.
  The parameter `sourceObject` should be a `InputFileInfo`,`FileInfo`, or `Uri` object.
- `GetContentField(PSObject sourceObject, string field)` - Returns the content of a file as one or more objects.
  The parameter `sourceObject` should be a `InputFileInfo`,`FileInfo`, or `Uri` object.
  The parameter `field` is an field within each object to return.
  If the field does not exist on the object, an object is not returned.
- `GetContentFirstOrDefault(PSObject sourceObject)` - Returns the content of a file as on object.
  The parameter `sourceObject` should be a `InputFileInfo`,`FileInfo`, or `Uri` object.
  If more than one object is contained in the file, only the first object is returned.
  When the source file contains no objects null is returned.
- `Import(PSObject[] sourceObject)` - Imports one or more source objects into the pipeline.
  This method can only be called within the `-Initialize` or `-Begin` block of a convention.
  Use this method to expand an object into child objects that will be processed independently.
  Objects imported using this method will be excluded from the `Input.ObjectPath` option if set.
- `ImportWithType(string type, PSObject[] sourceObject)` - Imports one or more source objects into the pipeline.
  This method can only be called within the `-Initialize` or `-Begin` block of a convention.
  Use this method to expand an object into child objects that will be processed independently.
  Objects imported using this method will be excluded from the `Input.ObjectPath` option if set.
- `AddService(string id, object service)` - Add a service to the current context.
  The service can be retrieved using `$PSRule.GetService(id)`.
  The service object will be available to all rules and cleaned up after all rules are executed.
  Services should implement the `IDisposable` interface to perform additional cleanup.
  This method can only be called within the `-Initialize` block of a convention.
- `GetService(string id)` - Retrieves a service previously added by a convention.
- `GetPath(object sourceObject, string path)` - Evaluate an object path expression and returns the resulting objects.

The file format is detected based on the same file formats as the option `Input.Format`.
i.e. Yaml, Json, Markdown, and PowerShell Data.

Syntax:

```powershell
$PSRule
```

Examples:

```powershell
# Synopsis: This rule determines if the target object matches the naming convention
Rule 'NamingConvention' {
    $PSRule.TargetName.ToLower() -ceq $PSRule.TargetName
}
```

```powershell
# Synopsis: Use allowed environment tags
Rule 'CustomData' {
    Recommend 'Environment must be set to an allowed value'
    Within 'Tags.environment' 'production', 'test', 'development'

    if ($TargetObject.Tags.environment -in 'prod') {
        $PSRule.Data['targetEnvironment'] = 'production'
    }
    elseif ($TargetObject.Tags.environment -in 'dev', 'develop') {
        $PSRule.Data['targetEnvironment'] = 'development'
    }
    elseif ($TargetObject.Tags.environment -in 'tst', 'testing') {
        $PSRule.Data['targetEnvironment'] = 'test'
    }
}
```

### Rule

An object representing the current rule during execution.

The following properties are available for read access:

- `RuleName` - The name of the rule.
- `RuleId` - A unique identifier for the rule.

Syntax:

```powershell
$Rule
```

Examples:

```powershell
# Synopsis: This rule determines if the target object matches the naming convention
Rule 'resource.NamingConvention' {
    $PSRule.TargetName.ToLower() -ceq $PSRule.TargetName
}
```

### TargetObject

The value of the pipeline object currently being processed.
`$TargetObject` is set by using the `-InputObject` parameter of `Invoke-PSRule`.

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

An online version of this document is available at https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Variables/.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Invoke-PSRule/)

## KEYWORDS

- Assert
- Configuration
- LocalizedData
- PSRule
- Rule
- TargetObject
