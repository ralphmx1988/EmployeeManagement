# Complete Fleet Management System - Implementation Guide

**Date**: July 24, 2025  
**Version**: 2.0  
**Author**: Fleet Management Development Team  

## 📋 Table of Contents

1. [System Overview](#system-overview)
2. [Architecture Components](#architecture-components)
3. [Ship VM Setup and Installation](#ship-vm-setup-and-installation)
4. [Shore Command Center Deployment](#shore-command-center-deployment)
5. [CruiseShip.UpdateAgent Installation](#cruiseshipupdateagent-installation)
6. [Fleet Container Registry Setup](#fleet-container-registry-setup)
7. [Deploying Updates to Fleet](#deploying-updates-to-fleet)
8. [Fleet Monitoring and Management](#fleet-monitoring-and-management)
9. [Troubleshooting Guide](#troubleshooting-guide)
10. [Security and Compliance](#security-and-compliance)
11. [Maintenance and Operations](#maintenance-and-operations)
12. [Offline Operation & Connectivity Management](#offline-operation--connectivity-management)
13. [Azure Integration and Scaling](#azure-integration-and-scaling)

---

## 🎯 System Overview

This comprehensive fleet management solution enables centralized management and automated updates across 25 cruise ships. The system combines cloud-based shore operations with robust offline capabilities for ship-based operations.

### Complete System Architecture
- **🏢 Shore Command Center**: ASP.NET Core Web API with real-time dashboard
- **🚢 Fleet of 25 Ships**: Independent VM-based deployments
- **🤖 CruiseShip.UpdateAgent**: .NET 9.0 Windows Service for automated updates
- **📦 Container Registry**: Centralized image distribution system
- **☁️ Azure Integration**: Demo environment and CI/CD pipeline
- **📊 Real-time Monitoring**: SignalR-based fleet monitoring
- **🔐 Enterprise Security**: JWT authentication and role-based access

### Key Benefits
- ✅ **Autonomous Operation**: Ships operate independently without shore connectivity
- ✅ **Centralized Management**: Single dashboard for entire fleet
- ✅ **Automated Updates**: Container updates during maintenance windows
- ✅ **Real-time Monitoring**: Live fleet health and status monitoring
- ✅ **Rollback Protection**: Automatic rollback on failed deployments
- ✅ **Enterprise Security**: Secure API communication and authentication
- ✅ **Azure Integration**: Cloud-based demo and development environment
- ✅ **Scalable Architecture**: Support for additional ships and applications

### Fleet Management Capabilities
- **Fleet Dashboard**: Real-time overview of all 25 ships
- **Deployment Orchestration**: Coordinated updates across multiple ships
- **Health Monitoring**: Continuous monitoring of ship systems and applications
- **Alert Management**: Automated alerts for critical issues
- **Maintenance Scheduling**: Automated deployment during maintenance windows
- **Reporting and Analytics**: Comprehensive fleet performance reporting

---

## 🏗️ Architecture Components

### 1. Shore Command Center (ASP.NET Core Web API)
- **Purpose**: Central fleet management and orchestration hub
- **Technology Stack**: .NET 9.0, Entity Framework Core, SignalR, Hangfire
- **Components**: 
  - Fleet Management API with comprehensive endpoints
  - Real-time monitoring dashboard with SignalR
  - Background job processing with Hangfire
  - JWT-based authentication and authorization
  - Fleet analytics and reporting system
- **Deployment**: Azure App Service or on-premises IIS
- **Database**: Azure SQL Database or SQL Server

### 2. CruiseShip.UpdateAgent (.NET Windows Service)
- **Purpose**: Ship-based automation and container management
- **Technology Stack**: .NET 9.0, Docker.DotNet, Windows Services
- **Components**:
  - UpdateOrchestrator for deployment coordination
  - DockerService for container lifecycle management
  - ShoreApiClient for secure shore communication
  - HealthMonitor for continuous system monitoring
  - ConfigurationManager for ship-specific settings
- **Deployment**: Windows Service on each ship VM
- **Features**: Offline operation, automatic rollback, health reporting

### 3. Ship VM Infrastructure
- **Purpose**: Local server environment for each cruise ship
- **Specifications**: 
  - Windows Server 2019/2022 (16GB RAM, 500GB SSD)
  - SQL Server Express/Standard for local databases
  - Docker Desktop for container orchestration
  - Dedicated network security configuration
- **Applications**: Employee Management System and custom ship applications
- **Data**: Local SQL Server databases with ship-specific data

### 4. Fleet Container Registry
- **Purpose**: Centralized container image distribution
- **Technology**: Azure Container Registry (Premium tier)
- **Features**:
  - Multi-region geo-replication for global fleet
  - Versioned container images with semantic versioning
  - Secure authentication for ship access
  - Automated image scanning and security policies
- **Integration**: Connected to CI/CD pipeline for automated deployments

### 5. Azure Demo Environment
- **Purpose**: Development and testing environment
- **Technology Stack**: Azure Kubernetes Service (AKS)
- **Components**:
  - Employee Management demo application
  - Azure Container Registry for demo images
  - Azure SQL Database for demo data
  - Application Gateway for secure access
- **CI/CD**: Azure DevOps pipeline with automated deployments

---

## 💻 Ship VM Setup and Installation

### Step 1: VM Specifications and Preparation

#### 1.1 Recommended Ship VM Specifications
**Production Environment Requirements:**
- **CPU**: 4 vCPU cores (Intel Xeon or AMD EPYC)
- **Memory**: 16 GB RAM (minimum 8 GB)
- **Storage**: 500 GB SSD (minimum 200 GB available)
- **Network**: Gigabit Ethernet with satellite internet capability
- **OS**: Windows Server 2022 (recommended) or Windows Server 2019

#### 1.2 Software Installation Sequence

**Install in this specific order:**

```powershell
# Step 1: Install .NET 9.0 Runtime and SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
# Verify installation
dotnet --version  # Should show 9.0.x

# Step 2: Install SQL Server 2022 Express (or Standard for larger ships)
# Download SQL Server 2022 Express
# Configure with Windows Authentication
# Enable TCP/IP protocol
# Set static port 1433

# Step 3: Install Docker Desktop for Windows
# Download from: https://www.docker.com/products/docker-desktop
# Enable both Windows and Linux container support
# Configure for automatic startup

# Step 4: Install PowerShell 7+
# Download from: https://github.com/PowerShell/PowerShell/releases
# Required for deployment scripts
```

#### 1.3 Network and Security Configuration

```powershell
# Configure Windows Firewall for ship operations
New-NetFirewallRule -DisplayName "Employee Management HTTP" -Direction Inbound -Port 80 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Employee Management HTTPS" -Direction Inbound -Port 443 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Shore Command API" -Direction Outbound -Port 443 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Port 1433 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Docker API" -Direction Inbound -Port 2375,2376 -Protocol TCP -Action Allow

# Configure services for automatic startup
Set-Service -Name "MSSQLSERVER" -StartupType Automatic
Set-Service -Name "Docker Desktop Service" -StartupType Automatic

# Create dedicated directories
New-Item -ItemType Directory -Path "C:\CruiseShip" -Force
New-Item -ItemType Directory -Path "C:\CruiseShip\Logs" -Force
New-Item -ItemType Directory -Path "C:\CruiseShip\Backups" -Force
New-Item -ItemType Directory -Path "C:\CruiseShip\Data" -Force
```

### Step 2: SQL Server Database Setup

#### 2.1 Create Employee Management Database

```sql
-- Connect to SQL Server Management Studio or use sqlcmd
-- Create database with proper configuration
CREATE DATABASE EmployeeManagement
ON (
    NAME = 'EmployeeManagement_Data',
    FILENAME = 'C:\CruiseShip\Data\EmployeeManagement.mdf',
    SIZE = 100MB,
    MAXSIZE = 10GB,
    FILEGROWTH = 10MB
)
LOG ON (
    NAME = 'EmployeeManagement_Log',
    FILENAME = 'C:\CruiseShip\Data\EmployeeManagement.ldf',
    SIZE = 10MB,
    MAXSIZE = 1GB,
    FILEGROWTH = 10%
);

-- Switch to the new database
USE EmployeeManagement;

-- Create enhanced tables for fleet management
CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId NVARCHAR(20) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Department NVARCHAR(100),
    Position NVARCHAR(100),
    HireDate DATE,
    Salary DECIMAL(10,2),
    IsActive BIT DEFAULT 1,
    ShipAssignment NVARCHAR(50),
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE()
);

-- Create ship metadata table
CREATE TABLE ShipMetadata (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShipId NVARCHAR(50) NOT NULL UNIQUE,
    ShipName NVARCHAR(100) NOT NULL,
    LastUpdateCheck DATETIME2,
    CurrentVersion NVARCHAR(50),
    SystemStatus NVARCHAR(20) DEFAULT 'Active',
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);

-- Insert ship-specific data (customize per ship)
INSERT INTO ShipMetadata (ShipId, ShipName, CurrentVersion)
VALUES ('SHIP-001', 'Ocean Explorer', '1.0.0');

-- Create sample employee data
INSERT INTO Employees (EmployeeId, FirstName, LastName, Email, Department, Position, HireDate, Salary, ShipAssignment)
VALUES 
    ('EMP001', 'John', 'Doe', 'john.doe@cruiseline.com', 'Engineering', 'Chief Engineer', '2024-01-15', 85000.00, 'SHIP-001'),
    ('EMP002', 'Jane', 'Smith', 'jane.smith@cruiseline.com', 'Navigation', 'First Officer', '2024-02-01', 75000.00, 'SHIP-001'),
    ('EMP003', 'Mike', 'Johnson', 'mike.johnson@cruiseline.com', 'Hospitality', 'Hotel Manager', '2024-03-10', 65000.00, 'SHIP-001'),
    ('EMP004', 'Sarah', 'Williams', 'sarah.williams@cruiseline.com', 'Medical', 'Ship Doctor', '2024-01-20', 95000.00, 'SHIP-001'),
    ('EMP005', 'David', 'Brown', 'david.brown@cruiseline.com', 'Security', 'Security Chief', '2024-02-15', 70000.00, 'SHIP-001');
```

#### 2.2 Configure Connection Strings

Create the configuration for ship-specific database connections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EmployeeManagement;Integrated Security=true;TrustServerCertificate=true;Application Name=EmployeeManagement-SHIP-001;",
    "HealthMonitoring": "Server=localhost\\SQLEXPRESS;Database=EmployeeManagement;Integrated Security=true;TrustServerCertificate=true;Application Name=HealthMonitor-SHIP-001;"
  },
  "DatabaseSettings": {
    "CommandTimeout": 30,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30"
  }
}
```

### Step 3: Docker Configuration

#### 3.1 Configure Docker for Ship Operations

```powershell
# Create Docker daemon configuration
$dockerConfig = @{
    "hosts" = @("tcp://0.0.0.0:2376", "npipe://")
    "log-driver" = "json-file"
    "log-opts" = @{
        "max-size" = "10m"
        "max-file" = "5"
    }
    "storage-driver" = "windowsfilter"
    "data-root" = "C:\CruiseShip\DockerData"
} | ConvertTo-Json

$dockerConfig | Out-File -FilePath "C:\ProgramData\Docker\config\daemon.json" -Encoding UTF8

# Restart Docker service
Restart-Service -Name "Docker Desktop Service"

# Verify Docker installation
docker version
docker info
```

#### 3.2 Prepare for Container Operations

```powershell
# Create Docker network for ship applications
docker network create --driver nat ship-network

# Test Docker functionality
docker run hello-world

# Clean up test container
docker system prune -f
```

---

## 🏢 Shore Command Center Deployment

### Step 1: Shore Command Center Architecture

The Shore Command Center is a comprehensive ASP.NET Core Web API application with real-time monitoring capabilities.

#### 1.1 Project Structure
```
src/ShoreCommandCenter/
├── Controllers/
│   ├── AuthController.cs           # JWT authentication
│   ├── ShipsController.cs          # Ship management
│   ├── DeploymentsController.cs    # Update deployments
│   ├── FleetController.cs          # Fleet overview
│   └── HealthController.cs         # Health monitoring
├── Services/
│   ├── ShipService.cs              # Ship operations
│   ├── DeploymentService.cs        # Deployment orchestration
│   ├── HealthMetricsService.cs     # Health data processing
│   ├── FleetService.cs             # Fleet-wide operations
│   ├── ContainerRegistryService.cs # Container management
│   └── NotificationService.cs      # Alert notifications
├── Hubs/
│   └── FleetMonitoringHub.cs       # SignalR real-time updates
├── Models/
│   ├── Ship.cs                     # Ship entity
│   ├── Deployment.cs               # Deployment entity
│   ├── HealthMetrics.cs            # Health metrics entity
│   └── DTOs/                       # Data transfer objects
├── Data/
│   └── ShoreCommandContext.cs      # Entity Framework context
└── BackgroundServices/
    ├── FleetHealthMonitor.cs       # Continuous health monitoring
    └── DeploymentOrchestrator.cs   # Deployment coordination
```

#### 1.2 Key API Endpoints

The Shore Command Center provides comprehensive REST API endpoints:

```csharp
// Controllers/ShipsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShipsController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterShip([FromBody] ShipRegistrationDto request)
    {
        var ship = await _shipService.RegisterShipAsync(request);
        
        // Notify dashboard of new ship registration
        await _hubContext.Clients.All.SendAsync("ShipRegistered", ship);
        
        return Ok(new { 
            Status = "registered", 
            ShipId = ship.ShipId,
            RegistrationTime = ship.RegisteredAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllShips()
    {
        var ships = await _shipService.GetAllShipsAsync();
        return Ok(ships);
    }

    [HttpGet("{shipId}")]
    public async Task<IActionResult> GetShip(string shipId)
    {
        var ship = await _shipService.GetShipByIdAsync(shipId);
        if (ship == null) return NotFound();
        return Ok(ship);
    }

    [HttpGet("{shipId}/health")]
    public async Task<IActionResult> GetShipHealth(string shipId)
    {
        var health = await _healthService.GetLatestHealthMetricsAsync(shipId);
        return Ok(health);
    }

    [HttpPost("{shipId}/health")]
    public async Task<IActionResult> ReceiveHealthMetrics(string shipId, [FromBody] HealthMetricsDto metrics)
    {
        await _healthService.StoreHealthMetricsAsync(shipId, metrics);
        
        // Real-time update to dashboard
        await _hubContext.Clients.All.SendAsync("HealthMetricsUpdated", new { shipId, metrics });
        
        return Ok();
    }

    [HttpGet("{shipId}/deployments/pending")]
    public async Task<IActionResult> GetPendingDeployments(string shipId)
    {
        var deployments = await _deploymentService.GetPendingDeploymentsAsync(shipId);
        return Ok(deployments);
    }
}

// Controllers/DeploymentsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeploymentsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateDeployment([FromBody] CreateDeploymentDto request)
    {
        var deployment = await _deploymentService.CreateDeploymentAsync(request);
        
        // Schedule deployment using Hangfire
        BackgroundJob.Enqueue<DeploymentOrchestrator>(x => x.ExecuteDeploymentAsync(deployment.Id));
        
        return CreatedAtAction(nameof(GetDeployment), new { id = deployment.Id }, deployment);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeployment(int id)
    {
        var deployment = await _deploymentService.GetDeploymentAsync(id);
        if (deployment == null) return NotFound();
        return Ok(deployment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDeployments([FromQuery] string? status = null, [FromQuery] string? shipId = null)
    {
        var deployments = await _deploymentService.GetDeploymentsAsync(status, shipId);
        return Ok(deployments);
    }

    [HttpPost("{id}/rollback")]
    public async Task<IActionResult> RollbackDeployment(int id)
    {
        await _deploymentService.RollbackDeploymentAsync(id);
        return Ok();
    }
}

// Controllers/FleetController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FleetController : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetFleetOverview()
    {
        var overview = await _fleetService.GetFleetOverviewAsync();
        return Ok(overview);
    }

    [HttpGet("health/summary")]
    public async Task<IActionResult> GetFleetHealthSummary()
    {
        var summary = await _fleetService.GetFleetHealthSummaryAsync();
        return Ok(summary);
    }

    [HttpPost("deployments/bulk")]
    public async Task<IActionResult> CreateBulkDeployment([FromBody] BulkDeploymentDto request)
    {
        var deployments = await _deploymentService.CreateBulkDeploymentAsync(request);
        return Ok(deployments);
    }
}
```

### Step 2: Database Configuration

#### 2.1 Entity Framework Models

```csharp
// Models/Ship.cs
public class Ship
{
    public int Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public ShipStatus Status { get; set; } = ShipStatus.Offline;
    public DateTime LastContact { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public bool IsMaintenanceMode { get; set; }
    
    // Navigation properties
    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
    public ICollection<HealthMetrics> HealthMetrics { get; set; } = new List<HealthMetrics>();
}

// Models/Deployment.cs
public class Deployment
{
    public int Id { get; set; }
    public string DeploymentId { get; set; } = string.Empty;
    public string ShipId { get; set; } = string.Empty;
    public string ContainerImage { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;
    public DeploymentPriority Priority { get; set; } = DeploymentPriority.Normal;
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool RequiresMaintenanceWindow { get; set; } = true;
    
    // Navigation properties
    public Ship Ship { get; set; } = null!;
}

// Models/HealthMetrics.cs
public class HealthMetrics
{
    public int Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskSpaceAvailableGB { get; set; }
    public string DockerStatus { get; set; } = string.Empty;
    public int RunningContainers { get; set; }
    public string SystemUptime { get; set; } = string.Empty;
    public bool NetworkConnectivity { get; set; }
    public DateTime LastUpdateCheck { get; set; }
    public string? AdditionalMetrics { get; set; } // JSON for extensibility
    
    // Navigation properties
    public Ship Ship { get; set; } = null!;
}
```

#### 2.2 Database Context Configuration

```csharp
// Data/ShoreCommandContext.cs
public class ShoreCommandContext : DbContext
{
    public ShoreCommandContext(DbContextOptions<ShoreCommandContext> options) : base(options) { }

    public DbSet<Ship> Ships { get; set; }
    public DbSet<Deployment> Deployments { get; set; }
    public DbSet<HealthMetrics> HealthMetrics { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ship configuration
        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShipId).IsUnique();
            entity.Property(e => e.ShipId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ShipName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CurrentVersion).HasMaxLength(50);
        });

        // Deployment configuration
        modelBuilder.Entity<Deployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DeploymentId).IsUnique();
            entity.Property(e => e.DeploymentId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ContainerImage).HasMaxLength(500).IsRequired();
            
            entity.HasOne(d => d.Ship)
                  .WithMany(p => p.Deployments)
                  .HasForeignKey(d => d.ShipId)
                  .HasPrincipalKey(p => p.ShipId);
        });

        // Health metrics configuration
        modelBuilder.Entity<HealthMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ShipId, e.Timestamp });
            
            entity.HasOne(h => h.Ship)
                  .WithMany(s => s.HealthMetrics)
                  .HasForeignKey(h => h.ShipId)
                  .HasPrincipalKey(s => s.ShipId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
```

### Step 3: Real-time Monitoring with SignalR

#### 3.1 Fleet Monitoring Hub

```csharp
// Hubs/FleetMonitoringHub.cs
[Authorize]
public class FleetMonitoringHub : Hub
{
    private readonly IFleetService _fleetService;
    private readonly ILogger<FleetMonitoringHub> _logger;

    public FleetMonitoringHub(IFleetService fleetService, ILogger<FleetMonitoringHub> logger)
    {
        _fleetService = fleetService;
        _logger = logger;
    }

    public async Task JoinFleetMonitoring()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "FleetMonitors");
        _logger.LogInformation("User {UserId} joined fleet monitoring", Context.UserIdentifier);
    }

    public async Task LeaveFleetMonitoring()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "FleetMonitors");
        _logger.LogInformation("User {UserId} left fleet monitoring", Context.UserIdentifier);
    }

    public async Task MonitorShip(string shipId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Ship-{shipId}");
        
        // Send current ship status immediately
        var ship = await _fleetService.GetShipStatusAsync(shipId);
        await Clients.Caller.SendAsync("ShipStatusUpdate", ship);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {UserId} disconnected from fleet monitoring", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}
```

### Step 4: Background Services

#### 4.1 Fleet Health Monitor

```csharp
// BackgroundServices/FleetHealthMonitor.cs
public class FleetHealthMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FleetHealthMonitor> _logger;
    private readonly IHubContext<FleetMonitoringHub> _hubContext;

    public FleetHealthMonitor(
        IServiceProvider serviceProvider,
        ILogger<FleetHealthMonitor> logger,
        IHubContext<FleetMonitoringHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var fleetService = scope.ServiceProvider.GetRequiredService<IFleetService>();
                
                // Check fleet health status
                var fleetHealth = await fleetService.GetFleetHealthSummaryAsync();
                
                // Broadcast to all connected clients
                await _hubContext.Clients.Group("FleetMonitors")
                    .SendAsync("FleetHealthUpdate", fleetHealth, stoppingToken);
                
                // Check for critical alerts
                await CheckAndSendAlertsAsync(fleetService, stoppingToken);
                
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in fleet health monitoring");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CheckAndSendAlertsAsync(IFleetService fleetService, CancellationToken cancellationToken)
    {
        var criticalAlerts = await fleetService.GetCriticalAlertsAsync();
        
        foreach (var alert in criticalAlerts)
        {
            await _hubContext.Clients.Group("FleetMonitors")
                .SendAsync("CriticalAlert", alert, cancellationToken);
        }
    }
}
```

### Step 5: Deployment Configuration

#### 5.1 Program.cs Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<ShoreCommandContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add SignalR
builder.Services.AddSignalR();

// Add Hangfire for background jobs
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Add application services
builder.Services.AddScoped<IShipService, ShipService>();
builder.Services.AddScoped<IDeploymentService, DeploymentService>();
builder.Services.AddScoped<IHealthMetricsService, HealthMetricsService>();
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<IContainerRegistryService, ContainerRegistryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add background services
builder.Services.AddHostedService<FleetHealthMonitor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure SignalR hub
app.MapHub<FleetMonitoringHub>("/fleetHub");

// Configure Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();

app.Run();
```

---

## 🤖 CruiseShip.UpdateAgent Installation

### Step 1: Understanding the Update Agent Architecture

The CruiseShip.UpdateAgent is a sophisticated .NET 9.0 Windows Service that manages container lifecycle and maintains communication with the Shore Command Center.

#### 1.1 Update Agent Components

```
CruiseShip.UpdateAgent/
├── Services/
│   ├── UpdateOrchestrator.cs       # Main orchestration logic
│   ├── DockerService.cs            # Docker container management
│   ├── ShoreApiClient.cs           # Shore communication
│   ├── HealthMonitor.cs            # System health monitoring
│   ├── ConfigurationManager.cs    # Configuration management
│   └── BackupService.cs            # Container backup/restore
├── Models/
│   ├── UpdateRequest.cs            # Update request model
│   ├── HealthMetrics.cs            # Health metrics model
│   ├── DeploymentResult.cs         # Deployment result model
│   └── ShipConfiguration.cs       # Ship-specific configuration
├── Configuration/
│   ├── appsettings.json            # Base configuration
│   ├── appsettings.Production.json # Production overrides
│   └── ship-config.template.json   # Ship-specific template
└── Scripts/
    ├── install-service.ps1         # Service installation
    ├── uninstall-service.ps1       # Service removal
    └── verify-installation.ps1     # Installation verification
```

### Step 2: Build and Package Update Agent

#### 2.1 Build Process

```powershell
# Navigate to the UpdateAgent project directory
cd c:\EmployeeManagement\CruiseShip.UpdateAgent

# Clean previous builds
dotnet clean

# Restore packages
dotnet restore

# Build for production
dotnet build --configuration Release --runtime win-x64 --self-contained true

# Publish self-contained deployment
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output bin\Release\Publish
```

#### 2.2 Create Deployment Package

```powershell
# Create deployment structure
$deploymentPath = "C:\temp\CruiseShip-UpdateAgent-Deploy"
New-Item -ItemType Directory -Path $deploymentPath -Force

# Copy published files
Copy-Item -Path "bin\Release\Publish\*" -Destination $deploymentPath -Recurse

# Copy configuration templates
Copy-Item -Path "Configuration\*" -Destination "$deploymentPath\Configuration" -Force

# Copy installation scripts
Copy-Item -Path "Scripts\*" -Destination "$deploymentPath\Scripts" -Force

# Create deployment package
Compress-Archive -Path "$deploymentPath\*" -DestinationPath "C:\temp\CruiseShip-UpdateAgent.zip" -Force

Write-Host "✅ Deployment package created: C:\temp\CruiseShip-UpdateAgent.zip"
```

### Step 3: Ship-Specific Configuration

#### 3.1 Create Ship Configuration File

For each ship, create a customized configuration file based on the template:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Extensions.Hosting": "Warning",
      "CruiseShip.UpdateAgent": "Information"
    },
    "EventLog": {
      "LogLevel": {
        "Default": "Warning"
      },
      "LogName": "Application",
      "SourceName": "CruiseShipUpdateAgent"
    },
    "File": {
      "Path": "C:\\CruiseShip\\Logs\\updateagent-{Date}.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 30
    }
  },
  "ShipConfiguration": {
    "ShipId": "OCEAN_EXPLORER_001",
    "ShipName": "Ocean Explorer",
    "ShoreApiUrl": "https://shore-command.cruiseline.com",
    "ApiKey": "sk-ship-001-secure-key-here",
    "ContainerRegistry": {
      "Url": "fleetregistry.azurecr.io",
      "Username": "fleetregistry",
      "Password": "registry-password-here"
    },
    "OperationSettings": {
      "UpdateCheckIntervalMinutes": 5,
      "HealthReportIntervalMinutes": 2,
      "HeartbeatIntervalMinutes": 1,
      "OfflineQueueMaxSize": 1000,
      "BackupRetentionDays": 7,
      "MaxConcurrentOperations": 2
    },
    "MaintenanceWindows": [
      {
        "Name": "Primary Window",
        "Days": ["Sunday", "Wednesday"],
        "StartTime": "02:00",
        "EndTime": "04:00",
        "TimeZone": "UTC",
        "AllowEmergencyUpdates": true
      },
      {
        "Name": "Secondary Window", 
        "Days": ["Saturday"],
        "StartTime": "14:00",
        "EndTime": "16:00",
        "TimeZone": "America/New_York",
        "AllowEmergencyUpdates": false
      }
    ],
    "ContainerSettings": {
      "DefaultNetwork": "ship-network",
      "DataVolumePath": "C:\\CruiseShip\\Data",
      "LogVolumePath": "C:\\CruiseShip\\Logs",
      "BackupPath": "C:\\CruiseShip\\Backups",
      "MaxImagePullRetries": 3,
      "ImagePullTimeoutMinutes": 30
    },
    "HealthMonitoring": {
      "EnableCpuMonitoring": true,
      "EnableMemoryMonitoring": true,
      "EnableDiskMonitoring": true,
      "EnableNetworkMonitoring": true,
      "CpuThresholdPercent": 85,
      "MemoryThresholdPercent": 90,
      "DiskSpaceThresholdGB": 10
    },
    "Security": {
      "RequireHttps": true,
      "ValidateCertificates": true,
      "EncryptLocalData": true,
      "ApiTimeoutSeconds": 30
    }
  },
  "Docker": {
    "ApiUrl": "npipe://./pipe/docker_engine",
    "ApiVersion": "1.41",
    "DefaultTimeout": "00:05:00"
  }
}
```

#### 3.2 Ship-Specific Environment Variables

```powershell
# Set ship-specific environment variables
[Environment]::SetEnvironmentVariable("CRUISE_SHIP_ID", "OCEAN_EXPLORER_001", "Machine")
[Environment]::SetEnvironmentVariable("CRUISE_SHIP_NAME", "Ocean Explorer", "Machine")
[Environment]::SetEnvironmentVariable("SHORE_API_URL", "https://shore-command.cruiseline.com", "Machine")

# Verify environment variables
Get-ChildItem Env: | Where-Object Name -like "CRUISE_*"
```

### Step 4: Installation on Ship VMs

#### 4.1 Installation Script

Create an automated installation script for each ship:

```powershell
# Scripts/install-ship-agent.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ShipId,
    
    [Parameter(Mandatory=$true)]
    [string]$ShipName,
    
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\CruiseShip\UpdateAgent"
)

Write-Host "🚢 Installing CruiseShip UpdateAgent for $ShipName ($ShipId)"

try {
    # Create installation directory
    if (!(Test-Path $InstallPath)) {
        New-Item -ItemType Directory -Path $InstallPath -Force
        Write-Host "✅ Created installation directory: $InstallPath"
    }

    # Copy application files
    Copy-Item -Path ".\*" -Destination $InstallPath -Recurse -Force -Exclude @("Scripts", "Configuration")
    Write-Host "✅ Copied application files"

    # Configure ship-specific settings
    $configPath = Join-Path $InstallPath "appsettings.Production.json"
    $configTemplate = Get-Content "Configuration\ship-config.template.json" -Raw
    $configContent = $configTemplate -replace "{{SHIP_ID}}", $ShipId -replace "{{SHIP_NAME}}", $ShipName
    
    $configContent | Out-File -FilePath $configPath -Encoding UTF8
    Write-Host "✅ Created ship-specific configuration"

    # Install Windows Service
    $serviceName = "CruiseShipUpdateAgent"
    $serviceDisplayName = "CruiseShip Update Agent - $ShipName"
    $servicePath = Join-Path $InstallPath "CruiseShip.UpdateAgent.exe"

    # Remove existing service if it exists
    $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Host "⚠️ Stopping existing service..."
        Stop-Service -Name $serviceName -Force
        sc.exe delete $serviceName
        Start-Sleep -Seconds 5
    }

    # Create new service
    New-Service -Name $serviceName `
                -DisplayName $serviceDisplayName `
                -BinaryPathName $servicePath `
                -Description "Manages container updates and health monitoring for $ShipName" `
                -StartupType Automatic `
                -Credential (Get-Credential -Message "Enter service account credentials")

    Write-Host "✅ Windows Service installed successfully"

    # Set service recovery options
    sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

    # Create required directories
    @("C:\CruiseShip\Logs", "C:\CruiseShip\Backups", "C:\CruiseShip\Data") | ForEach-Object {
        if (!(Test-Path $_)) {
            New-Item -ItemType Directory -Path $_ -Force
            Write-Host "✅ Created directory: $_"
        }
    }

    # Set permissions
    icacls "C:\CruiseShip" /grant "NT SERVICE\$serviceName:(OI)(CI)F" /T

    # Start the service
    Start-Service -Name $serviceName
    Start-Sleep -Seconds 10

    # Verify installation
    $service = Get-Service -Name $serviceName
    if ($service.Status -eq "Running") {
        Write-Host "✅ Service is running successfully"
        
        # Test health endpoint
        try {
            $healthResponse = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing -TimeoutSec 30
            if ($healthResponse.StatusCode -eq 200) {
                Write-Host "✅ Health endpoint is responding"
            }
        } catch {
            Write-Host "⚠️ Health endpoint not yet available (normal during startup)"
        }
    } else {
        Write-Host "❌ Service failed to start. Check Event Log for details."
        exit 1
    }

    Write-Host ""
    Write-Host "🎉 CruiseShip UpdateAgent installed successfully!"
    Write-Host "Service Name: $serviceName"
    Write-Host "Installation Path: $InstallPath"
    Write-Host "Configuration: $configPath"
    Write-Host ""
    Write-Host "📊 Monitor the service:"
    Write-Host "  - Event Viewer: Applications and Services Logs → CruiseShipUpdateAgent"
    Write-Host "  - Service Status: Get-Service '$serviceName'"
    Write-Host "  - Logs: C:\CruiseShip\Logs\"

} catch {
    Write-Host "❌ Installation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

#### 4.2 Installation Process for Each Ship

```powershell
# On each ship VM, run as Administrator:

# 1. Extract deployment package
Expand-Archive -Path "CruiseShip-UpdateAgent.zip" -DestinationPath "C:\temp\UpdateAgent" -Force

# 2. Navigate to extracted files
cd C:\temp\UpdateAgent

# 3. Run installation script with ship-specific parameters
.\Scripts\install-ship-agent.ps1 -ShipId "OCEAN_EXPLORER_001" -ShipName "Ocean Explorer"

# 4. Verify installation
Get-Service "CruiseShipUpdateAgent"
Get-EventLog -LogName Application -Source "CruiseShipUpdateAgent" -Newest 5
```

### Step 5: Post-Installation Verification

#### 5.1 Service Verification

```powershell
# Check service status
$service = Get-Service -Name "CruiseShipUpdateAgent"
Write-Host "Service Status: $($service.Status)"
Write-Host "Service Start Type: $($service.StartType)"

# Check service dependencies
sc.exe qc CruiseShipUpdateAgent

# Verify service can start/stop
Stop-Service -Name "CruiseShipUpdateAgent"
Start-Sleep -Seconds 5
Start-Service -Name "CruiseShipUpdateAgent"
Start-Sleep -Seconds 10

# Check final status
Get-Service -Name "CruiseShipUpdateAgent"
```

#### 5.2 Connectivity Verification

```powershell
# Test shore connectivity (when internet available)
$shoreUrl = "https://shore-command.cruiseline.com"
try {
    $response = Invoke-WebRequest -Uri "$shoreUrl/api/health" -UseBasicParsing
    Write-Host "✅ Shore Command Center connectivity: OK ($($response.StatusCode))"
} catch {
    Write-Host "⚠️ Shore Command Center not reachable (expected when offline): $($_.Exception.Message)"
}

# Test Docker connectivity
try {
    docker version
    Write-Host "✅ Docker connectivity: OK"
} catch {
    Write-Host "❌ Docker connectivity: FAILED"
}

# Test local health endpoint
try {
    $healthResponse = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing
    Write-Host "✅ UpdateAgent health endpoint: OK"
} catch {
    Write-Host "⚠️ UpdateAgent health endpoint not available yet"
}
```

#### 5.3 Log Verification

```powershell
# Check Windows Event Log
Get-EventLog -LogName Application -Source "CruiseShipUpdateAgent" -Newest 10 | 
    Select-Object TimeGenerated, EntryType, Message

# Check file logs
if (Test-Path "C:\CruiseShip\Logs") {
    Get-ChildItem "C:\CruiseShip\Logs" -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | 
        ForEach-Object { Get-Content $_.FullName -Tail 20 }
}

# Check agent configuration
$configPath = "C:\CruiseShip\UpdateAgent\appsettings.Production.json"
if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    Write-Host "Ship ID: $($config.ShipConfiguration.ShipId)"
    Write-Host "Ship Name: $($config.ShipConfiguration.ShipName)"
    Write-Host "Shore API URL: $($config.ShipConfiguration.ShoreApiUrl)"
}
```

---

## 📦 Fleet Container Registry Setup

### Step 1: Azure Container Registry Configuration

The fleet uses Azure Container Registry (Premium tier) to distribute container images across all ships with global geo-replication support.

#### 1.1 Create Fleet Container Registry

```powershell
# Set variables for fleet container registry
$FLEET_RESOURCE_GROUP = "rg-fleet-container-registry"
$FLEET_ACR_NAME = "fleetregistrycruiseline"  # Must be globally unique
$FLEET_LOCATION = "East US"
$SECONDARY_LOCATIONS = @("West Europe", "Southeast Asia", "Australia East")

# Create resource group
az group create --name $FLEET_RESOURCE_GROUP --location $FLEET_LOCATION

# Create Premium ACR (required for geo-replication)
az acr create `
    --resource-group $FLEET_RESOURCE_GROUP `
    --name $FLEET_ACR_NAME `
    --sku Premium `
    --location $FLEET_LOCATION

# Enable admin access for initial setup (will be replaced with managed identities)
az acr update -n $FLEET_ACR_NAME --admin-enabled true

# Configure geo-replication for global fleet coverage
foreach ($location in $SECONDARY_LOCATIONS) {
    Write-Host "Adding replication in $location..."
    az acr replication create `
        --registry $FLEET_ACR_NAME `
        --location $location
}

# Verify replication status
az acr replication list --registry $FLEET_ACR_NAME --output table
```

#### 1.2 Configure Registry Security

```powershell
# Create service principal for ship fleet access
$sp = az ad sp create-for-rbac `
    --name "CruiseShipFleet" `
    --scopes $(az acr show --name $FLEET_ACR_NAME --query id --output tsv) `
    --role acrpull `
    --output json | ConvertFrom-Json

Write-Host "Service Principal Created:"
Write-Host "  App ID: $($sp.appId)"
Write-Host "  Password: $($sp.password)"
Write-Host "  Tenant: $($sp.tenant)"

# Create tokens for ship authentication (alternative to service principal)
az acr token create `
    --name ship-fleet-token `
    --registry $FLEET_ACR_NAME `
    --scope-map _repositories_pull `
    --status enabled

# Create scope map for specific repositories if needed
az acr scope-map create `
    --name ship-applications `
    --registry $FLEET_ACR_NAME `
    --repository employee-management content/read `
    --repository ship-systems content/read
```

### Step 2: Container Image Management

#### 2.1 Image Versioning Strategy

```powershell
# Employee Management image versioning
$BASE_IMAGE = "$FLEET_ACR_NAME.azurecr.io/employee-management"
$VERSION = "2.1.0"
$BUILD_NUMBER = "20250724.1"

# Tag strategy:
# - latest: Always points to the most recent stable version
# - semver: Semantic versioning (2.1.0)
# - build: Build-specific version (2.1.0-20250724.1)
# - environment: Environment-specific tags (2.1.0-prod, 2.1.0-staging)

docker tag employee-management:local "$BASE_IMAGE:latest"
docker tag employee-management:local "$BASE_IMAGE:$VERSION"
docker tag employee-management:local "$BASE_IMAGE:$VERSION-$BUILD_NUMBER"
docker tag employee-management:local "$BASE_IMAGE:$VERSION-prod"
```

#### 2.2 Automated Image Building and Publishing

```powershell
# Build script for automated image creation
# build-and-publish.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "prod",
    
    [Parameter(Mandatory=$false)]
    [string]$BuildNumber = (Get-Date -Format "yyyyMMdd.HHmm")
)

$ErrorActionPreference = "Stop"

try {
    Write-Host "🔨 Building Employee Management v$Version for $Environment"
    
    # Build the application
    cd src/EmployeeManagement.Web
    dotnet clean
    dotnet restore
    dotnet build --configuration Release
    
    # Build Docker image
    $imageName = "$FLEET_ACR_NAME.azurecr.io/employee-management"
    docker build -t "employee-management:local" .
    
    # Apply all tags
    $tags = @(
        "$imageName:latest",
        "$imageName:$Version",
        "$imageName:$Version-$BuildNumber",
        "$imageName:$Version-$Environment"
    )
    
    foreach ($tag in $tags) {
        docker tag "employee-management:local" $tag
        Write-Host "✅ Tagged: $tag"
    }
    
    # Login to ACR
    az acr login --name $FLEET_ACR_NAME
    
    # Push all tags
    foreach ($tag in $tags) {
        Write-Host "📤 Pushing: $tag"
        docker push $tag
    }
    
    # Scan for vulnerabilities
    Write-Host "🔍 Scanning image for vulnerabilities..."
    az acr task run `
        --registry $FLEET_ACR_NAME `
        --name vulnerability-scan `
        --set image="$imageName:$Version"
    
    Write-Host "🎉 Successfully built and published Employee Management v$Version"
    
    # Create manifest for fleet deployment
    $manifest = @{
        version = $Version
        buildNumber = $BuildNumber
        environment = $Environment
        images = @{
            "employee-management" = "$imageName:$Version"
        }
        createdAt = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
        changelog = "Updated Employee Management System with latest features and security patches"
    } | ConvertTo-Json -Depth 3
    
    $manifest | Out-File -FilePath "fleet-deployment-manifest-$Version.json" -Encoding UTF8
    Write-Host "📋 Created deployment manifest: fleet-deployment-manifest-$Version.json"
    
} catch {
    Write-Host "❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

### Step 3: Ship Registry Configuration

#### 3.1 Configure Ships for Registry Access

Each ship needs to be configured to authenticate with the fleet registry:

```json
{
  "ContainerRegistry": {
    "Url": "fleetregistrycruiseline.azurecr.io",
    "Authentication": {
      "Type": "ServicePrincipal",
      "ClientId": "00000000-0000-0000-0000-000000000000",
      "ClientSecret": "service-principal-secret",
      "TenantId": "00000000-0000-0000-0000-000000000000"
    },
    "Settings": {
      "PullTimeoutMinutes": 30,
      "MaxRetries": 3,
      "RetryDelaySeconds": 30,
      "UseLocalCache": true,
      "CacheExpirationHours": 24
    },
    "PreferredRegions": [
      "East US",
      "West Europe", 
      "Southeast Asia"
    ]
  }
}
```

#### 3.2 Registry Client Implementation

```csharp
// Services/ContainerRegistryService.cs in CruiseShip.UpdateAgent
public class ContainerRegistryService
{
    private readonly ILogger<ContainerRegistryService> _logger;
    private readonly ShipConfiguration _config;
    private readonly DockerClient _dockerClient;

    public async Task<bool> PullImageAsync(string imageName, string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullImageName = $"{_config.ContainerRegistry.Url}/{imageName}:{tag}";
            _logger.LogInformation("Pulling image: {ImageName}", fullImageName);

            // Authenticate with registry
            var authConfig = new AuthConfig
            {
                Username = _config.ContainerRegistry.Authentication.ClientId,
                Password = _config.ContainerRegistry.Authentication.ClientSecret,
                ServerAddress = _config.ContainerRegistry.Url
            };

            // Pull image with progress tracking
            var pullProgress = new Progress<JSONMessage>(message =>
            {
                if (!string.IsNullOrEmpty(message.Status))
                {
                    _logger.LogDebug("Pull progress: {Status} {Progress}", 
                        message.Status, message.ProgressMessage);
                }
            });

            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = fullImageName,
                    Tag = tag
                },
                authConfig,
                pullProgress,
                cancellationToken);

            _logger.LogInformation("Successfully pulled image: {ImageName}", fullImageName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pull image: {ImageName}:{Tag}", imageName, tag);
            return false;
        }
    }

    public async Task<bool> IsImageAvailableAsync(string imageName, string tag)
    {
        try
        {
            var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["reference"] = new Dictionary<string, bool>
                    {
                        [$"{_config.ContainerRegistry.Url}/{imageName}:{tag}"] = true
                    }
                }
            });

            return images.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check image availability: {ImageName}:{Tag}", imageName, tag);
            return false;
        }
    }
}
```

### Step 4: Registry Monitoring and Management

#### 4.1 Registry Health Monitoring

```powershell
# Monitor registry health and usage
function Get-RegistryHealth {
    param($RegistryName)
    
    Write-Host "📊 Fleet Container Registry Health Report" -ForegroundColor Green
    Write-Host "Registry: $RegistryName" -ForegroundColor Yellow
    Write-Host ""
    
    # Get registry info
    $registry = az acr show --name $RegistryName --output json | ConvertFrom-Json
    Write-Host "✅ Status: $($registry.status)"
    Write-Host "✅ SKU: $($registry.sku.name)"
    Write-Host "✅ Location: $($registry.location)"
    
    # Get replication status
    Write-Host ""
    Write-Host "🌍 Geo-Replication Status:" -ForegroundColor Yellow
    az acr replication list --registry $RegistryName --output table
    
    # Get repository list
    Write-Host ""
    Write-Host "📦 Repositories:" -ForegroundColor Yellow
    az acr repository list --name $RegistryName --output table
    
    # Get recent activity
    Write-Host ""
    Write-Host "📈 Recent Activity:" -ForegroundColor Yellow
    az acr task list-runs --registry $RegistryName --top 5 --output table
    
    # Storage usage
    Write-Host ""
    Write-Host "💾 Storage Usage:" -ForegroundColor Yellow
    az acr show-usage --name $RegistryName --output table
}

