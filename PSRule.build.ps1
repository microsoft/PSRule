
param (
    [Parameter(Mandatory = $False)]
    [String]$ModuleVersion = '0.0.1',

    [Parameter(Mandatory = $False)]
    [AllowNull()]
    [String]$ReleaseVersion,

    [Parameter(Mandatory = $False)]
    [String]$Configuration = 'Debug',

    [Parameter(Mandatory = $False)]
    [String]$NuGetApiKey,

    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False,

    [Parameter(Mandatory = $False)]
    [Switch]$Benchmark = $False,

    [Parameter(Mandatory = $False)]
    [String]$ArtifactPath = (Join-Path -Path $PWD -ChildPath out/modules)
)

if ($Env:COVERAGE -eq 'true') {
    $CodeCoverage = $True;
}

# Copy the PowerShell modules files to the destination path
function CopyModuleFiles {

    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$DestinationPath
    )

    process {
        $sourcePath = Resolve-Path -Path $Path;

        Get-ChildItem -Path $sourcePath -Recurse -File -Include *.ps1,*.psm1,*.psd1,*.ps1xml | Where-Object -FilterScript {
            ($_.FullName -notmatch '(\.(cs|csproj)|(\\|\/)(obj|bin))')
        } | ForEach-Object -Process {
            $filePath = $_.FullName.Replace($sourcePath, $destinationPath);

            $parentPath = Split-Path -Path $filePath -Parent;

            if (!(Test-Path -Path $parentPath)) {
                $Null = New-Item -Path $parentPath -ItemType Directory -Force;
            }

            Copy-Item -Path $_.FullName -Destination $filePath -Force;
        };
    }
}

task BuildDotNet {
    exec {
        # Build library
        # Add build version -p:versionPrefix=$ModuleVersion
        dotnet publish src/PSRule -c $Configuration -f netstandard2.0 -o $(Join-Path -Path $PWD -ChildPath out/modules/PSRule)
    }
}

task TestDotNet {
    exec {
        # Test library
        dotnet test --collect:"Code Coverage" --logger trx -r (Join-Path $PWD -ChildPath reports/) tests/PSRule.Tests
    }
}

