# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

[CmdletBinding()]
param (
    [Parameter(Mandatory = $False)]
    [String]$Build = '0.0.1',

    [Parameter(Mandatory = $False)]
    [String]$Configuration = 'Debug',

    [Parameter(Mandatory = $False)]
    [String]$ApiKey,

    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False,

    [Parameter(Mandatory = $False)]
    [Switch]$Benchmark = $False,

    [Parameter(Mandatory = $False)]
    [String]$ArtifactPath = (Join-Path -Path $PWD -ChildPath out/modules),

    [Parameter(Mandatory = $False)]
    [String]$AssertStyle = 'AzurePipelines',

    [Parameter(Mandatory = $False)]
    [String]$TestGroup = $Null
)

Write-Host -Object "[Pipeline] -- PowerShell v$($PSVersionTable.PSVersion.ToString())" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- PWD: $PWD" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- ArtifactPath: $ArtifactPath" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- BuildNumber: $($Env:BUILD_BUILDNUMBER)" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- SourceBranch: $($Env:BUILD_SOURCEBRANCH)" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- SourceBranchName: $($Env:BUILD_SOURCEBRANCHNAME)" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- Culture: $((Get-Culture).Name), $((Get-Culture).Parent)" -ForegroundColor Green;

if ($Env:SYSTEM_DEBUG -eq 'true') {
    $VerbosePreference = 'Continue';
}

$forcePublish = $False;
if ($Env:FORCE_PUBLISH -eq 'true') {
    $forcePublish = $True;
}

if ($Env:BUILD_SOURCEBRANCH -like '*/tags/*' -and $Env:BUILD_SOURCEBRANCHNAME -like 'v*.*') {
    $Build = $Env:BUILD_SOURCEBRANCHNAME.Substring(1);
}

$PSUpstream = 'PSGallery';
if ($Env:PSUPSTREAM -ne $Null -and $Env:PSUPSTREAM -ne '') {
    $PSUpstream = $Env:PSUPSTREAM;
}

Write-Host -Object "[Pipeline] -- Using PSUpstream: $PSUpstream" -ForegroundColor Green;

$version = $Build;
$versionSuffix = [String]::Empty;

if ($version -like '*-*') {
    [String[]]$versionParts = $version.Split('-', [System.StringSplitOptions]::RemoveEmptyEntries);
    $version = $versionParts[0];

    if ($versionParts.Length -eq 2) {
        $versionSuffix = $versionParts[1];
    }
}

Write-Host -Object "[Pipeline] -- Using version: $version" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- Using versionSuffix: $versionSuffix" -ForegroundColor Green;

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
        dotnet restore
    }
    exec {
        # Build library
        dotnet build src/PSRule.Tool -c $Configuration -f net8.0 -p:version=$Build
        dotnet build src/PSRule.SDK -c $Configuration -f netstandard2.0 -p:version=$Build
        dotnet publish src/PSRule -c $Configuration -f netstandard2.0 -o $(Join-Path -Path $PWD -ChildPath out/modules/PSRule) -p:version=$Build
    }
}

task TestDotNet {
    exec {
        # Test library
        dotnet test
    }
}

task BuildCLI BuildModule, {
    exec {
        # Build library
        dotnet publish src/PSRule.Tool -c $Configuration --self-contained true -o ./out/cli/build/ -p:version=$Build
    }
}

task ReleaseCLI BuildModule, {

    # Build win-x64
    exec {
        dotnet publish src/PSRule.Tool -c $Configuration --self-contained true --runtime win-x64 -o ./out/cli/win-x64/ -p:version=$Build
    }

    # Build linux-x64
    exec {
        dotnet publish src/PSRule.Tool -c $Configuration --self-contained true --runtime linux-x64 -o ./out/cli/linux-x64/ -p:version=$Build
    }

    # Build linux-musl-x64
    exec {
        dotnet publish src/PSRule.Tool -c $Configuration --self-contained true --runtime linux-musl-x64 -o ./out/cli/linux-musl-x64/ -p:version=$Build
    }

    # Build osx-x64
    exec {
        dotnet publish src/PSRule.Tool -c $Configuration --self-contained true --runtime osx-x64 -o ./out/cli/osx-x64/ -p:version=$Build
    }
}

