# PSRule_Assert

## about_PSRule_Assert

## SHORT DESCRIPTION

Describes the assertion helper that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule includes an assertion helper exposed as a built-in variable `$Assert`.
The `$Assert` object provides a consistent set of methods to evaluate objects.

Each `$Assert` method returns an `AssertResult` object that contains the result of the assertion.

The following built-in assertion methods are provided:

- [Contains](#contains) - The field value must contain at least one of the strings.
- [EndsWith](#endswith) - The field value must match at least one suffix.
- [FileHeader](#fileheader) - The file must contain a comment header.
- [FilePath](#filepath) - The file path must exist.
- [Greater](#greater) - The field value must be greater.
- [GreaterOrEqual](#greaterorequal) - The field value must be greater or equal to.
- [HasDefaultValue](#hasdefaultvalue) - The object should not have the field or the field value is set to the default value.
- [HasField](#hasfield) - The object must have any of the specified fields.
- [HasFields](#hasfields) - The object must have all of the specified fields.
- [HasFieldValue](#hasfieldvalue) - The object must have the specified field and that field is not empty.
- [HasJsonSchema](#hasjsonschema) - The object must reference a JSON schema with the `$schema` field.
- [In](#in) - The field value must be included in the set.
- [IsArray](#isarray) - The field value must be an array.
- [IsBoolean](#isboolean) - The field value must be a boolean.
- [IsDateTime](#isdatetime) - The field value must be a DateTime.
- [IsInteger](#isinteger) - The field value must be an integer.
- [IsLower](#islower) - The field value must include only lowercase characters.
- [IsNumeric](#isnumeric) - The field value must be a numeric type.
- [IsString](#isstring) - The field value must be a string.
- [IsUpper](#isupper) - The field value must include only uppercase characters.
- [JsonSchema](#jsonschema) - The object must validate successfully against a JSON schema.
- [Less](#less) - The field value must be less.
- [LessOrEqual](#lessorequal) - The field value must be less or equal to.
- [Match](#match) - The field value matches a regular expression pattern.
- [NotHasField](#nothasfield) - The object must not have any of the specified fields.
- [NotIn](#notin) - The field value must not be included in the set.
- [NotMatch](#notmatch) - The field value does not match a regular expression pattern.
- [NotNull](#notnull) - The field value must not be null.
- [NotWithinPath](#notwithinpath) - The field must not be within the specified path.
- [Null](#null) - The field value must not exist or be null.
- [NullOrEmpty](#nullorempty) - The object must not have the specified field or it must be empty.
- [TypeOf](#typeof) - The field value must be of the specified type.
- [StartsWith](#startswith) - The field value must match at least one prefix.
- [Version](#version) - The field value must be a semantic version string.
- [WithinPath](#withinpath) - The field value must be within the specified path.

The `$Assert` variable can only be used within a rule definition block or script pre-conditions.

### Using assertion methods

An assertion method can be used like other methods in PowerShell. i.e. `$Assert.methodName(parameters)`.

Assertion methods use the following standard pattern:

- The first parameter is _always_ the input object of type `PSObject`, additional parameters can be included based on the functionality required by the method.
  - In many cases the input object will be `$TargetObject`, however assertion methods must not assume that `$TargetObject` will be used.
  - Assertion methods must accept a `$Null` input object.
- Assertion methods return the `AssertResult` object that is interpreted by the rule pipeline.

Some assertion methods may overlap or provide similar functionality to built-in keywords.
Where you have the choice, use built-in keywords.
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

### Field names

Many of the built-in assertion methods accept a field name.
The field name is an expression that traverses object properties, keys or indexes of the _input object_.

The field name can contain:

- Property names for PSObjects or .NET objects.
- Keys for hash table or dictionaries.
- Indexes for arrays or collections.

For example:

- `.` refers to _input object_ itself.
- `Name` or `.Name` refers to the name property/ key of the _input object_.
- `Properties.enabled` refers to the enabled property under the Properties property.
- `Tags.env` refers to the env key under a hash table property of the _input object_.
- `Properties.securityRules[0].name` references to the name property of the first security rule.

### Contains

The `Contains` assertion method checks the field value contains the specified string.
Optionally a case-sensitive compare can be used, however case is ignored by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `text` - A string or an array of strings to compare the field value with.
Only one string must match.
When an empty array of strings is specified or text is an empty string, `Contains` always passes.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value.
Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'text' is null._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The field '{0}' does not contain '{1}'._

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
- `field` - The name of the field to check.
This is a case insensitive compare.
- `suffix` - A suffix or an array of suffixes to compare the field value with.
Only one suffix must match.
When an empty array of suffixes is specified or suffix is an empty string, `EndsWith` always passes.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value.
Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'suffix' is null._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The field '{0}' does not end with '{1}'._

Examples:

```powershell
Rule 'EndsWith' {
    $Assert.EndsWith($TargetObject, 'ResourceGroupName', 'eus')
    $Assert.EndsWith($TargetObject, 'Name', @('db', 'web'), $True)
}
```

### FileHeader

The `FileHeader` assertion method checks a file for a comment header.
When comparing the file header, the format of line comments are automatically detected by file extension.
Single line comments are supported. Multi-line comments are not supported.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field containing a valid file path.
- `header` - One or more lines of a header to compare with file contents.
- `prefix` (optional) - An optional comment prefix for each line.
By default a comment prefix will automatically detected based on file extension.
When set, detection by file extension is skipped.

Prefix detection for line comments is supported with the following file extensions:

- `.bicep`, `.cs`, `.csx` `.ts`, `.js`, `.jsx`,
`.fs`, `.go`, `.groovy`, `.php`, `.cpp`, `.h`,
`.java`, `.json`, `.jsonc`, `.scala`, `Jenkinsfile` - Use a prefix of (`// `).
- `.ps1`, `.psd1`, `.psm1`, `.yaml`, `.yml`,
`.r`, `.py`, `.sh`, `.tf`, `.tfvars`, `.gitignore`,
`.pl`, `.rb`, `Dockerfile` - Use a prefix of (`# `).
- `.sql`, `.lau` - Use a prefix of (`-- `).
- `.bat`, `.cmd` - Use a prefix of (`:: `).

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The file '{0}' does not exist._
- _The header was not set._

Examples:

```powershell
Rule 'FileHeader' {
    $Assert.FileHeader($TargetObject, 'FullName', @(
        'Copyright (c) Microsoft Corporation.'
        'Licensed under the MIT License.'
    ));
}
```

### FilePath

The `FilePath` assertion method checks the file exists.
Checks use file system case-sensitivity rules.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field containing a file path.
- `suffix` (optional) - Additional file path suffixes to append.
When specified each suffix is combined with the file path.
Only one full file path must be a valid file for the assertion method to pass.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The file '{0}' does not exist._

Examples:

```powershell
Rule 'FilePath' {
    $Assert.FilePath($TargetObject, 'FullName', @('CHANGELOG.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('LICENSE', 'LICENSE.txt'));
    $Assert.FilePath($TargetObject, 'FullName', @('CODE_OF_CONDUCT.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('CONTRIBUTING.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('SECURITY.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('README.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('.github/CODEOWNERS'));
    $Assert.FilePath($TargetObject, 'FullName', @('.github/PULL_REQUEST_TEMPLATE.md'));
}
```

### Greater

The `Greater` assertion method checks the field value is greater than the specified value.
The field value can either be an integer, float, array, or string.
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `value` - A integer to compare the field value against.
- `convert` (optional) - Convert numerical strings and use a numerical comparison instead of using string length.
By default the string length is compared.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The value '{0}' was not > '{1}'._
- _The field value '{0}' can not be compared with '{1}'._

Examples:

```powershell
Rule 'Greater' {
    $Assert.Greater($TargetObject, 'value', 3)
}
```

### GreaterOrEqual

The `GreaterOrEqual` assertion method checks the field value is greater or equal to the specified value.
The field value can either be an integer, float, array, or string.
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `value` - A integer to compare the field value against.
- `convert` (optional) - Convert numerical strings and use a numerical comparison instead of using string length.
By default the string length is compared.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The value '{0}' was not >= '{1}'._
- _The field value '{0}' can not be compared with '{1}'._

Examples:

```powershell
Rule 'GreaterOrEqual' {
    $Assert.GreaterOrEqual($TargetObject, 'value', 3)
}
```

### HasDefaultValue

The `HasDefaultValue` assertion method check that the field does not exist or the field value is set to the default value.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `defaultValue` - The expected value if the field exists.

This assertion will pass if:

- The field does not exist.
- The field value is set to `defaultValue`.

This assertion will fail if:

- The field value is set to a value different from `defaultValue`.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' is set to '{1}'._

Examples:

```powershell
Rule 'HasDefaultValue' {
    $Assert.HasDefaultValue($TargetObject, 'Properties.osProfile.linuxConfiguration.provisionVMAgent', $True)
}
```

### HasField

The `HasField` assertion method checks the object has any of the specified fields.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of one or more fields to check.
By default, a case insensitive compare is used.
If more than one field is specified, only one must exist.
- `caseSensitive` (optional) - Use a case sensitive compare of the field name.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._

Examples:

```powershell
Rule 'HasField' {
    $Assert.HasField($TargetObject, 'Name')
    $Assert.HasField($TargetObject, 'tag.Environment', $True)
    $Assert.HasField($TargetObject, @('tag.Environment', 'tag.Env'), $True)
}
```

### HasFields

The `HasFields` assertion method checks the object has all of the specified fields.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified fields.
- `field` - The name of one or more fields to check.
By default, a case insensitive compare is used.
If more than one field is specified, all fields must exist.
- `caseSensitive` (optional) - Use a case sensitive compare of the field name.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._

Examples:

```powershell
Rule 'HasFields' {
    $Assert.HasFields($TargetObject, 'Name')
    $Assert.HasFields($TargetObject, 'tag.Environment', $True)
    $Assert.HasFields($TargetObject, @('tag.Environment', 'tag.Env'), $True)
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
- `expectedValue` (optional) - Check that the field value is set to a specific value.
To check `$Null` use `NullOrEmpty` instead.
If `expectedValue` is `$Null` the field value will not be compared.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The value of '{0}' is null or empty._
- _The field '{0}' is set to '{1}'._

Examples:

```powershell
Rule 'HasFieldValue' {
    $Assert.HasFieldValue($TargetObject, 'Name')
    $Assert.HasFieldValue($TargetObject, 'tag.Environment', 'production')
}
```

### HasJsonSchema

The `HasJsonSchema` assertion method determines if the input object has a `$schema` property defined.
If the `$schema` property is defined, it must match one of the supplied schemas.
If a trailing `#` is specified it is ignored from the `$schema` property and `uri` parameter below.

The following parameters are accepted:

- `inputObject` - The object being compared.
- `uri` - Optional.
When specified, the object being compared must have a `$schema` property set to one of the specified schemas.
- `ignoreScheme` - Optional.
By default, `ignoreScheme` is `$False`.
When `$True`, the schema will match if `http` or `https` is specified.

Reasons include:

- _The parameter 'inputObject' is null._
- _The field '$schema' does not exist._
- _The field value '$schema' is not a string._
- _None of the specified schemas match '{0}'._

Examples:

```powershell
Rule 'HasFieldValue' {
    $Assert.HasJsonSchema($TargetObject)
    $Assert.HasJsonSchema($TargetObject, "http://json-schema.org/draft-07/schema`#")
    $Assert.HasJsonSchema($TargetObject, "https://json-schema.org/draft-07/schema", $True)
}
```

### JsonSchema

The `JsonSchema` assertion method compares the input object against a defined JSON schema.

The following parameters are accepted:

- `inputObject` - The object being compared against the JSON schema.
- `uri` - A URL or file path to a JSON schema file formatted as UTF-8.
Either a file path or URL can be used to specify the location of the schema file.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'uri' is null or empty._
- _The JSON schema '{0}' could not be found._
- _Failed schema validation on {0}. {1}_

Examples:

```powershell
Rule 'JsonSchema' {
    $Assert.JsonSchema($TargetObject, 'tests/PSRule.Tests/FromFile.Json.schema.json')
}
```

### In

The `In` assertion method checks the field value is included in a set of values.
The field value can either be an integer, float, array, or string.
When the field value is an array, only one item must be included in the set.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `values` - An array of values that the field value is compared against.
When an empty array is specified, `In` will always fail.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value. Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'values' is null._
- _The field '{0}' does not exist._
- _The field value '{0}' was not included in the set._

Examples:

```powershell
Rule 'In' {
    $Assert.In($TargetObject, 'Sku.tier', @('PremiumV2', 'Premium', 'Standard'))
    $Assert.In($TargetObject, 'Sku.tier', @('PremiumV2', 'Premium', 'Standard'), $True)
}
```

### IsArray

The `IsArray` assertion method checks the field value is an array type.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{1}' of type {0} is not \[array\]._

Examples:

```powershell
Rule 'IsArray' {
    # Require Value1 to be an array
    $Assert.IsArray($TargetObject, 'Value1')
}
```

### IsBoolean

The `IsBoolean` assertion method checks the field value is a boolean type.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `convert` (optional) - Try to convert strings.
By default strings are not converted.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{1}' of type {0} is not \[bool\]._

Examples:

```powershell
Rule 'IsBoolean' {
    # Require Value1 to be a boolean
    $Assert.IsBoolean($TargetObject, 'Value1')

    # Require Value1 to be a boolean or a boolean string
    $Assert.IsBoolean($TargetObject, 'Value1', $True)
}
```

### IsDateTime

The `IsBoolean` assertion method checks the field value is a DateTime type.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `convert` (optional) - Try to convert strings.
By default strings are not converted.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{1}' of type {0} is not \[DateTime\]._

Examples:

```powershell
Rule 'IsBoolean' {
    # Require Value1 to be a DateTime
    $Assert.IsDateTime($TargetObject, 'Value1')

    # Require Value1 to be a DateTime or a DateTime string
    $Assert.IsDateTime($TargetObject, 'Value1', $True)
}
```

### IsInteger

The `IsInteger` assertion method checks the field value is a integer type.
The following types are considered integer types `int`, `long`, `byte`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `convert` (optional) - Try to convert strings.
By default strings are not converted.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{1}' of type {0} is not an integer._

Examples:

```powershell
Rule 'IsInteger' {
    # Require Value1 to be an integer
    $Assert.IsInteger($TargetObject, 'Value1')

    # Require Value1 to be an integer or a integer string
    $Assert.IsInteger($TargetObject, 'Value1', $True)
}
```

### IsLower

The `IsLower` assertion method checks the field value uses only lowercase characters.
Non-letter characters are ignored by default and will pass.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `requireLetters` (optional) - Require each character to be lowercase letters only.
Non-letter characters are ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The value '{0}' does not contain only lowercase characters._
- _The value '{0}' does not contain only letters._

Examples:

```powershell
Rule 'IsLower' {
    # Require Name to be lowercase
    $Assert.IsLower($TargetObject, 'Name')

    # Require Name to only contain lowercase letters
    $Assert.IsLower($TargetObject, 'Name', $True)
}
```

### IsNumeric

The `IsNumeric` assertion method checks the field value is a numeric type.
The following types are considered numeric types `int`, `long`, `float`, `byte`, `double`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `convert` (optional) - Try to convert numerical strings.
By default strings are not converted.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{1}' of type {0} is not numeric._

Examples:

```powershell
Rule 'IsNumeric' {
    # Require Value1 to be numeric
    $Assert.IsNumeric($TargetObject, 'Value1')

    # Require Value1 to be numeric or a numerical string
    $Assert.IsNumeric($TargetObject, 'Value1', $True)
}
```

### IsString

The `IsString` assertion method checks the field value is a string type.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{1}' of type {0} is not \[string\]._

Examples:

```powershell
Rule 'IsString' {
    # Require Value1 to be a string
    $Assert.IsString($TargetObject, 'Value1')
}
```

### IsUpper

The `IsUpper` assertion method checks the field value uses only uppercase characters.
Non-letter characters are ignored by default and will pass.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `requireLetters` (optional) - Require each character to be uppercase letters only.
Non-letter characters are ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The value '{0}' does not contain only uppercase characters._
- _The value '{0}' does not contain only letters._

Examples:

```powershell
Rule 'IsUpper' {
    # Require Name to be uppercase
    $Assert.IsUpper($TargetObject, 'Name')

    # Require Name to only contain uppercase letters
    $Assert.IsUpper($TargetObject, 'Name', $True)
}
```

### Less

The `Less` assertion method checks the field value is less than the specified value.
The field value can either be an integer, float, array, or string.
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `value` - A integer to compare the field value against.
- `convert` (optional) - Convert numerical strings and use a numerical comparison instead of using string length.
By default the string length is compared.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The value '{0}' was not < '{1}'._
- _The field value '{0}' can not be compared with '{1}'._

Examples:

```powershell
Rule 'Less' {
    $Assert.Less($TargetObject, 'value', 3)
}
```

### LessOrEqual

The `LessOrEqual` assertion method checks the field value is less or equal to the specified value.
The field value can either be an integer, float, array, or string.
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `value` - A integer to compare the field value against.
- `convert` (optional) - Convert numerical strings and use a numerical comparison instead of using string length.
By default the string length is compared.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The value '{0}' was not <= '{1}'._
- _The field value '{0}' can not be compared with '{1}'._

Examples:

```powershell
Rule 'LessOrEqual' {
    $Assert.LessOrEqual($TargetObject, 'value', 3)
}
```

### Match

The `Match` assertion method checks the field value matches a regular expression pattern.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `pattern` - A regular expression pattern to match.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value.
Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The field value '{0}' does not match the pattern '{1}'._

Examples:

```powershell
Rule 'Match' {
    $Assert.Match($TargetObject, 'value', '^[a-z]*$')
    $Assert.Match($TargetObject, 'value', '^[a-z]*$', $True)
}
```

### NotHasField

The `NotHasField` assertion method checks the object does not have any of the specified fields.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of one or more fields to check.
By default, a case insensitive compare is used.
If more than one field is specified, all must not exist.
- `caseSensitive` (optional) - Use a case sensitive compare of the field name.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' exists._

Examples:

```powershell
Rule 'NotHasField' {
    $Assert.NotHasField($TargetObject, 'Name')
    $Assert.NotHasField($TargetObject, 'tag.Environment', $True)
    $Assert.NotHasField($TargetObject, @('tag.Environment', 'tag.Env'), $True)
}
```

### NotIn

The `NotIn` assertion method checks the field value is not in a set of values.
The field value can either be an integer, array, float, or string.
When the field value is an array, none of the items must be included in the set.
If the field does not exist at all, it is not in the set and passes.
To check the field exists combine this assertion method with `HasFieldValue`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `values` - An array values that the field value is compared against.
When an empty array is specified, `NotIn` will always pass.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value.
Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'values' is null._
- _The field value '{0}' was in the set._

Examples:

```powershell
Rule 'In' {
    $Assert.NotIn($TargetObject, 'Sku.tier', @('Free', 'Shared', 'Basic'))
    $Assert.NotIn($TargetObject, 'Sku.tier', @('Free', 'Shared', 'Basic'), $True)
}
```

### NotMatch

The `NotMatch` assertion method checks the field value does not match a regular expression pattern.
If the field does not exist at all, it does not match and passes.
To check the field exists combine this assertion method with `HasFieldValue`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check. This is a case insensitive compare.
- `pattern` - A regular expression pattern to match.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value.
Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field value '{0}' is not a string._
- _The field value '{0}' matches the pattern '{1}'._

Examples:

```powershell
Rule 'NotMatch' {
    $Assert.NotMatch($TargetObject, 'value', '^[a-z]*$')
    $Assert.NotMatch($TargetObject, 'value', '^[a-z]*$', $True)
}
```

### NotNull

The `NotNull` assertion method checks the field value of the object is not null.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._

Examples:

```powershell
Rule 'NotNull' {
    $Assert.NotNull($TargetObject, 'Name')
    $Assert.NotNull($TargetObject, 'tag.Environment')
}
```

### NotWithinPath

The `NotWithinPath` assertion method checks the file is not within a specified path.
Checks use file system case-sensitivity rules by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field containing a file path.
When the field is `InputFileInfo` or `FileInfo`, PSRule will automatically resolve the file path.
- `path` - An array of one or more directory paths to check.
Only one path must match.
- `caseSensitive` (optional) - Determines if case-sensitive path matching is used.
This can be set to `$True` or `$False`.
When not set or `$Null`, the case-sensitivity rules of the working path file system will be used.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'path' is null or empty._
- _The field '{0}' does not exist._
- _The file '{0}' is within the path '{1}'._

Examples:

```powershell
Rule 'NotWithinPath' {
    # The file must not be within either policy/ or security/ sub-directories.
    $Assert.NotWithinPath($TargetObject, 'FullName', @('policy/', 'security/'));
}
```

### Null

The `Null` assertion method checks the field value of the object is null.

A field value is null if any of the following are true:

- The field does not exist.
- The field value is `$Null`.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field value '{0}' is not null._

Examples:

```powershell
Rule 'Null' {
    $Assert.Null($TargetObject, 'NotField')
    $Assert.Null($TargetObject, 'tag.NullField')
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
- `field` - The name of the field to check.
This is a case insensitive compare.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' is not empty._

Examples:

```powershell
Rule 'NullOrEmpty' {
    $Assert.NullOrEmpty($TargetObject, 'Name')
    $Assert.NullOrEmpty($TargetObject, 'tag.Environment')
}
```

### TypeOf

The `TypeOf` assertion method checks the field value is a specified type.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `type` - One or more specified types to check.
The field value only has to match a single type of more than one type is specified.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'type' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is null._
- _The field value '{2}' of type {1} is not {0}._

Examples:

```powershell
Rule 'TypeOf' {
    # Require Value1 to be [int]
    $Assert.TypeOf($TargetObject, 'Value1', [int])

    # Require Value1 to be [int] or [long]
    $Assert.TypeOf($TargetObject, 'Value1', @([int], [long]))
}
```

### StartsWith

The `StartsWith` assertion method checks the field value starts with the specified prefix.
Optionally a case-sensitive compare can be used, however case is ignored by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field to check.
This is a case insensitive compare.
- `prefix` - A prefix or an array of prefixes to compare the field value with.
Only one prefix must match.
When an empty array of prefixes is specified or prefix is an empty string, `StartsWith` always passes.
- `caseSensitive` (optional) - Use a case sensitive compare of the field value.
Case is ignored by default.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'prefix' is null._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a string._
- _The field '{0}' does not start with '{1}'._

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

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The field '{0}' does not exist._
- _The field value '{0}' is not a version string._
- _The version '{0}' does not match the constraint '{1}'._

Examples:

```powershell
Rule 'ValidVersion' {
    $Assert.Version($TargetObject, 'version')
}

Rule 'MinimumVersion' {
    $Assert.Version($TargetObject, 'version', '>=1.2.3')
}
```

### WithinPath

The `WithinPath` assertion method checks if the file path is within a required path.
Checks use file system case-sensitivity rules by default.

The following parameters are accepted:

- `inputObject` - The object being checked for the specified field.
- `field` - The name of the field containing a file path.
When the field is `InputFileInfo` or `FileInfo`, PSRule will automatically resolve the file path.
- `path` - An array of one or more directory paths to check.
Only one path must match.
- `caseSensitive` (optional) - Determines if case-sensitive path matching is used.
This can be set to `$True` or `$False`.
When not set or `$Null`, the case-sensitivity rules of the working path file system will be used.

Reasons include:

- _The parameter 'inputObject' is null._
- _The parameter 'field' is null or empty._
- _The parameter 'path' is null or empty._
- _The field '{0}' does not exist._
- _The file '{0}' is not within the path '{1}'._

Examples:

```powershell
Rule 'WithinPath' {
    # Require the file to be within either policy/ or security/ sub-directories.
    $Assert.WithinPath($TargetObject, 'FullName', @('policy/', 'security/'));
}
```

### Advanced usage

The `AssertResult` object returned from assertion methods:

- Handles pass/ fail conditions and collection of reason information.
- Allows rules to implement their own handling or forward it up the stack to affect the rule outcome.

The following properties are available:

- `Result` - Either `$True` (Pass) or `$False` (Fail).

The following methods are available:

- `AddReason(<string> text)` - Can be used to append additional reasons to the result.
A reason can only be set if the assertion failed.
Reason text should be localized before calling this method.
Localization can be done using the `$LocalizedData` automatic variable.
- `WithReason(<string> text, <bool> replace)` - Can be used to append or replace reasons on the result.
In addition, `WithReason` can be chained.
- `Reason(<string> text, params <object[]> args)` - Replaces the reason on the results with a formatted string.
This method can be chained.
For usage see examples below.
- `GetReason()` - Gets any reasons currently associated with the failed result.
- `Complete()` - Returns `$True` (Pass) or `$False` (Fail) to the rule record.
If the assertion failed, any reasons are automatically added to the rule record.
To read the result without adding reason to the rule record use the `Result` property.
- `Ignore()` - Ignores the result. Nothing future is returned and any reasons are cleared.
Use this method when implementing custom handling.

Use of `Complete` is optional, uncompleted results are automatically completed after the rule has executed.
Uncompleted results may return reasons out of sequence.

Using these advanced methods is not supported in rule script pre-conditions.

In this example, `Complete` is used to find the first field with an empty value.

```powershell
Rule 'Assert.HasFieldValue' {
    $Assert.HasFieldValue($TargetObject, 'Name').Complete() -and
        $Assert.HasFieldValue($TargetObject, 'Type').Complete() -and
        $Assert.HasFieldValue($TargetObject, 'Value').Complete()
}
```

In this example, the built-in reason is replaced with a custom reason, and immediately returned.
The reason text is automatically formatted with any parameters provided.

```powershell
Rule 'Assert.HasCustomValue' {
    $Assert.
        HasDefaultValue($TargetObject, 'value', 'test').
        Reason('The field {0} is using a non-default value: {1}', 'value', $TargetObject.value)

    # With localized string
    $Assert.
        HasDefaultValue($TargetObject, 'value', 'test').
        Reason($LocalizedData.NonDefaultValue, 'value', $TargetObject.value)
}
```

### Authoring assertion methods

The following built-in helper methods are provided for working with `$Assert` when authoring new assertion methods:

- `Create(<bool> condition, <string> reason, params <object[]> args)` - Returns a result either pass or fail assertion result.
Additional arguments can be provided to format the custom reason string.
- `Pass()` - Returns a pass assertion result.
- `Fail()` - Results a fail assertion result.
- `Fail(<string> reason, params <object[]> args)` - Results a fail assertion result with a custom reason.
Additional arguments can be provided to format the custom reason string.

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSRule/blob/main/docs/concepts/PSRule/en-US/about_PSRule_Assert.md.

## SEE ALSO

- [about_PSRule_Variables](https://github.com/Microsoft/PSRule/blob/main/docs/concepts/PSRule/en-US/about_PSRule_Variables.md)

## KEYWORDS

- Assert
- Contains
- EndsWith
- Greater
- GreaterOrEqual
- HasDefaultValue
- HasField
- HasFieldValue
- JsonSchema
- Less
- LessOrEqual
- NullOrEmpty
- StartsWith
- Version
- Rule
