# Using within continuous integration

PSRule supports several features that make it easy to a continuous integration (CI) pipeline.
When added to a pipeline, PSRule can validate files, template and objects dynamically.

This scenario covers the following:

- Installing within a CI pipeline.
- Validating objects.
- Formatting output.
- Failing the pipeline.
- Generating NUnit output.
- Additional options.

## Installing within a CI pipeline

Typically, PSRule is not pre-installed on CI worker nodes and must be installed.
If your CI pipeline runs on a persistent virtual machine that you control, consider pre-installing PSRule.
The following examples focus on installing PSRule dynamically during execution of the pipeline.
Which is suitable for cloud-based CI worker nodes.

To install PSRule within a CI pipeline execute the `Install-Module` PowerShell cmdlet.

In the example below:

- When installing modules on Windows, modules will be installed into _Program Files_ by default, which requires administrator permissions.
Depending on your environment, the CI worker process may not have administrative permissions.
Instead we can install PSRule for the current context running the CI pipeline by using the `-Scope CurrentUser` parameter.
- By default, this cmdlet will install the module from the PowerShell Gallery which is not trusted by default.
Since a CI pipeline is not interactive, use the `-Force` switch to suppress the confirmation prompt.

```powershell
Install-Module -Name PSRule -Scope CurrentUser -Force;
```

In some cases, installing NuGet and PowerShellGet may be required to connect to the PowerShell Gallery.
The NuGet package provider can be installed using the `Install-PackageProvider` PowerShell cmdlet.

```powershell
Install-PackageProvider -Name NuGet -Scope CurrentUser -Force;
Install-Module PowerShellGet -MinimumVersion '2.2.1' -Scope CurrentUser -Force -AllowClobber;
```

The example below includes both steps together with checks:

```powershell
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
    Install-PackageProvider -Name NuGet -Scope CurrentUser -Force;
}

if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion '2.2.1' -ErrorAction Ignore)) {
    Install-Module PowerShellGet -MinimumVersion '2.2.1' -Scope CurrentUser -Force -AllowClobber;
}
```

```powershell
if ($Null -eq (Get-InstalledModule -Name PSRule -MinimumVersion '2.1.0' -ErrorAction SilentlyContinue)) {
    Install-Module -Name PSRule -Scope CurrentUser -MinimumVersion '2.1.0' -Force;
}
```

See the [change log](https://github.com/Microsoft/PSRule/blob/main/CHANGELOG.md) for the latest version.

## Validating objects

To validate objects use `Invoke-PSRule`, `Assert-PSRule` or `Test-PSRuleTarget`.
In a CI pipeline, `Assert-PSRule` is recommended.
`Assert-PSRule` outputs preformatted results ideal for use within a CI pipeline.

For rules within the same source control repository, put rules in the `.ps-rule` directory.
A directory `.ps-rule` in the repository root, is used by convention.

In the following example, objects are validated against rules from the `./.ps-rule/` directory:

```powershell
$items | Assert-PSRule -Path './.ps-rule/'
```

Example output:

```text
 -> ObjectFromFile.psd1 : System.IO.FileInfo

    [PASS] File.Header
    [PASS] File.Encoding
    [WARN] Target object 'ObjectFromFile.yaml' has not been processed because no matching rules were found.
    [WARN] Target object 'ObjectFromNestedFile.yaml' has not been processed because no matching rules were found.
    [WARN] Target object 'Baseline.Rule.yaml' has not been processed because no matching rules were found.

 -> FromFile.Rule.ps1 : System.IO.FileInfo

    [FAIL] File.Header
    [PASS] File.Encoding
```

In the next example, objects from file are validated against pre-defined rules from a module:

```powershell
Assert-PSRule -InputPath .\resources-*.json -Module PSRule.Rules.Azure;
```

## Formatting output

When executing a CI pipeline, feedback on any validation failures is important.
The `Assert-PSRule` cmdlet provides easy to read formatted output instead of PowerShell objects.

Additionally, `Assert-PSRule` supports styling formatted output for Azure Pipelines and GitHub Actions.
Use the `-Style AzurePipelines` or `-Style GitHubActions` parameter to style output.

For example:

```powershell
$items | Assert-PSRule -Path './.ps-rule/' -Style AzurePipelines;
```

## Failing the pipeline

When using PSRule within a CI pipeline, a failed rule should stop the pipeline.
When using `Assert-PSRule` if any rules fail, an error will be generated.

```text
Assert-PSRule : One or more rules reported failure.
At line:1 char:10
+ $items | Assert-PSRule -Path ./.ps-rule/
+          ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
+ CategoryInfo          : InvalidData: (:) [Assert-PSRule], FailPipelineException
+ FullyQualifiedErrorId : PSRule.Fail,Assert-PSRule
```

A single PowerShell error is typically enough to stop a CI pipeline.
If you are using a different configuration additionally `-ErrorAction Stop` can be used.

For example:

```powershell
$items | Assert-PSRule -Path './.ps-rule/' -ErrorAction Stop;
```

Using `-ErrorAction Stop` will stop the current script and return an exit code of 1.

To continue running the current script but return an exit code, use:

```powershell
try {
    $items | Assert-PSRule -Path './.ps-rule/' -ErrorAction Stop;
}
catch {
    $Host.SetShouldExit(1);
}
```

## Generating NUnit output

NUnit is a popular unit test framework for .NET.
NUnit generates a test report format that is widely interpreted by CI systems.
While PSRule does not use NUnit directly, it support outputting validation results in the NUnit3 format.
Using a common format allows integration with any system that supports the NUnit3 for publishing test results.

To generate an NUnit report:

- Use the `-OutputFormat NUnit3` parameter.
- Use the `-OutputPath` parameter to specify the path of the report file to write.

```powershell
$items | Assert-PSRule -Path './.ps-rule/' -OutputFormat NUnit3 -OutputPath reports/rule-report.xml;
```

The output path will be created if it does not exist.

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
    testResultsFiles: 'reports/rule-report.xml'
    mergeTestResults: true
    publishRunAttachments: true
  condition: succeededOrFailed()
```

## Complete example

Putting each of these steps together.

### Install dependencies

```powershell
# Install dependencies for connecting to PowerShell Gallery
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion '2.2.1' -ErrorAction SilentlyContinue)) {
    Install-Module PowerShellGet -MinimumVersion '2.2.1' -Scope CurrentUser -Force -AllowClobber;
}
```

### Validate files

```powershell
# Install PSRule module
if ($Null -eq (Get-InstalledModule -Name PSRule -MinimumVersion '2.1.0' -ErrorAction SilentlyContinue)) {
    Install-Module -Name PSRule -Scope CurrentUser -MinimumVersion '2.1.0' -Force;
}

