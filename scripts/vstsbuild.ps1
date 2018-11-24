
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

if ($Null -eq (Get-Module -Name InvokeBuild -ListAvailable -ErrorAction SilentlyContinue | Where-Object -FilterScript { $_.Version -like '5.*' })) {
    Install-Module InvokeBuild -MinimumVersion 5.4.0 -Scope CurrentUser -Force;
}
