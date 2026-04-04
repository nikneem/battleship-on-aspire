using './main.bicep'

param workloadName = 'battleship'
param env = 'dev'

// apiImage and frontendImage default to the hello-world placeholder.
// Override after pushing images, e.g.:
//   --parameters apiImage='myregistry.azurecr.io/battleship-api:1.0.0'
//   --parameters frontendImage='myregistry.azurecr.io/battleship-frontend:1.0.0'

// Registry credentials and jwtSigningKey MUST be supplied at deployment time.
// Do not commit real values here.
// Example:
//   az deployment group create ... \
//     --parameters containerRegistryServer=myregistry.azurecr.io \
//     --parameters containerRegistryUsername=myuser \
//     --parameters containerRegistryPassword=<secret> \
//     --parameters jwtSigningKey=<secret>
param containerRegistryServer = ''
param containerRegistryUsername = ''
param containerRegistryPassword = ''
param jwtSigningKey = ''