# Run health check
Get-RegistryHealth -RegistryName $FLEET_ACR_NAME
```

#### 4.2 Automated Registry Cleanup

```powershell
# Cleanup old images to manage storage costs
function Remove-OldImages {
    param(
        $RegistryName,
        $Repository,
        $KeepLatest = 10,
        $OlderThanDays = 30
    )
    
    Write-Host "🧹 Cleaning up old images in $Repository"
    
    # Get image manifests older than specified days
    $cutoffDate = (Get-Date).AddDays(-$OlderThanDays).ToString("yyyy-MM-dd")
    
    # List all tags
    $tags = az acr repository show-tags --name $RegistryName --repository $Repository --output json | ConvertFrom-Json
    
    # Sort by creation date and remove old ones
    $tagsToDelete = $tags | 
        Sort-Object createdTime | 
        Select-Object -SkipLast $KeepLatest |
        Where-Object { $_.createdTime -lt $cutoffDate }
    
    foreach ($tag in $tagsToDelete) {
        Write-Host "🗑️ Deleting tag: $($tag.name)"
        az acr repository delete --name $RegistryName --image "$Repository:$($tag.name)" --yes
    }
    
    # Run garbage collection
    Write-Host "♻️ Running garbage collection..."
    az acr run --registry $RegistryName --cmd "acr purge --ago 30d --keep 10 --untagged" /dev/null
}

