@description('Resource name prefix')
param prefix string

@description('Environment name (dev, prod)')
param environment string

@description('Azure region for all resources')
param location string

@description('Full Key Vault secret URI for Database__ConnectionString (used as KV reference)')
param dbSecretUri string

@description('Azure Container Registry login server (e.g. myregistry.azurecr.io)')
param acrLoginServer string

@description('Azure Container Registry resource name (for listCredentials)')
param acrName string

@description('Docker image name without registry prefix or tag (e.g. dotnetazurestarter-api)')
param imageName string

var appServicePlanName = 'asp-${prefix}-${environment}'
var webAppName = 'app-${prefix}-${environment}'
var appInsightsName = 'appi-${prefix}-${environment}'
var logAnalyticsName = 'log-${prefix}-${environment}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// F1 (Free) — Linux, reserved must be true for Linux plans
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/${imageName}:latest'
      http20Enabled: true
      // alwaysOn is not supported on F1 Free tier
      alwaysOn: false
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:8080'
        }
        {
          // Key Vault reference — resolved at runtime using the App Service managed identity
          name: 'Database__ConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${dbSecretUri})'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          // App Service routes external traffic to the container port matching ASPNETCORE_URLS
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acrLoginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: listCredentials(resourceId('Microsoft.ContainerRegistry/registries', acrName), '2023-07-01').username
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: listCredentials(resourceId('Microsoft.ContainerRegistry/registries', acrName), '2023-07-01').passwords[0].value
        }
      ]
    }
  }
}

output appUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output principalId string = webApp.identity.principalId
