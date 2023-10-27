$RES_GROUP = "learn-7bed6760-8506-4ef5-b77b-354c93c52ef4"
$ACR_NAME = "acr330bd18b"
$AKV_NAME = "${ACR_NAME}-vault"
#az acr build --registry $ACR_NAME --image serviceregistry:v1 --file Dockerfile .

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
az container create --resource-group $RES_GROUP --name acr-tasks --image $ACR_NAME.azurecr.io/serviceregistry:v1 --registry-login-server $ACR_NAME.azurecr.io --registry-username $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-pull-usr --query value -o tsv) --registry-password $(az keyvault secret show --vault-name $AKV_NAME --name $ACR_NAME-pull-pwd --query value -o tsv) --dns-name-label acr-tasks-$ACR_NAME --query "{FQDN:ipAddress.fqdn}" --output table
write-host "Attach to container"
az container attach --resource-group $RES_GROUP --name acr-tasks
