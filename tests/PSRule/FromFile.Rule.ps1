
# Description: Test rule 1
Rule 'FromFile1' -Tag @{ category = "group1"; test = "Test1" } {

    Hint -TargetName 'TestTarget1'

    # Successful
    $True;
}

# Description: Test rule 2
Rule 'FromFile2' -Tag @{ category = "group1"; test = "Test2" } {

    Hint -TargetName 'TestTarget2'

    # Failed
    $False;
}

# Description: Test rule 3
Rule 'FromFile3' -Tag @{ category = "group1" } {

    Hint -TargetName 'TestTarget3'

    # Inconclusive
}

Rule 'ExistsTest' {

    Exists 'Name'
}
