#
# Install dependencies for integration with Azure DevOps
#

if ($Env:SYSTEM_DEBUG -eq 'true') {
    $VerbosePreference = 'Continue';
}

if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion 2.1.4 -ErrorAction Ignore)) {
    Install-Module PowerShellGet -MinimumVersion 2.1.4 -Scope CurrentUser -Force -AllowClobber;
}

if ($Null -eq (Get-InstalledModule -Name InvokeBuild -MinimumVersion 5.4.0 -ErrorAction Ignore)) {
    Install-Module InvokeBuild -MinimumVersion 5.4.0 -Scope CurrentUser -Force;
}
