@description('Principal ID of the managed identity')
param managedIdentityPrincipalId string

@description('Name of the shared storage account')
param sharedStorageAccountName string

// Reference existing shared resources
resource sharedStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: sharedStorageAccountName
}

// Role assignment for managed identity to access Storage Account
resource storageTableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sharedStorageAccount.id, managedIdentityPrincipalId, 'Storage Table Data Contributor')
  scope: sharedStorageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
    ) // Storage Table Data Contributor
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}
