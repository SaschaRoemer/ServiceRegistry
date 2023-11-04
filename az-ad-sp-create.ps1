# Create a service principal
#az account list --refresh --query "[?contains(name, 'Concierge Subscription')].id" --output table
az account set --subscription "0efa1d89-df92-47ed-903e-2dfcbc92ab9f"

az ad sp create-for-rbac --name ServiceRegistry-563e3594
