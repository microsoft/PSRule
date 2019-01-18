
# Description: Has environment tag
Rule 'environmentTag' {
    Exists 'Tags.environment' -CaseSensitive
    Within 'Tags.environment' 'production', 'test', 'development' -CaseSensitive
}

# Description: Has costCentre tag
Rule 'costCentreTag' {
    Exists 'Tags.costCentre' -CaseSensitive
}
