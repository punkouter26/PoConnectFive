param location string = 'eastus2'
param appName string = 'PoConnectFive'

// Static Web App resource
resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: appName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
} 
