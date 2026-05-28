@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Short prefix used in all resource names (e.g. dotnetazstarter). Keep it short — Key Vault max is 24 chars.')
param prefix string

@description('Deployment environment tag.')
@allowed(['dev', 'prod'])
param environment string = 'dev'

@description('SQL Server administrator login name.')
param sqlAdminLogin string

@description('SQL Server administrator password. Pass at deploy time — never commit this value.')
@secure()
param sqlAdminPassword string

// ─── SQL Server + Database ────────────────────────────────────────────────────

module sqlModule 'sql-server.bicep' = {
  name: 'deploy-sql-server'
  params: {
    prefix: prefix
    environment: environment
    location: location
    adminLogin: sqlAdminLogin
    adminPassword: sqlAdminPassword
  }
}

// ─── Azure Container Registry ────────────────────────────────────────────────

module acrModule 'acr.bicep' = {
  name: 'deploy-acr'
  params: {
    prefix: prefix
    environment: environment
    location: location
  }
}

// ─── Key Vault + DB secret ────────────────────────────────────────────────────

module kvModule 'keyvault.bicep' = {
  name: 'deploy-keyvault'
  params: {
    prefix: prefix
    environment: environment
    location: location
    connectionString: sqlModule.outputs.connectionString
  }
}

// ─── App Service + App Insights ───────────────────────────────────────────────

module appModule 'app-service.bicep' = {
  name: 'deploy-app-service'
  params: {
    prefix: prefix
    environment: environment
    location: location
    dbSecretUri: kvModule.outputs.dbSecretUri
    acrLoginServer: acrModule.outputs.acrLoginServer
    acrName: acrModule.outputs.acrName
    imageName: 'dotnetazurestarter-api'
  }
}

// ─── Key Vault access policy ──────────────────────────────────────────────────
// Grant the App Service managed identity Get/List on secrets so the KV reference
// in Database__ConnectionString resolves at runtime.
// Name must be computed at deploy-start, so we reproduce the same formula used in keyvault.bicep.

var keyVaultName = 'kv-${prefix}-${environment}'

resource existingKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource kvAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: existingKeyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: appModule.outputs.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
  }
}

// ─── Outputs ──────────────────────────────────────────────────────────────────

output appUrl string = appModule.outputs.appUrl
output webAppName string = appModule.outputs.webAppName
output keyVaultName string = kvModule.outputs.keyVaultName
output sqlServerFqdn string = sqlModule.outputs.sqlServerFqdn
output acrLoginServer string = acrModule.outputs.acrLoginServer
output acrName string = acrModule.outputs.acrName
