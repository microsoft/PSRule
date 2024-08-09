---
reviewed: 2022-03-21
author: BernieWhite
---

# Create a standalone rule

You can use PSRule to create tests for PowerShell objects piped to PSRule for validation.
Each test is called a _rule_.

PSRule allows you to write rules using YAML, JSON, or PowerShell.
Regardless of the format you choose, any combination of YAML, JSON, or PowerShell rules can be used together.

!!! Abstract
    This topic covers how to create a rule using YAML, JSON, and PowerShell by example.
    In this quickstart, will be using native PowerShell objects.
    For an example of reading objects from disk, continue reading [Testing infrastructure][1].

  [1]: ../authoring/testing-infrastructure.md

## Prerequisites

For this quickstart, PSRule must be installed locally on MacOS, Linux, or Windows.
To install PSRule locally, open PowerShell and run the following `Install-Module` command.
If you don't have PowerShell installed, complete [Installing PowerShell][2] first.

```powershell title="PowerShell"
Install-Module -Name 'PSRule' -Repository PSGallery -Scope CurrentUser
```

!!! Tip
    PowerShell is installed by default on Windows.
    If these instructions don't work for you,
    your administrator may have restricted how PowerShell can be used in your environment.
    You or your administrator may be able to install PSRule for all users as a local administrator.
    See [Getting the modules][3] for instructions on how to do this.

!!! Tip
    To make you editing experience even better, consider installing the Visual Studio Code extension.

  [2]: ../install-instructions.md#installing-powershell
  [3]: ../install-instructions.md#getting-the-modules

## Scenario - Test for image files

In our quickstart scenario, we have been tasked with creating a rule to test for image files.
When a file ending with the `.jpg` or `.png` extension is found the rule should fail.

We will be using the following PowerShell code to get a list of files.

```powershell title="PowerShell"
$pathToSearch = $Env:HOME;
$files = Get-ChildItem -Path $pathToSearch -File -Recurse;
```

!!! Info
    The path to search `$Env:HOME` defaults to the current user's home directory.
    This directory is used so this quickstart works on Windows and Linux operating systems.
    Feel free to update this path to a more suitable directory on your local machine.

### Define the file type rule

Before an object can be tested with PSRule, one or more rules must be defined.
Each rule is defined in a file named with the suffix `.Rule.yaml`, `.Rule.jsonc`, or `.Rule.ps1`.
Multiple rules can be defined in a single file.

A rule that fails on files with `.jpg` or `.png` extensions is shown in YAML, JSON, and PowerShell formats.
You only need to choose one format, however you can choose to create all three to try out each format.

=== "YAML"

    Create the `FileType.Rule.yaml` file with the following contents.
    This file can be created in Visual Studio Code or any text editor.
    Make a note of the location you save `FileType.Rule.yaml`.

    ```yaml title="YAML"
    ---
    # Synopsis: Image files are not permitted.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: Yaml.FileType
    spec:
      type:
      - System.IO.FileInfo
      condition:
        field: Extension
        notIn:
        - .jpg
        - .png
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        This will be shown in output as the default recommendation.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name `Yaml.FileType`.
    3.  The `type` property ensures the rule will only run for file info objects.
        Other objects that might be piped to PSRule will be skipped by the `Yaml.FileType` rule.
    4.  The `condition` property determines the checks PSRule will use to test each file returned with `Get-ChildItem`.
        Specifically, the `Extension` property of each `FileInfo` object will be compared.
        The value of `Extension` should not be either `.jpg` or `.png`.

=== "JSON"

    Create the `FileType.Rule.jsonc` file with the following contents.
    This file can be created in Visual Studio Code or any text editor.
    Make a note of the location you save `FileType.Rule.jsonc`.

    ```json title="JSON"
    [
        {
            // Synopsis: Image files are not permitted.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Rule",
            "metadata": {
                "name": "Json.FileType"
            },
            "spec": {
                "type": [
                    "System.IO.FileInfo"
                ],
                "condition": {
                    "field": "Extension",
                    "notIn": [
                        ".jpg",
                        ".png"
                    ]
                }
            }
        }
    ]
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        This will be shown in output as the default recommendation.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name `Json.FileType`.
    3.  The `type` property ensures the rule will only run for file info objects.
        Other objects that might be piped to PSRule will be skipped by the `Json.FileType` rule.
    4.  The `condition` property determines the checks PSRule will use to test each file returned with `Get-ChildItem`.
        Specifically, the `Extension` property of each `FileInfo` object will be compared.
        The value of `Extension` should not be either `.jpg` or `.png`.

