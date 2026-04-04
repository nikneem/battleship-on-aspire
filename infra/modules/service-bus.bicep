param name string
param location string
param managedIdentityPrincipalId string

// Azure Service Bus Data Owner built-in role ID
var serviceBusDataOwnerRoleId = '090c5cfd-751d-490a-894a-3ce6f1109419'

// Mirrors IntegrationEventTopics.cs exactly
var topicNames = [
  'battleship.game.game-created'
  'battleship.game.player-joined'
  'battleship.game.player-marked-ready'
  'battleship.game.fleet-submitted'
  'battleship.game.fleet-locked'
  'battleship.game.game-started'
  'battleship.game.shot-fired'
  'battleship.game.game-finished'
  'battleship.game.game-cancelled'
  'battleship.game.game-abandoned'
  'battleship.player.connection-lost'
  'battleship.player.connection-reestablished'
  'battleship.player.connection-timed-out'
]

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: name
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {}
}

resource topics 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = [
  for topicName in topicNames: {
    parent: serviceBusNamespace
    name: topicName
    properties: {
      defaultMessageTimeToLive: 'PT1H'
      maxSizeInMegabytes: 1024
    }
  }
]

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, managedIdentityPrincipalId, serviceBusDataOwnerRoleId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      serviceBusDataOwnerRoleId
    )
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output id string = serviceBusNamespace.id
output name string = serviceBusNamespace.name
output hostName string = '${serviceBusNamespace.name}.servicebus.windows.net'
