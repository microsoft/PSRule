# PSRule_Assert

## about_PSRule_Assert

## SHORT DESCRIPTION

Describes the assertion helper that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule includes an assertion helper exposed as a built-in variable `$Assert`. The `$Assert` object provides a consistent set of methods to evaluate objects.

Each `$Assert` method returns an `AssertResult` object that contains the result of the condition.

The following built-in assertion methods are provided:

- [HasField](#hasfield) - Asserts that the object must have the specified field.
- [HasFieldValue](#hasfieldvalue) - Asserts that the object must have the specified field and that field is not empty.
- [JsonSchema](#jsonschema) - Validates an object against a JSON schema.
- [NullOrEmpty](#nullorempty) - Asserts that the object must not have the specified field or it must be empty.

The `$Assert` variable can only be used within a rule definition block.

### Using assertion methods

An assertion method can be used like other methods in PowerShell. i.e. `$Assert.methodName(parameters)`.

Assertion methods use the following standard pattern:

- The first parameter is _always_ the input object of type `PSObject`, additional parameters can be included based on the functionality required by the method.
  - In many cases the input object will be `$TargetObject`, however assertion methods must not assume that `$TargetObject` will be used.
  - Assertion methods must not assume that the input object is not `$Null`.
- Assertion methods return the `AssertResult` object that is interpreted by the rule pipeline.

Some assertion methods may overlap or provide similar functionality to built-in keywords. Where you have the choice, use built-in keywords. Use assertion methods for advanced cases or increased flexibility.

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

### HasField

The `HasField` assertion method checks the object has the specified field.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. By default a case insensitive compare is used.
- `caseSensitive` (optional) - Use a case sensitive compare of the field name instead.

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

The `JsonSchema` assertion method allows an object to validated against a defined JSON schema.

The following parameters are accepted:

- `inputObject` - The object being compared against the JSON schema.
- `uri` - A URL or file path to a JSON schema file formatted as UTF-8.

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

### Advanced usage

The `AssertResult` object returned from assertion methods allows:

- Handles pass/ fail conditions and collection of reason information.
- Allows rules to implement their own handling or forward it up the stack to affect the rule outcome.

The following properties are available:

- `Result` - Either `$True` (Pass) or `$False` (Fail).

The following methods are available:

- `AddReason(<string> text)` - Can be used to add additional reasons to the result. A reason can only be set if the assertion failed. Reason text should be localized before calling this method. Localization can be done using the `$LocalizedData` automatic variable.
- `GetReason()` - Gets any reasons currently associated with the failed result.
- `Complete()` - Returns `$True` (Pass) or `$False` (Fail) to the rule record. If the assertion failed, any reasons that are automatically added to the rule record.
- `Ignore()` - Ignores the result. This method is use when implementing custom handling.

Use of `Complete()` is optional, uncompleted results are automatically completed after the rule has executed. Uncompleted results may return reason messages out of sequence.

In this example `Complete()` is used to find the first field with an empty value.

```powershell
Rule 'Assert.HasFieldValue' {
    $Assert.HasFieldValue($TargetObject, 'Name').Complete() -and
        $Assert.HasFieldValue($TargetObject, 'Type').Complete() -and
        $Assert.HasFieldValue($TargetObject, 'Value').Complete()
}
```

### Authoring assertion methods

The following built-in helper methods are provided for working with `AssertResult` when authoring new assertion methods:

- `Create(<bool> condition, <string> reason)` - Returns a result either pass or fail.
- `Pass()` - Returns a pass result.
- `Fail(<string> reason)` - Results a fail result.

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Assert.md.

## SEE ALSO

- [about_PSRule_Variables](https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Variables.md)

## KEYWORDS

- Assert
- Rule