=== "PowerShell"

    Create the `FileType.Rule.ps1` file with the following contents.
    This file can be created in Visual Studio Code, Windows PowerShell ISE, or any text editor.
    Make a note of the location you save `FileType.Rule.ps1`.

    ```powershell title="PowerShell"
    # Synopsis: Image files are not permitted.
    Rule 'PS.FileType' -Type 'System.IO.FileInfo' {
        $Assert.NotIn($TargetObject, 'Extension', @('.jpg', '.png'))
    }
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        This will be shown in output as the default recommendation.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name `PS.FileType`.
    3.  The `-Type` parameter ensures the rule will only run for file info objects.
        Other objects that might be piped to PSRule will be skipped by the `PS.FileType` rule.
    4.  The condition contained within the curly braces `{ }` determines the checks PSRule will use to test each file returned with `Get-ChildItem`.
    5.  The `$Assert.NotIn` method checks the `Extension` property is not set to `.jpg` or `.png`.

### Testing file extensions

You can test the rule by using the `Invoke-PSRule` command.
For example:

```powershell title="PowerShell"
$pathToSearch = $Env:HOME;
$files = Get-ChildItem -Path $pathToSearch -File -Recurse;

# The path to the rule file. Update this to the location of your saved file.
$rulePath = 'C:\temp\FileType.Rule.ps1'
# Or the directory can be used to find all rules in the path:
# $rulePath = 'C:\temp\'

# Test the rule
$files | Invoke-PSRule -Path $rulePath
```

After running `Invoke-PSRule` you will get output which includes all files in the _pathToSeach_.
Files with a `.jpg` or `.png` extension should have the outcome of `Fail`.
All other files should report an outcome of `Pass`.

For example:

```text title="Output"
   TargetName: main.html

RuleName                            Outcome    Recommendation
--------                            -------    --------------
Yaml.FileType                       Pass       Image files are not permitted.

   TargetName: favicon.png

RuleName                            Outcome    Recommendation
--------                            -------    --------------
Yaml.FileType                       Fail       Image files are not permitted.
```

!!! Tip
    - If you didn't get any results with `Fail` try creating or saving a `.jpg` file in _pathToSeach_.
    - If you have too many `Pass` results you can filter the output to only fails by using `-Outcome Fail`.
      For example:

      ```powershell
      $files | Invoke-PSRule -Path $rulePath -Outcome Fail
      ```

## Scenario - Test for service status

:octicons-milestone-24: v2.0.0

In our quickstart scenario, we have been tasked to:

- Find any services that are set to start automatically with `StartType` beginning with `Automatic`.
- Fail for any service with a `Status` other than `Running`.

We will be using the following PowerShell code to get a list of local services.

```powershell title="PowerShell"
$services = Get-Service
```

!!! Note
    This scenario is designed for Windows clients.
    The PowerShell cmdlet `Get-Service` is only available on Windows.

  [v2]: ../CHANGELOG-v2.md

### Define a selector

A _selector_ can be used to filter a list of all services to only services that are set to start automatically.
Selectors use YAML or JSON expressions and are similar to rules in many ways.
A selector determines _if_ the rule will be run or skipped.

- If the selector is `true` then the rule will be run and either pass or fail.
- If the selector is `false` then the rule will be skipped.

=== "YAML"

    Create the `Service.Rule.yaml` file with the following contents.
    This file can be created in Visual Studio Code or any text editor.
    Make a note of the location you save `Service.Rule.yaml`.

    ```yaml title="YAML"
    ---
    # Synopsis: Find services with an automatic start type.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Selector
    metadata:
      name: Yaml.IsAutomaticService
    spec:
      if:
        field: StartType
        startsWith: Automatic
        convert: true
    ```

    1.  Use a short `Synopsis: ` to describe your selector in a line comment above your rule.
    2.  Name your selector with a unique name `Yaml.IsAutomaticService`.
    3.  The `if` property determines if PSRule will evaluate the service rule.
        Specifically, the `StartType` property of each service object will be compared.
        The value of `StartType` must start with `Automatic`.
    4.  The `convert` property automatically converts the enum type of `StartType` to a string.

=== "JSON"

    Create the `Service.Rule.jsonc` file with the following contents.
    This file can be created in Visual Studio Code or any text editor.
    Make a note of the location you save `Service.Rule.jsonc`.

    ```json title="JSON"
    [
        {
            // Synopsis: Find services with an automatic start type.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Selector",
            "metadata": {
                "name": "Json.IsAutomaticService"
            },
            "spec": {
                "if": {
                    "field": "StartType",
                    "startsWith": "Automatic",
                    "convert": true
                }
            }
        }
    ]
    ```

    1.  Use a short `Synopsis: ` to describe your selector in a line comment above your rule.
    2.  Name your selector with a unique name `Json.IsAutomaticService`.
    3.  The `if` property determines if PSRule will evaluate the service rule.
        Specifically, the `StartType` property of each service object will be compared.
        The value of `StartType` must start with `Automatic`.
    4.  The `convert` property automatically converts the enum type of `StartType` to a string.

### Define the service rule

Similar to the selector, the `Status` field will be tested to determine if the service is `Running`.

=== "YAML"

    Append the following contents to the existing `Service.Rule.yaml` file.

    ```yaml title="YAML"
    ---
    # Synopsis: Automatic services should be running.
    apiVersion: github.com/microsoft/PSRule/v1
    kind: Rule
    metadata:
      name: Yaml.ServiceStarted
    spec:
      with:
      - Yaml.IsAutomaticService
      condition:
        field: Status
        equals: Running
        convert: true
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        This will be shown in output as the default recommendation.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name `Yaml.ServiceStarted`.
    3.  The `with` property indicates to only run this rule on selected service objects.
        The `Yaml.IsAutomaticService` selector must first return `true` otherwise this rule will be skipped.
    4.  The `condition` property determines the checks PSRule will use to test each service.
        Specifically, the `Status` property will be compared.
        The value of `Status` must be `Running`.
    5.  The `convert` property automatically converts the enum type of `Status` to a string.

