# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Pester unit test rules checking nesting
#

# Synopsis: Should error with nested rule
Rule 'WithNestedRule' {
    Rule 'NestedRule' {
        $True;
    }
}

# Synopsis: Must have name
Rule '' {
    $True;
}

# Synopsis: Must have name
Rule {
    $True;
}

# Synopsis: Must have body
Rule -Name 'WithoutBody'

# Synopsis: Success
Rule 'WithNameBody' {
    $True;
}

# Synopsis: Rule with invalid ErrorAction.
Rule 'WithRuleErrorActionContinue' -ErrorAction Continue {
    $True
}
