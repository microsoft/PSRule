#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

$Null = [Console]::WriteLine('Should fail');

# Synopsis: A rule used for constrained language testing
Rule 'UnconstrainedFile1' {
    $True;
}
