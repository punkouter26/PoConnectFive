@description('Principal ID of the managed identity')
param managedIdentityPrincipalId string

@description('Name of the shared storage account')
param sharedStorageAccountName string

@description('Name of the shared OpenAI service')
param sharedOpenAIName string

// Reference existing shared resources
resource sharedStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: sharedStorageAccountName
}

resource sharedOpenAI 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: sharedOpenAIName
}

// Role assignment for managed identity to access Storage Account
resource storageTableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sharedStorageAccount.id, managedIdentityPrincipalId, 'Storage Table Data Contributor')
  scope: sharedStorageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3') // Storage Table Data Contributor
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for managed identity to access OpenAI
resource cognitiveServicesUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sharedOpenAI.id, managedIdentityPrincipalId, 'Cognitive Services User')
  scope: sharedOpenAI
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a97b65f3-24c7-4388-baec-2e87135dc908') // Cognitive Services User
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}