# Schedule cleanup for employee management images
Remove-OldImages -RegistryName $FLEET_ACR_NAME -Repository "employee-management" -KeepLatest 15 -OlderThanDays 60
```

### Step 5: Integration with Shore Command Center

#### 5.1 Registry API Integration

```csharp
// Services/ContainerRegistryService.cs in Shore Command Center
public class ContainerRegistryService
{
    public async Task<List<ContainerImageInfo>> GetAvailableImagesAsync(string repository)
    {
        var images = new List<ContainerImageInfo>();
        
        // Get all tags for the repository
        var tags = await GetRepositoryTagsAsync(repository);
        
        foreach (var tag in tags)
        {
            var manifest = await GetImageManifestAsync(repository, tag);
            images.Add(new ContainerImageInfo
            {
                Repository = repository,
                Tag = tag,
                Digest = manifest.Digest,
                CreatedAt = manifest.CreatedAt,
                Size = manifest.Size,
                Architecture = manifest.Architecture,
                Vulnerabilities = await ScanImageAsync(repository, tag)
            });
        }
        
        return images.OrderByDescending(i => i.CreatedAt).ToList();
    }

    public async Task<DeploymentPackage> CreateDeploymentPackageAsync(string repository, string tag)
    {
        var imageInfo = await GetImageInfoAsync(repository, tag);
        
        return new DeploymentPackage
        {
            Id = Guid.NewGuid().ToString(),
            ContainerImage = $"{_registryUrl}/{repository}:{tag}",
            Version = tag,
            CreatedAt = DateTime.UtcNow,
            Metadata = new DeploymentMetadata
            {
                ImageDigest = imageInfo.Digest,
                Size = imageInfo.Size,
                Vulnerabilities = imageInfo.Vulnerabilities,
                ChangeLog = await GetChangeLogAsync(repository, tag)
            }
        };
    }
}
```

This Fleet Container Registry setup provides a robust, scalable solution for distributing container images across your 25-ship fleet with proper security, monitoring, and automation capabilities.

## 🚀 Deploying Updates to Fleet

### Step 1: Fleet Deployment Architecture

The fleet deployment system orchestrates updates across 25 ships using a sophisticated deployment pipeline with rollback capabilities and maintenance window scheduling.

#### 1.1 Deployment Types

**Standard Deployment**
- Scheduled during maintenance windows
- Gradual rollout across fleet
- Automatic rollback on failure
- Full testing and validation

**Emergency Deployment**
- Immediate deployment when ships are online
- Override maintenance windows
- Critical security patches
- Accelerated validation

**Pilot Deployment**
- Deploy to selected pilot ships first
- Validate before fleet-wide rollout
- Risk mitigation strategy
- A/B testing capabilities

#### 1.2 Deployment Orchestration Components

```csharp
// Services/DeploymentService.cs
public class DeploymentService : IDeploymentService
{
    private readonly ShoreCommandContext _context;
    private readonly IContainerRegistryService _registryService;
    private readonly IHubContext<FleetMonitoringHub> _hubContext;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<DeploymentService> _logger;

