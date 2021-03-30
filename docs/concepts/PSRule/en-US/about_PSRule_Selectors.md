# PSRule_Selectors

## about_PSRule_Selectors

## SHORT DESCRIPTION

Describes PSRule Selectors including how to use and author them.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When evaluating an object from input, PSRule can use selectors to perform complex matches of an object.

- A selector is a YAML-based expression that evaluates an object.
- Each selector is comprised of nested conditions, operators, and comparison properties.
- Selectors must use one or more available conditions with a comparison property to evaluate the object.
- Optionally a condition can be nested in an operator.
- Operators can be nested within other operators.

The following conditions are available:

- [Exists](#exists)
- [Equals](#equals)
- [Greater](#greater)
- [GreaterOrEquals](#greaterorequals)
- [HasValue](#hasvalue)
- [In](#in)
- [Less](#less)
- [LessOrEquals](#lessorequals)
- [Match](#match)
- [NotEquals](#notequals)
- [NotIn](#notin)
- [NotMatch](#notmatch)

The following operators are available:

- [AllOf](#allof)
- [AnyOf](#anyof)
- [Not](#not)

The following comparison properties are available:

- [Field](#field)

Currently the following limitations apply:

- Selectors can only evaluate a field of the target object.
The following examples can not be evaluated by selectors:
  - Bound properties such as `TargetName`, `TargetType`, and `Field`.
  - State variables such as `$PSRule`.

### Using selectors as pre-conditions

Selectors can be referenced by name as a rule pre-condition by using the `-With` parameter.
For example:

```powershell
Rule 'RuleWithSelector' -With 'BasicSelector' {
    # Rule condition
}
```

Selector pre-conditions can be used together with type and script block pre-conditions.
If one or more selector pre-conditions are used, they are evaluated before type or script block pre-conditions.

### Defining selectors

Selectors are defined in YAML and can be included within a module or standalone `.Rule.yaml` file.
In either case, define a selector within a file ending with the `.Rule.yaml` extension.
A selector can be defined side-by-side with other resources such as baselines or module configurations.

Use the following template to define a selector:

```yaml
---
# Synopsis: {{ Synopsis }}
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: '{{ Name }}'
spec:
  if: { }
```

Within the `if` object, one or more conditions or logical operators can be used.

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
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleExists'
spec:
  if:
    field: 'Name'
    exists: true
```

### Equals

The `equals` condition can be used to compare if a field is equal to a supplied value.

Syntax:

```yaml
equals: <string | int | bool>
```

For example:

```yaml
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleEquals'
spec:
  if:
    field: 'Name'
    equals: 'TargetObject1'
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

Syntax:

```yaml
greater: <int>
```

For example:

```yaml
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

Syntax:

```yaml
greaterOrEquals: <int>
```

For example:

```yaml
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

### Less

Syntax:

```yaml
less: <int>
```

For example:

```yaml
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

Syntax:

```yaml
lessOrEquals: <int>
```

For example:

```yaml
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
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: 'ExampleNotMatch'
spec:
  if:
    field: 'Name'
    notMatch: '$(abc|efg)$'
```

## EXAMPLES

### Example Selectors.Rule.yaml

```yaml
# Example Selectors.Rule.yaml
---
# Synopsis: Require the CustomValue field.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: RequireCustomValue
spec:
  if:
    field: 'CustomValue'
    exists: true

---
# Synopsis: Require a Name or AlternativeName.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: RequireName
spec:
  if:
    anyOf:
    - field: 'AlternateName'
      exists: true
    - field: 'Name'
      exists: true

---
# Synopsis: Require a specific CustomValue
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: RequireSpecificCustomValue
spec:
  if:
    field: 'CustomValue'
    in:
    - 'Value1'
    - 'Value2'
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/concepts/PSRule/en-US/about_PSRule_Selectors.md.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/commands/PSRule/en-US/Invoke-PSRule.html)

## KEYWORDS

- Selectors
- PSRule
