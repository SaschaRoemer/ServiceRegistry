$RESOURCE_GROUP = "learn-415f644e-2e8a-4dc4-bcb1-47943220833b"
$AZURE_REGION = "eastus"
$guid = "6141f1e3-802c-4aa1-b11d-c0c7dd74f0f0"
$AZURE_APP_PLAN = "AppServicePlan-" + $guid
$AZURE_WEB_APP = "wapp-" + $guid

dotnet publish --configuration Release --runtime linux-x64 --self-contained
Compress-Archive -Force -Path bin\Release\net8.0\linux-x64\publish\* -DestinationPath bin\package.zip

az webapp deploy --resource-group $RESOURCE_GROUP --name $AZURE_WEB_APP --src-path bin\package.zip