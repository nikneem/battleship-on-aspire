using './main.bicep'

param workloadName = 'battleship'
param env = 'dev'

// apiImage and frontendImage default to the hello-world placeholder.
// After pushing images to ACR, override with e.g.:
//   param apiImage = 'crname.azurecr.io/battleship-api:latest'
//   param frontendImage = 'crname.azurecr.io/battleship-frontend:latest'

// jwtSigningKey MUST be supplied at deployment time. Do not commit a real value here.
// Example: az deployment group create ... --parameters jwtSigningKey=<secret>
param jwtSigningKey = ''
