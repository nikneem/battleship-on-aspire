param name string
param location string
param logAnalyticsWorkspaceCustomerId string

@secure()
param logAnalyticsWorkspacePrimarySharedKey string

param managedIdentityClientId string
param storageAccountName string
param serviceBusHostName string

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspaceCustomerId
        sharedKey: logAnalyticsWorkspacePrimarySharedKey
      }
    }
  }
}

resource statestoreDaprComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  parent: containerAppsEnv
  name: 'statestore'
  properties: {
    componentType: 'state.azure.tablestorage'
    version: 'v1'
    metadata: [
      { name: 'accountName', value: storageAccountName }
      { name: 'tableName', value: 'statestore' }
      { name: 'azureClientId', value: managedIdentityClientId }
    ]
    scopes: ['battleship-api']
  }
}

resource pubsubDaprComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  parent: containerAppsEnv
  name: 'pubsub'
  properties: {
    componentType: 'pubsub.azure.servicebus.topics'
    version: 'v1'
    metadata: [
      { name: 'namespaceName', value: serviceBusHostName }
      { name: 'azureClientId', value: managedIdentityClientId }
    ]
    scopes: ['battleship-api']
  }
}

output id string = containerAppsEnv.id
output name string = containerAppsEnv.name
output defaultDomain string = containerAppsEnv.properties.defaultDomain

// ── Managed certificate ────────────────────────────────────────────────────────
// Prerequisite: add a CNAME record for battleship.hexmaster.nl pointing to
// the environment's defaultDomain before deploying, so Azure can validate ownership.

resource managedCertificate 'Microsoft.App/managedEnvironments/managedCertificates@2024-03-01' = {
  parent: containerAppsEnv
  name: 'cert-battleship-hexmaster-nl'
  location: location
  properties: {
    subjectName: 'battleship.hexmaster.nl'
    domainControlValidation: 'CNAME'
  }
}

output managedCertificateId string = managedCertificate.id
