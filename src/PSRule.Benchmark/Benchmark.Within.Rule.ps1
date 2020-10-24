# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# A set of benchmark rules for testing PSRule performance
#

# Description: A rule for testing PSRule performance
Rule 'BenchmarkWithin' {
    Within 'Value' 'Microsoft.Compute/virtualMachines', 'Microsoft.Sql/servers/databases'
}

Rule 'BenchmarkWithinBulk' {
    Within 'Value' @(
        # Compute: Virtual machines
        'Microsoft.Compute/virtualMachines'
        'Microsoft.Compute/virtualMachineScaleSets'
        'Microsoft.Compute/virtualMachineScaleSets/virtualmachines'
        'Microsoft.Compute/availabilitySets'
        'Microsoft.Compute/proximityPlacementGroups'
        'Microsoft.Compute/images'
        'Microsoft.Compute/disks'
        'Microsoft.Compute/snapshots'
        'Microsoft.Network/networkInterfaces'
        'Microsoft.Compute/virtualMachines/extensions'
        'Microsoft.Compute/virtualMachineScaleSets/extensions'

        # Compute: Batch
        'Microsoft.Batch/batchAccounts'
        'Microsoft.Batch/batchAccounts/applications'
        'Microsoft.Batch/batchAccounts/applications/versions'
        'Microsoft.Batch/batchAccounts/certificates'
        'Microsoft.Batch/batchAccounts/pools'

        # Compute: Service Fabric
        'Microsoft.ServiceFabric/clusters'
        'Microsoft.ServiceFabric/clusters/applications'
        'Microsoft.ServiceFabric/clusters/applications/services'
        'Microsoft.ServiceFabric/clusters/applicationTypes'
        'Microsoft.ServiceFabric/clusters/applicationTypes/versions'

        # Networking: Virtual network
        'Microsoft.Network/virtualNetworks'
        'Microsoft.Network/virtualNetworks/subnets'
        'Microsoft.Network/virtualNetworks/virtualNetworkPeerings'
        'Microsoft.Network/routeTables'
        'Microsoft.Network/routeTables/routes'
        'Microsoft.Network/networkSecurityGroups'
        'Microsoft.Network/networkSecurityGroups/securityRules'
        'Microsoft.Network/applicationSecurityGroups'
        'Microsoft.Network/connections'

        # Networking: ExpressRoute
        'Microsoft.Network/expressRouteCircuits'
        'Microsoft.Network/expressRouteCircuits/authorizations'
        'Microsoft.Network/expressRouteCircuits/peerings'
        'Microsoft.Network/expressRouteCircuits/peerings/connections'
        'Microsoft.Network/expressRouteGateways'
        'Microsoft.Network/expressRouteGateways/expressRouteConnections'
        'Microsoft.Network/routeFilters/routeFilterRules'

        # Networking: VPN gateway
        'Microsoft.Network/localNetworkGateways'

        # Storage: Storage Accounts
        'Microsoft.Storage/storageAccounts'
        'Microsoft.Storage/storageAccounts/blobServices'
        'Microsoft.Storage/storageAccounts/blobServices/containers'
        'Microsoft.Storage/storageAccounts/blobServices/containers/immutabilityPolicies'
        'Microsoft.Storage/storageAccounts/managementPolicies'

        # Storage: Azure Backup
        'Microsoft.RecoveryServices/vaults'
        'Microsoft.RecoveryServices/vaults/backupFabrics/backupProtectionIntent'
        'Microsoft.RecoveryServices/vaults/backupPolicies'
        'Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers'
        'Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers/protectedItems'
        'Microsoft.RecoveryServices/vaults/certificates'

        # Storage: Site Recovery
        'Microsoft.RecoveryServices/vaults/replicationAlertSettings'
        'Microsoft.RecoveryServices/vaults/replicationFabrics'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationNetworks/replicationNetworkMappings'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationProtectionContainers'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationProtectionContainers/replicationMigrationItems'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationProtectionContainers/replicationProtectedItems'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationProtectionContainers/replicationProtectionContainerMappings'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationRecoveryServicesProviders'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationStorageClassifications/replicationStorageClassificationMappings'
        'Microsoft.RecoveryServices/vaults/replicationFabrics/replicationvCenters'
        'Microsoft.RecoveryServices/vaults/replicationPolicies'
        'Microsoft.RecoveryServices/vaults/replicationRecoveryPlans'
        'Microsoft.RecoveryServices/vaults/replicationVaultSettings'

        # Databases: SQL Database
        'Microsoft.Sql/servers'
        'Microsoft.Sql/servers/administrators'
        'Microsoft.Sql/servers/auditingSettings'
        'Microsoft.Sql/servers/backupLongTermRetentionVaults'
        'Microsoft.Sql/servers/communicationLinks'
        'Microsoft.Sql/servers/connectionPolicies'
        'Microsoft.Sql/servers/databases'
        'Microsoft.Sql/servers/databases/auditingSettings'
        'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies'
        'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies'
        'Microsoft.Sql/servers/databases/connectionPolicies'
        'Microsoft.Sql/servers/databases/dataMaskingPolicies'
    )
}

Rule 'BenchmarkWithinLike' {
    Within 'Value' -Like @(
        # Compute: Virtual machines
        'Microsoft.Compute/virtualMachines'
        'Microsoft.Compute/virtualMachineScaleSets'
        'Microsoft.Compute/virtualMachineScaleSets/virtualmachines'
        'Microsoft.Compute/availabilitySets'
        'Microsoft.Compute/proximityPlacementGroups'
        'Microsoft.Compute/images'
        'Microsoft.Compute/disks'
        'Microsoft.Compute/snapshots'
        'Microsoft.Network/networkInterfaces'
        'Microsoft.Compute/virtualMachines/extensions'
        'Microsoft.Compute/virtualMachineScaleSets/extensions'

        # Compute: Batch
        'Microsoft.Batch/batchAccounts'
        'Microsoft.Batch/batchAccounts/*'

        # Compute: Service Fabric
        'Microsoft.ServiceFabric/clusters'
        'Microsoft.ServiceFabric/clusters/*'

        # Networking: Virtual network
        'Microsoft.Network/virtualNetworks'
        'Microsoft.Network/virtualNetworks/*'
        'Microsoft.Network/routeTables'
        'Microsoft.Network/routeTables/routes'
        'Microsoft.Network/networkSecurityGroups'
        'Microsoft.Network/networkSecurityGroups/securityRules'
        'Microsoft.Network/applicationSecurityGroups'
        'Microsoft.Network/connections'

        # Networking: ExpressRoute
        'Microsoft.Network/expressRouteCircuits'
        'Microsoft.Network/expressRouteCircuits/*'
        'Microsoft.Network/expressRouteGateways'
        'Microsoft.Network/expressRouteGateways/*'
        'Microsoft.Network/routeFilters/routeFilterRules'

        # Networking: VPN gateway
        'Microsoft.Network/localNetworkGateways'

        # Storage: Storage Accounts
        'Microsoft.Storage/storageAccounts'
        'Microsoft.Storage/storageAccounts/*'

        # Storage: Azure Backup
        'Microsoft.RecoveryServices/vaults'
        'Microsoft.RecoveryServices/vaults/*'

        # Databases: SQL Database
        'Microsoft.Sql/servers'
        'Microsoft.Sql/servers/*'
    )
}