    public async Task<Deployment> CreateFleetDeploymentAsync(CreateFleetDeploymentDto request)
    {
        _logger.LogInformation("Creating fleet deployment for {ImageName} targeting {ShipCount} ships", 
            request.ContainerImage, request.TargetShips?.Count ?? 0);

        var deployment = new Deployment
        {
            DeploymentId = GenerateDeploymentId(),
            ContainerImage = request.ContainerImage,
            Version = request.Version,
            Priority = request.Priority,
            RequiresMaintenanceWindow = request.RequiresMaintenanceWindow,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            Status = DeploymentStatus.Pending
        };

        // Determine target ships
        var targetShips = await DetermineTargetShipsAsync(request);
        
        // Create individual ship deployments
        foreach (var ship in targetShips)
        {
            var shipDeployment = new ShipDeployment
            {
                DeploymentId = deployment.DeploymentId,
                ShipId = ship.ShipId,
                Status = DeploymentStatus.Pending,
                ScheduledAt = CalculateDeploymentTime(ship, request),
                CreatedAt = DateTime.UtcNow
            };
            
            deployment.ShipDeployments.Add(shipDeployment);
        }

        _context.Deployments.Add(deployment);
        await _context.SaveChangesAsync();

        // Schedule deployment execution
        await ScheduleDeploymentAsync(deployment);

        // Notify dashboard
        await _hubContext.Clients.All.SendAsync("DeploymentCreated", deployment);

        return deployment;
    }

