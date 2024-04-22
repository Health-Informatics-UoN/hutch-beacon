// This deploys an entire environment stack
// It reuses some shared resources within a resource group (e.g. prod / non-prod)
// and then deploys and configures environment specific resources
// based on parameters passed through
// for a given service and environment combination (e.g. biobankinguk dev)

// USE PARAMETER FILES to deploy an actual environment

import { referenceSecret } from 'br/DrsUtils:functions:v1'

type ServiceNames = 'BeaconBridge'
param serviceName ServiceNames

type Environments = 'dev' | 'qa' | 'uat' | 'prod'
param env Environments

param directoryAppName string = '${env}-${serviceName}-directory'
param directoryHostnames array = []
param directoryAppSettings object = {}
param keyVaultName string = '${serviceName}-${env}-kv'

param elasticUrl string

param location string = resourceGroup().location

param sharedEnv string = 'shared'
var sharedPrefix = '${serviceName}-${sharedEnv}'

param appServicePlanSku string = 'B1'

// Shared Resources

// log analytics workspace
module la 'br/DrsComponents:log-analytics-workspace:v1' = {
  name: 'la-ws-${uniqueString(sharedPrefix)}'
  params: {
    location: location
    logAnalyticsWorkspaceName: '${sharedPrefix}-la-ws'
    tags: {
      Environment: sharedEnv
    }
  }
}

// App Service Plan
module asp 'br/DrsComponents:app-service-plan:v1' = {
  name: 'asp'
  params: {
    location: location
    aspName: '${sharedPrefix}-asp'
    sku: appServicePlanSku
    tags: {
      Environment: sharedEnv
    }
  }
}

// Per Environment Resources

// Environment Key Vault pre-existing and populated
resource kv 'Microsoft.KeyVault/vaults@2019-09-01' existing = {
  name: keyVaultName
}

var appName = directoryAppName

// Create a storage account for worker operations
// And add its connection string to keyvault :)
// Blob and Queue reads/writes/triggers use this account
module workerStorage 'br/DrsComponents:storage-account:v1' = {
  name: 'storage-${uniqueString(appName)}'
  params: {
    location: location
    baseAccountName: 'worker'
    keyVaultName: kv.name
    uniqueStringSource: appName
    tags: {
      Service: serviceName
      Environment: env
    }
  }
}

// Create the Directory App and related bits
// App Insights
// App Service
// VNET Integration
// Hostnames
module directory 'br/DrsComponents:app-service:v1' = {
  name: 'directory-${uniqueString(appName)}'
  params: {
    location: location
    appName: appName
    aspName: asp.outputs.name
    logAnalyticsWorkspaceName: la.outputs.name
    appHostnames: directoryHostnames
    tags: {
      Service: serviceName
      Environment: env
    }
  }
}

// Grant the app Key Vault access
module directoryKvAccess 'br/DrsConfig:keyvault-access:v1' = {
  name: 'kvAccess-${uniqueString(appName)}'
  params: {
    keyVaultName: kv.name
    tenantId: directory.outputs.identity.tenantId
    objectId: directory.outputs.identity.principalId
  }
}

// Config (App Settings, Connection strings) here now that Key Vault links will resolve
// Overrides for environments come through as params
var friendlyEnvironmentNames = {
  dev: 'Dev'
  qa: 'QA'
  uat: 'UAT'
  prod: 'Production'
}

// Shared configs are defined inline here
var appInsightsSettings = {
  ApplicationInsightsAgent_EXTENSION_VERSION: '~2'
  XDT_MicrosoftApplicationInsights_Mode: 'recommended'
  DiagnosticServices_EXTENSION_VERSION: '~3'
  APPINSIGHTS_PROFILERFEATURE_VERSION: '1.0.0'
  APPINSIGHTS_SNAPSHOTFEATURE_VERSION: '1.0.0'
  InstrumentationEngine_EXTENSION_VERSION: '~1'
  SnapshotDebugger_EXTENSION_VERSION: '~1'
  XDT_MicrosoftApplicationInsights_BaseExtensions: '~1'
}


var baseDirectorySettings = {
  DOTNET_Environment: friendlyEnvironmentNames[env]

  // App specific Azure/AI config
  APPLICATIONINSIGHTS_CONNECTION_STRING: directory.outputs.appInsights.connectionString
  WEBSITE_RUN_FROM_PACKAGE: 1

  // Default App Settings
  SiteProperties__GoogleRecaptchaSecret: referenceSecret(kv.name, 'google-recaptcha-secret')

  OutboundEmail__Provider: 'sendgrid'
  OutboundEmail__SendGridApiKey: referenceSecret(kv.name, 'sendgrid-api-key')

  ElasticSearch__ApiBaseUrl: elasticUrl
  ElasticSearch__Username: 'elastic'
  ElasticSearch__Password: referenceSecret(kv.name, 'elastic-pw')
  ElasticSearch__DefaultCollectionsSearchIndex: '${serviceName}-${env}-collections'
  ElasticSearch__DefaultCapabilitiesSearchIndex: '${serviceName}-${env}-capabilities'

  JWT__Secret: referenceSecret(kv.name, 'api-jwt-secret')
}

module directoryConfig 'br/DrsConfig:webapp:v1' = {
  name: 'siteConfig-${uniqueString(appName)}'
  params: {
    appName: directory.outputs.name
    appSettings: union(
      appInsightsSettings,
      baseDirectorySettings,
      directoryAppSettings)
    connectionStrings: {
      Default: {
        type: 'SQLServer'
        value: referenceSecret(kv.name, 'db-connection-string')
      }
      AzureStorage: {
        type: 'Custom'
        value: referenceSecret(kv.name, workerStorage.outputs.connectionStringKvRef)
      }
    }
  }
}

// Add SSL certificates
// this needs to be done as a separate stage to creating the app with a bound hostname
@batchSize(1) // also needs to be done serially to avoid concurrent updates to the app service
module apiCert 'br/DrsComponents:managed-cert:v1' = [for hostname in directoryHostnames: {
  name: 'api-cert-${uniqueString(hostname)}'
  params: {
    location: location
    hostname: hostname
    appName: directory.outputs.name
    aspId: directory.outputs.aspId
  }
}]
