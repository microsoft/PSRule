# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Note:
# This is run during container creation.

sudo apt-get update -y
sudo apt-get upgrade -y
dotnet restore

# . /home/vscode/python/dev/bin/Activate.ps1

pip install -r requirements-docs.txt

# Install PowerShell dependencies
$ProgressPreference = [System.Management.Automation.ActionPreference]::SilentlyContinue;
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}
if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion 2.2.1 -ErrorAction Ignore)) {
    Install-Module PowerShellGet -MinimumVersion 2.2.1 -Scope CurrentUser -Force -AllowClobber;
}
if ($Null -eq (Get-InstalledModule -Name InvokeBuild -MinimumVersion 5.4.0 -ErrorAction Ignore)) {
    Install-Module InvokeBuild -MinimumVersion 5.4.0 -Scope CurrentUser -Force;
}

Import-Module ./scripts/dependencies.psm1;
Install-Dependencies -Dev;
