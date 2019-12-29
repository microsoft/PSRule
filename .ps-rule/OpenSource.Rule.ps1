# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Check for recommended community files
Rule 'OpenSource.Community' -Type 'System.IO.DirectoryInfo' {
    $requiredFiles = @(
        'CHANGELOG.md'
        'LICENSE.txt'
        'CODE_OF_CONDUCT.md'
        'CONTRIBUTING.md'
        'SECURITY.md'
        'README.md'
        '.github/CODEOWNERS'
        '.github/PULL_REQUEST_TEMPLATE.md'
    )
    Test-Path -Path $TargetObject.FullName;
    for ($i = 0; $i -lt $requiredFiles.Length; $i++) {
        $filePath = Join-Path -Path $TargetObject.FullName -ChildPath $requiredFiles[$i];
        $Assert.Create((Test-Path -Path $filePath -PathType Leaf), "$($requiredFiles[$i]) does not exist");
    }
}

# Synopsis: Check for license in code files
Rule 'OpenSource.License' -Type 'System.IO.FileInfo' -If { $TargetObject.Extension -in '.cs', '.ps1', '.psd1', '.psm1' } {
    $commentPrefix = "`# ";
    if ($TargetObject.Extension -eq '.cs') {
        $commentPrefix = '// '
    }
    $header = GetLicenseHeader -CommentPrefix $commentPrefix;
    $content = Get-Content -Path $TargetObject.FullName -Raw;
    $content.StartsWith($header);
}

function global:GetLicenseHeader {
    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$CommentPrefix
    )
    process {
        $text = @(
            'Copyright (c) Microsoft Corporation.'
            'Licensed under the MIT License.'
        )
        $builder = [System.Text.StringBuilder]::new();
        foreach ($line in $text) {
            $Null = $builder.Append($CommentPrefix);
            $Null = $builder.Append($line);
            $Null = $builder.Append([System.Environment]::NewLine);
        }
        return $builder.ToString();
    }
}
