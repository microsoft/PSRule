---
author: BernieWhite
---

# Using expressions

PSRule allows you to write rules using YAML, JSON, or PowerShell.
This offers a lot of flexibility to use PSRule for a variety of use cases.
Some examples of use cases for each format include:

- **YAML** &mdash; Start authoring quickly with minimal knowledge of PowerShell.
- **JSON** &mdash; Generate rules automatically using automation tools.
- **PowerShell** &mdash; Integrate with other tools using PowerShell cmdlets.

!!! Abstract
    This topic covers the differences and limitations between authoring rules using YAML, JSON, and PowerShell.
    For an example of authoring rules see [Writing rules][1] or [Testing infrastructure][2] topics.

  [1]: ../quickstart/standalone-rule.md
  [2]: testing-infrastructure.md

## Language comparison

Expressions and assertion methods can be used to build similar conditions.

- **Expressions** &mdash; Schema-based conditions written in YAML or JSON.
  Expressions can be used in rules and selectors.
- **Assertion methods** &mdash; PowerShell-based condition helpers that make rules faster to author.
  Assertion methods can be used in combination with standard PowerShell code to build rules or conventions.

### Quick reference

In most cases expressions and assertion method names match.
There are some cases where these names do not directly align.
This lookup table provides a quick reference for expressions and their assertion method counterpart.

Expression      | Assertion method
----------      | ----------------
Contains        | Contains
Count           | Count
Equals [^1]     | _n/a_
EndsWith        | EndsWith
Exists          | HasField
Greater         | Greater
GreaterOrEquals | GreaterOrEqual
HasDefault      | HasDefaultValue
HasSchema       | HasJsonSchema
HasValue [^1]   | _n/a_
In              | In
IsLower         | IsLower
IsString        | IsString
IsUpper         | IsUpper
Less            | Less
LessOrEquals    | LessOrEqual
Match           | Match
NotEquals       | _n/a_
NotIn           | NotIn
NotMatch        | NotMatch
SetOf           | SetOf
StartsWith      | StartsWith
Subset          | Subset
Version         | Version
_n/a_           | FileHeader
_n/a_           | FilePath
_n/a_           | HasFields
_n/a_           | HasFieldValue [^1]
IsArray         | IsArray
IsBoolean       | IsBoolean
IsDateTime      | IsDateTime
IsInteger       | IsInteger
IsNumeric       | IsNumeric
_n/a_           | JsonSchema
Exists          | NotHasField
_n/a_           | NotNull
NotWithinPath   | NotWithinPath
_n/a_           | Null
_n/a_           | NullOrEmpty
_n/a_           | TypeOf
WithinPath      | WithinPath

[^1]: The `Equals`, `HasValue` expressions and `HasFieldValue` are similar.
