@description('The name of the application')
param appName string = 'poconnectfive'

@description('Location for all resources')
param location string = resourceGroup().location

@description('The name of the shared resource group containing shared resources')
param sharedResourceGroupName string = 'PoShared'

// Reference to existing shared resources
resource sharedResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: sharedResourceGroupName
  scope: subscription()
}

resource existingAppServicePlan 'Microsoft.Web/serverfarms@2022-09-01' existing = {
  name: 'free-app-service-plan'
  scope: sharedResourceGroup
}

resource existingApplicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'shared-application-insights'
  scope: sharedResourceGroup
}

// Storage Account for Azure Table Storage
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${appName}storage${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: existingApplicationInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
        {
          name: 'ConnectionStrings__StorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
      ]
    }
  }
}

// Outputs
output appServiceName string = appService.name
output storageAccountName string = storageAccount.name
output applicationInsightsConnectionString string = existingApplicationInsights.properties.ConnectionString
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