task CopyModule {
    CopyModuleFiles -Path src/PSRule -DestinationPath out/modules/PSRule;

    # Copy third party notices
    Copy-Item -Path ThirdPartyNotices.txt -Destination out/modules/PSRule;

    # Copy schemas
    Copy-Item -Path schemas/* -Destination out/modules/PSRule;
}

# Synopsis: Build modules only
task BuildModule BuildDotNet, CopyModule

# Synopsis: Build help
task BuildHelp BuildModule, PlatyPS, {

    # Generate MAML and about topics
    $Null = New-ExternalHelp -OutputPath out/docs/PSRule -Path '.\docs\commands\PSRule\en-US','.\docs\keywords\PSRule\en-US', '.\docs\concepts\PSRule\en-US' -Force;

    # Copy generated help into module out path
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-US;
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-AU;
}

task ScaffoldHelp Build, {
    Import-Module (Join-Path -Path $PWD -ChildPath out/modules/PSRule) -Force;
    Update-MarkdownHelp -Path '.\docs\commands\PSRule\en-US';
}

# Synopsis: Remove temp files.
task Clean {
    Remove-Item -Path out,reports -Recurse -Force -ErrorAction SilentlyContinue;
}

task VersionModule {

    if (![String]::IsNullOrEmpty($ReleaseVersion)) {
        Write-Verbose -Message "[VersionModule] -- ReleaseVersion: $ReleaseVersion";
        $ModuleVersion = $ReleaseVersion;
    }

    if ($PSBoundParameters.ContainsKey('ModuleVersion') -and ![String]::IsNullOrEmpty($ModuleVersion)) {
        Write-Verbose -Message "[VersionModule] -- ModuleVersion: $ModuleVersion";

        $version = $ModuleVersion;
        $revision = [String]::Empty;

        Write-Verbose -Message "[VersionModule] -- Using Version: $version";
        Write-Verbose -Message "[VersionModule] -- Using Revision: $revision";

        if ($version -like '*-*') {
            [String[]]$versionParts = $version.Split('-', [System.StringSplitOptions]::RemoveEmptyEntries);
            $version = $versionParts[0];

            if ($versionParts.Length -eq 2) {
                $revision = $versionParts[1];
            }
        }

        # Update module version
        if (![String]::IsNullOrEmpty($version)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest ModuleVersion";
            Update-ModuleManifest -Path (Join-Path -Path $ArtifactPath -ChildPath PSRule/PSRule.psd1) -ModuleVersion $version;
        }

        # Update pre-release version
        if (![String]::IsNullOrEmpty($revision)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest Prerelease";
            Update-ModuleManifest -Path (Join-Path -Path $ArtifactPath -ChildPath PSRule/PSRule.psd1) -Prerelease $revision;
        }
    }
}

task ReleaseModule VersionModule, {
    if (![String]::IsNullOrEmpty($NuGetApiKey)) {
        # Publish to PowerShell Gallery
        Publish-Module -Path (Join-Path -Path $ArtifactPath -ChildPath PSRule) -NuGetApiKey $NuGetApiKey;
    }
}

task NuGet {
    $Null = Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

# Synopsis: Install Pester module
task Pester {
    if ($Null -eq (Get-InstalledModule -Name Pester -MinimumVersion '4.0.0' -ErrorAction Ignore)) {
        Install-Module -Name Pester -Scope CurrentUser -MinimumVersion '4.0.0' -Force -SkipPublisherCheck;
    }

    Import-Module -Name Pester -Verbose:$False;
}

# Synopsis: Install PlatyPS module
task platyPS {
    if ($Null -eq (Get-InstalledModule -Name PlatyPS -MinimumVersion '0.14.0' -ErrorAction Ignore)) {
        Install-Module -Name PlatyPS -Scope CurrentUser -MinimumVersion '0.14.0' -Force;
    }

    Import-Module -Name PlatyPS -Verbose:$False;
}

# Synopsis: Install PSScriptAnalyzer module
task PSScriptAnalyzer {
    if ($Null -eq (Get-InstalledModule -Name PSScriptAnalyzer -ErrorAction Ignore)) {
        Install-Module -Name PSScriptAnalyzer -Scope CurrentUser -Force;
    }

    Import-Module -Name PSScriptAnalyzer -Verbose:$False;
}

task TestModule TestDotNet, Pester, PSScriptAnalyzer, {
    # Run Pester tests
    $pesterParams = @{ Path = $PWD; OutputFile = 'reports/pester-unit.xml'; OutputFormat = 'NUnitXml'; PesterOption = @{ IncludeVSCodeMarker = $True }; PassThru = $True; };

    if ($CodeCoverage) {
        $pesterParams.Add('CodeCoverage', (Join-Path -Path $PWD -ChildPath 'out/modules/**/*.psm1'));
        $pesterParams.Add('CodeCoverageOutputFile', (Join-Path -Path $PWD -ChildPath reports/pester-coverage.xml));
    }

    if (!(Test-Path -Path reports)) {
        $Null = New-Item -Path reports -ItemType Directory -Force;
    }

    $results = Invoke-Pester @pesterParams;

    # Throw an error if pester tests failed
    if ($Null -eq $results) {
        throw 'Failed to get Pester test results.';
    }
    elseif ($results.FailedCount -gt 0) {
        throw "$($results.FailedCount) tests failed.";
    }
}

task Benchmark {
    if ($Benchmark -or $BuildTask -eq 'Benchmark') {
        dotnet run -p src/PSRule.Benchmark -f netcoreapp2.1 -c Release -- benchmark --output $PWD;
    }
}

# Synopsis: Run script analyzer
task Analyze Build, PSScriptAnalyzer, {
    Invoke-ScriptAnalyzer -Path out/modules/PSRule;
}

task CleanSite {
    git branch -D gh-pages;
    Remove-Item -Path out/site -Recurse -Force -ErrorAction Ignore;
}

# Synopsis: Build project site
task BuildSite CleanSite, {
    docfx build --force docs/docfx.json;
}

# Synopsis: Publish project site to gh-pages
task PublishSite CleanSite, {
    git worktree add -b gh-pages -f out/site origin/gh-pages;
    docfx build --force docs/docfx.json;

    try {
        Push-Location -Path out/site;
        git add *;
        git commit -m 'Update documentation';
        git push;
    }
    finally {
        Pop-Location;
    }

    git worktree remove out/site
    git worktree prune
}

# Synopsis: Build and clean.
task . Build, Test, Benchmark

# Synopsis: Build the project
task Build Clean, BuildModule, BuildHelp, VersionModule

task Test Build, TestModule

task Release ReleaseModule
