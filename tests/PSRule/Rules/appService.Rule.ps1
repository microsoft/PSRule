#
# Validation rules for Azure App Services
#

# Description: App Service Plan has multiple instances
Rule 'appServicePlan.MinInstanceCount' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/serverfarms' } {

    Hint 'Use at least two (2) instances' -TargetName $TargetObject.ResourceName

    $TargetObject.Sku.capacity -ge 2
}

# Description: Use at least a Standard App Service Plan
Rule 'appServicePlan.MinPlan' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/serverfarms' } {

    Hint 'Use a Standard or high plans for production services' -TargetName $TargetObject.ResourceName

    ($TargetObject.Sku.tier -eq 'PremiumV2') -or
    ($TargetObject.Sku.tier -eq 'Premium') -or
    ($TargetObject.Sku.tier -eq 'Standard')
}

# Description: Disable client affinity for stateless services
Rule 'appServiceApp.ARRAfinity' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/sites' } {

    Hint 'Disable ARR affinity when not required' -TargetName $TargetObject.ResourceName

    $TargetObject.Properties.clientAffinityEnabled -eq $False
}

# Description: Use HTTPS only
Rule 'appServiceApp.UseHTTPS' -If { $TargetObject.ResourceType -eq 'Microsoft.Web/sites' } {

    Hint 'Disable HTTP when not required' -TargetName $TargetObject.ResourceName

    $TargetObject.Properties.httpsOnly -eq $True
}
