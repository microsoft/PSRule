#
# Validation rules for Azure resource tagging standard
#

if ($Null -eq $Rule.Configuration.allowedBusinessUnits) {
    Write-Warning -Message 'The allowedBusinessUnits option is not configured';
}

# Description: Resource must have environment tag
Rule 'environmentTag' {
    Exists 'Tags.environment' -CaseSensitive
    Within 'Tags.environment' 'production', 'test', 'development' -CaseSensitive
}

# Description: Resource must have costCentre tag
Rule 'costCentreTag' {
    Exists 'Tags.costCentre' -CaseSensitive
    Match 'Tags.costCentre' '^([1-9][0-9]{4})$'
}

# Description: Resource must have businessUnit tag
Rule 'businessUnitTag' {
    Exists 'Tags.businessUnit' -CaseSensitive
    Within 'Tags.businessUnit' $Rule.Configuration.allowedBusinessUnits
}
