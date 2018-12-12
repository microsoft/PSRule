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

To define a Rule use the `Rule` keyword followed by a unique identifier and a pair of squiggly brackets `{`. Within the `{ }` one or more expressions can be used.

Syntax:

```text
Rule [-RuleId] <string> [-Tag <hashtable>] [-If <scriptBlock>] [-DependsOn <string[]>] [-Body] {
    ...
}
```

Examples:

```powershell
# This rule checks for the presence of a name field
Rule 'NameMustExist' {
    ...
}
```

```powershell
# This rule checks that the name field is valid, when the rule NameMustExist is successful
Rule 'NameIsValid' -DependsOn 'NameMustExist' {
    ...
}
```

### Exists

The `Exists` assertion is used within a `Rule` definition to assert that a _field_ or property must exist on the pipeline object.

Syntax:

```text
Exists [-Field] <string[]> [-CaseSensitive]
```

Examples:

```powershell
# This rule checks for the presence of a name property
Rule 'nameMustExist' {
    Exists 'Name'
}
```

Output:

If the specified field exists then Exists will return `$True`, otherwise `$False`.

### Match

The `Match` assertion is used within a `Rule` definition to assert that the value of a _field_ or property from pipeline data must match one or more regular expressions. To optionally perform a case sensitive match use the `-CaseSensitive` switch, otherwise a case insensitive match will be used.

Syntax:

```text
Match [-Field] <string[]> [-Expression] <string[]> [-CaseSensitive]
```

Examples:

```powershell
Rule 'validatePhoneNumber' {
    Match 'PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$'
}
```

Output:

If __any__ of the specified regular expressions match the field then Match returns `$True`, otherwise `$False`.

### Within

The `Within` assertion is used within a `Rule` definition to assert that the value of a field or property from pipeline data must equal an item from a supplied list. To optionally perform a case sensitive match use the `-CaseSensitive` switch, otherwise a case insensitive match will be used.

Syntax:

```text
Within [-Field] <string> [-CaseSensitive] [-List] <PSObject[]]>
```

Examples:

```powershell
Rule 'validateTitle' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
}
```

Output:

If __any__ of the items match the field then Within returns `$True`, otherwise `$False`.

### AllOf

The `AllOf` assertion is used within a `Rule` definition to aggregate the result of assertions within a pair of squiggly brackets `{ }`. `AllOf` is functionally equivalent to a binary __and__, where when all of the contained assertions return `$True`, `AllOf` will return `$True`.

Syntax:

```text
AllOf {
    <assertion>
    [<assertion>]
    ...
}
```

Examples:

```powershell
# The Name field must exist and have a value of either John or Jane
AllOf {
    Exists 'Name'

    Within 'Name' {
        'John'
        'Jane'
    }
}
```

### AnyOf

The `AnyOf` assertion is used within a `Rule` definition to aggregate the result of assertions within a pair of squiggly brackets `{ }`. `AnyOf` is functionally equivalent to a binary __or__, where if any of the contained assertions returns `$True`, `AnyOf` will return `$True`.

Syntax:

```text
AnyOf {
    <assertion>
    [<assertion>]
    ...
}
```

Examples:

```powershell
# The Last or Surname field must exist
AllOf {
    Exists 'Last'

    Exists 'Surname'
}
```

### TypeOf

The `TypeOf` assertion is used within a `Rule` definition to evaluate if the pipeline object matches one or more of the supplied type names.

Syntax:

```powershell
TypeOf <string[]>
```

Examples:

```powershell
TypeOf 'System.Collections.Hashtable'
```

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
