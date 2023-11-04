$RESOURCE_GROUP = "learn-5101111f-8f55-4d95-abd3-6187d736467f"
$AZURE_REGION = "eastus"
$guid = "6141f1e3-802c-4aa1-b11d-c0c7dd74f0f0"
$AZURE_WEB_APP = "wapp-" + $guid
$AZURE_APP_PLAN = "AppServicePlan-" + $AZURE_WEB_APP

# Azure setup
az login
az account set --subscription "Concierge Subscription"
az account list --refresh --query "[?contains(name, 'Concierge Subscription')].id" --output table

az account set --subscription "0efa1d89-df92-47ed-903e-2dfcbc92ab9f"
az configure --defaults group=$RESOURCE_GROUP

az group list --output table
az group list --query "[?name == '$RESOURCE_GROUP']"

write-host "Creating App Service Plan ${AZURE_APP_PLAN}"
az deployment group create --resource-group $RESOURCE_GROUP --parameters name=$AZURE_WEB_APP --template-file templates/az-appservice.json
#az appservice plan create --name $AZURE_APP_PLAN --resource-group $RESOURCE_GROUP --location $AZURE_REGION --sku FREE
#az appservice plan list --output table

write-host "Creating Web App ${AZURE_WEB_APP}"
az webapp create --name $AZURE_WEB_APP --resource-group $RESOURCE_GROUP --plan $AZURE_APP_PLAN --runtime "DOTNETCORE:8.0"
az webapp list --output table

# Deploy
dotnet publish --configuration Release --runtime linux-x64 --self-contained
Compress-Archive -Force -Path bin\Release\net8.0\linux-x64\publish\* -DestinationPath bin\package.zip

az webapp deploy --resource-group $RESOURCE_GROUP --name $AZURE_WEB_APP --src-path bin\package.zip