=== "JSON"

    Update the contents of `Service.Rule.jsonc` to the following.

    ```json title="JSON"
    [
        {
            // Synopsis: Find services with an automatic start type.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Selector",
            "metadata": {
                "name": "Json.IsAutomaticService"
            },
            "spec": {
                "if": {
                    "field": "StartType",
                    "startsWith": "Automatic",
                    "convert": true
                }
            }
        },
        {
            // Synopsis: Automatic services should be running.
            "apiVersion": "github.com/microsoft/PSRule/v1",
            "kind": "Rule",
            "metadata": {
                "name": "Json.ServiceStarted"
            },
            "spec": {
                "with": [
                    "Json.IsAutomaticService"
                ],
                "condition": {
                    "field": "Status",
                    "equals": "Running",
                    "convert": true
                }
            }
        }
    ]
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        This will be shown in output as the default recommendation.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name `Json.ServiceStarted`.
    3.  The `with` property indicates to only run this rule on selected service objects.
        The `Json.IsAutomaticService` selector must first return `true` otherwise this rule will be skipped.
    4.  The `condition` property determines the checks PSRule will use to test each service.
        Specifically, the `Status` property will be compared.
        The value of `Status` must be `Running`.
    5.  The `convert` property automatically converts the enum type of `Status` to a string.

=== "PowerShell"

    Create the `Service.Rule.ps1` file with the following contents.
    This file can be created in Visual Studio Code, Windows PowerShell ISE, or any text editor.
    Make a note of the location you save `Service.Rule.ps1`.

    ```powershell title="PowerShell"
    # Synopsis: Automatic services should be running.
    Rule 'PS.ServiceStarted' -With 'Yaml.IsAutomaticService' {
        $status = $TargetObject.Status.ToString()
        $Assert.HasFieldValue($status, '.', 'Running')
    }
    ```

    1.  Use a short `Synopsis: ` to describe your rule in a line comment above your rule.
        This will be shown in output as the default recommendation.
        For this to be interpreted by PSRule, only a single line is allowed.
    2.  Name your rule with a unique name `PS.ServiceStarted`.
    3.  The `-With` parameter indicates to only run this rule on selected service objects.
        The `Yaml.IsAutomaticService` selector must first return `true` otherwise this rule will be skipped.
    4.  The condition contained within the curly braces `{ }` determines the checks PSRule will use to test each service object.
    5.  The `Status` enum property is converted to a string.
    6.  The `$Assert.HasFieldValue` method checks the converted `Status` property is set to `Running`.

### Testing service objects

You can test the rule with service object by using the `Invoke-PSRule` command.
For example:

```powershell title="PowerShell"
$services = Get-Service

# The directory path to the rule file. Update this to the location of your saved file.
$rulePath = 'C:\temp\'

# Test the rule
$services | Invoke-PSRule -Path $rulePath
```

After running `Invoke-PSRule` you will get output which include for services that start automatically.
Services that are `Running` should pass whereas other stopped services should fail.
For manual or disabled services a warning will be generated indicating that no matching rules were found.

For example:

```text title="Output"
   TargetName: edgeupdate

RuleName                            Outcome    Recommendation
--------                            -------    --------------
PS.ServiceStarted                   Fail       Automatic services should be running.
Yaml.ServiceStarted                 Fail       Automatic services should be running.
Json.ServiceStarted                 Fail       Automatic services should be running.


   TargetName: EventLog

RuleName                            Outcome    Recommendation
--------                            -------    --------------
PS.ServiceStarted                   Pass       Automatic services should be running.
Yaml.ServiceStarted                 Pass       Automatic services should be running.
Json.ServiceStarted                 Pass       Automatic services should be running.

WARNING: Target object 'TermService' has not been processed because no matching rules were found.
```

!!! Tip
    You can disable the warning by setting [Execution.UnprocessedObject][5] option.
    Alternatively you can ignore all warnings by using the `-WarningAction SilentlyContinue` parameter.

  [5]: ../concepts/PSRule/en-US/about_PSRule_Options.md#executionunprocessedobject
