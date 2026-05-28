@description('Resource name prefix')
param prefix string

@description('Environment name (dev, prod)')
param environment string

@description('Azure region for all resources')
param location string

// ACR names: alphanumeric only, 5–50 chars, globally unique — strip hyphens from prefix
var acrName = '${replace(prefix, '-', '')}${environment}acr'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
  }
}

output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
