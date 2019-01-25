#
# Pester unit test rules
#

# Logging in main script
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

Rule 'WithInformation' {
    Write-Information -MessageData 'Rule information message';
    $True;
}
