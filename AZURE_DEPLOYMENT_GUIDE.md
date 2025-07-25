# Employee Management Blazor App - Azure Deployment Guide

## Overview
This guide covers deploying the Employee Management Blazor application to Azure Kubernetes Service (AKS) for demonstration and production use.

## Architecture
- **Blazor Server Application**: ASP.NET Core with server-side rendering targeting .NET 9
- **Azure Kubernetes Service**: Container orchestration platform
- **Azure Container Registry**: Container image storage
- **Azure SQL Database**: Application database (optional)

## Prerequisites
- Azure CLI installed and configured
- Docker Desktop installed and running
- kubectl installed and configured
- .NET 9.0 SDK installed
- Azure subscription with appropriate permissions
- Visual Studio Code or Visual Studio (recommended)
- PowerShell 7+ (for scripts)

---

## Step 1: Login to Azure

```bash
az login
az account set --subscription "Your-Subscription-ID"
```

## Step 2: Create Resource Group

```bash
# Set variables for Azure demo environment
$RESOURCE_GROUP="rg-employee-management"
$LOCATION="East US"
$AKS_CLUSTER_NAME="aks-employee-management"
$ACR_NAME="employeemanagementacr"  # Must be globally unique

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
```

## Step 3: Create Azure Container Registry (ACR)

```bash
# Create ACR
az acr create --resource-group $RESOURCE_GROUP --name employeemanagementacr --sku Basic

# Enable admin access (for development - use managed identity in production)
az acr update -n $ACR_NAME --admin-enabled true

# Get ACR credentials
az acr credential show --name employeemanagementacr
```

## Step 4: Create Azure Kubernetes Service (AKS)

```bash
# Create AKS cluster with 3 nodes for demo
az aks create `
    --resource-group $RESOURCE_GROUP `
    --name $AKS_CLUSTER_NAME `
    --node-count 3 `            
    --node-vm-size Standard_DS2_v2 `
    --enable-addons monitoring `
    --generate-ssh-keys `
    --attach-acr $employeemanagementacr `
    --enable-cluster-autoscaler `
    --min-count 2 `
    --max-count 5
```

## Step 5: Get AKS Credentials

```bash
# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME

# Verify connection
kubectl get nodes
```

## Step 6: Build and Push Employee Management Image

```bash
# Build the Employee Management Docker image
# Build with a unique tag (e.g., timestamp or version)
$TAG = "v$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Write-Host $TAG
docker build -t employeemanagementacr.azurecr.io/employee-management:$TAG .


# Login to ACR
az acr login --name employeemanagementacr

# Push image to ACR
docker push employeemanagementacr.azurecr.io/employee-management:$TAG

# Verify image was pushed
az acr repository list --name employeemanagementacr --output table
```

## Step 7: Update Kubernetes Manifests

Update the `k8s/deployment.yaml` file with your ACR name:

```yaml
# In k8s/deployment.yaml, update the image reference
spec:
  containers:
  - name: employee-management
    image: employeemanagementacr.azurecr.io/employee-management:latest
```

## Step 8: Deploy to AKS

```bash
# Install NGINX Ingress Controller first
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

# Wait for ingress controller to be ready
kubectl wait --namespace ingress-nginx --for=condition=ready pod --selector=app.kubernetes.io/component=controller --timeout=120s

# Apply Kubernetes manifests
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/ingress.yaml

# Wait for pods to be ready
kubectl wait --for=condition=ready pod --selector=app=employee-management --timeout=300s

# Check deployment status
kubectl get pods -l app=employee-management
kubectl get svc employee-management-service
kubectl get ingress employee-management-ingress
```

## Step 9: Access Your Demo Application

```bash
# Get external IP addresses
kubectl get svc employee-management-service
kubectl get ingress employee-management-ingress -n ingress-nginx

# The application will be available at:
# LoadBalancer: http://[EXTERNAL-IP]
# Ingress: http://[INGRESS-IP]
```

## Step 10: Clean Up Resources

```bash
#Step 1: Delete Images from Container Registry
# PowerShell
# List and delete images from Azure Container Registry
Write-Host "=== Deleting Images from Azure Container Registry ===" -ForegroundColor Cyan

# Login to Azure (if not already logged in)
az login

# List all repositories in your ACR
$acrName = "employeemanagementacr"  # Replace with your ACR name
az acr repository list --name $acrName --output table

# Delete all images in a specific repository
$repositoryName = "employee-management"  # Replace with your repository name
az acr repository delete --name $acrName --repository $repositoryName --yes

# Or delete all repositories
az acr repository list --name $acrName --query "[].name" --output tsv | ForEach-Object {
    Write-Host "Deleting repository: $_" -ForegroundColor Yellow
    az acr repository delete --name $acrName --repository $_ --yes
}
```

## DELETE Local Images
```bash
# Delete all local Docker images
Write-Host "=== Cleaning Local Docker Images ===" -ForegroundColor Cyan

# Stop all running containers
docker stop $(docker ps -q) 2>$null

# Remove all containers
docker rm $(docker ps -aq) 2>$null

# Remove all images
docker rmi $(docker images -q) --force 2>$null

# Clean up Docker system
docker system prune -af --volumes

# Verify cleanup
Write-Host "Remaining Docker images:" -ForegroundColor Green
docker images
```

## Step 11: CLEANUP AKS

```bash
# Clean up existing AKS deployments
Write-Host "=== Cleaning AKS Deployments ===" -ForegroundColor Cyan

$resourceGroup = "rg-employee-management"      # Replace with your resource group
$clusterName = "aks-employee-management"    # Replace with your AKS cluster name
$namespace = "default"               # Replace with your namespace

# Get AKS credentials
az aks get-credentials --resource-group $resourceGroup --name $clusterName --overwrite-existing

# Delete all deployments in namespace
kubectl delete deployments --all -n $namespace

# Delete all services (except kubernetes service)
kubectl delete services --all -n $namespace --field-selector metadata.name!=kubernetes

# Delete all pods
kubectl delete pods --all -n $namespace

# Delete all configmaps and secrets (optional)
kubectl delete configmaps --all -n $namespace
kubectl delete secrets --all -n $namespace

# Verify cleanup
Write-Host "Current AKS resources:" -ForegroundColor Green
kubectl get all -n $namespace

```
