#
# Pester unit test rules checking nesting
#

# Synopsis: Should error with nested rule
Rule 'WithNestedRule' {
    Rule 'NestedRule' {
        $True;
    }
}
