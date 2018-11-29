
# Description: Test rule 1
Rule 'FromFile1' -Tag @{ category = "group1"; test = "Test1" } {

    Hint -TargetName 'TestTarget1'

    # Successful
    $True;
    $True;
}

# Description: Test rule 2
Rule 'FromFile2' -Tag @{ category = "group1"; test = "Test2" } {

    Hint -TargetName 'TestTarget2'

    # Failed
    $False;
    $True;
    $True;
}

# Description: Test rule 3
Rule 'FromFile3' -Tag @{ category = "group1" } {

    Hint -TargetName 'TestTarget3'

    # Inconclusive
}

Rule 'WithPreconditionTrue' -If { $True } -Tag @{ category = 'precondition' } {
    $True;
}

Rule 'WithPreconditionFalse' -If { $False } -Tag @{ category = 'precondition' } {
    $True;
}

Rule 'ConstrainedTest1' {
    $True;
}

Rule 'ConstrainedTest2' {
    $Null = [Console]::WriteLine('Should fail');
    $True;
}

Rule 'ExistsTest' {

    Exists 'Name'

    # Exists 'Value.Value1'
}

Rule 'WithinTest' {

    Within 'Title' {
        'Mr'
        'Mrs'
    }
}

Rule 'MatchTest' {

    Match 'PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$'
}

Rule 'TypeOfTest' {

    TypeOf 'System.Collections.Hashtable'
}

Rule 'AllOfTest' {

    AllOf {
        $True
        $True
    }
}

Rule 'AllOfTestNegative' {

    AllOf {
        $True
        $False
    }
}

Rule 'AnyOfTest' {

    AnyOf {
        $True
        $False
        $False
    }
}

Rule 'AnyOfTestNegative' {

    AnyOf {
        $False
        $False
        $False
    }
}

