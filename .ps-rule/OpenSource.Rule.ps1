# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Check for recommended community files
Rule 'OpenSource.Community' -Type 'System.IO.DirectoryInfo', 'PSRule.Data.RepositoryInfo' {
    $Assert.FilePath($TargetObject, 'FullName', @('CHANGELOG.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('LICENSE', 'LICENSE.txt'));
    $Assert.FilePath($TargetObject, 'FullName', @('CODE_OF_CONDUCT.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('CONTRIBUTING.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('SECURITY.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('README.md'));
    $Assert.FilePath($TargetObject, 'FullName', @('.github/CODEOWNERS'));
    $Assert.FilePath($TargetObject, 'FullName', @('.github/PULL_REQUEST_TEMPLATE.md'));
}

# Synopsis: Check for license in code files
Rule 'OpenSource.License' -Type '.cs', '.ps1', '.psd1', '.psm1' {
    $Assert.FileHeader($TargetObject, 'FullName', @(
        'Copyright (c) Microsoft Corporation.'
        'Licensed under the MIT License.'
    ));
}
