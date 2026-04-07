targetScope = 'subscription'

@description('Azure region for all resources.')
param location string = 'northeurope'

@description('Name of the resource group to create.')
param resourceGroupName string

@description('Short workload name used as the base for resource names (max 12 characters).')
@maxLength(12)
param workloadName string = 'battleship'

@description('Environment discriminator appended to resource names.')
@allowed(['dev', 'tst', 'prd'])
param env string = 'prd'

@description('Full container image reference for the API. Defaults to a hello-world placeholder for initial infrastructure deployment.')
param apiImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Full container image reference for the frontend. Defaults to a hello-world placeholder for initial infrastructure deployment.')
param frontendImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('JWT signing key for anonymous player session tokens (minimum 32 characters).')
@secure()
@minLength(32)
param jwtSigningKey string

@description('Container registry server hostname (e.g. myregistry.azurecr.io).')
param containerRegistryServer string

@description('Container registry username.')
param containerRegistryUsername string

@description('Container registry password.')
@secure()
param containerRegistryPassword string

// ── Resource group ─────────────────────────────────────────────────────────────

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// ── Naming helpers ─────────────────────────────────────────────────────────────
var suffix = '${workloadName}-${env}'
var uniqueSuffix = uniqueString(rg.id)

// Storage account: 3–24 lowercase alphanumeric characters
var storageAccountName = toLower(take('st${replace(suffix, '-', '')}${uniqueSuffix}', 24))

// Service Bus namespace: 6–50 characters
var serviceBusName = 'sbns-${suffix}-${take(uniqueSuffix, 6)}'

// ── Modules ────────────────────────────────────────────────────────────────────

module managedIdentity 'modules/managed-identity.bicep' = {
  name: 'managed-identity'
  scope: rg
  params: {
    name: 'id-${suffix}'
    location: location
  }
}

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics'
  scope: rg
  params: {
    name: 'log-${suffix}'
    location: location
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  scope: rg
  params: {
    name: storageAccountName
    location: location
    managedIdentityPrincipalId: managedIdentity.outputs.principalId
  }
}

module serviceBus 'modules/service-bus.bicep' = {
  name: 'service-bus'
  scope: rg
  params: {
    name: serviceBusName
    location: location
    managedIdentityPrincipalId: managedIdentity.outputs.principalId
  }
}

module containerAppsEnv 'modules/container-apps-environment.bicep' = {
  name: 'container-apps-environment'
  scope: rg
  params: {
    name: 'cae-${suffix}'
    location: location
    logAnalyticsWorkspaceCustomerId: logAnalytics.outputs.customerId
    logAnalyticsWorkspacePrimarySharedKey: logAnalytics.outputs.primarySharedKey
    managedIdentityClientId: managedIdentity.outputs.clientId
    storageAccountName: storage.outputs.name
    serviceBusHostName: serviceBus.outputs.hostName
  }
}

module containerApps 'modules/container-apps.bicep' = {
  name: 'container-apps'
  scope: rg
  params: {
    location: location
    environmentId: containerAppsEnv.outputs.id
    containerRegistryServer: containerRegistryServer
    containerRegistryUsername: containerRegistryUsername
    containerRegistryPassword: containerRegistryPassword
    managedIdentityId: managedIdentity.outputs.id
    managedIdentityClientId: managedIdentity.outputs.clientId
    apiImage: apiImage
    frontendImage: frontendImage
    jwtSigningKey: jwtSigningKey
  }
}

// ── Outputs ────────────────────────────────────────────────────────────────────

@description('Public URL of the battleship API.')
output apiUrl string = containerApps.outputs.apiUrl

@description('Public URL of the battleship frontend.')
output frontendUrl string = containerApps.outputs.frontendUrl
