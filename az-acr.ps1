$RES_GROUP = "learn-e4e1ab41-637d-4131-9740-0e90a18aa099"
$SUBSCRIPTION = "3d94ea93-25fb-480f-a274-071703b1f783"
$ACR_NAME = "acr330bd18b"
$AKV_NAME = "${ACR_NAME}-3d94ea93"

az account list --refresh --output table
az account list --refresh --query "[?contains(name, 'Concierge Subscription')].id" --output table
az group list --output table
#az account set --subscription $SUBSCRIPTION

# Create Container Registry
az acr create --resource-group $RES_GROUP --name $ACR_NAME --sku Standard --location eastus

# Build image
pushd .\src
az acr build --resource-group $RES_GROUP --registry $ACR_NAME --image serviceregistry:v1 --file Dockerfile-serviceregistry .
az acr build --resource-group $RES_GROUP --registry $ACR_NAME --image echo:v1 --file Dockerfile-echo .
popd

<# Local image build for testing
docker image rm serviceregistry:v1
docker image rm echo:v1
docker build --pull --rm -f "src\Dockerfile-serviceregistry" -t serviceregistry:v1 "src"
docker build --pull --rm -f "src\Dockerfile-echo" -t echo:v1 "src"
#>

write-host "Creating key vault"
az keyvault create --resource-group $RES_GROUP --name $AKV_NAME
write-host "Create service principal, store its password in AKV (the registry *password*)"
az keyvault secret set --vault-name $AKV_NAME --name $ACR_NAME-pull-pwd `
    --value $(az ad sp create-for-rbac --name $ACR_NAME-pull `
        --scopes $(az acr show --name $ACR_NAME --query id --output tsv) `
        --role acrpull --query password --output tsv)
write-host "Store service principal ID in AKV (the registry *username*)"
az keyvault secret set --vault-name $AKV_NAME --name $ACR_NAME-pull-usr `
    --value $(az ad sp list --display-name $ACR_NAME-pull --query [].appId --output tsv)

write-host "Create container"
az container create --resource-group $RES_GROUP --name acr-tasks --image $ACR_NAME.azurecr.io/serviceregistry:v1 --registry-login-server $ACR_NAME.azurecr.io `
    --registry-username $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-pull-usr --query value -o tsv) `
    --registry-password $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-pull-pwd --query value -o tsv) `
    --dns-name-label acr-tasks-$ACR_NAME --query "{FQDN:ipAddress.fqdn}" --output table
az container create --resource-group $RES_GROUP --name acr-tasks --image $ACR_NAME.azurecr.io/echo:v1 --registry-login-server $ACR_NAME.azurecr.io `
    --registry-username $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-pull-usr --query value -o tsv) `
    --registry-password $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-pull-pwd --query value -o tsv) `
    --dns-name-label acr-tasks-$ACR_NAME --query "{FQDN:ipAddress.fqdn}" --output table
write-host "Attach to container"
az container attach --resource-group $RES_GROUP --name acr-tasks
