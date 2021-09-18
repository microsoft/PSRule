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
- [HasValue](#hasvalue)
- [In](#in)
- [IsLower](#islower)
- [IsString](#isstring)
- [IsUpper](#isupper)
- [Less](#less)
- [LessOrEquals](#lessorequals)
- [Match](#match)
- [NotEquals](#notequals)
- [NotIn](#notin)
- [NotMatch](#notmatch)
- [SetOf](#setof)
- [StartsWith](#startswith)
- [Subset](#subset)

The following operators are available:

- [AllOf](#allof)
- [AnyOf](#anyof)
- [Not](#not)

The following comparison properties are available:

- [Field](#field)

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

Syntax:

```yaml
contains: <string | array>
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

Syntax:

```yaml
equals: <string | int | bool>
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

Syntax:

```yaml
endsWith: <string | array>
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
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
greater: <int>
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
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
greaterOrEquals: <int>
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
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
less: <int>
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
When the field value is:

- An integer or float, a numerical comparison is used.
- An array, the number of elements is compared.
- A string, the length of the string is compared.
- A DateTime, the number of days from the current time is compared.

Syntax:

```yaml
lessOrEquals: <int>
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

### NotEquals

The `notEquals` condition can be used to compare if a field is equal to a supplied value.

Syntax:

```yaml
notEquals: <string | int | bool>
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

### StartsWith

The `startsWith` condition can be used to determine if the operand starts with a specified string.
One or more strings to compare can be specified.

Syntax:

```yaml
startsWith: <string | array>
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
Additionally the following properties are accepted:

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

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/concepts/PSRule/en-US/about_PSRule_Expressions.md.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- Rules
- Expressions
- PSRule
