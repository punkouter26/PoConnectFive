@description('Name of the environment')
param environmentName string

@description('Primary location for resources')
param location string = resourceGroup().location

@description('Name of the shared resource group containing shared services')
param sharedResourceGroupName string = 'PoShared'

@description('Tags for all resources')
param tags object = {}

// Reference existing shared resources from PoShared
resource sharedContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: 'acrposhared'
  scope: resourceGroup(sharedResourceGroupName)
}

resource sharedContainerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: 'cae-poshared'
  scope: resourceGroup(sharedResourceGroupName)
}

resource sharedAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'appi-poshared'
  scope: resourceGroup(sharedResourceGroupName)
}

resource sharedKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: 'kv-poshared'
  scope: resourceGroup(sharedResourceGroupName)
}

// Create User Assigned Managed Identity for this app
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-poconnectfive'
  location: location
  tags: tags
}

// Create Table Storage account for this app (app-specific data)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'stpoconnectfive'
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

// Enable Table service
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// Create tables for the game
resource playersTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: tableService
  name: 'PoConnectFivePlayers'
}

resource gamesTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: tableService
  name: 'PoConnectFiveGames'
}

// Note: Role assignment removed - use storage connection string with key instead of managed identity for storage access

// Container App for the web application
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-poconnectfive-web'
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: sharedContainerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: [
        {
          server: sharedContainerRegistry.properties.loginServer
          identity: managedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: 'mcr.microsoft.com/dotnet/aspnet:10.0'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: sharedAppInsights.properties.ConnectionString
            }
            {
              name: 'ConnectionStrings__tableStorage'
              value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentity.properties.clientId
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

// Outputs
output containerRegistryEndpoint string = sharedContainerRegistry.properties.loginServer
output containerAppsEnvironmentId string = sharedContainerAppsEnvironment.id
output webUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output storageAccountName string = storageAccount.name
output managedIdentityId string = managedIdentity.id
output managedIdentityClientId string = managedIdentity.properties.clientId
