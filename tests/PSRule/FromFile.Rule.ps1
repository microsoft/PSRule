
# Description: Test rule 1
Rule 'FromFile1' -Tag @{ category = "group1"; test = "Test1" } {
    # Pass
    $True;
    $True;
}

# Description: Test rule 2
Rule 'FromFile2' -Tag @{ category = "group1"; test = "Test2" } {
    # Fail
    $False;
    $True;
    $True;
}

# Description: Test rule 3
Rule 'FromFile3' -Tag @{ category = "group1" } {
    # Inconclusive
}

# Description: Test rule 4
Rule 'FromFile4' -Tag @{ category = "group1" } -DependsOn 'FromFile3' {
    # Inconclusive
}

# Description: Test for tags
Rule 'WithTag' -Tag @{ severity = 'critical'; feature = 'tag' } {
    $True;
}

# Description: Test for tags
Rule 'WithTag2' -Tag @{ feature = 'tag' } {
    $True;
}

# Description: Test for tags
Rule 'WithTag3' -Tag @{ severity = 'information'; feature = 'tag' } {
    $True;
}

# Description: Test for tags
Rule 'WithTag4' -Tag @{ Severity = 'critical'; feature = 'tag' } {
    $True;
}

# Description: Test for tags
Rule 'WithTag5' -Tag @{ severity = 'Critical'; feature = 'tag' } {
    $True;
}

Rule 'WithPreconditionTrue' -If { $True } -Tag @{ category = 'precondition' } {
    $True;
}

Rule 'WithPreconditionFalse' -If { $False } -Tag @{ category = 'precondition' } {
    $True;
}

# Description: Should fail, because of dependency fail
Rule 'WithDependency1' -DependsOn 'WithDependency3','WithDependency2' {
    # Pass
    $True;
}

# Description: Should fail, because of dependency fail
Rule 'WithDependency2' -DependsOn 'WithDependency5' {
    # Pass
    $True;
}

# Description: Should pass, with a passing dependency
Rule 'WithDependency3' -DependsOn 'WithDependency4' {
    # Pass
    $True
}

# Description: Pass
Rule 'WithDependency4' {
    # Pass
    $True
}

# Description: Fail
Rule 'WithDependency5' {
    # Fail
    $False
}

# Description: Test for constrained language
Rule 'ConstrainedTest1' {
    $True;
}

# Description: Test for constrained language, should not execute
Rule 'ConstrainedTest2' {
    $Null = [Console]::WriteLine('Should fail');
    $True;
}

# Description: Test for constrained language, should not execute
Rule 'ConstrainedTest3' -If { $Null = [Console]::WriteLine('Should fail'); return $True; } {
    $True;
}

# Description: Test automatic variables
Rule 'VariableTest' {
    $TargetObject.Name -eq $Rule.RuleName;
    $TargetObject.Name -eq $Rule.TargetName;
}

# Description: Test for Hint keyword
Rule 'HintTest' {
    Hint 'This is a message' -TargetName 'HintTarget'
}

# Description: Test for Exists keyword
Rule 'ExistsTest' -Tag @{ keyword = 'Exists' } {
    Exists 'Name'
    Exists -Not 'NotName'
}

# Description: Test for Exists keyword
Rule 'ExistsTestNegative' -Tag @{ keyword = 'Exists' } {
    Exists 'NotName'
    Exists 'name' -CaseSensitive
}

Rule 'WithinTest' -Tag @{ keyword = 'Within' } {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
}

Rule 'WithinTestCaseSensitive' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms' -CaseSensitive
}

Rule 'MatchTest' -Tag @{ keyword = 'Match' } {
    Match 'PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$', '^(0{1,3})$'
}

Rule 'MatchTestCaseSensitive' -Tag @{ keyword = 'Match' } {
    Match 'Title' '^(Mr|Miss|Mrs|Ms)$' -CaseSensitive
}

Rule 'TypeOfTest' {
    TypeOf 'System.Collections.Hashtable', 'PSRule.Test.OtherType'
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