    private async Task ScheduleDeploymentAsync(Deployment deployment)
    {
        foreach (var shipDeployment in deployment.ShipDeployments)
        {
            if (deployment.Priority == DeploymentPriority.Emergency)
            {
                // Execute immediately
                _taskQueue.QueueBackgroundWorkItem(async token =>
                    await ExecuteShipDeploymentAsync(shipDeployment.Id, token));
            }
            else
            {
                // Schedule for maintenance window
                BackgroundJob.Schedule<DeploymentOrchestrator>(
                    x => x.ExecuteShipDeploymentAsync(shipDeployment.Id),
                    shipDeployment.ScheduledAt ?? DateTime.UtcNow);
            }
        }
    }
}
```

### Step 2: Creating Deployment Packages

#### 2.1 Deployment Package Structure

```json
{
  "deploymentId": "deploy-emp-mgmt-v2.1.0-20250724",
  "metadata": {
    "version": "2.1.0",
    "createdAt": "2025-07-24T10:30:00Z",
    "createdBy": "fleet.admin@cruiseline.com",
    "description": "Employee Management System v2.1.0 - Enhanced reporting and performance improvements",
    "priority": "normal",
    "requiresMaintenanceWindow": true,
    "estimatedDurationMinutes": 20,
    "rollbackOnFailure": true
  },
  "targetConfiguration": {
    "ships": ["SHIP-001", "SHIP-002", "SHIP-003"],
    "deploymentStrategy": "rolling",
    "maxConcurrentDeployments": 5,
    "deploymentGroups": [
      {
        "name": "pilot-group",
        "ships": ["SHIP-001"],
        "delayMinutes": 0
      },
      {
        "name": "main-fleet",
        "ships": ["SHIP-002", "SHIP-003"],
        "delayMinutes": 60
      }
    ]
  },
  "containerUpdates": [
    {
      "containerName": "employee-management-web",
      "currentImage": "fleetregistry.azurecr.io/employee-management:2.0.0",
      "newImage": "fleetregistry.azurecr.io/employee-management:2.1.0",
      "healthCheck": {
        "endpoint": "/health",
        "expectedStatusCode": 200,
        "timeoutSeconds": 30,
        "retryCount": 3
      },
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "Logging__LogLevel__Default": "Information",
        "FeatureFlags__EnableNewReporting": "true"
      },
      "volumeMounts": [
        {
          "hostPath": "C:\\CruiseShip\\Data\\EmployeeManagement",
          "containerPath": "/app/data"
        },
        {
          "hostPath": "C:\\CruiseShip\\Logs\\EmployeeManagement",
          "containerPath": "/app/logs"
        }
      ],
      "portMappings": [
        {
          "hostPort": 80,
          "containerPort": 8080,
          "protocol": "tcp"
        },
        {
          "hostPort": 443,
          "containerPort": 8443,
          "protocol": "tcp"
        }
      ],
      "resourceLimits": {
        "memory": "2GB",
        "cpu": "1.0"
      }
    }
  ],
  "preDeploymentTasks": [
    {
      "name": "database-backup",
      "type": "script",
      "script": "backup-database.ps1",
      "timeoutMinutes": 10,
      "criticalTask": true
    },
    {
      "name": "health-check",
      "type": "http",
      "url": "http://localhost/api/health",
      "expectedStatus": 200,
      "timeoutSeconds": 30
    }
  ],
  "postDeploymentTasks": [
    {
      "name": "application-warmup",
      "type": "script", 
      "script": "warmup-application.ps1",
      "timeoutMinutes": 5
    },
    {
      "name": "health-verification",
      "type": "http",
      "url": "http://localhost/api/health",
      "expectedStatus": 200,
      "timeoutSeconds": 60,
      "retryCount": 5
    },
    {
      "name": "smoke-tests",
      "type": "script",
      "script": "run-smoke-tests.ps1",
      "timeoutMinutes": 10
    }
  ],
  "rollbackConfiguration": {
    "enabled": true,
    "automaticRollback": true,
    "rollbackTriggers": [
      "health-check-failure",
      "deployment-timeout",
      "post-deployment-task-failure"
    ],
    "rollbackTimeoutMinutes": 15
  }
}
```

#### 2.2 Deployment Scripts

**Pre-deployment Script (backup-database.ps1):**
```powershell
param(
    [string]$BackupPath = "C:\CruiseShip\Backups",
    [string]$DatabaseName = "EmployeeManagement"
)

