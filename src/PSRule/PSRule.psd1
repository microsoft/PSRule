# Copyright (c) Bernie White. All rights reserved.

#
# PSRule module
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'PSRule.psm1'

# Version number of this module.
ModuleVersion = '0.0.1'

# Supported PSEditions
CompatiblePSEditions = 'Core', 'Desktop'

# ID used to uniquely identify this module
GUID = '0130215d-58eb-4887-b6fa-31ed02500569'

# Author of this module
Author = 'Bernie White'

# Company or vendor of this module
CompanyName = 'Bernie White'

# Copyright statement for this module
Copyright = '(c) Bernie White. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Validate objects using PowerShell rules.

This project is to be considered a proof-of-concept and not a supported product.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
DotNetFrameworkVersion = '4.7.2'

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
RequiredAssemblies = @(
    'PSRule.dll'
)

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @('PSRule.Format.ps1xml')

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @(
    'Rule'
    'Invoke-PSRule'
    'Test-PSRuleTarget'
    'Get-PSRule'
    'Get-PSRuleHelp'
    'New-PSRuleOption'
    'Set-PSRuleOption'
    'AllOf'
    'AnyOf'
    'Exists'
    'Match'
    'TypeOf'
    'Within'
    'Recommend'
)

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @()

# Variables to export from this module
VariablesToExport = @(
    'Configuration'
    'LocalizedData'
    'Rule'
    'TargetObject'
)

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @(
    'Hint'
)

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{
    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('Rule', 'Test')

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/BernieWhite/PSRule/blob/master/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/BernieWhite/PSRule'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = 'https://github.com/BernieWhite/PSRule/blob/master/CHANGELOG.md'

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
