# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Pester unit test rules
#

# Logging in main script
Write-Debug -Message 'Script debug message';
Write-Information -MessageData 'Script information message';
Write-Verbose -Message 'Script verbose message';
Write-Warning -Message 'Script warning message';

Rule 'WithError' {
    Write-Error -Message 'Rule error message';
    $True;
}

Rule 'WithWarning' {
    Write-Warning -Message 'Rule warning message';
    $True;
}

Rule 'WithVerbose' {
    Write-Verbose -Message 'Rule verbose message';
    $True;
}

Rule 'WithVerbose2' {
    Write-Verbose -Message 'Rule verbose message 2';
    $True;
}

Rule 'WithInformation' {
    Write-Information -MessageData 'Rule information message';
    $True;
}

Rule 'WithDebug' {
    Write-Debug -Message 'Rule debug message';
    $True;
}

Rule 'WithDebug2' {
    Write-Debug -Message 'Rule debug message 2';
    $True;
}
