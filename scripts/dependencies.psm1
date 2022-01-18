# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Note:
# Handles dependencies updates.

function Update-Dependencies {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $False)]
        [String]$Repository = 'PSGallery'
    )
    process {
        $modules = Get-Content -Path $Path -Raw | ConvertFrom-Json -AsHashtable;
        $dependencies = CheckVersion $modules.dependencies -Repository $Repository;
        $devDependencies = CheckVersion $modules.devDependencies -Repository $Repository -Dev;

        $modules = [Ordered]@{
            dependencies = $dependencies
            devDependencies = $devDependencies
        }
        $modules | ConvertTo-Json -Depth 10 | Set-Content -Path $Path;

        $updates = @(git status --porcelain);
        if ($Null -ne $Env:WORKING_BRANCH -and $Null -ne $updates -and $updates.Length -gt 0) {
            git add modules.json;
            git commit -m "Update $path";
            git push --force -u origin $Env:WORKING_BRANCH;

            $existingBranch = @(gh pr list --head $Env:WORKING_BRANCH --state open --json number | ConvertFrom-Json);
            if ($Null -eq $existingBranch -or $existingBranch.Length -eq 0) {
                gh pr create -B 'main' -H $Env:WORKING_BRANCH -l 'dependencies' -t 'Bump PowerShell dependencies' -F 'out/updates.txt';
            }
            else {
                $pr = $existingBranch[0].number
                gh pr edit $pr -F 'out/updates.txt';
            }
        }
    }
}

function Install-Dependencies {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $False)]
        [String]$Repository = 'PSGallery'
    )
    process {
        $modules = Get-Content -Path $Path -Raw | ConvertFrom-Json;
        InstallVersion $modules.dependencies -Repository $Repository;
        InstallVersion $modules.devDependencies -Repository $Repository -Dev;
    }
}

function CheckVersion {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param (
        [Parameter(Mandatory = $True)]
        [Hashtable]$InputObject,

        [Parameter(Mandatory = $True)]
        [String]$Repository,

        [Parameter(Mandatory = $False)]
        [Switch]$Dev,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath = 'out/'
    )
    begin {
        $group = 'Dependencies';
        if ($Dev) {
            $group = 'DevDependencies';
        }
        if (!(Test-Path -Path $OutputPath)) {
            $Null = New-Item -Path $OutputPath -ItemType Directory -Force;
        }
        $changeNotes = Join-Path -Path $OutputPath -ChildPath 'updates.txt';
    }
    process {
        $dependencies = [Ordered]@{ };
        $InputObject.GetEnumerator() | Sort-Object -Property Name | ForEach-Object {
            $dependencies[$_.Name] = $_.Value
        }
        foreach ($module in $dependencies.GetEnumerator()) {
            Write-Host -Object "[$group] -- Checking $($module.Name)";
            $installParams = @{}
            $installParams += $module.Value;
            $installParams.MinimumVersion = $installParams.version;
            $installParams.Remove('version');
            $available = @(Find-Module -Repository $Repository -Name $module.Name @installParams -ErrorAction Ignore);
            foreach ($found in $available) {
                if (([Version]$found.Version) -gt ([Version]$module.Value.version)) {
                    Write-Host -Object "[$group] -- Newer version found $($found.Version)";
                    $dependencies[$module.Name].version = $found.Version;
                    $Null = Add-Content -Path $changeNotes -Value "Bump $($module.Name) to $($found.Version).";
                }
                else {
                    Write-Host -Object "[$group] -- Already up to date.";
                }
            }
        }
        return $dependencies;
    }
}

function InstallVersion {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $True)]
        [String]$Repository,

        [Parameter(Mandatory = $False)]
        [Switch]$Dev
    )
    begin {
        $group = 'Dependencies';
        if ($Dev) {
            $group = 'DevDependencies';
        }
    }
    process {
        foreach ($module in $InputObject.PSObject.Properties.GetEnumerator()) {
            Write-Host -Object "[$group] -- Installing $($module.Name) v$($module.Value.version)";
            $installParams = @{ MinimumVersion = $module.Value.version };
            if ($Null -eq (Get-InstalledModule -Name $module.Name @installParams -ErrorAction Ignore)) {
                Install-Module -Name $module.Name @installParams -Force -Repository $Repository;
            }
        }
    }
}

Export-ModuleMember -Function @(
    'Update-Dependencies'
    'Install-Dependencies'
)
