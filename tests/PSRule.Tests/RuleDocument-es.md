---
reviewed: 2022-09-22
severity: Critico
pillar: Seguridad
category: Autenticación
resource: Container Registry
online version: https://azure.github.io/PSRule.Rules.Azure/es/rules/Azure.ACR.AdminUser/
---

# Deshabilitar el usuario adminstrador para ACR

## Sinopsis

Usar identidades de Azure AD en lugar de usar el usuario administrador del registro.

## Descripción

Azure Container Registry (ACR) incluye una cuenta de usuario administrador incorporada.
La cuenta de usuario administrador es una cuenta de usuario única con acceso administrativo al registro.
Esta cuenta proporciona acceso de usuario único para pruebas y desarrollo tempranos.
La cuenta de usuario administrador no está diseñada para usarse con registros de contenedores de producción.

En su lugar, utilice el control de acceso basado en roles (RBAC).
RBAC se puede usar para delegar permisos de registro a una identidad de Azure AD (AAD).

## Recomendación

Considere deshabilitar la cuenta de usuario administrador y solo use la autenticación basada en identidad para las operaciones de registro.

## Ejemplos

### Configurar con plantilla de ARM

Para implementar Container Registries, pasa la siguiente regla:

- Establezca `properties.adminUserEnabled` a `false`.

Por ejemplo:

```json
{
  "type": "Microsoft.ContainerRegistry/registries",
  "apiVersion": "2021-06-01-preview",
  "name": "[parameters('registryName')]",
  "location": "[parameters('location')]",
  "sku": {
    "name": "Premium"
  },
  "identity": {
    "type": "SystemAssigned"
  },
  "properties": {
    "adminUserEnabled": false,
    "policies": {
      "quarantinePolicy": {
        "status": "enabled"
      },
      "trustPolicy": {
        "status": "enabled",
        "type": "Notary"
      },
      "retentionPolicy": {
        "status": "enabled",
        "days": 30
      }
    }
  }
}
```

### Configurar con Bicep

Para implementar Container Registries, pasa la siguiente regla:

- Establezca `properties.adminUserEnabled` a `false`.

Por ejemplo:

```bicep
resource acr 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: registryName
  location: location
  sku: {
    name: 'Premium'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    adminUserEnabled: false
    policies: {
      quarantinePolicy: {
        status: 'enabled'
      }
      trustPolicy: {
        status: 'enabled'
        type: 'Notary'
      }
      retentionPolicy: {
        status: 'enabled'
        days: 30
      }
    }
  }
}
```

### Configurar con Azure CLI

```bash
az acr update --admin-enabled false -n '<name>' -g '<resource_group>'
```

### Configurar con Azure PowerShell

```powershell
Update-AzContainerRegistry -ResourceGroupName '<resource_group>' -Name '<name>' -DisableAdminUser
```

## Enlaces

- [Uso de la autenticación basada en identidad](https://docs.microsoft.com/azure/architecture/framework/security/design-identity-authentication#use-identity-based-authentication)
- [Autenticación con un registro de contenedor de Azure](https://docs.microsoft.com/azure/container-registry/container-registry-authentication?tabs=azure-cli)
