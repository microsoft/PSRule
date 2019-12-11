# PSRule_Assert

## about_PSRule_Assert

## SHORT DESCRIPTION

Describes the assertion helper that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule includes an assertion helper exposed as a built-in variable `$Assert`. The `$Assert` object provides a consistent set of methods to evaluate objects.

Each `$Assert` method returns an `AssertResult` object that contains the result of the assertion.

The following built-in assertion methods are provided:

- [Contains](#contains) - The field value must contain at least one of the strings.
- [EndsWith](#endswith) - The field value must match at least one suffix.
- [HasDefaultValue](#hasdefaultvalue) - The object should not have the field or the field value is set to the default value.
- [HasField](#hasfield) - The object must have the specified field.
- [HasFieldValue](#hasfieldvalue) - The object must have the specified field and that field is not empty.
- [JsonSchema](#jsonschema) - The object must validate successfully against a JSON schema.
- [NullOrEmpty](#nullorempty) - The object must not have the specified field or it must be empty.
- [StartsWith](#startswith) - The field value must match at least one prefix.
- [Version](#version) - The field value must be a semantic version string.

The `$Assert` variable can only be used within a rule definition block.

### Using assertion methods

An assertion method can be used like other methods in PowerShell. i.e. `$Assert.methodName(parameters)`.

Assertion methods use the following standard pattern:

- The first parameter is _always_ the input object of type `PSObject`, additional parameters can be included based on the functionality required by the method.
  - In many cases the input object will be `$TargetObject`, however assertion methods must not assume that `$TargetObject` will be used.
  - Assertion methods must a `$Null` input object.
- Assertion methods return the `AssertResult` object that is interpreted by the rule pipeline.

Some assertion methods may overlap or provide similar functionality to built-in keywords. Where you have the choice, use built-in keywords.
Use assertion methods for advanced cases or increased flexibility.

In the following example, `Assert.HasFieldValue` asserts that `$TargetObject` should have a field named `Type` with a non-empty value.

```powershell
Rule 'Assert.HasTypeField' {
    $Assert.HasFieldValue($TargetObject, 'Type')
}
```

To find perform multiple assertions use.

```powershell
Rule 'Assert.HasRequiredFields' {
    $Assert.HasFieldValue($TargetObject, 'Name')
    $Assert.HasFieldValue($TargetObject, 'Type')
    $Assert.HasFieldValue($TargetObject, 'Value')
}
```

### Contains

The `Contains` assertion method checks the field value contains with the specified string.
Optionally a case-sensitive compare can be used, however case is ignored by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `text` - One or more strings to compare the field value with. Only one string must match.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value. Case is ignored by default.

Examples:

```powershell
Rule 'Contains' {
    $Assert.Contains($TargetObject, 'ResourceGroupName', 'prod')
    $Assert.Contains($TargetObject, 'Name', @('prod', 'test'), $True)
}
```

### EndsWith

The `EndsWith` assertion method checks the field value ends with the specified suffix.
Optionally a case-sensitive compare can be used, however case is ignored by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `suffix` - One or more suffixes to compare the field value with. Only one suffix must match.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value. Case is ignored by default.

Examples:

```powershell
Rule 'EndsWith' {
    $Assert.EndsWith($TargetObject, 'ResourceGroupName', 'eus')
    $Assert.EndsWith($TargetObject, 'Name', @('db', 'web'), $True)
}
```

### HasDefaultValue

The `HasDefaultValue` assertion method check that the field does not exist or the field value is set to the default value.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `defaultValue` - The expected value if the field exists.

This assertion will pass if:

- The field does not exist.
- The field value is set to `defaultValue`.

This assertion will fail if:

- The field value is set to a value different from `defaultValue`.

Examples:

```powershell
Rule 'HasDefaultValue' {
    $Assert.HasDefaultValue($TargetObject, 'Properties.osProfile.linuxConfiguration.provisionVMAgent', $True)
}
```

### HasField

The `HasField` assertion method checks the object has the specified field.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. By default, a case insensitive compare is used.
- `caseSensitive` (optional) - Use a case sensitive compare of the field name.

Examples:

```powershell
Rule 'HasField' {
    $Assert.HasField($TargetObject, 'Name')
    $Assert.HasField($TargetObject, 'tag.Environment', $True)
}
```

### HasFieldValue

The `HasFieldValue` assertion method checks the field value of the object is not empty.

A field value is empty if any of the following are true:

- The field does not exist.
- The field value is `$Null`.
- The field value is an empty array or collection.
- The field value is an empty string `''`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `expectedValue` (optional) - Check that the field value is set to a specific value. To check `$Null` use `NullOrEmpty` instead. If `expectedValue` is `$Null` the field value will not be compared.

Examples:

```powershell
Rule 'HasFieldValue' {
    $Assert.HasFieldValue($TargetObject, 'Name')
    $Assert.HasFieldValue($TargetObject, 'tag.Environment', 'production')
}
```

### JsonSchema

The `JsonSchema` assertion method compares the input object against a defined JSON schema.

The following parameters are accepted:

- `inputObject` - The object being compared against the JSON schema.
- `uri` - A URL or file path to a JSON schema file formatted as UTF-8. Either a file path or URL can be used to specify the location of the schema file.

Examples:

```powershell
Rule 'JsonSchema' {
    $Assert.JsonSchema($TargetObject, 'tests/PSRule.Tests/FromFile.Json.schema.json')
}
```

### NullOrEmpty

The `NullOrEmpty` assertion method checks the field value of the object is null or empty.

A field value is null or empty if any of the following are true:

- The field does not exist.
- The field value is `$Null`.
- The field value is an empty array or collection.
- The field value is an empty string `''`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.

Examples:

```powershell
Rule 'NullOrEmpty' {
    $Assert.NullOrEmpty($TargetObject, 'Name')
    $Assert.NullOrEmpty($TargetObject, 'tag.Environment')
}
```

### StartsWith

The `StartsWith` assertion method checks the field value starts with the specified prefix.
Optionally a case-sensitive compare can be used, however case is ignored by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `prefix` - One or more prefixes to compare the field value with. Only one prefix must match.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value. Case is ignored by default.

Examples:

```powershell
Rule 'StartsWith' {
    $Assert.StartsWith($TargetObject, 'ResourceGroupName', 'rg-')
    $Assert.StartsWith($TargetObject, 'Name', @('st', 'diag'), $True)
}
```

### Version

The `Version` assertion method checks the field value is a valid semantic version.
A constraint can optionally be provided to require the semantic version to be within a range.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `constraint` (optional) - A version constraint, see below for details of version constrain format.

The following are supported constraints:

- `version` - Must match version exactly. This also accepts the following prefixes; `=`, `v`, `V`.
  - e.g. `1.2.3`, `=1.2.3`
- `>version` - Must be greater than version.
  - e.g. `>1.2.3`
- `>=version` - Must be greater than or equal to version.
  - e.g. `>=1.2.3`
- `<version` - Must be less than version.
  - e.g. `<1.2.3`
- `<=version` - Must be less than or equal to version.
  - e.g. `<=1.2.3`
- `^version` - Compatible with version.
  - e.g. `^1.2.3` - >=1.2.3, <2.0.0
- `~version` - Approximately equivalent to version
  - e.g. `~1.2.3` - >=1.2.3, <1.3.0

An empty, null or `*` version constraint matches all valid semantic versions.

Examples:

```powershell
Rule 'ValidVersion' {
    $Assert.Version($TargetObject, 'version')
}

Rule 'MinimumVersion' {
    $Assert.Version($TargetObject, 'version', '>=1.2.3')
}
```

### Advanced usage

The `AssertResult` object returned from assertion methods:

- Handles pass/ fail conditions and collection of reason information.
- Allows rules to implement their own handling or forward it up the stack to affect the rule outcome.

The following properties are available:

- `Result` - Either `$True` (Pass) or `$False` (Fail).

The following methods are available:

- `AddReason(<string> text)` - Can be used to append additional reasons to the result. A reason can only be set if the assertion failed. Reason text should be localized before calling this method. Localization can be done using the `$LocalizedData` automatic variable.
- `WithReason(<string> text, <bool> replace)` - Can be used to append or replace reasons on the result. In addition, `WithReason` can be chained.
- `GetReason()` - Gets any reasons currently associated with the failed result.
- `Complete()` - Returns `$True` (Pass) or `$False` (Fail) to the rule record. If the assertion failed, any reasons are automatically added to the rule record. To read the result without adding reason to the rule record use the `Result` property.
- `Ignore()` - Ignores the result. Nothing future is returned and any reasons are cleared. Use this method when implementing custom handling.

Use of `Complete()` is optional, uncompleted results are automatically completed after the rule has executed.
Uncompleted results may return reasons out of sequence.

In this example, `Complete()` is used to find the first field with an empty value.

```powershell
Rule 'Assert.HasFieldValue' {
    $Assert.HasFieldValue($TargetObject, 'Name').Complete() -and
        $Assert.HasFieldValue($TargetObject, 'Type').Complete() -and
        $Assert.HasFieldValue($TargetObject, 'Value').Complete()
}
```

The this example, the built-in reason is replaced with a custom reason, and immediately returned.

```powershell
Rule 'Assert.HasCustomValue' {
    $Assert.
        HasDefaultValue($TargetObject, 'value', 'test').
        WithReason('Value is set to custom value', $True)
}
```

### Authoring assertion methods

The following built-in helper methods are provided for working with `$Assert` when authoring new assertion methods:

- `Create(<bool> condition, <string> reason)` - Returns a result either pass or fail assertion result.
- `Pass()` - Returns a pass assertion result.
- `Fail(<string> reason)` - Results a fail assertion result.

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Assert.md.

## SEE ALSO

- [about_PSRule_Variables](https://github.com/Microsoft/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Variables.md)

## KEYWORDS

- Assert
- Contains
- EndsWith
- HasDefaultValue
- HasField
- HasFieldValue
- JsonSchema
- NullOrEmpty
- StartsWith
- Version
- Rule
