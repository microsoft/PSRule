# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Note:
# This is run during container creation.

# Install Python 3 dependencies
sudo apt-get update
sudo apt-get install dotnet-sdk-8.0 -y
sudo apt-get install python3-pip -y
sudo python3 -m pip install --upgrade pip
sudo python3 -m pip install wheel

# Install Python packages
pip install -r requirements-docs.txt

# Restore .NET packages
dotnet restore

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