$ErrorActionPreference = "Stop"

try {
    Write-Host "📦 Creating database backup before deployment..."
    
    # Create backup directory if it doesn't exist
    if (!(Test-Path $BackupPath)) {
        New-Item -ItemType Directory -Path $BackupPath -Force
    }
    
    # Generate backup filename with timestamp
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupFile = Join-Path $BackupPath "$DatabaseName-pre-deployment-$timestamp.bak"
    
    # Create backup
    $backupCommand = @"
BACKUP DATABASE [$DatabaseName] 
TO DISK = '$backupFile'
WITH FORMAT, INIT, 
    NAME = 'Full Backup of $DatabaseName - Pre-deployment $timestamp',
    COMPRESSION;
"@
    
    sqlcmd -S "localhost\SQLEXPRESS" -Q $backupCommand -l 300
    
    if (Test-Path $backupFile) {
        $backupSize = (Get-Item $backupFile).Length / 1MB
        Write-Host "✅ Database backup completed successfully"
        Write-Host "   File: $backupFile"
        Write-Host "   Size: $([math]::Round($backupSize, 2)) MB"
        
        # Store backup info for potential rollback
        $backupInfo = @{
            BackupFile = $backupFile
            DatabaseName = $DatabaseName
            Timestamp = $timestamp
            Size = $backupSize
        } | ConvertTo-Json
        
        $backupInfo | Out-File -FilePath (Join-Path $BackupPath "latest-backup-info.json") -Encoding UTF8
        
        exit 0
    } else {
        Write-Host "❌ Backup file was not created" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Database backup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

**Post-deployment Script (warmup-application.ps1):**
```powershell
param(
    [string]$ApplicationUrl = "http://localhost",
    [int]$WarmupTimeoutMinutes = 5
)

$ErrorActionPreference = "Stop"

try {
    Write-Host "🔥 Warming up application after deployment..."
    
    $timeout = (Get-Date).AddMinutes($WarmupTimeoutMinutes)
    $warmupEndpoints = @(
        "$ApplicationUrl/",
        "$ApplicationUrl/api/health",
        "$ApplicationUrl/api/employees",
        "$ApplicationUrl/employees"
    )
    
    # Wait for application to start
    Write-Host "⏳ Waiting for application to start..."
    do {
        try {
            $response = Invoke-WebRequest -Uri "$ApplicationUrl/api/health" -UseBasicParsing -TimeoutSec 10
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Application is responding"
                break
            }
        } catch {
            Write-Host "⏳ Application not ready yet, waiting..."
            Start-Sleep -Seconds 10
        }
    } while ((Get-Date) -lt $timeout)
    
    if ((Get-Date) -ge $timeout) {
        Write-Host "❌ Application failed to start within timeout period" -ForegroundColor Red
        exit 1
    }
    
    # Warm up critical endpoints
    foreach ($endpoint in $warmupEndpoints) {
        try {
            Write-Host "🌡️ Warming up: $endpoint"
            $response = Invoke-WebRequest -Uri $endpoint -UseBasicParsing -TimeoutSec 30
            Write-Host "   Status: $($response.StatusCode)"
        } catch {
            Write-Host "   Warning: Failed to warm up $endpoint - $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    
    Write-Host "✅ Application warmup completed successfully"
    exit 0
    
} catch {
    Write-Host "❌ Application warmup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

### Step 3: Fleet Deployment Dashboard

#### 3.1 Shore Command Center Dashboard

```razor
@* Pages/Deployments/FleetDashboard.razor *@
@page "/deployments/fleet"
@inject IJSRuntime JSRuntime
@inject NavigationManager Navigation
@implements IAsyncDisposable

<PageTitle>Fleet Deployment Dashboard</PageTitle>

<div class="container-fluid">
    <div class="row mb-4">
        <div class="col-12">
            <h1>🚀 Fleet Deployment Dashboard</h1>
            <p class="text-muted">Monitor and manage deployments across your 25-ship fleet</p>
        </div>
    </div>

    <!-- Fleet Overview Cards -->
    <div class="row mb-4">
        <div class="col-md-3">
            <div class="card bg-primary text-white">
                <div class="card-body">
                    <h5 class="card-title">Total Ships</h5>
                    <h2>@fleetOverview.TotalShips</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-success text-white">
                <div class="card-body">
                    <h5 class="card-title">Online Ships</h5>
                    <h2>@fleetOverview.OnlineShips</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-info text-white">
                <div class="card-body">
                    <h5 class="card-title">Active Deployments</h5>
                    <h2>@fleetOverview.ActiveDeployments</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-warning text-white">
                <div class="card-body">
                    <h5 class="card-title">Pending Updates</h5>
                    <h2>@fleetOverview.PendingUpdates</h2>
                </div>
            </div>
        </div>
    </div>

    <!-- Active Deployments -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h5>🔄 Active Deployments</h5>
                </div>
                <div class="card-body">
                    @if (activeDeployments.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-striped">
                                <thead>
                                    <tr>
                                        <th>Deployment ID</th>
                                        <th>Container Image</th>
                                        <th>Target Ships</th>
                                        <th>Progress</th>
                                        <th>Status</th>
                                        <th>Started</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var deployment in activeDeployments)
                                    {
                                        <tr>
                                            <td>
                                                <a href="/deployments/@deployment.Id">@deployment.DeploymentId</a>
                                            </td>
                                            <td>@deployment.ContainerImage</td>
                                            <td>@deployment.TargetShipCount ships</td>
                                            <td>
                                                <div class="progress">
                                                    <div class="progress-bar" 
                                                         style="width: @(deployment.ProgressPercent)%"
                                                         role="progressbar">
                                                        @deployment.ProgressPercent%
                                                    </div>
                                                </div>
                                            </td>
                                            <td>
                                                <span class="badge badge-@GetStatusColor(deployment.Status)">
                                                    @deployment.Status
                                                </span>
                                            </td>
                                            <td>@deployment.StartedAt?.ToString("MM/dd HH:mm")</td>
                                            <td>
                                                <button class="btn btn-sm btn-outline-danger" 
                                                        @onclick="() => CancelDeployment(deployment.Id)">
                                                    Cancel
                                                </button>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                    else
                    {
                        <p class="text-muted text-center">No active deployments</p>
                    }
                </div>
            </div>
        </div>
    </div>

    <!-- Ship Status Grid -->
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h5>🚢 Fleet Status</h5>
                    <button class="btn btn-primary" @onclick="CreateNewDeployment">
                        Create New Deployment
                    </button>
                </div>
                <div class="card-body">
                    <div class="row">
                        @foreach (var ship in ships)
                        {
                            <div class="col-md-4 col-lg-3 mb-3">
                                <div class="card ship-status-card @GetShipStatusClass(ship.Status)">
                                    <div class="card-body">
                                        <h6 class="card-title">@ship.ShipName</h6>
                                        <p class="card-text">
                                            <small>ID: @ship.ShipId</small><br>
                                            <small>Version: @ship.CurrentVersion</small><br>
                                            <small>Last Contact: @ship.LastContact.ToString("MM/dd HH:mm")</small>
                                        </p>
                                        <span class="badge badge-@GetStatusColor(ship.Status)">
                                            @ship.Status
                                        </span>
                                        @if (ship.HasPendingDeployment)
                                        {
                                            <span class="badge badge-warning ml-1">Pending Update</span>
                                        }
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private FleetOverview fleetOverview = new();
    private List<ActiveDeployment> activeDeployments = new();
    private List<ShipStatus> ships = new();
    private HubConnection? hubConnection;

    protected override async Task OnInitializedAsync()
    {
        await LoadFleetDataAsync();
        await InitializeSignalRAsync();
    }

    private async Task LoadFleetDataAsync()
    {
        // Load fleet overview, active deployments, and ship statuses
        // Implementation would call Shore Command Center APIs
    }

    private async Task InitializeSignalRAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/fleetHub"))
            .Build();

        hubConnection.On<FleetOverview>("FleetOverviewUpdate", async (overview) =>
        {
            fleetOverview = overview;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<ActiveDeployment>("DeploymentUpdate", async (deployment) =>
        {
            var existingDeployment = activeDeployments.FirstOrDefault(d => d.Id == deployment.Id);
            if (existingDeployment != null)
            {
                var index = activeDeployments.IndexOf(existingDeployment);
                activeDeployments[index] = deployment;
            }
            else
            {
                activeDeployments.Add(deployment);
            }
            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
    }

    private string GetStatusColor(string status) => status switch
    {
        "Online" or "Completed" or "Success" => "success",
        "Offline" or "Failed" or "Error" => "danger",
        "InProgress" or "Deploying" => "primary",
        "Pending" or "Scheduled" => "warning",
        _ => "secondary"
    };

    private string GetShipStatusClass(string status) => status switch
    {
        "Online" => "border-success",
        "Offline" => "border-danger", 
        "Deploying" => "border-primary",
        _ => "border-secondary"
    };

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

### Step 4: Deployment API Endpoints

#### 4.1 Fleet Deployment API

```csharp
// Controllers/FleetDeploymentsController.cs
[ApiController]
[Route("api/fleet/deployments")]
[Authorize(Roles = "FleetManager,Administrator")]
public class FleetDeploymentsController : ControllerBase
{
    private readonly IDeploymentService _deploymentService;
    private readonly IFleetService _fleetService;

    [HttpPost]
    public async Task<IActionResult> CreateFleetDeployment([FromBody] CreateFleetDeploymentDto request)
    {
        var deployment = await _deploymentService.CreateFleetDeploymentAsync(request);
        return CreatedAtAction(nameof(GetDeployment), new { id = deployment.Id }, deployment);
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveDeployments()
    {
        var deployments = await _deploymentService.GetActiveDeploymentsAsync();
        return Ok(deployments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeployment(int id)
    {
        var deployment = await _deploymentService.GetDeploymentWithDetailsAsync(id);
        if (deployment == null) return NotFound();
        return Ok(deployment);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelDeployment(int id)
    {
        await _deploymentService.CancelDeploymentAsync(id);
        return Ok();
    }

    [HttpPost("{id}/rollback")]
    public async Task<IActionResult> RollbackDeployment(int id)
    {
        await _deploymentService.RollbackDeploymentAsync(id);
        return Ok();
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetFleetDeploymentOverview()
    {
        var overview = await _fleetService.GetDeploymentOverviewAsync();
        return Ok(overview);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkDeployment([FromBody] BulkDeploymentDto request)
    {
        var deployments = await _deploymentService.CreateBulkDeploymentAsync(request);
        return Ok(deployments);
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleDeployment([FromBody] ScheduleDeploymentDto request)
    {
        var deployment = await _deploymentService.ScheduleDeploymentAsync(request);
        return CreatedAtAction(nameof(GetDeployment), new { id = deployment.Id }, deployment);
    }
}
```

This comprehensive fleet deployment system provides sophisticated orchestration capabilities for managing container updates across your entire cruise ship fleet with proper monitoring, rollback capabilities, and maintenance window scheduling.

---

## 📊 Monitoring and Management

### Ship-Level Monitoring

#### Health Metrics Collected
```json
{
  "shipId": "OCEAN_EXPLORER_001",
  "timestamp": "2025-07-23T10:30:00Z",
  "cpuUsagePercent": 45.2,
  "memoryUsagePercent": 67.8,
  "diskSpaceAvailableGB": 128.5,
  "dockerStatus": "running",
  "runningContainers": [
    {
      "id": "abc123",
      "name": "employee-management-web",
      "image": "myregistry/employee-management:v2.1.0",
      "status": "running",
      "healthStatus": "healthy",
      "uptime": "2.05:30:15"
    }
  ],
  "systemUptime": "15.08:45:30",
  "networkConnectivity": true,
  "lastUpdateCheck": "2025-07-23T10:25:00Z",
  "lastSuccessfulUpdate": "2025-07-21T03:15:00Z"
}
```

#### Local Ship Management

```powershell
# Check Update Agent status
Get-Service "CruiseShip Update Agent"

# View Update Agent logs
Get-EventLog -LogName Application -Source "CruiseShip.UpdateAgent" -Newest 20

# Check Docker containers
docker ps -a

# Check Employee Management app
docker logs employee-management-web

# Manual update check (emergency)
Restart-Service "CruiseShip Update Agent"
```

### Shore Command Monitoring

#### Fleet Dashboard Features
- 🗺️ **Fleet Map**: Geographic view of all ships
- 📊 **Health Dashboard**: System health across all ships
- 🔄 **Update Status**: Current update deployment status
- 📈 **Analytics**: Performance trends and statistics
- 🚨 **Alerts**: Critical issues requiring attention

#### API Endpoints for Monitoring
```http
GET /api/fleet/overview
GET /api/ships/{shipId}/health/current
GET /api/ships/{shipId}/health/history?days=7
GET /api/updates/active
GET /api/alerts/critical
```

---

## 🔧 Troubleshooting Guide

### Common Issues and Solutions

#### 1. Update Agent Won't Start

**Symptoms:**
- Service shows "Stopped" status
- No logs in Event Viewer

**Solutions:**
```powershell
# Check configuration file
Test-Path "C:\CruiseShip\UpdateAgent\appsettings.Production.json"

# Verify .NET 9.0 is installed
dotnet --version

# Check service account permissions
sc qc "CruiseShip Update Agent"

# Try running manually for debugging
cd C:\CruiseShip\UpdateAgent
.\CruiseShip.UpdateAgent.exe
```

#### 2. Docker Connection Failed

**Symptoms:**
- "Docker is not running" in health reports
- Container operations fail

**Solutions:**
```powershell
# Check Docker Desktop is running
Get-Service -Name "Docker Desktop Service"

# Restart Docker Desktop
Restart-Service -Name "Docker Desktop Service"

# Test Docker CLI
docker version
docker ps

# Check Docker API access
Test-NetConnection -ComputerName localhost -Port 2375
```

#### 3. Shore Command Unreachable

**Symptoms:**
- "No internet connection" warnings
- Updates not received

**Solutions:**
```powershell
# Test shore command connectivity
Test-NetConnection -ComputerName "shore-command.cruiseline.com" -Port 443

# Check API key configuration
# Verify firewall settings
# Check proxy settings if applicable

# Manual connectivity test
Invoke-WebRequest -Uri "https://shore-command.cruiseline.com/api/health"
```

**Note**: This is normal behavior when ships are at sea without internet. The system will:
- Continue running all applications normally
- Queue health metrics and status updates locally
- Automatically reconnect and sync when internet becomes available
- Apply any pending updates during the next maintenance window with connectivity

#### 4. Container Update Failed

**Symptoms:**
- Update status shows "failed"
- Containers not updated to new version

**Solutions:**
```powershell
# Check Docker image availability
docker pull myregistry/employee-management:v2.1.0

# Check container logs
docker logs employee-management-web

# Review update agent logs
Get-EventLog -LogName Application -Source "CruiseShip.UpdateAgent" -EntryType Error -Newest 10

# Manual rollback if needed
docker stop employee-management-web
docker rm employee-management-web
# Restore from backup
```

#### 5. Database Connection Issues

**Symptoms:**
- Application can't connect to database
- SQL Server connection errors

**Solutions:**
```powershell
# Check SQL Server service
Get-Service MSSQLSERVER

# Test database connection
sqlcmd -S localhost -E -Q "SELECT @@VERSION"

# Check connection string in container
docker exec employee-management-web env | grep ConnectionStrings

# Verify database exists
sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases WHERE name = 'EmployeeManagement'"
```

---

## 🔐 Security Considerations

### Network Security

#### Firewall Configuration
```powershell
# Ship VM Firewall Rules
New-NetFirewallRule -DisplayName "Shore Command HTTPS" -Direction Outbound -Port 443 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Employee Management HTTP" -Direction Inbound -Port 80 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Employee Management HTTPS" -Direction Inbound -Port 443 -Protocol TCP -Action Allow

# Block unnecessary outbound traffic
New-NetFirewallRule -DisplayName "Block Unknown Outbound" -Direction Outbound -Action Block -RemoteAddress Any
```

#### API Security
- **API Keys**: Unique per ship, rotated regularly
- **HTTPS Only**: All shore communication encrypted
- **Authentication**: Bearer token or API key authentication
- **Rate Limiting**: Prevent API abuse

### Data Security

#### Sensitive Data Handling
```json
{
  "ApiKey": "sk-ship-001-abc123...",  // Store in secure configuration
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeManagement;Trusted_Connection=true;"
  }
}
```

#### Backup Security
```powershell
# Encrypt backups
$backupPath = "C:\Backups\EmployeeDB_encrypted.bak"
sqlcmd -Q "BACKUP DATABASE EmployeeManagement TO DISK = '$backupPath' WITH ENCRYPTION (ALGORITHM = AES_256, SERVER_CERTIFICATE = BackupCert)"
```

### Access Control

#### Service Account Security
```powershell
# Create dedicated service account
New-LocalUser -Name "CruiseShipAgent" -Password $securePassword -Description "CruiseShip Update Agent Service Account"

# Grant minimal required permissions
Add-LocalGroupMember -Group "Log on as a service" -Member "CruiseShipAgent"

# Install service with specific account
sc create "CruiseShip Update Agent" binPath="C:\CruiseShip\UpdateAgent\CruiseShip.UpdateAgent.exe" obj=".\CruiseShipAgent" password="SecurePassword"
```

---

## 🔄 Maintenance Procedures

### Regular Maintenance Tasks

#### Daily Tasks (Automated)
- ✅ Health metric collection and reporting
- ✅ Update availability checking
- ✅ Log rotation and cleanup
- ✅ Container health monitoring

#### Weekly Tasks
```powershell
# 1. Review update agent logs
Get-EventLog -LogName Application -Source "CruiseShip.UpdateAgent" -After (Get-Date).AddDays(-7) | 
    Where-Object {$_.EntryType -eq "Error" -or $_.EntryType -eq "Warning"}

# 2. Check disk space
Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, @{Name="Size(GB)";Expression={[math]::round($_.Size/1GB,2)}}, @{Name="FreeSpace(GB)";Expression={[math]::round($_.FreeSpace/1GB,2)}}

# 3. Verify container health
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Image}}"

# 4. Test shore connectivity (when internet available)
Test-NetConnection -ComputerName "shore-command.cruiseline.com" -Port 443
```

#### Monthly Tasks
```powershell
# 1. Update Windows and security patches
Install-WindowsUpdate -AcceptAll -AutoReboot

# 2. Clean up old backups
Get-ChildItem "C:\Backups" -Filter "*.bak" | Where-Object {$_.CreationTime -lt (Get-Date).AddDays(-30)} | Remove-Item

# 3. Review and rotate API keys
# (Coordinate with shore command center)

# 4. Database maintenance
sqlcmd -Q "ALTER INDEX ALL ON Employees REBUILD"
sqlcmd -Q "UPDATE STATISTICS Employees"
```

### Update Rollback Procedures

#### Automatic Rollback
The update agent automatically rolls back failed updates, but manual rollback may be needed:

```powershell
# 1. Stop current container
docker stop employee-management-web
docker rm employee-management-web

# 2. Find backup
Get-ChildItem "C:\CruiseShip\UpdateAgent\backups" -Filter "container_*.tar" | Sort-Object CreationTime -Descending | Select-Object -First 1

# 3. Restore from backup
docker load -i "C:\CruiseShip\UpdateAgent\backups\container_abc123_20250723_020000.tar"

# 4. Start restored container
docker run -d --name employee-management-web -p 80:8080 employee-management:backup-20250723
```

### Disaster Recovery

#### Ship VM Recovery
```powershell
# 1. Restore VM from snapshot/backup
# 2. Verify all services are running
Get-Service | Where-Object {$_.Name -in @("MSSQLSERVER", "Docker Desktop Service", "CruiseShip Update Agent")}

# 3. Test database connectivity
sqlcmd -S localhost -E -Q "SELECT COUNT(*) FROM Employees"

# 4. Verify container status
docker ps -a

# 5. Test application
Invoke-WebRequest -Uri "http://localhost/health"
```

---

## 📞 Support and Contact Information

### Emergency Contacts
- **Shore IT Support**: +1-555-SHORE-IT (24/7)
- **Ship Technical Support**: Ext. 2222 (Ship internal)
- **Emergency Email**: fleet-support@cruiseline.com

### Documentation and Resources
- **Technical Documentation**: `C:\CruiseShip\Documentation\`
- **Shore Command Center**: `https://shore-command.cruiseline.com`
- **Update Agent Logs**: Windows Event Viewer → Application → CruiseShip.UpdateAgent
- **System Health Dashboard**: `http://localhost/health` (when containers running)

### Escalation Procedures
1. **Level 1**: Ship IT team attempts local resolution
2. **Level 2**: Contact shore command center via satellite
3. **Level 3**: Emergency shore-to-ship remote assistance
4. **Level 4**: Physical technician dispatch at next port

---

## 📈 Performance Optimization

### Recommended Settings

#### Update Agent Configuration
```json
{
  "ShipConfiguration": {
    "CheckIntervalSeconds": 300,        // 5 minutes (adjust based on bandwidth)
    "HealthReportIntervalSeconds": 60,  // 1 minute (can increase if bandwidth limited)
    "BackupRetentionDays": 7,          // Adjust based on storage capacity
    "MaintenanceWindows": [
      {
        "Days": ["Sunday", "Wednesday"], // Choose low-traffic periods
        "StartTime": "02:00",           // Avoid passenger activity hours
        "EndTime": "04:00"
      }
    ]
  }
}
```

#### Docker Configuration
```json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "storage-driver": "windowsfilter"
}
```

This comprehensive guide provides everything needed to successfully deploy and manage the cruise ship container update system. Each section includes practical examples and troubleshooting steps to ensure smooth operation across your fleet of 25 ships.

---

## 🌊 Offline Operation & Connectivity Management

### How the System Handles No Internet Connection

#### **1. Normal Ship Operations (No Internet Required)**
```powershell
# These operations work 100% offline:
# ✅ Employee Management application continues running
# ✅ Database operations (SQL Server on VM)
# ✅ Docker containers remain operational
# ✅ Local health monitoring continues
# ✅ Update Agent service keeps running
# ✅ All business applications function normally
```

#### **2. Update Agent Behavior When Offline**
The Update Agent gracefully handles no internet connectivity:

```csharp
// From UpdateReceiverService.cs - handles offline scenarios
try
{
    await CheckForUpdatesAsync(stoppingToken);
    await CleanupOldBackupsAsync(stoppingToken);
}
catch (HttpRequestException ex)
{
    _logger.LogDebug("No internet connection available: {Message}", ex.Message);
    // ✅ Service continues running normally
    // ✅ No errors or crashes
    // ✅ Will retry when connectivity returns
}
```

#### **3. What Happens When Ship Goes Offline**
- 🔄 **Applications Keep Running**: All containerized apps continue serving users
- 📊 **Local Monitoring**: Health metrics collected locally, queued for later transmission
- ⏱️ **Update Checks**: Agent stops checking for updates, resumes when online
- 💾 **Data Persistence**: All data remains on local SQL Server database
- 🔒 **Security**: Local operations don't require external authentication

#### **4. What Happens When Ship Comes Back Online**
```powershell
# Automatic reconnection sequence:
# 1. Update Agent detects internet connectivity
# 2. Re-registers ship with shore command center
# 3. Sends queued health metrics from offline period
# 4. Checks for any pending updates
# 5. Downloads and applies updates during next maintenance window
```

#### **5. Connectivity Status Monitoring**
```json
{
  "healthMetrics": {
    "networkConnectivity": false,  // Tracked in health reports
    "lastShoreContact": "2025-07-23T08:30:00Z",
    "offlineDuration": "4.15:30:00",  // 4 days, 15 hours offline
    "queuedHealthReports": 127,
    "pendingUpdateChecks": 2
  }
}
```

### Shore Command Center - Offline Ship Management

#### **Dashboard Features for Offline Ships**
- 🔴 **Offline Status**: Ships marked as "Offline" with last contact time
- ⏰ **Offline Duration**: Shows how long ship has been unreachable
- 📊 **Last Known Health**: Displays most recent health metrics received
- 📋 **Queued Updates**: Shows updates waiting to be deployed when ship reconnects
- 🎯 **Auto-Deploy**: Updates automatically pushed when ship comes online

#### **Update Scheduling for Offline Ships**
```powershell
# Shore operators can:
# ✅ Schedule updates for offline ships
# ✅ Set priority levels (normal/high/emergency)
# ✅ Queue multiple updates
# ✅ Updates deploy automatically when ship reconnects during maintenance window
```

### Emergency Connectivity Scenarios

#### **Emergency Updates (High Priority)**
```json
{
  "updateRequest": {
    "priority": "emergency",
    "requiresMaintenanceWindow": false,  // Can deploy immediately
    "description": "Critical security patch",
    "forceDeployment": true
  }
}
```

#### **Satellite/Limited Connectivity**
For ships with limited satellite internet:
```json
{
  "ShipConfiguration": {
    "CheckIntervalSeconds": 1800,        // Check every 30 minutes (less frequent)
    "HealthReportIntervalSeconds": 300,  // Report every 5 minutes (less frequent)
    "CompressHealthData": true,          // Reduce bandwidth usage
    "OnlyEmergencyUpdates": true         // Skip normal updates on limited bandwidth
  }
}
```
