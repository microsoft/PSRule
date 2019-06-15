# Azure resource tagging example

This is an example of how PSRule can be used to validate tags on Azure resources to match an internal tagging standard.

This scenario covers the following:

- Defining a basic rule.
- Basic usage of `Exists`, `Within` and `Match` keywords.
- Using configuration in a rule definition.
- Setting configuration in YAML.
- Running rules with configuration.

In this scenario we will use a JSON file:

- [`resources.json`](resources.json) - An export of Azure resource properties saved for offline use.

To generate a similar file of your own, the use following command.

```powershell
# Get all resources using the Az modules. Alternatively use Get-AzureRmResource if using AzureRm modules.
# This command also requires authentication with Connect-AzAccount or Connect-AzureRmAccount
Get-AzResource -ExpandProperties | ConvertTo-Json -Depth 10 | Set-Content -Path .\resources.json;
```

For this example, we ran this command:

```powershell
Get-AzResource -ExpandProperties | ConvertTo-Json -Depth 10 | Set-Content -Path docs/scenarios/azure-resources/resources.json;
```

## Define rules

To validate our Azure resources, we need to define some rules. Rules are defined by using the `Rule` keyword in a file ending with the `.Rule.ps1` extension.

Our business rules for Azure resource tagging can be defined with the following dot points:

- Tag names should be easy to read and understand.
- Tag names will use lower-camel/ pascal casing.
- The following mandatory tags will be used:
  - environment: An operational environment for systems and services. Valid environments are _production_, _testing_ and _development_.
  - costCentre: A allocation account within financial systems used for charging costs to a business unit. A cost centre is a number with 5 digits and can't start with a 0.
  - businessUnit: The name of the organizational unit or team that owns the application/ solution.

To start we are going to define an `environmentTag` rule, which will ensure that the _environment_ tag exists and that the value only uses allowed values.

In the example below:

- We use `environmentTag` directly after the `Rule` keyword to name the rule definition. Each rule must be named uniquely.
- The `# Synopsis: ` comment is used to add additional metadata interpreted by PSRule.
- One or more conditions are defined within the curly braces `{ }`.
- The rule definition is saved within a file named `azureTags.Rule.ps1`.

```powershell
# Synopsis: Resource must have environment tag
Rule 'environmentTag' {
    # Rule conditions go here
}
```

### Check that tag exists

Conditions can be any valid PowerShell expression that results in a `$True` or `$False`, just like an `If` statement, but without specifically requiring the `If` keyword to be used.

In `resources.json` one of our example storage accounts has the `Tags` property as shown below, this is how Azure Resource Manager stores tags for a resource. We will use this property as the basis of our rules to determine if the resource is tagged and what the tag value is.

```json
{
    "Name": "storage",
    "ResourceName": "storage",
    "ResourceType": "Microsoft.Storage/storageAccounts",
    "Tags": {
        "role": "deployment",
        "environment": "production"
    }
}
```

PSRule also defines several additional keywords to supplement PowerShell. These additional keywords help to create readable rules that can be built out quickly.

In the example below:

- We use the `Exists` keyword to check if the _environment_ tag exists.
- The `-CaseSensitive` switch is also used to ensure that the tag name uses lowercase.
- The condition will return `$True` or `$False` back to the pipeline, where:
  - `$True` - the _environment_ tag exists.
  - `$False` - the _environment_ tag does not exist.

```powershell
# Synopsis: Resource must have environment tag
Rule 'environmentTag' {
    Exists 'Tags.environment' -CaseSensitive
}
```

### Tag uses only allowed values

In our scenario, we have three environments that our environment tag could be set to. In the next example we will ensure that only allowed environment values are used.

In the example below:

- We use the `Within` keyword to check if the _environment_ tag uses any of the allowed values.
- The `-CaseSensitive` switch is also used to ensure that the tag value is only a lowercase environment name.
- The condition will return `$True` or `$False` back to the pipeline, where:
  - `$True` - an allowed environment is used.
  - `$False` - the _environment_ tag does not use one of the allowed values.

```powershell
# Synopsis: Resource must have environment tag
Rule 'environmentTag' {
    Exists 'Tags.environment' -CaseSensitive
    Within 'Tags.environment' 'production', 'test', 'development' -CaseSensitive
}
```

Alternatively, instead of using the `Within` keyword the `-cin` operator could be used. `Within` provides additional verbose logging, however either syntax is valid.

In the example below:

- `$TargetObject` automatic variable is used to get the pipeline object being evaluated.
- We use the `-cin` operator to check the _environment_ tag only uses allowed values.
- The `-cin` operator performs a cases sensitive match on _production_, _test_ and _development_.
- The condition will return `$True` or `$False` back to the pipeline, where:
  - `$True` - an allowed environment is used.
  - `$False` - the _environment_ tag does not use one of the allowed values.

```powershell
# Synopsis: Resource must have environment tag
Rule 'environmentTag' {
    Exists 'Tags.environment' -CaseSensitive
    $TargetObject.Tags.environment -cin 'production', 'test', 'development'
}
```

### Tag value matches regular expression

