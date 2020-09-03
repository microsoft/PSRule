# Getting started with PSRule

### Define a rule

To define a rule, use a `Rule` block saved to a file with the `.Rule.ps1` extension.

```powershell
Rule 'NameOfRule' {
    # Rule conditions
}
```

Within the body of the rule provide one or more conditions.
A condition is valid PowerShell that results in `$True` or `$False`.

For example:

```powershell
Rule 'isFruit' {
    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

An optional result message can be added to by using the `Recommend` keyword.

```powershell
Rule 'isFruit' {
    # An recommendation to display in output
    Recommend 'Fruit is only Apple, Orange and Pear'

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

The rule is saved to a file named [`isFruit.Rule.ps1`](fruit/isFruit.Rule.ps1) file.
One or more rules can be defined within a single file.

### Execute a rule

To execute the rule use `Invoke-PSRule`.

For example:

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge' };
$items += [PSCustomObject]@{ Name = 'Apple' };

# Validate each item using rules saved in current working path
$items | Invoke-PSRule;
```

The output of this example is:

```text
   TargetName: Fridge

RuleName                            Outcome    Recommendation
--------                            -------    --------------
isFruit                             Fail       Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Recommendation
--------                            -------    --------------
isFruit                             Pass       Fruit is only Apple, Orange and Pear
```

### Additional options

To filter results to only non-fruit results, use `Invoke-PSRule -Outcome Fail`.
Passed, failed and error results are shown by default.

```powershell
# Only show non-fruit results
$items | Invoke-PSRule -Outcome Fail;
```

For a summary of results for each rule use `Invoke-PSRule -As Summary`.

For example:

```powershell
# Show rule summary
$items | Invoke-PSRule -As Summary;
```

The output of this example is:

```text
RuleName                            Pass  Fail  Outcome
--------                            ----  ----  -------
isFruit                             1     1     Fail
```

An optional failure reason can be added to the rule block by using the `Reason` keyword.

```powershell
Rule 'isFruit' {
    # An recommendation to display in output
    Recommend 'Fruit is only Apple, Orange and Pear'

    # An failure reason to display for non-fruit
    Reason "$($PSRule.TargetName) is not fruit."

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

To include the reason with output use `Invoke-PSRule -OutputFormat Wide`.

For example:

```powershell
# Show failure reason for failing results
$items | Invoke-PSRule -OutputFormat Wide;
```

The output of this example is:

```text

   TargetName: Fridge

RuleName                            Outcome    Reason                              Recommendation
--------                            -------    ------                              --------------
isFruit                             Fail       Fridge is not fruit.                Fruit is only Apple, Orange and Pear


   TargetName: Apple

RuleName                            Outcome    Reason                              Recommendation
--------                            -------    ------                              --------------
isFruit                             Pass                                           Fruit is only Apple, Orange and Pear
```

The final rule is saved to [`isFruit.Rule.ps1`](fruit/isFruit.Rule.ps1).

### Scenarios

For walk through examples of PSRule usage see:

- [Validate Azure resource configuration](azure-resources/azure-resources.md)
- [Validate Azure resources tags](azure-tags/azure-tags.md)
- [Validate Kubernetes resources](kubernetes-resources/kubernetes-resources.md)
- [Using within continuous integration](validation-pipeline/validation-pipeline.md)
- [Packaging rules in a module](rule-module/rule-module.md)
- [Writing rule help](rule-docs/rule-docs.md)
