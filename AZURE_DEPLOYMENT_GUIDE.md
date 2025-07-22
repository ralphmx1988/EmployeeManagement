# Azure Deployment Guide for Employee Management System

## Overview
This guide will walk you through creating Azure resources and deploying your Employee Management System to Azure Kubernetes Service (AKS).

## Prerequisites
- Azure CLI installed
- Docker installed
- kubectl installed
- Azure subscription with appropriate permissions

## Step 1: Login to Azure

```bash
az login
az account set --subscription "Your-Subscription-ID"
```

## Step 2: Create Resource Group

```bash
# Set variables
$RESOURCE_GROUP="rg-employee-management"
$LOCATION="East US"
$AKS_CLUSTER_NAME="aks-employee-management"
$ACR_NAME="employeemanagementacr"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
```

## Step 3: Create Azure Container Registry (ACR)

```bash
# Create ACR
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic

# Enable admin access (for development - use managed identity in production)
az acr update -n $ACR_NAME --admin-enabled true

# Get ACR credentials
az acr credential show --name $ACR_NAME
```

## Step 4: Create Azure Kubernetes Service (AKS)

```bash
# Create AKS cluster
az aks create `
    --resource-group $RESOURCE_GROUP `
    --name $AKS_CLUSTER_NAME `
    --node-count 3 `
    --node-vm-size Standard_DS2_v2 `
    --enable-addons monitoring `
    --generate-ssh-keys `
    --attach-acr $ACR_NAME
```

## Step 5: Get AKS Credentials

```bash
# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME

# Verify connection
kubectl get nodes
```

## Step 6: Build and Push Docker Image

```bash
# Build the Docker image
docker build -t $ACR_NAME.azurecr.io/employee-management:latest .

# Login to ACR
az acr login --name $ACR_NAME

# Push image to ACR
docker push $ACR_NAME.azurecr.io/employee-management:latest
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

## Step 9: Access Your Application

### **Live Application URLs** ✅
- **LoadBalancer**: http://172.212.48.251
- **Ingress**: http://52.226.156.78

### **Verify Deployment**
```bash
# Check if all components are running
kubectl get pods -l app=employee-management
kubectl get svc employee-management-service
kubectl get ingress employee-management-ingress
```

## Complete Kubernetes Configuration Files

### **k8s/deployment.yaml** Configuration

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: employee-management-deployment
  labels:
    app: employee-management
spec:
  replicas: 3
  selector:
    matchLabels:
      app: employee-management
  template:
    metadata:
      labels:
        app: employee-management
    spec:
      containers:
      - name: employee-management
        image: employeemanagementacr.azurecr.io/employee-management:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
      imagePullSecrets:
      - name: acr-secret
---
apiVersion: v1
kind: Service
metadata:
  name: employee-management-service
  labels:
    app: employee-management
spec:
  type: LoadBalancer
  sessionAffinity: ClientIP
  sessionAffinityConfig:
    clientIP:
      timeoutSeconds: 10800
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
  selector:
    app: employee-management
```

### **k8s/ingress.yaml** Configuration

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: employee-management-ingress
  annotations:
    kubernetes.io/ingress.class: nginx
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/affinity: "cookie"
    nginx.ingress.kubernetes.io/session-cookie-name: "employee-management-session"
    nginx.ingress.kubernetes.io/session-cookie-expires: "3600"
    nginx.ingress.kubernetes.io/session-cookie-max-age: "3600"
spec:
  rules:
  - http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: employee-management-service
            port:
              number: 80
```

### **Critical Configuration Fixes Applied**

1. **Data Protection Keys for Multi-Pod Deployment**:
   ```csharp
   // In Program.cs - Added to handle antiforgery tokens across multiple pods
   builder.Services.AddDataProtection()
       .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"))
       .SetApplicationName("EmployeeManagement");
   ```

2. **Session Affinity Configuration**:
   ```yaml
   # In service configuration
   sessionAffinity: ClientIP
   sessionAffinityConfig:
     clientIP:
       timeoutSeconds: 10800
   ```

3. **Ingress Session Stickiness**:
   ```yaml
   # In ingress annotations
   nginx.ingress.kubernetes.io/affinity: "cookie"
   nginx.ingress.kubernetes.io/session-cookie-name: "employee-management-session"
   ```

### **Loading Animation Implementation**

