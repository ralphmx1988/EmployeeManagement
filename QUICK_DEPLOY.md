# Quick Deployment Guide 🚀

## Prerequisites Checklist
- ✅ Azure CLI installed (`az --version`)
- ✅ Docker installed (`docker --version`)  
- ✅ kubectl installed (`kubectl version --client`)
- ✅ Azure subscription access

## 🎯 Quick Start (5-Step Deployment)

### Step 1: Clone and Setup
```bash
git clone https://github.com/yourusername/EmployeeManagement.git
cd EmployeeManagement
```

### Step 2: Login to Azure
```bash
az login
az account set --subscription "Your-Subscription-Name"
```

### Step 3: Create Azure Resources (10-15 minutes)
```powershell
# Run in PowerShell
.\create-azure-resources.ps1
```

### Step 4: Build and Deploy Application
```powershell
# Build Docker image and deploy to AKS
.\deploy-to-aks.ps1
```

### Step 5: Access Your Application
```bash
# Get external IP
kubectl get svc employee-management-service

# Or check ingress
kubectl get ingress
```

## 🎯 Manual Steps (If you prefer step-by-step)

### 1. Create Resource Group
```bash
az group create --name "rg-employee-management" --location "East US"
```

### 2. Create Container Registry
```bash
# Create ACR (replace XXXX with random numbers)
az acr create --resource-group "rg-employee-management" --name "employeemanagementacrXXXX" --sku Basic
az acr update -n "employeemanagementacrXXXX" --admin-enabled true
```

### 3. Create AKS Cluster
```bash
# This takes 10-15 minutes
az aks create \
    --resource-group "rg-employee-management" \
    --name "aks-employee-management" \
    --node-count 2 \
    --node-vm-size "Standard_DS2_v2" \
    --enable-addons monitoring \
    --generate-ssh-keys \
    --attach-acr "employeemanagementacrXXXX"
```

### 4. Get Credentials
```bash
az aks get-credentials --resource-group "rg-employee-management" --name "aks-employee-management"
kubectl get nodes
```

### 5. Build and Push Image
```bash
# Get ACR login server
ACR_SERVER=$(az acr show --name "employeemanagementacrXXXX" --query "loginServer" -o tsv)

# Build and push
docker build -t $ACR_SERVER/employee-management:latest .
az acr login --name "employeemanagementacrXXXX"
docker push $ACR_SERVER/employee-management:latest
```

### 6. Update Kubernetes Files
Update `k8s/deployment.yaml` with your ACR server:
```yaml
containers:
- name: employee-management
  image: YOUR_ACR_SERVER.azurecr.io/employee-management:latest
```

### 7. Deploy to Kubernetes
```bash
# Install NGINX ingress controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

# Deploy application
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/ingress.yaml

# Check status
kubectl get pods
kubectl get svc
```

## 📊 Cost Estimation

| Resource | Monthly Cost (USD) |
|----------|-------------------|
| AKS (2 nodes) | ~$73 |
| ACR Basic | $5 |
| Load Balancer | $20 |
| Storage | $5 |
| **Total** | **~$103** |

## 🔧 Troubleshooting

### Common Issues:

1. **DevExpress Package Not Found (NU1101)**
   ```bash
   # If you get: "Unable to find package DevExpress.Blazor"
   # Solution: Ensure NuGet.Config exists with DevExpress feed
   # File should be at project root with content:
   ```
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
       <add key="DevExpress" value="https://nuget.devexpress.com/uJ3AkIcE85caGHTUScqihvRJeUjxPT1pEr48PFYcrmn8BSpOFl/api/v3/index.json" />
     </packageSources>
   </configuration>
   ```

2. **ACR Permission Errors**
   ```bash
   az aks check-acr --resource-group "rg-employee-management" --name "aks-employee-management" --acr "employeemanagementacrXXXX"
   ```

2. **Pod Not Starting**
   ```bash
   kubectl describe pod [POD_NAME]
   kubectl logs [POD_NAME]
   ```

3. **Service Not Accessible**
   ```bash
   kubectl get endpoints
   kubectl describe svc employee-management-service
   ```

4. **Ingress Issues**
   ```bash
   kubectl get ingress
   kubectl describe ingress employee-management-ingress
   ```

## 🎯 Azure DevOps Setup (CI/CD)

### Service Connections Needed:
1. **Docker Registry Connection**
   - Name: `EmployeeManagementACR`
   - Registry URL: `your-acr.azurecr.io`

2. **Kubernetes Service Connection**
   - Name: `EmployeeManagementAKS` 
   - Use service account token

### Pipeline Variables:
```yaml
containerRegistry: 'your-acr.azurecr.io'
imageRepository: 'employee-management'
```

## 📱 Application Features

Your deployed application includes:
- ✅ Employee CRUD operations
- ✅ DevExpress Blazor Grid with sorting/filtering  
- ✅ Modern Bootstrap UI
- ✅ 50 mock employees
- ✅ Clean Architecture implementation
- ✅ Responsive design

## 🌍 Access URLs

After deployment, access your app via:
- **LoadBalancer Service**: `http://[EXTERNAL-IP]`
- **Ingress**: `http://[INGRESS-IP]` 
- **Port Forward** (testing): `kubectl port-forward svc/employee-management-service 8080:80`

## 🔄 Update Deployment

To update your application:
```bash
# Build new image
docker build -t $ACR_SERVER/employee-management:v2 .
docker push $ACR_SERVER/employee-management:v2

# Update deployment
kubectl set image deployment/employee-management employee-management=$ACR_SERVER/employee-management:v2

# Check rollout
kubectl rollout status deployment/employee-management
```

## 🗑️ Cleanup Resources

To delete everything:
```bash
az group delete --name "rg-employee-management" --yes --no-wait
```

**Note**: This will delete ALL resources in the resource group!
