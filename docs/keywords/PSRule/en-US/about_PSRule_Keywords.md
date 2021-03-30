# PSRule_Keywords

## about_PSRule_Keywords

## SHORT DESCRIPTION

Describes the language keywords that can be used within PSRule rule definitions.

## LONG DESCRIPTION

PSRule lets you define rules using PowerShell blocks. To define a rule use the `Rule` keyword.

- [Rule](#rule) - Creates a rule definition.

The following are the built-in keywords that can be used within a rule definition:

- [AnyOf](#anyof) - Assert that any of the child expressions must be true.
- [AllOf](#allof) - Assert that all of the child expressions must be true.
- [Exists](#exists) - Assert that a field or property must exist.
- [Match](#match) - Assert that the field must match any of the regular expressions.
- [Reason](#reason) - Return a reason for why the rule failed.
- [Recommend](#recommend) - Return a recommendation to resolve the issue and pass the rule.
- [TypeOf](#typeof) - Assert that the object must be of a specific type.
- [Within](#within) - Assert that the field must match any of the values.

A subset of built-in keywords can be used within script preconditions:

- [Exists](#exists) - Assert that a field or property must exist.
- [Match](#match) - Assert that the field must match any of the regular expressions.
- [TypeOf](#typeof) - Assert that the object must be of a specific type.
- [Within](#within) - Assert that the field must match any of the values.

### Rule

A `Rule` definition describes an individual business rule that will be executed against each input object.
Input objects can be passed on the PowerShell pipeline or supplied from file.

To define a Rule use the `Rule` keyword followed by a name and a pair of squiggly brackets `{`.
Within the `{ }` one or more conditions can be used.

Conditions determine if the input object either _Pass_ or _Fail_ the rule.

Syntax:

```text
Rule [-Name] <string> [-Tag <hashtable>] [-When <string[]>] [-Type <string[]>] [-If <scriptBlock>] [-DependsOn <string[]>] [-Configure <hashtable>] [-ErrorAction <ActionPreference>] [-Body] {
    ...
}
```

- `Name` - The name of the rule definition. Each rule name must be unique.
When packaging rules within a module, rule names must only be unique within the module.
- `Tag` - A hashtable of key/ value metadata that can be used to filter and identify rules and rule results.
- `When` - A selector precondition that must evaluate true before the rule is executed.
- `Type` - A type precondition that must match the _TargetType_ of the pipeline object before the rule is executed.
- `If` - A script precondition that must evaluate to `$True` before the rule is executed.
- `DependsOn` - A list of rules this rule depends on.
Rule dependencies must execute successfully before this rule is executed.
- `Configure` - A set of default configuration values.
These values are only used when the baseline configuration does not contain the key.
- `ErrorAction` - The action to take when an error occur.
Only a subset of preferences are supported, either `Stop` or `Ignore`.
When `-ErrorAction` is not specified the default preference is `Stop`.
When errors are ignored a rule will pass or fail based on the rule condition.
Uncaught exceptions will still cause rule return an error outcome.
- `Body` - A script block that specifies one or more conditions that are required for the rule to _Pass_.

A condition is any valid PowerShell that return either `$True` or `$False`.
Optionally, PSRule keywords can be used to help build out conditions quickly.
When a rule contains more then one condition, all must return `$True` for the rule to _Pass_.
If any one condition returns `$False` the rule has failed.

The following restrictions apply:

- Rule conditions should only return `$True` or `$False`. Other objects should be caught with `Out-Null` or null assigned like `$Null = SomeCommand`.
- The `Rule` keyword can not be nested in a `Rule` definition.
- Variables and functions defined within `.Rule.ps1` files, but outside the `Rule` definition block are not accessible unless the `Global` scope is applied.
- Functions and variables within the caller's scope (the scope calling `Invoke-PSRule`, `Get-PSRule`, `Test-PSRuleTarget`) are not accessible.
- Cmdlets that require user interaction are not supported, i.e. `Read-Host`.
- Script preconditions can contain `Exists`, `Match`, `TypeOf` and `Within` keywords.

Examples:

```powershell
# Synopsis: This rule checks for the presence of a name field
Rule 'NameMustExist' {
    Exists 'Name'
}
```

```powershell
# Synopsis: This rule checks that the title field is valid, when the rule NameMustExist is successful
Rule 'TitleIsValid' -DependsOn 'NameMustExist' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
}
```

```powershell
# Synopsis: This rule uses a threshold stored as $Configuration.minInstanceCount
Rule 'HasMinInstances' {
    $TargetObject.Sku.capacity -ge $Configuration.minInstanceCount
} -Configure @{ minInstanceCount = 2 }
```

```powershell
# Synopsis: This rule still passes because errors are ignored
Rule 'WithRuleErrorActionIgnore' -ErrorAction Ignore {
    Write-Error 'Some error';
    $True;
}
```

### Exists

The `Exists` assertion is used within a `Rule` definition to assert that a _field_ or property must exist on the pipeline object.

Syntax:

```text
Exists [-Field] <string[]> [-CaseSensitive] [-Not] [-All] [-Reason <string>] [-InputObject <PSObject>]
```

- `Field` - One or more fields/ properties that must exist on the pipeline object.
- `CaseSensitive` - The field name must match exact case.
- `Not` - Instead of checking if the field names exists they should not exist.
- `All` - All fields must exist on the pipeline object, instead of only one.
- `Reason` - A custom reason provided if the condition fails.
- `InputObject` - Supports objects being piped directly.

Examples:

```powershell
# Synopsis: Checks for the presence of a name property
Rule 'nameMustExist' {
    Exists 'Name'
}
```

```powershell
# Synopsis: Checks for the presence of name nested under the metadata property
Rule 'nameMustExist' {
    Exists 'metadata.name'
}
```

```powershell
# Synopsis: Checks for the presence of name nested under the metadata property
Rule 'nameMustExist' {
    $TargetObject.metadata | Exists 'name'
}
```

```powershell
# Synopsis: Checks that the NotName property does not exist
Rule 'NotNameMustNotExist' {
    Exists -Not 'NotName'
}
```

```powershell
# Synopsis: Checks one of Name or AlternativeName properties exist
Rule 'EitherMustExist' {
    Exists 'Name', 'AlternativeName'
}
```

```powershell
# Synopsis: Checks that both Name and Type properties exist
Rule 'AllMustExist' {
    Exists 'Name', 'Type' -All
}
```

Output:

If **any** the specified fields exists then `Exists` will return `$True`, otherwise `$False`.

If `-Not` is used, then if **any** of the fields exist then `Exists` will return `$False` otherwise `$True`.

If `-All` is used, then then **all** of the fields must exist, or not with the `-Not` switch.
If all fields exist then `Exists` will return `$True`, otherwise `$False`.
If `-Not` is used with `-All`, if **all** of the fields exist `Exists` will return `$False` otherwise `$True`.

### Match

The `Match` assertion is used within a `Rule` definition to assert that the value of a _field_ or property from pipeline data must match one or more regular expressions.
To optionally perform a case sensitive match use the `-CaseSensitive` switch, otherwise a case insensitive match will be used.

Syntax:

```text
Match [-Field] <string> [-Expression] <string[]> [-CaseSensitive] [-Not] [-Reason <string>] [-InputObject <PSObject>]
```

- `Field` - The name of the field that will be evaluated on the pipeline object.
- `Expression` - One or more regular expressions that will be used to match the value of the field.
- `CaseSensitive` - The field _value_ must match exact case.
- `Not` - Instead of checking the field value matches, the field value must not match any of the expressions.
- `Reason` - A custom reason provided if the condition fails.
- `InputObject` - Supports objects being piped directly.

Examples:

```powershell
# Synopsis: Check that PhoneNumber is complete and formatted correctly
Rule 'validatePhoneNumber' {
    Match 'PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$'
}
```

Output:

If **any** of the specified regular expressions match the field value then `Match` returns `$True`, otherwise `$False`.

When `-Not` is used, if any of the regular expressions match the field value with `Match` return `$False`, otherwise `$True`.

### Within

The `Within` assertion is used within a `Rule` definition to assert that the value of a field or property from pipeline data must equal an item from a supplied list of allowed values.
To optionally perform a case sensitive match use the `-CaseSensitive` switch, otherwise a case insensitive match will be used.

Syntax:

```text
Within [-Field] <string> [-Not] [-Like] [-Value] <PSObject[]> [-CaseSensitive] [-Reason <string>] [-InputObject <PSObject>]
```

- `Field` - The name of the field that will be evaluated on the pipeline object.
- `Value` - A list of values that the field value must match.
- `CaseSensitive` - The field _value_ must match exact case. Only applies when the field value and allowed values are strings.
- `Not` - Instead of checking the field value matches, the field value must not match any of the supplied values.
- `Like` - Instead of using an exact match, a wildcard match is used.
This switch can only be used when `Value` a string type.
- `Reason` - A custom reason provided if the condition fails.
- `InputObject` - Supports objects being piped directly.

Examples:

```powershell
# Synopsis: Ensure that the title field has one of the allowed values
Rule 'validateTitle' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
}
```

```powershell
# Synopsis: Ensure that the title field is not one of the specified values
Rule 'validateTitle' {
    Within 'Title' -Not 'Mr', 'Sir'
}
```

```powershell
# Synopsis: Ensure that the title field has one of the allowed values
Rule 'validateTitle' {
    Within 'Title' -Like 'Mr', 'M*s'
}
```

Output:

If **any** of the values match the field value then `Within` returns `$True`, otherwise `$False`.

When `-Not` is used, if any of the values match the field value with `Within` return `$False`, otherwise `$True`.

When `-Like` is used, the field value is matched against one or more wildcard expressions.

### AllOf

The `AllOf` assertion is used within a `Rule` definition to aggregate the result of assertions within a pair of squiggly brackets `{ }`.
`AllOf` is functionally equivalent to a binary **and**, where when all of the contained assertions return `$True`, `AllOf` will return `$True`.

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
# Synopsis: The Name field must exist and have a value of either John or Jane
Rule 'nameCheck' {
    AllOf {
        Exists 'Name'
        Within 'Name' 'John', 'Jane'
    }
}
```

Output:

If **all** of the assertions return `$True` AllOf will return `$True`, otherwise `$False`.

### AnyOf

The `AnyOf` assertion is used within a `Rule` definition to aggregate the result of assertions within a pair of squiggly brackets `{ }`.
`AnyOf` is functionally equivalent to a binary **or**, where if any of the contained assertions returns `$True`, `AnyOf` will return `$True`.

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
# Synopsis: The Last or Surname field must exist
Rule 'personCheck' {
    AnyOf {
        Exists 'Last'
        Exists 'Surname'
    }
}
```

Output:

If **any** of the assertions return `$True` AnyOf will return `$True`, otherwise `$False`.

### TypeOf

The `TypeOf` assertion is used within a `Rule` definition to evaluate if the pipeline object matches one or more of the supplied type names.

Syntax:

```text
TypeOf [-TypeName] <string[]> [-Reason <string>] [-InputObject <PSObject>]
```

- `TypeName` - One or more type names which will be evaluated against the pipeline object.
`TypeName` is case sensitive.
- `Reason` - A custom reason provided if the condition fails.
- `InputObject` - Supports objects being piped directly.

Examples:

```powershell
# Synopsis: The object must be a hashtable
Rule 'objectType' {
    TypeOf 'System.Collections.Hashtable'
}
```

Output:

If **any** the specified type names match the pipeline object then TypeOf will return `$True`, otherwise `$False`.

### Reason

The `Reason` keyword is used within a `Rule` definition to provide a message that indicates the reason the rule failed.
The reason is included in detailed results.

A reason is only included when the rule fails or errors.
The outcomes `Pass` and `None` do not include reason.

Use this keyword when you want to implement custom logic.
Built-in keywords including `Exists`, `Match`, `Within` and `TypeOf` automatically include a reason when they fail.

Syntax:

```text
Reason [-Text] <string>
```

- `Text` - A message that includes the reason for the failure.

Examples:

```powershell
# Synopsis: Provide reason the rule failed
Rule 'objectRecommend' {
    Reason 'A minimum of two (2) instances are required'
    $TargetObject.count -ge 2
}
```

Output:

None.

### Recommend

The `Recommend` keyword is used within a `Rule` definition to provide a recommendation to resolve the issue and pass the rule.
This may include manual steps to change that state of the object or the desired state accessed by the rule.

The recommendation can only be set once per rule.
Each object will use the same recommendation.

Syntax:

```text
Recommend [-Text] <string>
```

- `Text` - A message that includes the process to resolve the issue and pass the rule.

Examples:

```powershell
# Synopsis: Provide recommendation to resolve the issue
Rule 'objectRecommend' {
    Recommend 'Use at least two (2) instances'
    $TargetObject.count -ge 2
}
```

Output:

None.

## EXAMPLES

```powershell
# Synopsis: App Service Plan has multiple instances
Rule 'appServicePlan.MinInstanceCount' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/serverfarms' } {
    Recommend 'Use at least two (2) instances'

    $TargetObject.Sku.capacity -ge 2
}
```

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSRule/blob/main/docs/keywords/PSRule/en-US/about_PSRule_Keywords.md.

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
- Reason
- Recommend

[Invoke-PSRule]: https://github.com/Microsoft/PSRule/blob/main/docs/commands/PSRule/en-US/Invoke-PSRule.md
