# Employee Management System

A comprehensive Employee Management System built with Blazor Server, DevExpress components, following Clean Architecture and SOLID principles, designed for deployment to Azure AKS.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

- **Domain Layer** (`EmployeeManagement.Domain`): Contains business entities, value objects, and domain interfaces
- **Application Layer** (`EmployeeManagement.Application`): Contains business logic, services, DTOs, and application interfaces
- **Infrastructure Layer** (`EmployeeManagement.Infrastructure`): Contains data access implementations and external services
- **Presentation Layer** (`EmployeeManagement.Web`): Blazor Server application with DevExpress UI components

## 🛠️ Technology Stack

- **.NET 9.0** - Latest framework
- **Blazor Server** - Server-side rendering with SignalR
- **DevExpress Blazor Components** - Rich UI controls (grids, forms, dialogs)
- **Entity Framework Core** - Data access (In-Memory for demo)
- **Docker** - Containerization with multi-stage builds
- **Kubernetes** - Orchestration for Azure AKS with session affinity
- **Azure Container Registry** - Private container registry
- **Azure DevOps** - CI/CD pipeline
- **xUnit** - Unit testing framework
- **Data Protection API** - Shared keys for multi-pod deployments

## 🚀 Features

- **Employee CRUD Operations**: Create, Read, Update, Delete employees with fully functional forms
- **Rich UI Components**: DevExpress grids, forms, charts, and navigation
- **Search and Filtering**: Filter by department, search by name/email/position
- **Dashboard**: Overview with statistics and recent hires
- **Responsive Design**: Bootstrap-based responsive layout
- **Mock Data**: 50 sample employees across 6 departments
- **Validation**: Client and server-side validation with DevExpress components
- **Clean Architecture**: Proper separation of concerns
- **SOLID Principles**: Dependency injection and interface segregation
- **Multi-Pod Support**: Data protection keys shared across Kubernetes pods
- **Session Affinity**: Client IP-based session persistence for Blazor Server

## 📁 Project Structure

```
EmployeeManagement/
├── src/
│   ├── EmployeeManagement.Domain/
│   │   ├── Entities/
│   │   │   └── Employee.cs
│   │   ├── ValueObjects/
│   │   │   └── Address.cs
│   │   └── Interfaces/
│   │       └── IEmployeeRepository.cs
│   ├── EmployeeManagement.Application/
│   │   ├── DTOs/
│   │   │   └── EmployeeDto.cs
│   │   ├── Interfaces/
│   │   │   └── IEmployeeService.cs
│   │   └── Services/
│   │       └── EmployeeService.cs
│   ├── EmployeeManagement.Infrastructure/
│   │   └── Repositories/
│   │       └── InMemoryEmployeeRepository.cs
│   └── EmployeeManagement.Web/
│       ├── Components/
│       │   ├── Pages/
│       │   │   ├── Home.razor
│       │   │   └── Employees.razor
│       │   └── Layout/
│       └── Program.cs
├── tests/
│   └── EmployeeManagement.Tests/
├── docker/
├── k8s/
│   ├── deployment.yaml
│   └── ingress.yaml
└── azure-pipelines.yml
```

## 🔧 Prerequisites

### Development Environment
- **Visual Studio 2022** or **Visual Studio Code**
- **.NET 9.0 SDK**
- **Docker Desktop**
- **Git**

### Azure Resources
- **Azure Subscription**
- **Azure Container Registry (ACR)**
- **Azure Kubernetes Service (AKS)**
- **Azure DevOps Organization**

### Tools to Install
```powershell
# Install Azure CLI
winget install Microsoft.AzureCLI

# Install kubectl
az aks install-cli

# Install Helm
winget install Helm.Helm

# Install Docker Desktop
winget install Docker.DockerDesktop
```

## 🏃‍♂️ Getting Started

### 1. Clone and Run Locally

```powershell
# Clone the repository
git clone <your-repo-url>
cd EmployeeManagement

# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project src/EmployeeManagement.Web
```

Navigate to `https://localhost:7071` to see the application.

### 2. Access Deployed Application

The application is deployed and accessible at:
- **LoadBalancer URL**: http://172.212.48.251
- **Ingress URL**: http://52.226.156.78

Both URLs provide full functionality including:
- Employee listing with DevExpress grid
- Add Employee dialog (fully functional)
- Edit Employee functionality
- Delete Employee with confirmation
- Search and filtering capabilities

### 2. Run Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🐳 Docker Deployment

### Build Docker Image

```powershell
# Build the Docker image (includes DevExpress NuGet configuration)
docker build -t employee-management:latest .

# Run the container
docker run -p 8080:8080 employee-management:latest
```

