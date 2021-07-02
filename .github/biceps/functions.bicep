param app_name string
param sku_app_name string = 'Y1'
param sku_luis_name string = 'F0'
param sku_storage_name string = 'Standard_LRS'
param storage_name string

var rg = resourceGroup()

resource storage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: storage_name
  location: resourceGroup().location
  sku: {
    name: sku_storage_name
    tier: 'Standard'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
    encryption: {
      keySource: 'Microsoft.Storage'
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
      }
    }
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2021-04-01' = {
  name: '${storage_name}/default'
  dependsOn: [
    storage
  ]
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' = {
  name: '${storage_name}/default'
  dependsOn: [
    storage
  ]
}

resource blobServiceWeb 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: '${storage_name}/default/$web'
  dependsOn: [
    storage
    blobService
  ]
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-01-01' = {
  name: '${app_name}-hosting'
  location: rg.location
  kind: 'linux'
  properties: {
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    maximumElasticWorkerCount: 1
    reserved: true
  }
  sku: {
    name: sku_app_name
    tier: 'Dynamic'
  }
}

resource appInsights 'Microsoft.Insights/components@2015-05-01' = {
  name: '${app_name}-insights'
  location: rg.location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource appLuis 'Microsoft.CognitiveServices/accounts@2021-04-30' = {
  name: '${app_name}luis'
  location: rg.location
  kind: 'LUIS'
  sku: {
    name: sku_luis_name
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource functionApp 'Microsoft.Web/sites@2018-11-01' = {
  name: '${app_name}-app'
  location: rg.location
  kind: 'functionapp,linux'
  properties: {
    httpsOnly: true
    serverFarmId: hostingPlan.id
    clientAffinityEnabled: false
    redundancyMode: 'None'
    siteConfig: {
      http20Enabled: true
      alwaysOn: false
      linuxFxVersion: ''
      numberOfWorkers: 1
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: app_name
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: 'FLE Standard Time'
        }
        {
          name: 'DefaultTimeZoneName'
          value: 'FLE Standard Time'
        }
        {
          name: 'GoogleCloudApiKey'
          value: ''
        }
        {
          name: 'GoogleCloudApplicationName'
          value: 'MentorBot'
        }
        {
          name: 'GoogleCreadentialsFilePath'
          value: ''
        }
        {
          name: 'HangoutChatRequestToken'
          value: ''
        }
        {
          name: 'LuisApiHostName'
          value: '${rg.location}.api.cognitive.microsoft.com'
        }
        {
          name: 'LuisApiAppId'
          value: appLuis.id
        }
        {
          name: 'LuisApiAppKey'
          value: ''
        }
      ]
    }
  }
}
