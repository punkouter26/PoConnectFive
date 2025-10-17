@description('Primary location for all resources')
param location string

@description('Name of the environment')
param environmentName string

@description('Name to use for all resources in this resource group (should match the .sln and resource group name)')
param resourceName string

@description('Existing shared resource group name that contains an App Service Plan to use')
param sharedResourceGroupName string

@description('Name of the existing shared app service plan in PoShared resource group')
param sharedAppServicePlanName string

// Reference existing shared App Service Plan
resource sharedAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: sharedAppServicePlanName
  scope: resourceGroup(sharedResourceGroupName)
}

// Create a Log Analytics workspace in the same resource group
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: resourceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      legacy: 0
      searchVersion: 1
    }
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Create Application Insights linked to the Log Analytics workspace
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    IngestionMode: 'LogAnalytics'
    WorkspaceResourceId: logAnalytics.id
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Create a Storage Account only for Azure (use lowercase due to naming constraints)
var storageAccountName = toLower(resourceName)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Create App Service using existing shared App Service Plan
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: resourceName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: sharedAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v4.0'  // Keep v4.0 for the framework version
      use32BitWorkerProcess: true  // Required for F1 tier
      alwaysOn: false  // AlwaysOn not available on F1 tier
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnetcore'
        }
      ]
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'AzureTableStorage__ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
  tags: {
    'azd-env-name': environmentName
    'azd-service-name': 'web'
  }
}

// Note: Site Extension removed to save disk space on F1 tier
// Application Insights is already configured via NuGet package

// Outputs
output WEB_APP_NAME string = appService.name
output WEB_APP_URI string = 'https://${appService.properties.defaultHostName}'
output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsights.properties.ConnectionString
output STORAGE_CONNECTION_STRING string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
