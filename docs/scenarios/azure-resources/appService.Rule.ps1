#
# Validation rules for Azure App Services
#

# Description: App Service Plan has multiple instances
Rule 'appServicePlan.MinInstanceCount' -If { ResourceType 'Microsoft.Web/serverfarms' } {
    Hint 'Use at least two (2) instances'

    $TargetObject.Sku.capacity -ge 2
}

# Description: Use at least a Standard App Service Plan
Rule 'appServicePlan.MinPlan' -If { ResourceType 'Microsoft.Web/serverfarms' } {
    Hint 'Use a Standard or high plans for production services'

    ($TargetObject.Sku.tier -eq 'PremiumV2') -or
    ($TargetObject.Sku.tier -eq 'Premium') -or
    ($TargetObject.Sku.tier -eq 'Standard')
}

# Description: Disable client affinity for stateless services
Rule 'appServiceApp.ARRAfinity' -If { ResourceType 'Microsoft.Web/sites' } {
    Hint 'Disable ARR affinity when not required'

    $TargetObject.Properties.clientAffinityEnabled -eq $False
}

# Description: Use HTTPS only
Rule 'appServiceApp.UseHTTPS' -If { ResourceType 'Microsoft.Web/sites' } {
    Hint 'Disable HTTP when not required'

    $TargetObject.Properties.httpsOnly -eq $True
}