**Note**: The Dockerfile includes proper NuGet.Config setup for DevExpress packages and multi-stage build optimization.

## ☁️ Azure Infrastructure Setup

### 1. Create Azure Resources

```bash
# Set variables
RESOURCE_GROUP="rg-employee-management"
LOCATION="East US"
ACR_NAME="employeemanagementacr"
AKS_NAME="employee-management-aks"

# Create resource group
az group create --name $RESOURCE_GROUP --location "$LOCATION"

# Create Azure Container Registry
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic

# Create AKS cluster
az aks create \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_NAME \
  --node-count 2 \
  --enable-addons monitoring \
  --attach-acr $ACR_NAME \
  --generate-ssh-keys

# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_NAME
```

### 2. Install NGINX Ingress Controller

```bash
# Add NGINX Ingress Helm repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Install NGINX Ingress Controller
helm install ingress-nginx ingress-nginx/ingress-nginx \
  --create-namespace \
  --namespace ingress-nginx \
  --set controller.service.loadBalancerIP="" \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz
```

### 3. Install Cert-Manager (Optional)

```bash
# Add cert-manager Helm repository
helm repo add jetstack https://charts.jetstack.io
helm repo update

# Install cert-manager
helm install cert-manager jetstack/cert-manager \
  --namespace cert-manager \
  --create-namespace \
  --set installCRDs=true
```

## 🔄 Azure DevOps Setup

### 1. Create Azure DevOps Project

