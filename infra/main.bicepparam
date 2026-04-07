using './main.bicep'

param workloadName = 'battleship'
param env = 'prd'
param location = 'northeurope'
param resourceGroupName = 'rg-battleship-prd'

// apiImage and frontendImage default to the hello-world placeholder.
// Override after pushing images, e.g.:
//   --parameters apiImage='myregistry.azurecr.io/battleship/battleship-backend-api:1.0.0'
//   --parameters frontendImage='myregistry.azurecr.io/battleship-frontend:1.0.0'

// containerRegistryServer, containerRegistryUsername, containerRegistryPassword,
// and jwtSigningKey MUST be supplied at deployment time via --parameters.
// Do not commit real values here.
