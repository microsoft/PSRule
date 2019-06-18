#
# Pester unit test rules for error handling
#

# Synopsis: Should fail
Rule 'WithNonBoolean' {
    $True
    'false' # Not a boolean
}
