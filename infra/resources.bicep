@description('Primary location for all resources')
param location string

@description('Name of the environment')
param environmentName string

@description('Resource token for unique naming')
param resourceToken string

@description('Resource prefix for naming')
param resourcePrefix string

@description('Existing shared resource group name')
param sharedResourceGroupName string

@description('Name of the existing shared app service plan')
param sharedAppServicePlanName string

@description('Name of the existing shared application insights')
param sharedApplicationInsightsName string

@description('Name of the existing shared storage account')
param sharedStorageAccountName string

@description('Name of the existing shared OpenAI service')
param sharedOpenAIName string

// Reference existing shared resources
resource sharedAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: sharedAppServicePlanName
  scope: resourceGroup(sharedResourceGroupName)
}

resource sharedApplicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: sharedApplicationInsightsName
  scope: resourceGroup(sharedResourceGroupName)
}

resource sharedStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: sharedStorageAccountName
  scope: resourceGroup(sharedResourceGroupName)
}

resource sharedOpenAI 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: sharedOpenAIName
  scope: resourceGroup(sharedResourceGroupName)
}

// Create Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'az-${resourcePrefix}-log-${resourceToken}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Create Key Vault for storing secrets
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'az-${resourcePrefix}-kv-${resourceToken}'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Create User Assigned Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'az-${resourcePrefix}-identity-${resourceToken}'
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// Create App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: 'az-${resourcePrefix}-web-${resourceToken}'
  location: location
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: sharedAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: sharedApplicationInsights.properties.ConnectionString
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
          name: 'TableStorageConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=TableStorageConnectionString)'
        }
        {
          name: 'OpenAI__Endpoint'
          value: sharedOpenAI.properties.endpoint
        }
        {
          name: 'OpenAI__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=OpenAIApiKey)'
        }
      ]
    }
  }
  tags: {
    'azd-env-name': environmentName
    'azd-service-name': 'web'
  }
}

// Create diagnostic settings for App Service
resource appServiceDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'appservice-diagnostics'
  scope: appService
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Role assignment for managed identity to access Key Vault
resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, managedIdentity.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Module for role assignments on shared resources
module sharedResourceRoleAssignments 'shared-role-assignments.bicep' = {
  name: 'shared-role-assignments'
  scope: resourceGroup(sharedResourceGroupName)
  params: {
    managedIdentityPrincipalId: managedIdentity.properties.principalId
    sharedStorageAccountName: sharedStorageAccountName
    sharedOpenAIName: sharedOpenAIName
  }
}

// Store connection strings in Key Vault
resource tableStorageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'TableStorageConnectionString'
  parent: keyVault
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${sharedStorageAccount.name};AccountKey=${sharedStorageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
}

resource openAIApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'OpenAIApiKey'
  parent: keyVault
  properties: {
    value: sharedOpenAI.listKeys().key1
  }
}

// App Service Site Extension for Application Insights
resource appServiceSiteExtension 'Microsoft.Web/sites/siteextensions@2023-01-01' = {
  name: 'Microsoft.ApplicationInsights.AzureWebSites'
  parent: appService
}

// Outputs
output WEB_APP_NAME string = appService.name
output WEB_APP_URI string = 'https://${appService.properties.defaultHostName}'
output APPLICATIONINSIGHTS_CONNECTION_STRING string = sharedApplicationInsights.properties.ConnectionString
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.properties.vaultUri
output TABLE_STORAGE_CONNECTION_STRING string = tableStorageConnectionStringSecret.properties.secretUri
output OPENAI_ENDPOINT string = sharedOpenAI.properties.endpoint
