// This deploys an entire environment stack
// It reuses some shared resources within a resource group (e.g. prod / non-prod)
// and then deploys and configures environment specific resources
// based on parameters passed through
// for a given service and environment combination (e.g. biobankinguk dev)

// USE PARAMETER FILES to deploy an actual environment

import { referenceSecret } from 'br/DrsUtils:functions:v1'

type ServiceNames = 'hutch-beacon'
param serviceName ServiceNames

type Environments = 'dev' | 'qa' | 'uat' | 'prod'
param env Environments

param frontendAppName string = '${env}-${serviceName}'
param frontendHostnames array = []
param backendAppName string = '${env}-${serviceName}-api'
param keyVaultName string = '${serviceName}-${env}-kv'

param location string = resourceGroup().location

param sharedEnv string = 'shared'
var sharedPrefix = '${serviceName}-${sharedEnv}'

param frontendAppSettings object = {}
param backendAppSettings object = {}

param aspName string

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

// Per Environment Resources

// Environment Key Vault pre-existing and populated
resource kv 'Microsoft.KeyVault/vaults@2019-09-01' existing = {
  name: keyVaultName
}

// Create the Beacon App and related bits
// App Insights
// App Service
// VNET Integration
// Hostnames
module frontend 'br/DrsComponents:app-service:v1' = {
  name: 'beacon-${uniqueString(frontendAppName)}'
  params: {
    location: location
    appName: frontendAppName
    aspName: aspName
    logAnalyticsWorkspaceName: la.outputs.name
    appHostnames: frontendHostnames
    tags: {
      Service: serviceName
      Environment: env
    }
  }
}

module backend 'br/DrsComponents:app-service:v1' = {
  name: 'beacon-${uniqueString(backendAppName)}'
  params: {
    location: location
    appName: backendAppName
    aspName: aspName
    logAnalyticsWorkspaceName: la.outputs.name
    tags: {
      Service: serviceName
      Environment: env
    }
  }
}

// Grant the app Key Vault access
module backendKvAccess 'br/DrsConfig:keyvault-access:v1' = {
  name: 'kvAccess-${uniqueString(backendAppName)}'
  params: {
    keyVaultName: kv.name
    tenantId: backend.outputs.identity.tenantId
    objectId: backend.outputs.identity.principalId
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

var baseBackendSettings = {
  DOTNET_Environment: friendlyEnvironmentNames[env]

  // App specific Azure/AI config
  APPLICATIONINSIGHTS_CONNECTION_STRING: backend.outputs.appInsights.connectionString
  WEBSITE_RUN_FROM_PACKAGE: 1

  // Default App Settings
  // TODO: add default settigns for BeaconBridge

  // Minio settings
  Minio__AccessKey: referenceSecret(kv.name, 'minio-access-key')
  Minio__SecretKey: referenceSecret(kv.name, 'minio-secret-key')
  Minio__Host: referenceSecret(kv.name, 'minio-host')
  Minio__Secure: referenceSecret(kv.name, 'minio-secure')
  Minio__Bucket: referenceSecret(kv.name, 'minio-bucket')

  // openid-connect settings
  IdentityProvider__OpenIdBaseUrl: referenceSecret(kv.name, 'oidc-base-url')
  IdentityProvider__ClientId: referenceSecret(kv.name, 'oidc-client-id')
  IdentityProvider__ClientSecret: referenceSecret(kv.name, 'oidc-client-secret')
  IdentityProvider__Username: referenceSecret(kv.name, 'oidc-username')
  IdentityProvider__Password: referenceSecret(kv.name, 'oidc-password')
  IdentityProvider__EgressOpenIdBaseUrl: referenceSecret(kv.name, 'oidc-egress-base-url')
  IdentityProvider__EgressClientId: referenceSecret(kv.name,'oidc-egress-client-id')
  IdentityProvider__EgressClientSecret: referenceSecret(kv.name,'oidc-egress-client-secret')

  // Submission Layer settings
  SubmissionLayer__ProjectName: referenceSecret(kv.name, 'submission-project-name')
  SubmissionLayer__Tres__0: referenceSecret(kv.name, 'submission-tre-name')
  SubmissionLayer__SubmissionLayerHost: referenceSecret(kv.name, 'submission-host')
  // Egress Layer settings
  EgressLayer__EgressLayerHost: referenceSecret(kv.name,'egress-host')
}

var baseFrontendSettings = {
  // App specific Azure/AI config
  APPLICATIONINSIGHTS_CONNECTION_STRING: frontend.outputs.appInsights.connectionString
  WEBSITE_RUN_FROM_PACKAGE: 1

  // Default App Settings
  BACKEND_URL: backend.outputs.defaultUrl
}

module backendConfig 'br/DrsConfig:webapp:v1' = {
  name: 'siteConfig-${uniqueString(backendAppName)}'
  params: {
    appName: backend.outputs.name
    appSettings: union(
      appInsightsSettings,
      baseBackendSettings,
      backendAppSettings)
    connectionStrings: {
      BeaconBridgeDb: {
        type: 'SQLServer'
        value: referenceSecret(kv.name, 'db-connection-string')
      }
    }
  }
}

module frontendConfig 'br/DrsConfig:webapp:v1' = {
  name: 'siteConfig-${uniqueString(frontendAppName)}'
  params: {
    appName: frontend.outputs.name
    appSettings: union(
      appInsightsSettings,
      baseFrontendSettings,
      frontendAppSettings)
    appFramework: 'NODE|20-lts'
    startCommand: 'server.js'
  }
}

// Add SSL certificates
// this needs to be done as a separate stage to creating the app with a bound hostname
@batchSize(1) // also needs to be done serially to avoid concurrent updates to the app service
module apiCert 'br/DrsComponents:managed-cert:v1' = [for hostname in frontendHostnames: {
  name: 'api-cert-${uniqueString(hostname)}'
  params: {
    location: location
    hostname: hostname
    appName: frontend.outputs.name
    aspId: frontend.outputs.aspId
  }
}]
