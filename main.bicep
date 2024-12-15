param location string = resourceGroup().location
param appName string = 'PoConnectFive'

// Static Web App resource
resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: appName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    repositoryUrl: ''  // Will be filled by GitHub Actions
    branch: 'main'
    buildProperties: {
      appLocation: 'PoConnectFive.Client'
      outputLocation: 'wwwroot'
    }
  }
} 