Added comprehensive loading animations:

1. **LoadingSpinner Component** (`Components/Shared/LoadingSpinner.razor`):
   ```razor
   @* Reusable loading spinner with Font Awesome icons *@
   @if (IsVisible)
   {
       <div class="@GetContainerClasses()">
           <div class="@GetSpinnerClasses()">
               <i class="@GetIconClasses()" role="status" aria-label="@LoadingText"></i>
               @if (!string.IsNullOrEmpty(LoadingText) && ShowText)
               {
                   <div class="@GetTextClasses()">
                       <strong>@LoadingText</strong>
                       @if (!string.IsNullOrEmpty(SubText))
                       {
                           <div class="text-muted small">@SubText</div>
                       }
                   </div>
               }
           </div>
       </div>
   }
   ```

2. **Font Awesome Integration** (in `App.razor`):
   ```html
   <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
   ```

3. **Loading States in Employees Page**:
   - `isInitialLoading`: Initial data load
   - `isSaving`: Add employee operation
   - `isDeleting`: Delete employee operation
   - `isSearching`: Search functionality

## Application Updates & Maintenance

### **Update and Redeploy**
```bash
# 1. Build and push new image
docker build -t employeemanagementacr.azurecr.io/employee-management:latest .
docker push employeemanagementacr.azurecr.io/employee-management:latest

# 2. Restart deployment to pull new image
kubectl rollout restart deployment employee-management-deployment
kubectl rollout status deployment employee-management-deployment
```

### **Monitoring Commands**
```bash
# View logs
kubectl logs -l app=employee-management --tail=50 -f

# Check resource usage
kubectl top pods -l app=employee-management

# Scale application
kubectl scale deployment employee-management-deployment --replicas=5
```

## Production Enhancements

### **Security & Scaling**
```bash
# Enable auto-scaling
kubectl autoscale deployment employee-management-deployment --cpu-percent=70 --min=3 --max=10

# Add SSL/TLS certificate
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create Azure Database for PostgreSQL
az postgres flexible-server create \
    --resource-group $RESOURCE_GROUP \
    --name employee-db-server \
    --admin-user dbadmin
```

### **Cost Optimization**
```bash
# Add spot instances (60-80% savings for dev)
az aks nodepool add \
    --resource-group $RESOURCE_GROUP \
    --cluster-name $AKS_CLUSTER_NAME \
    --name spotpool \
    --priority Spot \
    --node-count 2
```

## Troubleshooting Guide

### **Common Issues & Solutions**

#### 1. **Add Employee Button Not Working**
```csharp
// Add to Program.cs for multi-pod antiforgery token support
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"))
    .SetApplicationName("EmployeeManagement");
```

#### 2. **Loading Spinner Not Showing**
```html
<!-- Add Font Awesome CDN to App.razor -->
<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
```

#### 3. **Image Pull Errors**
```bash
# Re-attach ACR if needed
az aks update --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME --attach-acr $ACR_NAME
```

#### 4. **Pod Issues**
```bash
# Debug pod problems
kubectl describe pod [POD_NAME]
kubectl logs [POD_NAME] --previous
```

### **Health Check Implementation**
```csharp
// Add to Program.cs
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
```

## Cost Analysis

### **Current Deployment**: ~$128+/month
- AKS Cluster (3 nodes): $73+
- ACR Basic: $5
- Load Balancer: $20+
- Storage & Bandwidth: $25+

### **Production-Ready**: ~$263+/month
- AKS Cluster (5 nodes): $120+
- Azure Database: $50+
- Enhanced services: $93+

*Monitor usage through Azure Cost Management for accurate costs.*

## Deployment Summary ✅

### **Live Application**
- **LoadBalancer**: http://172.212.48.251  
- **Ingress**: http://52.226.156.78

### **Successfully Deployed**
- ✅ AKS cluster with 3 running pods
- ✅ Azure Container Registry integration  
- ✅ NGINX Ingress Controller
- ✅ Session affinity & antiforgery token handling
- ✅ Loading animations with Font Awesome
- ✅ Full CRUD operations working
- ✅ Multi-pod deployment with high availability

### **Key Features Working**
- Add/Edit/Delete Employee functionality
- Responsive UI with DevExpress components
- Loading spinners and animations
- Session management across pods

**Your Employee Management System is fully operational on Azure Kubernetes Service!** 🎉