# Validate files
$assertParams = @{
    Path = './.ps-rule/'
    Style = 'AzurePipelines'
    OutputFormat = 'NUnit3'
    OutputPath = 'reports/rule-report.xml'
}
$items = Get-ChildItem -Recurse -Path .\src\,.\tests\ -Include *.ps1,*.psd1,*.psm1,*.yaml;
$items | Assert-PSRule $assertParams -ErrorAction Stop;
```

### Azure DevOps Pipeline

```yaml
steps:

# Install dependencies
- powershell: ./pipeline-deps.ps1
  displayName: 'Install dependencies'

# Validate templates
- powershell: ./validate-files.ps1
  displayName: 'Validate files'

# Publish pipeline results
- task: PublishTestResults@2
  displayName: 'Publish PSRule results'
  inputs:
    testRunTitle: 'PSRule'
    testRunner: NUnit
    testResultsFiles: 'reports/rule-report.xml'
    mergeTestResults: true
    publishRunAttachments: true
  condition: succeededOrFailed()
```

## Additional options

### Using Invoke-Build

Invoke-Build is a build automation cmdlet that can be installed from the PowerShell Gallery by installing the _InvokeBuild_ module.
Within Invoke-Build, each build process is broken into tasks.

The following example shows an example of using PSRule with Invoke-Build tasks.

```powershell
# Synopsis: Install PSRule
task PSRule {
    if ($Null -eq (Get-InstalledModule -Name PSRule -MinimumVersion '2.1.0' -ErrorAction SilentlyContinue)) {
        Install-Module -Name PSRule -Scope CurrentUser -MinimumVersion '2.1.0' -Force;
    }
}

# Synopsis: Validate files
task ValidateFiles PSRule, {
    $assertParams = @{
        Path = './.ps-rule/'
        Style = 'AzurePipelines'
        OutputFormat = 'NUnit3'
        OutputPath = 'reports/rule-report.xml'
    }
    $items = Get-ChildItem -Recurse -Path .\src\,.\tests\ -Include *.ps1,*.psd1,*.psm1,*.yaml;
    $items | Assert-PSRule @assertParams -ErrorAction Stop;
}

# Synopsis: Run all build tasks
task Build ValidateFiles
```

```powershell
Invoke-Build Build;
```

### Calling from Pester

Pester is a unit test framework for PowerShell that can be installed from the PowerShell Gallery.

Typically, Pester unit tests are built for a particular pipeline.
PSRule can complement Pester unit tests by providing dynamic and sharable rules that are easy to reuse.
By using `-If` or `-Type` pre-conditions, rules can dynamically provide validation for a range of use cases.

When calling PSRule from Pester use `Invoke-PSRule` instead of `Assert-PSRule`.
`Invoke-PSRule` returns validation result objects that can be tested by Pester `Should` conditions.

For example:

```powershell
Describe 'Azure' {
    Context 'Resource templates' {
        It 'Use content rules' {
            $invokeParams = @{
                Path = './.ps-rule/'
                OutputFormat = 'NUnit3'
                OutputPath = 'reports/rule-report.xml'
            }
            $items = Get-ChildItem -Recurse -Path .\src\,.\tests\ -Include *.ps1,*.psd1,*.psm1,*.yaml;
            Invoke-PSRule @invokeParams -Outcome Fail,Error | Should -BeNullOrEmpty;
        }
    }
}
```

## More information

- [pipeline-deps.ps1](pipeline-deps.ps1) - Example script installing pipeline dependencies.
- [file.Rule.ps1](file.Rule.ps1) - Example rules for validating script files.
- [validate-files.ps1](validate-files.ps1) - Example script for running files validation.
- [azure-pipelines.yaml](azure-pipelines.yaml) - An example Azure DevOps Pipeline.

[publish-test-results]: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/test/publish-test-results
