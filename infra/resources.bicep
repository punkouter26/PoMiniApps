@description('Primary location for all resources')
param location string

@description('Environment name')
param environmentName string

@description('Resource group of the shared resources')
param sharedResourceGroupName string = 'PoShared'

@description('Name of the existing Log Analytics Workspace in PoShared')
param existingLogAnalyticsName string = 'PoShared-LogAnalytics'

@description('Name of the existing Application Insights in PoShared')
param existingAppInsightsName string = 'appi-PoShared'

@description('App Service Plan name')
param appServicePlanName string = 'asp-PoMiniApps'

// ── App Service Plan ──────────────────────────────────────────────────────
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource sharedAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: existingAppInsightsName
  scope: resourceGroup(sharedResourceGroupName)
}

// ── Web App (app-specific) ───────────────────────────────────────────────────
resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-pominiapps'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: false
      appSettings: [
        {
          name: 'Azure__KeyVaultName'
          value: 'kv-poshared'
        }
        {
          name: 'Azure__StorageConnectionString'
          value: storageAccount.properties.primaryEndpoints.table
        }
        {
          name: 'Azure__TableStorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: sharedAppInsights.properties.ConnectionString
        }
        {
          name: 'Azure__PoShared__ApplicationInsights__ConnectionString'
          value: sharedAppInsights.properties.ConnectionString
        }
      ]
    }
  }
}

// ── Storage Account (Table Storage — app-specific) ───────────────────────────
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'stpominiapps26'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

// ── Table Storage services ───────────────────────────────────────────────────
resource tableServices 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
