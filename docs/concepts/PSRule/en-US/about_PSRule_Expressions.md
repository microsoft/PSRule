---
tags:
- language
---

# PSRule_Expressions

## about_PSRule_Expressions

## SHORT DESCRIPTION

Describes PSRule expressions and how to use them.

## LONG DESCRIPTION

PSRule expressions are used within YAML-based rules or selectors to evaluate an object.
Expressions are comprised of nested conditions, operators, and comparison properties.

The following conditions are available:

- [Contains](#contains)
- [Count](#count)
- [Equals](#equals)
- [EndsWith](#endswith)
- [Exists](#exists)
- [Greater](#greater)
- [GreaterOrEquals](#greaterorequals)
- [HasDefault](#hasdefault)
- [HasSchema](#hasschema)
- [HasValue](#hasvalue)
- [In](#in)
- [IsLower](#islower)
- [IsString](#isstring)
- [IsArray](#isarray)
- [IsBoolean](#isboolean)
- [IsDateTime](#isdatetime)
- [IsInteger](#isinteger)
- [IsNumeric](#isnumeric)
- [IsUpper](#isupper)
- [Less](#less)
- [LessOrEquals](#lessorequals)
- [Like](#like)
- [Match](#match)
- [NotContains](#notcontains)
- [NotCount](#notcount)
- [NotEndsWith](#notendswith)
- [NotEquals](#notequals)
- [NotIn](#notin)
- [NotLike](#notlike)
- [NotMatch](#notmatch)
- [NotStartsWith](#notstartswith)
- [NotWithinPath](#notwithinpath)
- [SetOf](#setof)
- [StartsWith](#startswith)
- [Subset](#subset)
- [WithinPath](#withinpath)
- [Version](#version)

The following operators are available:

- [AllOf](#allof)
- [AnyOf](#anyof)
- [Not](#not)

The following comparison properties are available:

- [Field](#field)
- [Name](#name)
- [Scope](#scope)
- [Source](#source)
- [Type](#type)

### AllOf

The `allOf` operator is used to require all nested expressions to match.
When any nested expression does not match, `allOf` does not match.
This is similar to a logical _and_ operation.

Syntax:

```yaml
allOf: <expression[]>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleAllOf'
spec:
  condition:
    allOf:
    # Both Name and Description must exist.
    - field: 'Name'
      exists: true
    - field: 'Description'
      exists: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleAllOf'
spec:
  if:
    allOf:
    # Both Name and Description must exist.
    - field: 'Name'
      exists: true
    - field: 'Description'
      exists: true
```

### AnyOf

The `anyOf` operator is used to require one or more nested expressions to match.
When any nested expression matches, `allOf` matches.
This is similar to a logical _or_ operation.

Syntax:

```yaml
anyOf: <expression[]>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleAnyOf'
spec:
  condition:
    anyOf:
    # Name and/ or AlternativeName must exist.
    - field: 'Name'
      exists: true
    - field: 'AlternativeName'
      exists: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleAnyOf'
spec:
  if:
    anyOf:
    # Name and/ or AlternativeName must exist.
    - field: 'Name'
      exists: true
    - field: 'AlternativeName'
      exists: true
```

### Contains

The `contains` condition can be used to determine if the operand contains a specified sub-string.
One or more strings to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
contains: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _contains_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleContains'
spec:
  condition:
    anyOf:
    - field: 'url'
      contains: '/azure/'
    - field: 'url'
      contains:
      - 'github.io'
      - 'github.com'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleContains'
spec:
  if:
    anyOf:
    - field: 'url'
      contains: '/azure/'
    - field: 'url'
      contains:
      - 'github.io'
      - 'github.com'
```

### Count

The `count` condition is used to determine if the operand contains a specified number of items.

Syntax:

```yaml
count: <int>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleCount'
spec:
  condition:
    field: 'items'
    count: 2

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleCount'
spec:
  if:
    field: 'items'
    count: 2
```

### Equals

The `equals` condition can be used to compare if the operand is equal to a supplied value.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  This only applies to string comparisons.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, perform type conversion on operand type.
  By default `convert` is `false`.

Syntax:

```yaml
equals: <string | int | bool>
caseSensitive: <boolean>
convert: <boolean>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleEquals'
spec:
  condition:
    field: 'Name'
    equals: 'TargetObject1'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleEquals'
spec:
  if:
    field: 'Name'
    equals: 'TargetObject1'
```

### EndsWith

The `endsWith` condition can be used to determine if the operand ends with a specified string.
One or more strings to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
endsWith: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _endsWith_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleEndsWith'
spec:
  condition:
    anyOf:
    - field: 'hostname'
      endsWith: '.com'
    - field: 'hostname'
      endsWith:
      - '.com.au'
      - '.com'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleEndsWith'
spec:
  if:
    anyOf:
    - field: 'hostname'
      endsWith: '.com'
    - field: 'hostname'
      endsWith:
      - '.com.au'
      - '.com'
```

### Exists

The `exists` condition determines if the specified field exists.

Syntax:

```yaml
exists: <bool>
```

- When `exists: true`, exists will return `true` if the field exists.
- When `exists: false`, exists will return `true` if the field does not exist.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleExists'
spec:
  condition:
    field: 'Name'
    exists: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleExists'
spec:
  if:
    field: 'Name'
    exists: true
```

### Field

The comparison property `field` is used with a condition to determine field of the object to evaluate.
A field can be:

- A property name.
- A key within a hashtable or dictionary.
- An index in an array or collection.
- A nested path through an object.

Syntax:

```yaml
field: <string>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleField'
spec:
  condition:
    field: 'Properties.securityRules[0].name'
    exists: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleField'
spec:
  if:
    field: 'Properties.securityRules[0].name'
    exists: true
```

### Greater

The `greater` condition determines if the operand is greater than a supplied value.
The field value can either be an integer, float, array, or string.

- `convert` - Optionally, perform type conversion on operand type.
  By default `convert` is `false`.

When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
  If `convert` is `true`, the string is converted a number instead.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
greater: <int>
convert: <boolean>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleGreater'
spec:
  condition:
    field: 'Name'
    greater: 3

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleGreater'
spec:
  if:
    field: 'Name'
    greater: 3
```

### GreaterOrEquals

The `greaterOrEquals` condition determines if the operand is greater or equal to the supplied value.
The field value can either be an integer, float, array, or string.

- `convert` - Optionally, perform type conversion on operand type.
  By default `convert` is `false`.

When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
  If `convert` is `true`, the string is converted a number instead.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
greaterOrEquals: <int>
convert: <boolean>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleGreaterOrEquals'
spec:
  condition:
    field: 'Name'
    greaterOrEquals: 3

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleGreaterOrEquals'
spec:
  if:
    field: 'Name'
    greaterOrEquals: 3
```

### HasDefault

The `hasDefault` condition determines if the field exists that it is set to the specified value.
If the field does not exist, the condition will return `true`.

The following properties are accepted:

- `caseSensitive` - Optionally, a case-sensitive comparison can be performed for string values.
  By default, case-insensitive comparison is performed.

Syntax:

```yaml
hasDefault: <string | int | bool>
caseSensitive: <bool>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleHasDefault'
spec:
  condition:
    field: 'enabled'
    hasDefault: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleHasDefault'
spec:
  if:
    field: 'enabled'
    hasDefault: true
```

### HasSchema

The `hasSchema` condition determines if the operand has a `$schema` property defined.
If the `$schema` property is defined, it must match one of the specified schemas.
If a trailing `#` is specified it is ignored.

The following properties are accepted:

- `caseSensitive` - Optionally, a case-sensitive comparison can be performed.
  By default, case-insensitive comparison is performed.
- `ignoreScheme` - Optionally, the URI scheme is ignored in the comparison.
  By default, the scheme is compared.
  When `true`, the schema will match if either `http://` or `https://` is specified.

Syntax:

```yaml
hasSchema: <array>
caseSensitive: <bool>
ignoreScheme: <bool>
```

- When `hasSchema: []`, hasSchema will return `true` if any non-empty `$schema` property is defined.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleHasSchema'
spec:
  condition:
    field: '.'
    hasSchema:
    - https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleHasSchema'
spec:
  if:
    field: '.'
    hasSchema:
    - https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#
    - https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#
    ignoreScheme: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleHasAnySchema'
spec:
  if:
    field: '.'
    hasSchema: []
```

### HasValue

The `hasValue` condition determines if the field exists and has a non-empty value.

Syntax:

```yaml
hasValue: <bool>
```

- When `hasValue: true`, hasValue will return `true` if the field is not empty.
- When `hasValue: false`, hasValue will return `true` if the field is empty.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleHasValue'
spec:
  condition:
    field: 'Name'
    hasValue: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleHasValue'
spec:
  if:
    field: 'Name'
    hasValue: true
```

### In

The `in` condition can be used to compare if a field contains one of the specified values.

Syntax:

```yaml
in: <array>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleIn'
spec:
  condition:
    field: 'Name'
    in:
    - 'Value1'
    - 'Value2'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleIn'
spec:
  if:
    field: 'Name'
    in:
    - 'Value1'
    - 'Value2'
```

### IsLower

The `isLower` condition determines if the operand is a lowercase string.

Syntax:

```yaml
isLower: <bool>
```

- When `isLower: true`, _isLower_ will return `true` if the operand is a lowercase string.
  Non-letter characters are ignored.
- When `isLower: false`, _isLower_ will return `true` if the operand is not a lowercase string.
- If the operand is a field, and the field does not exist _isLower_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleIsLower'
spec:
  condition:
    field: 'Name'
    isLower: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleIsLower'
spec:
  if:
    field: 'Name'
    isLower: true
```

### IsString

The `isString` condition determines if the operand is a string or other type.

Syntax:

```yaml
isString: <bool>
```

- When `isString: true`, _isString_ will return `true` if the operand is a string.
- When `isString: false`, _isString_ will return `true` if the operand is not a string or is null.
- If the operand is a field, and the field does not exist _isString_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleIsString'
spec:
  condition:
    field: 'Name'
    isString: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleIsString'
spec:
  if:
    field: 'Name'
    isString: true
```

### IsArray

The `isArray` condition determines if the operand is an array or other type.

Syntax:

```yaml
isArray: <bool>
```

- When `isArray: true`, _isArray_ will return `true` if the operand is an array.
- When `isArray: false`, _isArray_ will return `true` if the operand is not an array or null.
- If the operand is a field, and the field does not exist, _isArray_ always returns `false`.

For example:

```yaml
---
# Synopsis: Using isArray
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsArrayExample
spec:
  if:
    field: 'Value'
    isArray: true
```

### IsBoolean

The `isBoolean` condition determines if the operand is a boolean or other type.

- `convert` - Optionally, types can be converted to boolean type.
  E.g. `'true'` can be converted to `true`.
  By default `convert` is `false`.

```yaml
isBoolean: <bool>
convert: <bool>
```

- When `isBoolean: true`, _isBoolean_ will return `true` if the operand is a boolean.
- When `isBoolean: false`, _isBoolean_ will return `false` if the operand is not a boolean or null.
- When `convert: true`, types will be converted to boolean before condition is evaluated.

For example:

```yaml
---
# Synopsis: Using isBoolean
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsBooleanExample
spec:
  if:
    field: 'Value'
    isBoolean: true

---
# Synopsis: Using isBoolean with conversion
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsBooleanExampleWithConversion
spec:
  if:
    field: 'Value'
    isBoolean: true
    convert: true
```

### IsDateTime

The `isDateTime` condition determines if the operand is a datetime or other type.

- `convert` - Optionally, types can be converted to datetime type.
  E.g. `'2021-04-03T15:00:00.00+10:00'` can be converted to a datetime.
  By default `convert` is `false`.

```yaml
isDateTime: <bool>
convert: <bool>
```

- When `isDateTime: true`, _isDateTime_ will return `true` if the operand is a datetime.
- When `isDateTime: false`, _isDateTime_ will return `false` if the operand is not a datetime or null.
- When `convert: true`, types will be converted to datetime before condition is evaluated.

For example:

```yaml
---
# Synopsis: Using isDateTime
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsDateTimeExample
spec:
  if:
    field: 'Value'
    isDateTime: true

---
# Synopsis: Using isDateTime with conversion
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsDateTimeExampleWithConversion
spec:
  if:
    field: 'Value'
    isDateTime: true
    convert: true
```

### IsInteger

The `isInteger` condition determines if the operand is a an integer or other type.
The following types are considered integer types `int`, `long`, `byte`.

- `convert` - Optionally, types can be converted to integer type.
  E.g. `'123'` can be converted to `123`.
  By default `convert` is `false`.

```yaml
isInteger: <bool>
convert: <bool>
```

- When `isInteger: true`, _isInteger_ will return `true` if the operand is an integer.
- When `isInteger: false`, _isInteger_ will return `false` if the operand is not an integer or null.
- When `convert: true`, types will be converted to integer before condition is evaluated.

For example:

```yaml
---
# Synopsis: Using isInteger
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsIntegerExample
spec:
  if:
    field: 'Value'
    isInteger: true

---
# Synopsis: Using isInteger with conversion
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsIntegerExampleWithConversion
spec:
  if:
    field: 'Value'
    isInteger: true
    convert: true
```

### IsNumeric

The `isNumeric` condition determines if the operand is a a numeric or other type.
The following types are considered numeric types `int`, `long`, `float`, `byte`, `double`.

- `convert` - Optionally, types can be converted to numeric type.
  E.g. `'123'` can be converted to `123`.
  By default `convert` is `false`.

```yaml
isNumeric: <bool>
convert: <bool>
```

- When `isNumeric: true`, _isNumeric_ will return `true` if the operand is a numeric.
- When `isNumeric: false`, _isNumeric_ will return `false` if the operand is not a numeric or null.
- When `convert: true`, types will be converted to numeric before condition is evaluated.

For example:

```yaml
---
# Synopsis: Using isNumeric
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsNumericExample
spec:
  if:
    field: 'Value'
    isNumeric: true

---
# Synopsis: Using isNumeric with conversion
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IsNumercExampleWithConversion
spec:
  if:
    field: 'Value'
    isNumeric: true
    convert: true
```

### IsUpper

The `isUpper` condition determines if the operand is an uppercase string.

Syntax:

```yaml
isUpper: <bool>
```

- When `isUpper: true`, _isUpper_ will return `true` if the operand is an uppercase string.
  Non-letter characters are ignored.
- When `isUpper: false`, _isUpper_ will return `true` if the operand is not an uppercase string.
- If the operand is a field, and the field does not exist _isUpper_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleIsUpper'
spec:
  condition:
    field: 'Name'
    isUpper: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleIsUpper'
spec:
  if:
    field: 'Name'
    isUpper: true
```

### Less

The `less` condition determines if the operand is less than a supplied value.
The field value can either be an integer, float, array, or string.

- `convert` - Optionally, perform type conversion on operand type.
  By default `convert` is `false`.

When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
  If `convert` is `true`, the string is converted a number instead.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
less: <int>
convert: <boolean>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleLess'
spec:
  condition:
    field: 'Name'
    less: 3

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleLess'
spec:
  if:
    field: 'Name'
    less: 3
```

### LessOrEquals

The `lessOrEquals` condition determines if the operand is less or equal to the supplied value.
The field value can either be an integer, float, array, or string.

- `convert` - Optionally, perform type conversion on operand type.
  By default `convert` is `false`.

When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
  If `convert` is `true`, the string is converted a number instead.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
lessOrEquals: <int>
convert: <boolean>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleLessOrEquals'
spec:
  condition:
    field: 'Name'
    lessOrEquals: 3

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleLessOrEquals'
spec:
  if:
    field: 'Name'
    lessOrEquals: 3
```

### Like

The `like` condition can be used to determine if the operand matches a wildcard pattern.
One or more patterns to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
like: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _like_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleLike'
spec:
  condition:
    anyOf:
    - field: 'url'
      like: 'http://*'
    - field: 'url'
      like:
      - 'http://*'
      - 'https://*'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleLike'
spec:
  if:
    anyOf:
    - field: 'url'
      like: 'http://*'
    - field: 'url'
      like:
      - 'http://*'
      - 'https://*'
```

### Match

The `match` condition can be used to compare if a field matches a supplied regular expression.

Syntax:

```yaml
match: <string>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleMatch'
spec:
  condition:
    field: 'Name'
    match: '$(abc|efg)$'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleMatch'
spec:
  if:
    field: 'Name'
    match: '$(abc|efg)$'
```

### Name

The comparison property `name` is used with a condition to evaluate the target name of the object.
The `name` property must be set to `.`.
Any other value will cause the condition to evaluate to `false`.

Syntax:

```yaml
name: '.'
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleName'
spec:
  condition:
    name: '.'
    equals: 'TargetObject1'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleName'
spec:
  if:
    name: '.'
    in:
    - 'TargetObject1'
    - 'TargetObject2'
```

### Not

The `any` operator is used to invert the result of the nested expression.
When a nested expression matches, `not` does not match.
When a nested expression does not match, `not` matches.

Syntax:

```yaml
not: <expression>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNot'
spec:
  condition:
    not:
      # The AlternativeName field must not exist.
      field: 'AlternativeName'
      exists: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNot'
spec:
  if:
    not:
      # The AlternativeName field must not exist.
      field: 'AlternativeName'
      exists: true
```

### NotContains

The `notContains` condition can be used to determine if the operand contains a specified sub-string.
This condition fails when any of the specified sub-strings are found in the operand.
One or more strings to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
notContains: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _notContains_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotContains'
spec:
  condition:
    anyOf:
    - field: 'url'
      notContains: '/azure/'
    - field: 'url'
      notContains:
      - 'github.io'
      - 'github.com'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotContains'
spec:
  if:
    anyOf:
    - field: 'url'
      notContains: '/azure/'
    - field: 'url'
      notContains:
      - 'github.io'
      - 'github.com'
```

### NotCount

The `notCount` condition is used to determine if the operand does not contain a specified number of items.

Syntax:

```yaml
notCount: <int>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotCount'
spec:
  condition:
    field: 'items'
    notCount: 2

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotCount'
spec:
  if:
    field: 'items'
    notCount: 2
```

### NotEndsWith

The `notEndsWith` condition can be used to determine if the operand ends with a specified string.
This condition fails when any of the specified sub-strings are found at the end of the operand.
One or more strings to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
notEndsWith: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _notEndsWith_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotEndsWith'
spec:
  condition:
    anyOf:
    - field: 'hostname'
      notEndsWith: '.com'
    - field: 'hostname'
      notEndsWith:
      - '.com.au'
      - '.com'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotEndsWith'
spec:
  if:
    anyOf:
    - field: 'hostname'
      notEndsWith: '.com'
    - field: 'hostname'
      notEndsWith:
      - '.com.au'
      - '.com'
```

### NotEquals

The `notEquals` condition can be used to compare if a field is equal to a supplied value.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  This only applies to string comparisons.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, perform type conversion on operand type.
  By default `convert` is `false`.

Syntax:

```yaml
notEquals: <string | int | bool>
caseSensitive: <boolean>
convert: <boolean>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotEquals'
spec:
  condition:
    field: 'Name'
    notEquals: 'TargetObject1'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotEquals'
spec:
  if:
    field: 'Name'
    notEquals: 'TargetObject1'
```

### NotIn

The `notIn` condition can be used to compare if a field does not contains one of the specified values.

Syntax:

```yaml
notIn: <array>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotIn'
spec:
  condition:
    field: 'Name'
    notIn:
    - 'Value1'
    - 'Value2'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotIn'
spec:
  if:
    field: 'Name'
    notIn:
    - 'Value1'
    - 'Value2'
```

### NotLike

The `notLike` condition can be used to determine if the operand matches a wildcard pattern.
This condition fails when any of the specified patterns match the operand.
One or more patterns to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
notLike: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _notLike_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotLike'
spec:
  condition:
    anyOf:
    - field: 'url'
      notLike: 'http://*'
    - field: 'url'
      notLike:
      - 'http://'
      - 'https://'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotLike'
spec:
  if:
    anyOf:
    - field: 'url'
      notLike: 'http://*'
    - field: 'url'
      notLike:
      - 'http://'
      - 'https://'
```

### NotMatch

The `notMatch` condition can be used to compare if a field does not matches a supplied regular expression.

Syntax:

```yaml
notMatch: <string>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotMatch'
spec:
  condition:
    field: 'Name'
    notMatch: '$(abc|efg)$'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotMatch'
spec:
  if:
    field: 'Name'
    notMatch: '$(abc|efg)$'
```

### NotStartsWith

The `notStartsWith` condition can be used to determine if the operand starts with a specified string.
This condition fails when any of the specified sub-strings are found at the start of the operand.
One or more strings to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
notStartsWith: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _notStartsWith_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleNotStartsWith'
spec:
  condition:
    anyOf:
    - field: 'url'
      notStartsWith: 'http'
    - field: 'url'
      notStartsWith:
      - 'http://'
      - 'https://'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotStartsWith'
spec:
  if:
    anyOf:
    - field: 'url'
      notStartsWith: 'http'
    - field: 'url'
      notStartsWith:
      - 'http://'
      - 'https://'
```

### NotWithinPath

The `notWithinPath` condition determines if a file path is not within a required path.

If the path is not within the required path, the condition will return `true`.
If the path is within the required path, the condition will return `false`.

The following properties are accepted:

- `caseSensitive` - Optionally, a case-sensitive comparison can be performed for string values.
  By default, case-insensitive comparison is performed.

For example:

```yaml
---
# Synopsis: Test notWithinPath with source
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: YamlSourceNotWithinPath
spec:
  if:
    source: 'Template'
    notWithinPath:
      - "deployments/path/"

---
# Synopsis: Test notWithinPath with source and case sensitive
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: YamlSourceNotWithinPathCaseSensitive
spec:
  if:
    source: 'Template'
    notWithinPath:
      - "Deployments/Path/"
    caseSensitive: true
```

### Scope

The comparison property `scope` is used with a condition to evaluate the scope of the object.
The `scope` property must be set to `.`.
Any other value will cause the condition to evaluate to `false`.

Syntax:

```yaml
scope: '.'
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleScope'
spec:
  condition:
    scope: '.'
    startsWith: '/'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleScope'
spec:
  if:
    scope: '.'
    startsWith: '/'
```

### SetOf

The `setOf` condition can be used to determine if the operand is a set of specified values.
Additionally the following properties are accepted:

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.

Syntax:

```yaml
setOf: <array>
caseSensitive: <bool>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleSetOf'
spec:
  condition:
    field: 'zones'
    setOf:
    - 1
    - 2
    - 3
    caseSensitive: false

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleSetOf'
spec:
  if:
    field: 'zones'
    setOf:
    - 1
    - 2
    - 3
    caseSensitive: false
```

### Source

The comparison property `source` is used with a condition to expose the source path for the resource.
The `source` property can be set to any value.
The default is `file` when objects loaded from a file don't identify a source.

Syntax:

```yaml
source: 'file'
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: IgnoreTestFiles
spec:
  if:
    source: 'file'
    withinPath: 'tests/PSRule.Tests/'
```

### StartsWith

The `startsWith` condition can be used to determine if the operand starts with a specified string.
One or more strings to compare can be specified.

- `caseSensitive` - Optionally, a case sensitive-comparison can be performed.
  By default, case-insensitive comparison is performed.
- `convert` - Optionally, types can be converted to string type.
  By default `convert` is `false`.

Syntax:

```yaml
startsWith: <string | array>
caseSensitive: <boolean>
convert: <boolean>
```

- If the operand is a field, and the field does not exist, _startsWith_ always returns `false`.

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleStartsWith'
spec:
  condition:
    anyOf:
    - field: 'url'
      startsWith: 'http'
    - field: 'url'
      startsWith:
      - 'http://'
      - 'https://'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleStartsWith'
spec:
  if:
    anyOf:
    - field: 'url'
      startsWith: 'http'
    - field: 'url'
      startsWith:
      - 'http://'
      - 'https://'
```

### Subset

The `subset` condition can be used to determine if the operand is a set of specified values.
The following properties are accepted:

- `caseSensitive` - Optionally, a case-sensitive comparison can be performed.
  By default, case-insensitive comparison is performed.
- `unique` - Optionally, the operand must not contain duplicates.
  By default, duplicates are allowed.

Syntax:

```yaml
subset: <array>
caseSensitive: <bool>
unique: <bool>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleSubset'
spec:
  condition:
    field: 'logs'
    subset:
    - 'cluster-autoscaler'
    - 'kube-apiserver'
    - 'kube-scheduler'
    caseSensitive: true
    unique: true

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleSubset'
spec:
  if:
    field: 'logs'
    subset:
    - 'cluster-autoscaler'
    - 'kube-apiserver'
    - 'kube-scheduler'
    caseSensitive: true
    unique: true
```

### Type

The comparison property `type` is used with a condition to evaluate the target type of the object.
The `type` property must be set to `.`.
Any other value will cause the condition to evaluate to `false`.

Syntax:

```yaml
type: '.'
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleType'
spec:
  condition:
    type: '.'
    equals: 'CustomType'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleType'
spec:
  if:
    type: '.'
    in:
    - 'Microsoft.Storage/storageAccounts'
    - 'Microsoft.Storage/storageAccounts/blobServices'
```

### Version

The `version` condition determines if the operand is a valid semantic version.
A constraint can optionally be provided to require the semantic version to be within a range.
Supported version constraints for expression are the same as the `$Assert.Version` assertion helper.

Syntax:

```yaml
version: <string>
includePrerelease: <bool>
```

For example:

```yaml
---
apiVersion: github.com/microsoft/PSRule/v1
kind: Rule
metadata:
  name: 'ExampleVersion'
spec:
  condition:
    field: 'engine.version'
    version: '^1.2.3'

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleAnyVersion'
spec:
  if:
    field: 'engine.version'
    version: ''

---
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleVersionIncludingPrerelease'
spec:
  if:
    field: 'engine.version'
    version: '>=1.5.0'
    includePrerelease: true
```

### WithinPath

The `withinPath` condition determines if a file path is within a required path.

If the path is within the required path, the condition will return `true`.
If the path is not within the required path, the condition will return `false`.

The following properties are accepted:

- `caseSensitive` - Optionally, a case-sensitive comparison can be performed for string values.
  By default, case-insensitive comparison is performed.

For example:

```yaml
---
# Synopsis: Test withinPath with source
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: YamlSourceWithinPath
spec:
  if:
    source: 'Template'
    withinPath:
      - "deployments/path/"

---
# Synopsis: Test withinPath with source and case sensitive
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: YamlSourceWithinPathCaseSensitive
spec:
  if:
    source: 'Template'
    withinPath:
      - "Deployments/Path/"
    caseSensitive: true
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Expressions/.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Invoke-PSRule/)

## KEYWORDS

- Rules
- Expressions
- PSRule
