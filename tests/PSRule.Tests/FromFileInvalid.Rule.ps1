#  Copyright (c) Microsoft Corporation.
#  Licensed under the MIT License.

#
# Pester unit test rules
#

# Synopsis: Null DependsOn is invalid.
Rule 'InvalidRule1' -DependsOn $Null {

}

# Synopsis: Empty DependsOn collection is invalid.
Rule 'InvalidRule2' -DependsOn @($Null) {

}