1. Go to [Azure DevOps](https://dev.azure.com)
2. Create a new project: "EmployeeManagement"
3. Initialize Git repository

### 2. Create Service Connections

#### Azure Container Registry Connection
1. Go to **Project Settings** > **Service connections**
2. Click **New service connection**
3. Select **Docker Registry**
4. Choose **Azure Container Registry**
5. Name: `EmployeeManagementACR`

#### Azure Kubernetes Service Connection
1. Create new service connection
2. Select **Kubernetes**
3. Choose **Azure Subscription**
4. Select your AKS cluster
5. Name: `EmployeeManagementAKS`

### 3. Configure Pipeline

1. Create a new pipeline from the `azure-pipelines.yml` file
2. Update the pipeline variables in the YAML file:
   - `containerRegistry`: Your ACR login server
   - `imageRepository`: Your image name
   - Update service connection names if different

### 4. Set Up Pipeline Variables

In Azure DevOps pipeline, set these variables:
- `dockerRegistryServiceConnection`: EmployeeManagementACR
- `kubernetesServiceConnection`: EmployeeManagementAKS
- `containerRegistry`: employeemanagementacr.azurecr.io

## 📦 Deployment Process

### Manual Deployment

```bash
# Build and push Docker image to ACR
az acr build --registry $ACR_NAME --image employee-management:latest .

# Deploy to AKS
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/ingress.yaml

# Check deployment status
kubectl get pods
kubectl get services
kubectl get ingress

# View application logs
kubectl logs -l app=employee-management --tail=50
```

**Current Deployment Status:**
- ✅ All pods running successfully
- ✅ LoadBalancer service accessible at: http://172.212.48.251
- ✅ Ingress controller accessible at: http://52.226.156.78
- ✅ Add Employee functionality fully operational
- ✅ Data protection keys shared across pods
- ✅ Session affinity configured for consistent user experience

### Automated Deployment

The Azure DevOps pipeline automatically:
1. **Builds** the .NET application
2. **Runs tests** and publishes coverage
3. **Builds** Docker image
4. **Pushes** to Azure Container Registry
5. **Deploys** to Azure Kubernetes Service

## 🧪 Testing Strategy

### Unit Tests
- Domain entity business logic
- Application service methods
- Repository implementations

### Integration Tests
- API endpoints
- Database interactions
- Service integrations

### Load Testing
```bash
# Install Artillery for load testing
npm install -g artillery

# Run load tests
artillery quick --count 10 --num 100 https://your-app-url
```

## 🔧 Configuration

### Kubernetes Multi-Pod Configuration

The application is configured for multi-pod deployments with:

```yaml
# Data protection volume mount for shared keys
volumeMounts:
- name: dataprotection-keys
  mountPath: /tmp/dataprotection-keys

# Session affinity for consistent user experience
sessionAffinity: ClientIP
sessionAffinityConfig:
  clientIP:
    timeoutSeconds: 3600
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | Production |
| `ASPNETCORE_URLS` | URLs to bind | http://+:8080 |

### Application Settings

Configure in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Data Protection Configuration

The application includes shared data protection keys for multi-pod Kubernetes deployments:

```csharp
// Program.cs configuration
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"))
    .SetApplicationName("EmployeeManagement");
```

This ensures that antiforgery tokens and form submissions work correctly across all pod instances.

## 📊 Monitoring and Observability

### Health Checks
The application includes health check endpoints:
- `/health` - Application health status

### Logging
- Structured logging with Serilog
- Application Insights integration
- Kubernetes logs via kubectl

### Metrics
- Custom application metrics
- Kubernetes resource metrics
- Azure Monitor integration

## 🔒 Security Considerations

### Authentication & Authorization
- Azure AD integration (ready for implementation)
- Role-based access control
- API key authentication for services

### Data Protection
- HTTPS enforcement
- Data encryption at rest
- Secure communication between services

## 🚀 Performance Optimization

### Current Deployment Status

**✅ FULLY OPERATIONAL** - All features are working correctly:

- **Application URLs**:
  - LoadBalancer: http://172.212.48.251
  - Ingress: http://52.226.156.78

- **Verified Functionality**:
  - ✅ Employee listing with DevExpress DxGrid
  - ✅ Add Employee dialog with validation
  - ✅ Edit Employee functionality
  - ✅ Delete Employee with confirmation
  - ✅ Search and department filtering
  - ✅ Responsive UI across devices
  - ✅ Form submissions working correctly
  - ✅ Multi-pod deployment stable (3 replicas)

### Blazor Server
- SignalR connection optimization
- Component lifecycle management
- Efficient state management

### Kubernetes
- Resource limits and requests
- Horizontal Pod Autoscaler
- Load balancing configuration

## 📈 Scalability

### Horizontal Scaling
```bash
# Scale deployment
kubectl scale deployment employee-management-deployment --replicas=5

# Enable autoscaling
kubectl autoscale deployment employee-management-deployment --cpu-percent=70 --min=2 --max=10
```

### Database Scaling
- Connection pooling
- Read replicas
- Caching strategies

## 🛠️ Development Guidelines

### Code Style
- Follow C# naming conventions
- Use explicit types over `var` when clarity needed
- Implement proper error handling
- Write comprehensive unit tests

### Git Workflow
1. Create feature branch from `develop`
2. Make changes following SOLID principles
3. Write/update tests
4. Submit pull request
5. Code review and merge

### DevExpress Components
- Utilize built-in validation with `<ValidationMessage>`
- Implement proper data binding with `@bind-Value`
- Follow DevExpress best practices for component lifecycle
- Optimize for performance with `InteractiveServer` render mode
- Use DevExpress popup dialogs for CRUD operations:
  - `DxPopup` for Add/Edit Employee dialogs
  - `DxGrid` for employee listing with built-in features
  - `DxButton`, `DxTextBox`, `DxComboBox` for form controls
  - `DxSpinEdit` for numeric inputs (salary)
  - `DxDateEdit` for date selection

## 🔍 Troubleshooting

### Common Issues

#### Add Employee Button Not Working
**RESOLVED**: This issue was caused by antiforgery token validation failures in multi-pod deployments. 

**Solution Applied**:
- ✅ Configured shared data protection keys
- ✅ Added Kubernetes volume mounts for key persistence
- ✅ Implemented session affinity for consistent user sessions
- ✅ Updated deployment with proper resource limits

#### Build Failures
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force
```

#### DevExpress Package Issues
```bash
# Ensure NuGet.Config is present with DevExpress feed
# The project includes proper configuration for DevExpress packages

# Verify NuGet sources
dotnet nuget list source
```

#### Docker Issues
```bash
# Check Docker daemon
docker version

# Build with verbose output
docker build --progress=plain -t employee-management .
```

#### Kubernetes Issues
```bash
# Check pod logs for antiforgery errors
kubectl logs -l app=employee-management --tail=100

# Describe deployment for resource issues
kubectl describe deployment employee-management-deployment

# Check ingress status
kubectl get ingress -o wide

# Verify service endpoints
kubectl get endpoints employee-management-service

# Check data protection volume mounts
kubectl describe pod <pod-name>
```

#### Form Submission Issues
If forms (Add/Edit Employee) are not working:
1. Check browser console for JavaScript errors
2. Verify antiforgery token in logs: `kubectl logs -l app=employee-management | grep -i antiforgery`
3. Ensure session affinity is working: `kubectl get svc employee-management-service -o yaml`

## 📚 Additional Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [DevExpress Blazor](https://docs.devexpress.com/Blazor/)
- [Azure Kubernetes Service](https://docs.microsoft.com/en-us/azure/aks/)
- [Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Team

- **Architecture**: Clean Architecture with SOLID principles
- **Frontend**: Blazor Server with DevExpress components
- **Backend**: .NET 9.0 Web API
- **Database**: Entity Framework Core
- **DevOps**: Azure DevOps with AKS deployment
- **Testing**: xUnit with code coverage
