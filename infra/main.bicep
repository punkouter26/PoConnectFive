targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Name of the resource group for app-specific resources')
param resourceGroupName string = 'PoConnectFive'

@description('Name of the shared resource group')
param sharedResourceGroupName string = 'PoShared'

// Tags for all resources
var tags = {
  'azd-env-name': environmentName
  'app': 'PoConnectFive'
}

// Create the app-specific resource group
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Reference the shared resource group
resource sharedRg 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: sharedResourceGroupName
}

// Deploy app-specific resources to PoConnectFive resource group
module resources 'resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    sharedResourceGroupName: sharedResourceGroupName
    tags: tags
  }
}

// Outputs for azd
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = resources.outputs.containerRegistryEndpoint
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = resources.outputs.containerAppsEnvironmentId
output WEB_URL string = resources.outputs.webUrl
