# Validation pipeline example

This example covers how PSRule can be used within a DevOps pipeline to validate files, templates and objects.

This scenario covers the following:

- Installing PSRule within a continuous integration (CI) pipeline
- Failing the pipeline based on validation results
- Generating NUNit output

## Installing PSRule within a CI pipeline

Typically PSRule is not pre-installed on CI worker nodes, so within a CI pipeline the PSRule PowerShell module needs to be installed prior to calling PSRule cmdlets such as `Invoke-PSRule`.

If your CI pipeline runs on a persistent virtual machine that you control consider pre-installing PSRule. The following examples focus on installing PSRule dynamically during execution of the pipeline. Which is suitable for cloud based CI worker nodes.

To install PSRule within a CI pipeline execute the `Install-Module` PowerShell cmdlet.

In the example below:

- When installing modules on Windows, by default modules will be installed into _Program Files_, which requires administrator permissions. Depending on your environment, the CI worker process may not have administrative permissions. Instead we can install PSRule for the current context running the CI pipeline by using the `-Scope CurrentUser` parameter.
- By default this cmdlet will install the module from the PowerShell Gallery which is not trusted by default. Since a CI pipeline is not interactive the `-Force` switch is used to suppress a prompt to install modules from PowerShell Gallery.

```powershell
$Null = Install-Module -Name PSRule -Scope CurrentUser -Force;
```

In some cases installing NuGet may be required before the module can be installed. The NuGet package provider can be installed using the `Install-PackageProvider` PowerShell cmdlet.

```powershell
$Null = Install-PackageProvider -Name NuGet -Scope CurrentUser -Force;
```

The example below includes both steps together with checks:

```powershell
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
    $Null = Install-PackageProvider -Name NuGet -Scope CurrentUser -Force;
}

if ($Null -eq (Get-InstalledModule -Name PSRule -MinimumVersion '0.4.0' -ErrorAction Ignore)) {
    $Null = Install-Module -Name PSRule -Scope CurrentUser -MinimumVersion '0.4.0' -Force;
}
```

### Using Invoke-Build

`Invoke-Build` is a build automation cmdlet that can be installed from the PowerShell Gallery by installing the _InvokeBuild_ module. Within Invoke-Build, each build process is broken into tasks as shown in the example below.

```powershell
# Synopsis: Install NuGet
task InstallNuGet {
    if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
        $Null = Install-PackageProvider -Name NuGet -Scope CurrentUser -Force;
    }
}

# Synopsis: Install PSRule
task InstallPSRule InstallNuGet, {
    if ($Null -eq (Get-InstalledModule -Name PSRule -MinimumVersion '0.4.0' -ErrorAction Ignore)) {
        $Null = Install-Module -Name PSRule -Scope CurrentUser -MinimumVersion '0.4.0' -Force;
    }
}
```

## Fail the pipeline

When using PSRule within a continuous integration pipeline, typically we need to catch errors and failures and stop the pipeline if any occur.

When using `Invoke-PSRule` an easy way to catch an failure or error conditions is to use the `-Outcome Fail,Error` parameter. By using this parameter only errors or failures are returned to the pipeline. A simple `$Null` test can then throw a terminating error to stop the pipeline.

```powershell
$result = Invoke-PSRule -Outcome Fail,Error;
if ($Null -ne $result) {
    throw 'PSRule validation failed.'
}
```

Extending on this further, PSRule has additional options that we can use to log passing/ failing validation rules to informational streams.

By using the `Logging.RuleFail` option shown in the next example an error will be created for each failure so that meaningful information is logged to the CI pipeline.

```powershell
$option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
$result = $inputObjects | Invoke-PSRule -Option $option -Outcome Fail,Error;
if ($Null -ne $result) {
    throw 'PSRule validation failed.'
}
```

### Calling from Pester

If you are looking at integrating PSRule into a CI pipeline, there is a good chance that you are already using Pester. Pester is a unit test framework for PowerShell that can be installed from the PowerShell Gallery.

PSRule can complement Pester unit tests with dynamic validation rules. By using `-If` or `-Type` pre-conditions rules can dynamically provide validation for a range of use cases.

In our example we are going to validate the script files themselves:

- Have a copyright file header
- Are encoded as UTF-8

Within a Pester test script include the following example:

```powershell
Describe 'Project files' {
    Context 'Script files' {
        It 'Use content rules' {
            $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
            $inputObjects = Get-ChildItem -Path *.ps1 -Recurse;
            $inputObjects | Invoke-PSRule -Option $option -Outcome Fail,Error | Should -BeNullOrEmpty;
        }
    }
}
```

## Generating NUnit output

NUnit is a popular unit test framework for .NET. NUnit generates a test report format that is widely interpreted by CI systems. While PSRule does not use NUnit, it can output the same test report format allowing integration with any system that supports the NUnit3 for publishing test results.

To generate an NUnit report use the `-OutputFormat NUnit3` parameter.

```powershell
$option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
$inputObjects = Get-ChildItem -Path *.ps1 -Recurse;
$inputObjects | Invoke-PSRule -OutputFormat NUnit3 | Set-Content -Path reports/rule.report.xml;
```

### Publishing NUnit report with Azure DevOps

With Azure DevOps, an NUnit report can be published using [Publish Test Results task][publish-test-results].

An example YAML snippet is included below:

```yaml
# PSRule results
- task: PublishTestResults@2
  displayName: 'Publish PSRule results'
  inputs:
    testRunTitle: 'PSRule'
    testRunner: NUnit
    testResultsFiles: 'reports/rule.report.xml'
    mergeTestResults: true
    publishRunAttachments: true
  condition: succeededOrFailed()
```

## Examples

For our example we ran:

```powershell
$option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
$inputObjects = Get-ChildItem -Path src/PSRule -Include *.ps1,*.psm1,*.psd1 -Recurse;
$inputObjects | Invoke-PSRule -Path docs/scenarios/validation-pipeline  -OutputFormat NUnit3 | Set-Content -Path reports/rule.report.xml;
```

## More information

- [file.Rule.ps1] - Example rules for validating script files.

[file.Rule.ps1]: file.Rule.ps1
[publish-test-results]: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/test/publish-test-results
