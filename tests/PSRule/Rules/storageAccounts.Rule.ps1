#
# Validation rules for Azure Storage Accounts
#

# Description: Configure storage accounts to only access encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' -If { $TargetObject.ResourceType -eq 'Microsoft.Storage/storageAccounts' } {

    Hint 'Secure access should only allow secure traffic' -TargetName $TargetObject.ResourceName

    $TargetObject.Properties.supportsHttpsTrafficOnly
}

# Description: Use at-rest storage encryption
Rule 'storageAccounts.UseEncryption' -If { $TargetObject.ResourceType -eq 'Microsoft.Storage/storageAccounts' } {

    Hint 'Storage accounts should have encryption enabled' -TargetName $TargetObject.ResourceName

    ($Null -ne $TargetObject.Properties.encryption) -and
    ($Null -ne $TargetObject.Properties.encryption.services.blob) -and
    ($Null -ne $TargetObject.Properties.encryption.services.file) -and
    ($TargetObject.Properties.encryption.services.blob.enabled -and $TargetObject.Properties.encryption.services.file.enabled)
}