task CopyModule {
    CopyModuleFiles -Path src/PSRule -DestinationPath out/modules/PSRule;

    # Copy LICENSE
    Copy-Item -Path LICENSE -Destination out/modules/PSRule;

    # Copy third party notices
    Copy-Item -Path ThirdPartyNotices.txt -Destination out/modules/PSRule;

    # Copy schemas
    Copy-Item -Path schemas/* -Destination out/modules/PSRule;
}

# Synopsis: Build modules only
task BuildModule BuildDotNet, CopyModule

# Synopsis: Build help
task BuildHelp BuildModule, Dependencies, {
    if (!(Test-Path out/modules/PSRule/en-US/)) {
        $Null = New-Item -Path out/modules/PSRule/en-US/ -ItemType Directory -Force;
    }
    if (!(Test-Path out/modules/PSRule/en-AU/)) {
        $Null = New-Item -Path out/modules/PSRule/en-AU/ -ItemType Directory -Force;
    }
    if (!(Test-Path out/modules/PSRule/en-GB/)) {
        $Null = New-Item -Path out/modules/PSRule/en-GB/ -ItemType Directory -Force;
    }

    # Avoid YamlDotNet issue in same app domain
    exec {
        $pwshPath = (Get-Process -Id $PID).Path;
        &$pwshPath -Command {
            # Generate MAML and about topics
            Import-Module -Name PlatyPS -Verbose:$False;
            $Null = New-ExternalHelp -OutputPath 'out/docs/PSRule' -Path '.\docs\commands\PSRule\en-US','.\docs\keywords\PSRule\en-US', '.\docs\concepts\PSRule\en-US' -Force;
        }
    }

    if (!(Test-Path -Path 'out/docs/PSRule/PSRule-help.xml')) {
        throw 'Failed find generated cmdlet help.';
    }

    # Copy generated help into module out path
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-US;
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-AU;
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-GB;
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
    $modulePath = Join-Path -Path $ArtifactPath -ChildPath 'PSRule';
    $manifestPath = Join-Path -Path $modulePath -ChildPath 'PSRule.psd1';
    Write-Verbose -Message "[VersionModule] -- Checking module path: $modulePath";

    if (![String]::IsNullOrEmpty($Build)) {
        # Update module version
        if (![String]::IsNullOrEmpty($version)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest ModuleVersion";
            Update-ModuleManifest -Path $manifestPath -ModuleVersion $version;
        }

        # Update pre-release version
        if (![String]::IsNullOrEmpty($versionSuffix)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest Prerelease";
            Update-ModuleManifest -Path $manifestPath -Prerelease $versionSuffix;
        }
    }
}

task PackageModule {
    if ($Env:PUBLISH) {
        $modulePath = Join-Path -Path $ArtifactPath -ChildPath 'PSRule';
        $nugetPath = Join-Path -Path $PWD -ChildPath 'out/nuget/';
        if (!(Test-Path -Path $nugetPath)) {
            $Null = New-Item -Path $nugetPath -ItemType Directory -Force;
        }
        Write-Verbose -Message "[PackageModule] -- Checking module path: $modulePath";
        try {
            Register-PSRepository -Name 'OutPath' -SourceLocation $nugetPath -PublishLocation $nugetPath;
            Publish-Module -Path $modulePath -NuGetApiKey na -Repository 'OutPath';
            Out-Host -InputObject "`#`#vso[artifact.upload containerfolder=PSRule.nupkg;artifactname=PSRule.nupkg;]$nugetPath"
        }
        finally {
            Unregister-PSRepository -Name 'OutPath';
        }
    }
}

# Synopsis: Publish to PowerShell Gallery
task ReleaseModule VersionModule, {
    $modulePath = (Join-Path -Path $ArtifactPath -ChildPath 'PSRule');
    Write-Verbose -Message "[ReleaseModule] -- Checking module path: $modulePath";

    if (!(Test-Path -Path $modulePath)) {
        Write-Error -Message "[ReleaseModule] -- Module path does not exist";
    }
    elseif (![String]::IsNullOrEmpty($ApiKey)) {
        Publish-Module -Path $modulePath -NuGetApiKey $ApiKey -Force:$forcePublish;
    }
}

# Synopsis: Install NuGet provider
task NuGet {
    if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
        Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
    }
}

task Dependencies NuGet, {
    Import-Module $PWD/scripts/dependencies.psm1;
    Install-Dependencies -Path $PWD/modules.json -Dev -Repository $PSUpstream;
}

# Synopsis: Test the module
task TestModule Dependencies, {
    # Run Pester tests
    $pesterOptions = @{
        Run = @{
            Path = $PWD;
            PassThru = $True;
        }
        TestResult = @{
            Enabled = $True;
            OutputFormat = 'NUnitXml';
            OutputPath = 'reports/pester-unit.xml';
        };
    }

    # Use Detailed output in CI(AzDO or Github Actions)
    if ($env:TF_BUILD -eq 'true' -or $env:GITHUB_ACTIONS -eq 'true') {
        $pesterOptions.Add('Output', @{ Verbosity = 'Detailed' });
    }

    if ($CodeCoverage) {
        $codeCoverageOptions = @{
            Enabled = $True;
            OutputPath = (Join-Path -Path $PWD -ChildPath reports/pester-coverage.xml);
            Path = (Join-Path -Path $PWD -ChildPath 'out/modules/**/*.psm1');
        };

        $pesterOptions.Add('CodeCoverage', $codeCoverageOptions);
    }

    if (!(Test-Path -Path reports)) {
        $Null = New-Item -Path reports -ItemType Directory -Force;
    }

    if ($Null -ne $TestGroup) {
        $pesterOptions.Add('Filter', @{ Tag = $TestGroup });
    }

    # https://pester.dev/docs/commands/New-PesterConfiguration
    $pesterConfiguration = New-PesterConfiguration -Hashtable $pesterOptions;

    $results = Invoke-Pester -Configuration $pesterConfiguration;

    # Throw an error if pester tests failed
    if ($Null -eq $results) {
        throw 'Failed to get Pester test results.';
    }
    elseif ($results.FailedCount -gt 0) {
        throw "$($results.FailedCount) tests failed.";
    }
}

# Synopsis: Run self-test validation.
task Rules {
    exec {
        dotnet run --project src/PSRule.Tool -f net8.0 -- run --path ./.ps-rule --output NUnit3 --output-path reports/ps-rule-dotnet.xml --option ./ps-rule-ci.yaml;
    }
}

task Benchmark {
    if ($Benchmark -or $BuildTask -eq 'Benchmark') {
        dotnet run --project src/PSRule.Benchmark -f net8.0 -c Release -- benchmark --output $PWD;
    }
}

# Synopsis: Run script analyzer
task Analyze Build, Dependencies, {
    Invoke-ScriptAnalyzer -Path out/modules/PSRule;
}

# Synopsis: Build and test.
task . Build, Rules, TestDotNet, Benchmark

# Synopsis: Build the project
task Build Clean, BuildModule, BuildHelp, VersionModule, PackageModule

task Test Build, Rules, TestDotNet, TestModule

task Release ReleaseModule

task AnalyzeRepository Build, Rules
