# Using PSRule from a Container

Depending on your development or CI/CD process for your environment you may desire to use PSRules to validate your Infrastructure as Code (IaC) from a container. This document shows how you can use a simple container based on the [mcr.microsoft.com/powershell](https://hub.docker.com/_/microsoft-powershell) image from Microsoft.

In this tutorial we are going to use a simple Ubuntu based PowerShell image to validate an ARM template. We will do this by creating a dockerfile to describe and create a container image that we can then run. When we run the container we will use a volume mount to share our ARM template and test code for the container to then execute the PSRule.Rules.Azure PSRules against our ARM template and output the results.

## Creating the image

Creating an image ready to run PSRules first requires a dockerfile. The below example will use the latest powershell image released and install the PSRule and PSRule.Rules.Azure modules. 

```dockerfile
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

FROM mcr.microsoft.com/powershell:7.2-ubuntu-22.04
SHELL ["pwsh", "-command"]

RUN Install-Module -Name 'PSRule','PSRule.Rules.Azure' -Force
```

The below docker command can be used to create the image locally.

```powershell
docker build --tag psrule:latest .
```

!!! Note
    While fine for an example, it is common to always reference a container by a version 
    number and not the "latest" tag. Using the "latest" tag may lead to unexpected behavior as
    version changes occur.

## Create your test script

Create a new directory and add a new file named `validate-files.ps1`. This file will run the PSRule
test for us on our new container image. Add the below code to the file.

```powershell
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Create a PSRule AzRuleTemplate data file and run the PSRule.Rules.Azure module rules against the output.
#>

Get-AzRuleTemplateLink "$PSScriptRoot/template" | Export-AzRuleTemplateData -OutputPath "$PSScriptRoot/out"

Assert-PSRule -InputPath "$PSScriptRoot/out/" -Module 'PSRule.Rules.Azure' -As Summary
```

Also, within the new directory add another directory named `template`. Add any ARM template you would like to
test in this directory. For a starting point you can get a template from [Azure Quickstart Templates.](https://azure.microsoft.com/resources/templates/)

Your directory should now look like the below.

```
- Directory 
  |--> validate-files.ps1
  |--> template
    |--> ARM template...
```

## Run PSRules in the container

Now we are ready to go! Run the below docker command to test the ARM template.

```powershell
docker run -it --rm -v $PWD/:/src psrule:latest pwsh -file /src/validate-files.ps1
```

This command runs the container and the PSRule tests by mounting the directory to the /src path
and then executing the `validate-files.ps1` script.

!!! Note
    The volume mount option expects your current working directory to be the new directory created.
    You can change this to an absolute or relative path if desired.

## Clean up

When you are ready to clean up the container image you can do so with the below command.

```powershell
docker image rm psrule
```
