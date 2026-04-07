using './main.bicep'

param workloadName = 'battleship'
param env = 'prd'
param location = 'northeurope'
param resourceGroupName = 'rg-battleship-prd'

// Image references — overridden at deployment time with the versioned image tag.
param apiImage = ''
param frontendImage = ''

// Supplied at deployment time via GitHub Actions secrets. Do not commit real values.
param containerRegistryServer = ''
param containerRegistryUsername = ''
param containerRegistryPassword = ''
param jwtSigningKey = ''
