# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Pester unit test rules for error handling
#

# Synopsis: Should pass
Rule 'WithPass' {
    $Assert.Pass();
}

# Synopsis: Should fail
Rule 'WithFail' {
    $Assert.Fail('This is a fail');
}

# Synopsis: Should fail
Rule 'WithNonBoolean' {
    $True
    'false' # Not a boolean
}

Rule 'WithDependency1' -DependsOn 'WithDependency2' {
    $True;
}

Rule 'WithDependency2' -DependsOn 'WithDependency3' {
    $True;
}

Rule 'WithDependency3' -DependsOn 'WithDependency1' {
    $True;
}

Rule 'WithDependency4' -DependsOn 'WithDependency5' {
    $True;
}

# Synopsis: Should pass because exception is caught.
Rule 'WithTryCatch' {
    try {
        $Null = Get-Command PSRule_NotCommand -ErrorAction Stop;
        $True;
    }
    catch {
        $False;
    }
}

# Synopsis: With parsing error.
Rule 'WithParameterNotFound' {
    $item = Get-Item -MisspelledParameter MyItem;
    $True;
}

# Synopsis: With throw.
Rule 'WithThrow' {
    throw 'Some error'
}

# Synopsis: Should pass because error is suppressed.
Rule 'WithCmdletErrorActionIgnore' {
    $result = Get-Command PSRule_NotCommand -ErrorAction Ignore;
    $Null -ne $result;
}

# Synopsis: Rule using the default stop action.
Rule 'WithRuleErrorActionDefault' {
    Write-Error 'Some error 1';
    Write-Error 'Some error 2';
    $True;
}

# Synopsis: Rule ignoring errors.
Rule 'WithRuleErrorActionIgnore' -ErrorAction Ignore {
    Write-Error 'Some error';
    $True;
}

# Synopsis: Rule to generate a PowerShell parsing exception
Rule 'WithParseError' {
    $Null = Get-Item -MisspelledParameter MyItem
    $True;
}