For our second rule (`costCentreTag`), the _costCentre_ tag value must be 5 numbers. We can validate this by using a regular expression.

In the example below:

- We use the `Match` keyword to check if the _costCentre_ tag uses a numeric only value with 5 digits, not starting with 0.
- The condition will return `$True` or `$False` back to the pipeline, where:
  - `$True` - the _costCentre_ tag value matches the regular expression.
  - `$False` - the _costCentre_ tag value does not use match the regular expression.

```powershell
# Synopsis: Resource must have costCentre tag
Rule 'costCentreTag' {
    Exists 'Tags.costCentre' -CaseSensitive
    Match 'Tags.costCentre' '^([1-9][0-9]{4})$'
}
```

An alternative way to write the rule would be to use the `-match` operator instead of the `Match` keyword. Like the `Within` keyword, the `Match` keyword provides additional verbose logging that the `-match` operator does not provide.

In the example below:

- `$TargetObject` automatic variable is used to get the pipeline object being evaluated.
- We use the `-match` operator to check the _costCentre_ tag value matches the regular expression.
- The condition will return `$True` or `$False` back to the pipeline, where:
  - `$True` - the _costCentre_ tag value matches the regular expression.
  - `$False` - the _costCentre_ tag value does not use match the regular expression.

```powershell
# Synopsis: Resource must have costCentre tag
Rule 'costCentreTag' {
    Exists 'Tags.costCentre' -CaseSensitive
    $TargetObject.Tags.costCentre -match '^([1-9][0-9]{4})$'
}
```

### Use business unit name from configuration

For our third rule (`businessUnitTag`), the _businessUnit_ must match a valid business unit. A list of business units will be referenced from configuration instead of hard coded in the rule.

Configuration can be used within rule definitions by defining configuration in a YAML file then using the automatic variable `$Configuration`.

In the example below:

- We use the `Within` keyword to check if the _businessUnit_ tag uses any of the allowed values.
- `allowedBusinessUnits` configuration value can be referenced using the syntax `$Configuration.allowedBusinessUnits`.
- The rule definition is defined in [azureTags.Rule.ps1].
- YAML configuration is defined in [ps-rule.yaml].

An extract from _azureTags.Rule.ps1_:

```powershell
# Synopsis: Resource must have businessUnit tag
Rule 'businessUnitTag' {
    Exists 'Tags.businessUnit' -CaseSensitive
    Within 'Tags.businessUnit' $Configuration.allowedBusinessUnits
}
```

An extract from _ps-rule.yaml_:

```yaml
# Configure business units that are allowed
baseline:
  configuration:
    allowedBusinessUnits:
    - 'IT Operations'
    - 'Finance'
    - 'HR'
```

## Execute rules

With a rule defined, the next step is to execute it. To execute rules, pipe the target object to `Invoke-PSRule`.

For example:

```powershell
# Read resources in from file
$resources = Get-Content -Path .\resources.json | ConvertFrom-Json;

# Evaluate each resource against tagging rules
$resources | Invoke-PSRule -Option .\ps-rule.yaml;
```

The `ps-rule.yaml` will automatically discovered if it exists in the current working path (i.e. `.\ps-rule.yaml`). Alternatively it can be specified with the `-Option` parameter as show above.

PSRule natively supports reading from YAML and JSON files so this command-line can be simplified to:

```powershell
# Evaluate each resource against tagging rules
Invoke-PSRule -InputPath .\resources.json;
```

You will notice, we didn't specify the rule. By default PSRule will look for any `.Rule.ps1` files in the current working path.

`Invoke-PSRule` supports `-Path`, `-Name` and `-Tag` parameters that can be used to specify the path to look for rules in or filter rules if you want to run a subset of the rules.

The `-Option` parameter allows us to specify a specific YAML configuration file to use.

For this example, we ran these commands:

```powershell
# Evaluate each resource against tagging rules
Invoke-PSRule -Path docs/scenarios/azure-tags -InputPath docs/scenarios/azure-tags/resources.json -Outcome Fail -Option docs/scenarios/azure-tags/ps-rule.yaml;
```

Our output looked like this:

```text
   TargetName: storage

RuleName                            Outcome    Recommendation
--------                            -------    --------------
costCentreTag                       Fail       Resource must have costCentre tag
businessUnitTag                     Fail       Resource must have businessUnit tag


   TargetName: web-app

RuleName                            Outcome    Recommendation
--------                            -------    --------------
environmentTag                      Fail       Resource must have environment tag
costCentreTag                       Fail       Resource must have costCentre tag


   TargetName: web-app/staging

RuleName                            Outcome    Recommendation
--------                            -------    --------------
environmentTag                      Fail       Resource must have environment tag
costCentreTag                       Fail       Resource must have costCentre tag
```

Any resources that don't follow the tagging standard are reported with an outcome of `Fail`.

## More information

- [azureTags.Rule.ps1] - Example rules for validating Azure resource tagging standard rules.
- [resources.json](resources.json) - Offline export of Azure resources.
- [ps-rule.yaml] - A YAML configuration file for PSRule.

[azureTags.Rule.ps1]: azureTags.Rule.ps1
[ps-rule.yaml]: ps-rule.yaml
