targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Environment name (used for tags only).')
param environmentName string = 'dev'

@description('Primary location for all resources')
param location string = 'eastus2'

// The resource group name and all resource names must match the .sln name exactly
@description('Name of the resource group to create (and the base name for all resources)')
param resourceGroupName string = 'PoConnectFive'

@description('Existing shared resource group name (contains the shared App Service Plan)')
param sharedResourceGroupName string = 'PoShared'

@description('Name of the existing shared App Service Plan in PoShared')
param sharedAppServicePlanName string = 'PoShared'

// Create new resource group named exactly after the solution
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// Module for the main application resources (all resources use the exact same name)
module resources 'resources.bicep' = {
  name: 'resources'
  scope: resourceGroup
  params: {
    location: location
    environmentName: environmentName
    resourceName: resourceGroupName
    sharedResourceGroupName: sharedResourceGroupName
    sharedAppServicePlanName: sharedAppServicePlanName
  }
}

// Output required values
output RESOURCE_GROUP_ID string = resourceGroup.id
output WEB_APP_NAME string = resources.outputs.WEB_APP_NAME
output WEB_APP_URI string = resources.outputs.WEB_APP_URI
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATIONINSIGHTS_CONNECTION_STRING
output STORAGE_CONNECTION_STRING string = resources.outputs.STORAGE_CONNECTION_STRING
