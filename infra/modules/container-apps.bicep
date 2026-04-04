param location string
param environmentId string
param containerRegistryServer string
param managedIdentityId string
param managedIdentityClientId string

@secure()
param containerRegistryUsername string

@secure()
param containerRegistryPassword string

@description('Full container image reference for the API. Defaults to a hello-world placeholder for initial infra deployment.')
param apiImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Full container image reference for the frontend. Defaults to a hello-world placeholder for initial infra deployment.')
param frontendImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@secure()
param jwtSigningKey string

resource apiContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'battleship-api'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        // 'auto' supports both HTTP/1.1 and HTTP/2 — required for SignalR WebSockets
        transport: 'auto'
        corsPolicy: {
          allowedOrigins: ['*']
          allowedHeaders: ['*']
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
          allowCredentials: true
        }
      }
      dapr: {
        enabled: true
        appId: 'battleship-api'
        appPort: 8080
        appProtocol: 'http'
      }
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistryPassword
        }
        {
          name: 'jwt-signing-key'
          value: jwtSigningKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'battleship-api'
          image: apiImage
          env: [
            { name: 'ASPNETCORE_URLS', value: 'http://+:8080' }
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'AnonymousPlayerSessions__StateStoreName', value: 'statestore' }
            { name: 'AnonymousPlayerSessions__JwtSigningKey', secretRef: 'jwt-signing-key' }
            { name: 'AnonymousPlayerSessions__Issuer', value: 'HexMaster.BattleShip.Api' }
            { name: 'AnonymousPlayerSessions__Audience', value: 'HexMaster.BattleShip.App' }
            // Used by Azure SDK DefaultAzureCredential to select the user-assigned identity
            { name: 'AZURE_CLIENT_ID', value: managedIdentityClientId }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 5
      }
    }
  }
}

resource frontendContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'battleship-frontend'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
      }
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistryPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'battleship-frontend'
          image: frontendImage
          env: [
            {
              name: 'BATTLESHIP_API_URL'
              value: 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

output apiFqdn string = apiContainerApp.properties.configuration.ingress.fqdn
output frontendFqdn string = frontendContainerApp.properties.configuration.ingress.fqdn
output apiUrl string = 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
output frontendUrl string = 'https://${frontendContainerApp.properties.configuration.ingress.fqdn}'
