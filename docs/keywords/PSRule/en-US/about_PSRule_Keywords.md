# PSRule_Keywords

## about_PSRule_Keywords

## SHORT DESCRIPTION

Describes the language keywords that can be used within PSRule document definitions.

## LONG DESCRIPTION

PSRule lets you define rules using PowerShell blocks. To create a rule use the `Rule` keyword. Within a rule several assertions can be used.

- Assertion - A specific test that always evaluates to true or false.

The following are the built-in keywords that can be used within PSRule:

- [Rule](#rule) - A rule definition
- [Exists](#exists) - Assert that a field or property must exist
- [Match](#match) - Assert that the field must match any of the regular expressions
- [AnyOf](#anyof) - Assert that any of the child expressions must be true
- [AllOf](#allof) - Assert that all of the child expressions must be true
- [Within](#within) - Assert that the field must match any of the values
- [TypeOf](#typeof) - Assert that the object must be of a specific type

### Rule

A `Rule` definition describes an individual business rule that will be applied to pipeline objects.

To define a Rule use the `Rule` keyword followed by a name and a pair of squiggly brackets `{`. Within the `{ }` one or more expressions can be used.

Syntax:

```text
Rule [-Name] <string> [-Tag <hashtable>] [-If <scriptBlock>] [-DependsOn <string[]>] [-Configure <hashtable>] [-Body] {
    ...
}
```

- `Name` - The name of the rule definition. This must be unique with in the same script file.
- `Tag` - A hashtable of key/ value metadata that can be used to filter and identify rules and rule results.
- `If` - A precondition that must evaluate to `$True` before the rule is executed.
- `DependsOn` - A list of rules this rule depends on. Rule dependencies must execute successfully before this rule is executed.
- `Configure` - A set of default configuration values. These values are only used when the baseline configuration does not contain the key.
- `Body` - A script block definition of the rule containing one or more PSRule keywords and PowerShell expressions.

Examples:

```powershell
# This rule checks for the presence of a name field
Rule 'NameMustExist' {
    Exists 'Name'
}
```

```powershell
# This rule checks that the title field is valid, when the rule NameMustExist is successful
Rule 'TitleIsValid' -DependsOn 'NameMustExist' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
}
```

```powershell
# This rule uses a threshold stored as $Configuration.minInstanceCount
Rule 'HasMinInstances' {
    $TargetObject.Sku.capacity -ge $Configuration.minInstanceCount
} -Configure @{ minInstanceCount = 2 }
```

### Exists

The `Exists` assertion is used within a `Rule` definition to assert that a _field_ or property must exist on the pipeline object.

Syntax:

```text
Exists [-Field] <string[]> [-CaseSensitive] [-Not]
```

- `Field` - One or more fields/ properties that must exist on the pipeline object.
- `CaseSensitive` - The field name must match exact case.
- `Not` - Instead of checking if the field names exists they should not exist.

Examples:

```powershell
# This rule checks for the presence of a name property
Rule 'nameMustExist' {
    Exists 'Name'
}
```

Output:

If **any** the specified field exists then Exists will return `$True`, otherwise `$False`.

If `-Not` is used, then if **any** of the specified fields exist then Exists will return `$False` otherwise `$True`.

### Match

The `Match` assertion is used within a `Rule` definition to assert that the value of a _field_ or property from pipeline data must match one or more regular expressions. To optionally perform a case sensitive match use the `-CaseSensitive` switch, otherwise a case insensitive match will be used.

Syntax:

```text
Match [-Field] <string> [-Expression] <string[]> [-CaseSensitive]
```

- `Field` - The name of the field that will be evaluated on the pipeline object.
- `Expression` - One or more regular expressions that will be used to match the value of the field.
- `CaseSensitive` - The field _value_ must match exact case.

Examples:

```powershell
Rule 'validatePhoneNumber' {
    Match 'PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$'
}
```

Output:

If **any** of the specified regular expressions match the field value then Match returns `$True`, otherwise `$False`.

### Within

The `Within` assertion is used within a `Rule` definition to assert that the value of a field or property from pipeline data must equal an item from a supplied list of allowed values. To optionally perform a case sensitive match use the `-CaseSensitive` switch, otherwise a case insensitive match will be used.

Syntax:

```text
Within [-Field] <string> [-AllowedValue] <PSObject[]]> [-CaseSensitive]
```

- `Field` - The name of the field that will be evaluated on the pipeline object.
- `AllowedValue` - A list of allowed values that the field value must match.
- `CaseSensitive` - The field _value_ must match exact case. Only applies when the field value and allowed values are strings.

Examples:

```powershell
# Ensure that the title field has one of the allowed values
Rule 'validateTitle' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
}
```

Output:

If **any** of the allow values match the field value then Within returns `$True`, otherwise `$False`.

### AllOf

The `AllOf` assertion is used within a `Rule` definition to aggregate the result of assertions within a pair of squiggly brackets `{ }`. `AllOf` is functionally equivalent to a binary **and**, where when all of the contained assertions return `$True`, `AllOf` will return `$True`.

Syntax:

```text
AllOf [-Body] {
    <assertion>
    [<assertion>]
    ...
}
```

- `Body` - A script block definition of the containing one or more PSRule keywords and PowerShell expressions.

Examples:

```powershell
# The Name field must exist and have a value of either John or Jane
AllOf {
    Exists 'Name'
    Within 'Name' 'John', 'Jane'
}
```

Output:

If **all** of the assertions return `$True` AllOf will return `$True`, otherwise `$False`.

### AnyOf

The `AnyOf` assertion is used within a `Rule` definition to aggregate the result of assertions within a pair of squiggly brackets `{ }`. `AnyOf` is functionally equivalent to a binary **or**, where if any of the contained assertions returns `$True`, `AnyOf` will return `$True`.

Syntax:

```text
AnyOf [-Body] {
    <assertion>
    [<assertion>]
    ...
}
```

- `Body` - A script block definition of the containing one or more PSRule keywords and PowerShell expressions.

Examples:

```powershell
# The Last or Surname field must exist
AnyOf {
    Exists 'Last'
    Exists 'Surname'
}
```

Output:

If **any** of the assertions return `$True` AnyOf will return `$True`, otherwise `$False`.

### TypeOf

The `TypeOf` assertion is used within a `Rule` definition to evaluate if the pipeline object matches one or more of the supplied type names.

Syntax:

```text
TypeOf [-TypeName] <string[]>
```

- `TypeName` - One or more type names which will be evaluated against the pipeline object. `TypeName` is case insensitive.

Examples:

```powershell
TypeOf 'System.Collections.Hashtable'
```

Output:

If **any** the specified type names match the pipeline object then TypeOf will return `$True`, otherwise `$False`.

## EXAMPLES

```powershell
# Description: App Service Plan has multiple instances
Rule 'appServicePlan.MinInstanceCount' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/serverfarms' } {

    Hint 'Use at least two (2) instances' -TargetName $TargetObject.ResourceName

    $TargetObject.Sku.capacity -ge 2
}
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/keywords/PSRule/en-US/about_PSRule_Keywords.md.

## SEE ALSO

- [Invoke-PSRule]

## KEYWORDS

- Rule
- Exists
- Match
- AnyOf
- AllOf
- Within
- TypeOf

[Invoke-PSRule]: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md
