targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Name of the resource group to create')
param resourceGroupName string = 'PoConnectFive'

@description('Existing shared resource group name')
param sharedResourceGroupName string = 'PoShared'

@description('Name of the existing shared app service plan')
param sharedAppServicePlanName string = 'PoSharedAppServicePlan'

@description('Name of the existing shared application insights')
param sharedApplicationInsightsName string = 'PoSharedApplicationInsights'

@description('Name of the existing shared storage account')
param sharedStorageAccountName string = 'posharedtablestorage'

@description('Name of the existing shared OpenAI service')
param sharedOpenAIName string = 'PoSharedOpenaiEastUS'

// Resource token for unique naming - use solution name directly for clean URLs
var resourceToken = 'PoConnectFive'
var resourcePrefix = ''

// Reference existing shared resource group
resource sharedResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' existing = {
  name: sharedResourceGroupName
}

// Create new resource group for PoConnectFive
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: {
    'azd-env-name': environmentName
  }
  dependsOn: [
    sharedResourceGroup
  ]
}

// Module for the main application resources
module resources 'resources.bicep' = {
  name: 'resources'
  scope: resourceGroup
  params: {
    location: location
    environmentName: environmentName
    resourceToken: resourceToken
    resourcePrefix: resourcePrefix
    sharedResourceGroupName: sharedResourceGroupName
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedApplicationInsightsName: sharedApplicationInsightsName
    sharedStorageAccountName: sharedStorageAccountName
    sharedOpenAIName: sharedOpenAIName
  }
}

// Output required values
output RESOURCE_GROUP_ID string = resourceGroup.id
output WEB_APP_NAME string = resources.outputs.WEB_APP_NAME
output WEB_APP_URI string = resources.outputs.WEB_APP_URI
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATIONINSIGHTS_CONNECTION_STRING
output OPENAI_ENDPOINT string = resources.outputs.OPENAI_ENDPOINT
