# Requires: Azure CLI and AZD CLI installed and logged in
# Hard-coded values per requirements
$ErrorActionPreference = 'Stop'

$rgName = 'PoConnectFive'
$location = 'eastus2'
$sharedRg = 'PoShared'
$sharedPlan = 'PoSharedAppServicePlan'

Write-Host "Creating resource group $rgName in $location..."
az group create --name $rgName --location $location --tags azd-env-name=dev | Out-Null

Write-Host "Deploying bicep to $rgName..."
az deployment sub create `
  --name PoConnectFive-deploy `
  --location $location `
  --template-file ./infra/main.bicep `
  --parameters environmentName=dev location=$location resourceGroupName=$rgName sharedResourceGroupName=$sharedRg sharedAppServicePlanName=$sharedPlan `
  --only-show-errors

Write-Host "azd provisioning (optional) ..."
$env:A Z U R E _ D E V _ E N V _ N A M E = 'dev'
$env:AZURE_ENV_NAME = 'dev'
$env:AZURE_LOCATION = $location
azd provision --no-prompt --environment dev

Write-Host "Deploying app with azd..."
azd deploy --no-prompt --environment dev

Write-Host "Done."
