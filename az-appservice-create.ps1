$RESOURCE_GROUP = "learn-415f644e-2e8a-4dc4-bcb1-47943220833b"
$AZURE_REGION = "eastus"
$guid = "6141f1e3-802c-4aa1-b11d-c0c7dd74f0f0"
$AZURE_WEB_APP = "wapp-" + $guid
$AZURE_APP_PLAN = "AppServicePlan-" + $AZURE_WEB_APP

<#
az login
az account set --subscription "Concierge Subscription"
az account list --refresh --query "[?contains(name, 'Concierge Subscription')].id" --output table
#>
az account set --subscription "0efa1d89-df92-47ed-903e-2dfcbc92ab9f"
az configure --defaults group=$RESOURCE_GROUP

az deployment group create --resource-group $RESOURCE_GROUP --parameters location=$AZURE_REGION webAppName=$AZURE_WEB_APP --template-file templates\az-appservice.json

<#
az group list --output table
az group list --query "[?name == '$RESOURCE_GROUP']"

write-host "Creating App Service Plan ${AZURE_APP_PLAN}"
az appservice plan create --name $AZURE_APP_PLAN --resource-group $RESOURCE_GROUP --location $AZURE_REGION --sku FREE
az appservice plan list --output table

write-host "Creating Web App ${AZURE_WEB_APP}"
az webapp create --name $AZURE_WEB_APP --resource-group $RESOURCE_GROUP --plan $AZURE_APP_PLAN --runtime "DOTNETCORE:8.0"
az webapp list --output table
#>
