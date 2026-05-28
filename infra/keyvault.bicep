@description('Resource name prefix')
param prefix string

@description('Environment name (dev, prod)')
param environment string

@description('Azure region for all resources')
param location string

@description('Database connection string stored as a Key Vault secret')
@secure()
param connectionString string

// Key Vault name max 24 chars — keep prefix short
var keyVaultName = 'kv-${prefix}-${environment}'
var secretName = 'DatabaseConnectionString'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enabledForTemplateDeployment: true
    accessPolicies: []
  }
}

resource dbSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretName
  properties: {
    value: connectionString
  }
}

output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output dbSecretUri string = dbSecret.properties.secretUri
