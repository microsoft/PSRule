# Azure Resource Manager example

This is an example of how PSRule can be used to validate Azure resources match internal standards or assess if resources meet a defined baseline.

In this scenario we will use a JSON file:

- [`resources.json`](resources.json) - An export for the Azure resource properties saved for offline use.

To generate a similar `resources.json` file of your own, the use following command.

```powershell
# Get all resources using the Az modules. Alternatively use Get-AzureRmResource if using AzureRm modules.
# This command also requires authentication with Connect-AzAccount or Connect-AzureRmAccount
Get-AzResource -ExpandProperties | ConvertTo-Json -Depth 10 | Set-Content -path .\resources.json;
```

For this example we ran this command:

```powershell
Get-AzResource -ExpandProperties | ConvertTo-Json -Depth 10 | Set-Content -path docs/scenarios/azure-resources/resources.json;
```

## Define rules

To validate our Azure resources we need to define some rules. Rules are defined by using the `Rule` keyword in a file ending with the `.Rule.ps1` extension.

So start we are going to define a `storageAccounts.UseHttps` rule, which will validate that Azure Storage resources have a [Secure Transfer Required][azure-docs-secure-transfer] enabled.

In the example below:

- We use `storageAccounts.UseHttps` directly after the `Rule` keyword to name the rule definition. Each rule must be named uniquely.
- The `# Description: ` comment is used to add additional metadata interpreted by PSRule.
- One or more conditions are defined within the curly braces `{ }`.
- The rule definition is saved within a file named `storageAccounts.Rule.ps1`.

```powershell
# Description: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' {
    # Rule conditions go here
}
```

### Set rule condition

Conditions can be any valid PowerShell expression that results in a `$True` or `$False`, just like an `If` statement, but without specifically requiring the `If` keyword to be used.

> Several PSRule keywords such as `Exists` and `AllOf` can supplement PowerShell to quickly build out rules that are easy to read.

In `resources.json` one of our example storage accounts has a property `Properties.supportsHttpsTrafficOnly` as shown below, which will be how our rule will pass `$True` or fail `$False` Azure resources that we throw at it.

```json
{
    "Name": "storage",
    "ResourceName": "storage",
    "ResourceType": "Microsoft.Storage/storageAccounts",
    "Kind": "Storage",
    "ResourceGroupName": "test-rg",
    "Location": "eastus2",
    "Properties": {
        "supportsHttpsTrafficOnly": false
    }
}
```

In the example below:

- We use the `$TargetObject` variable to get the object on the pipeline and access it's properties.
- The condition will return `$True` or `$False` back to the pipeline, where:
  - `$True` - the object passed the validation check
  - `$False` - the object failed the validation check

```powershell
# Description: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' {
    # This property returns true or false, so nothing more needs to be done
    $TargetObject.Properties.supportsHttpsTrafficOnly

    # Alternatively this could be written as:
    # $TargetObject.Properties.supportsHttpsTrafficOnly -eq $True
}
```

### Add rule hint

Additionally to provide feedback to the person or process running the rules, we can use the `Hint` keyword to set a message that appears in results.

If a hint message is not provided the description will be used instead.

In the example below:

- Directly after the `Hint` keyword is a message to help understand why the rule failed or passed.

```powershell
# Description: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' {
    Hint 'Storage accounts should only allow secure traffic'

    $TargetObject.Properties.supportsHttpsTrafficOnly
}
```

### Filter with preconditions

So far our rule works for a Storage Account, but there are many type of resources that could be returned by calling `Get-AzResource`. Most of these resources won't have the `Properties.supportsHttpsTrafficOnly` property, and if it did, it may use different configuration options instead of just `true` and `false`. This is where preconditions help out.

Preconditions can be specified by using the `-If` parameter when defining a rule. When the rule is executed, if the precondition is `$True` then the rule is processed, otherwise it is skipped.

In the example below:

- A check against `$TargetObject.ResourceType` ensured that our rule is only processed for Storage Accounts.

```powershell
# Description: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' -If { $TargetObject.ResourceType -eq 'Microsoft.Storage/storageAccounts' } {
    Hint 'Storage accounts should only allow secure traffic'

    $TargetObject.Properties.supportsHttpsTrafficOnly
}
```

## Execute rules

With a rule defined, the next step is to execute it. To execute rules, pipe the target object to `Invoke-PSRule`.

For example:

```powershell
# Read resources in from file
$resources = Get-Content -Path .\resources.json | ConvertFrom-Json;

# Process resources
$resources | Invoke-PSRule;
```

PSRule natively supports reading from YAML and JSON files so this command-line can be simplified to:

```powershell
Invoke-PSRule -InputPath .\resources.json;
```

You will notice, we didn't specify the rule. By default PSRule will look for any `.Rule.ps1` files in the current working path.

`Invoke-PSRule` supports `-Path`, `-Name` and `-Tag` parameters that can be used to specify the path to look for rules in or filter rules if you want to run a subset of the rules.

For this example we ran these commands:

```powershell
Invoke-PSRule -Path docs/scenarios/azure-resources -InputPath docs/scenarios/azure-resources/resources.json;
```

Our output looked like this:

```text
   TargetName: storage

RuleName                            Outcome    Message
--------                            -------    -------
storageAccounts.UseHttps            Fail       Storage accounts should only allow secure traffic
```

In our case `storageAccounts.UseHttps` returns a `Fail` outcome because our storage account has `supportsHttpsTrafficOnly` = `false`, which is exactly what should happen.

## Define helper functions

Using helper functions is completely optional and not required in many cases. However, you may prefer to use helper functions when rule conditions or preconditions are complex and hard to understand.

To use helper functions use a `function` block within a file with a `.Rule.ps1` extension. Any code within `.Rule.ps1` files called by `Invoke-PSRule` will be executed, however to make it available for use within a rule, a global scope modifier must be used.

For functions this is done by prefixing the function name with `global:`.

For example:

```powershell
function global:NameOfFunction {
    # Function body
}
```

In our example, we are going to define a `ResourceType` function in a file named `common.Rule.ps1`. This function will be used by preconditions to check the type of Azure resource.

```powershell
# A custom function to filter by resource type
function global:ResourceType {
    param (
        [String]$ResourceType
    )

    process {
        return $TargetObject.ResourceType -eq $ResourceType;
    }
}
```

Updating our existing `storageAccounts.UseHttps` rule, our rule definition becomes:

```powershell
# Description: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' -If { ResourceType 'Microsoft.Storage/storageAccounts' } {
    Hint 'Storage accounts should only allow secure traffic'

    $TargetObject.Properties.supportsHttpsTrafficOnly
}
```

## More information

- [storageAccounts.Rule.ps1](storageAccounts.Rule.ps1) - Example rules for validating Azure Storage.
- [appService.Rule.ps1](appService.Rule.ps1) - Example rules for validating Azure App Service.
- [resources.json](resources.json) - Offline export of Azure resources.
- [common.Rule.ps1](common.Rule.ps1) - ResourceType helper function.

[azure-docs-secure-transfer]: https://docs.microsoft.com/en-us/azure/storage/common/storage-require-secure-transfer
