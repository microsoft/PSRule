#
# Unit tests for core PSRule functionality
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;

$here = (Resolve-Path $PSScriptRoot).Path;
$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Common;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSRule keywords' {

    Context 'Exists' {

        $exampleObject = [PSObject]@{ Property1 = 'test'; }
        
        Rule 'ShouldExist' {

            Exists 'Property1'
        }

        Rule 'ShouldNotExist' {

            Exists 'Property2'
        }

        $result = $exampleObject | Invoke-RuleEngine -Verbose;

        It 'Success when field exists' {
            $result | Should
        }

        It 'Success when field does not exist' {

        }
    }

    Context 'Match' {

    }

    Context 'AnyOf' {

    }

    Context 'AllOf' {

    }

    Context 'Within' {

    }
}

Describe 'Get-PSRule' {


    Context 'Get rule list' {

        $result = Get-PSRule -Path $here -Verbose;

        It 'Returns rules' {
            $result | Should -Not -BeNullOrEmpty;
            $result.Count | Should -BeGreaterThan 0;
        }
    }

    Context 'Get rule with invalid path' {

        $result = Get-PSRule -Path (Join-Path -Path $here -ChildPath invalid);
    }
}

