# 🚢 Cruise Ship VM Installation Guide (Remote Deployment)

## Overview
This guide provides step-by-step instructions for installing and configuring cruise ship VMs that will **receive remote deployments** from the Shore Command Center. The VMs act as passive receivers that automatically apply updates when connectivity is available, using the **CruiseShip.UpdateAgent** service.

## Remote Deployment Architecture
- **Shore Command Center**: Complete ASP.NET Core Web API with SignalR for real-time fleet management
- **Ship VMs**: Receive and apply updates automatically via .NET Update Agent service
- **Minimal Footprint**: Ships only need Docker, .NET runtime, and the Update Agent
- **Zero Manual Intervention**: All deployments are pushed remotely from shore
- **Offline Operation**: Ships continue running with local database when disconnected

## Prerequisites
- **VM Specifications**: Minimum 8GB RAM, 4 CPU cores, 500GB SSD storage
- **Operating System**: Windows Server 2019/2022 OR Ubuntu 20.04/22.04 LTS
- **Network**: Static IP configuration on ship's LAN + internet access for updates
- **Administrative Access**: Local administrator privileges for initial setup
- **Ship Information**: Unique Ship ID and Ship Name from fleet management
- **Shore Command Center**: Deployed and accessible at fleet management URL

---

## 📋 Required Software Components

### **1. Container Runtime** (Essential)
- **Docker Desktop** (Windows) OR **Docker Engine** (Linux)
- **Docker Compose v2** (included with Docker Desktop)

### **2. CruiseShip.UpdateAgent** (.NET Service) - **RECOMMENDED**

**The .NET Update Agent is the officially supported solution** that integrates seamlessly with the Shore Command Center and Employee Management System.

**✅ Why Choose the .NET Update Agent:**
- ✅ **Complete Integration**: Works perfectly with Shore Command Center API
- ✅ **Professional Service**: Windows Service with proper logging and monitoring  
- ✅ **Real-time Communication**: SignalR integration for live status updates
- ✅ **Health Monitoring**: Comprehensive system health reporting
- ✅ **Automatic Recovery**: Self-healing capabilities and error recovery
- ✅ **Secure Communication**: JWT authentication with Shore Command Center
- ✅ **Background Processing**: Hangfire integration for scheduled tasks
- ✅ **Team Expertise**: Uses your existing .NET/C# skills

**Key Features:**
- **Automatic Ship Registration**: Registers with Shore Command Center on startup
- **Container Management**: Pulls and deploys new container versions
- **Health Reporting**: CPU, memory, disk, and service status monitoring  
- **Offline Operation**: Continues operating when shore connection is unavailable
- **Update Scheduling**: Respects maintenance windows and priority settings
- **Rollback Capability**: Can revert to previous container versions
- **Configuration Management**: Applies environment-specific settings

### **3. Required Runtime Components**

#### **For CruiseShip.UpdateAgent (.NET Service):**
- **.NET 9.0 Runtime** - Required for running the update service
- **Windows Service Support** - Built-in Windows Service hosting
- **Docker.DotNet SDK** - Automatically included with the agent
- **SignalR Client** - For real-time communication with Shore Command Center
- **JWT Authentication** - Secure API communication

### **4. Local Infrastructure** (Pre-configured)
- **SSL Certificates** (for secure shore communication)
- **Local SQL Server Database** (for Employee Management data)
- **Health Monitoring Endpoints** (built into Update Agent)

### **5. Network Tools** (For Troubleshooting)
- **Curl/wget** (for connectivity testing)
- **PowerShell 7.x** (for automation scripts)
- **Docker CLI** (for container inspection)

---

## 🚀 Quick Start (Recommended Installation)

### **Automated Ship Setup Script**
```powershell
# Download the automated ship setup script from shore command
$ShoreCommandUrl = "https://fleet-command.cruiseline.com"
Invoke-WebRequest -Uri "$ShoreCommandUrl/scripts/setup-ship-vm.ps1" -OutFile "setup-ship-vm.ps1"

# Run with your ship-specific parameters
.\setup-ship-vm.ps1 -ShipId "SHIP-001" -ShipName "Ocean Explorer" -ShoreApiUrl $ShoreCommandUrl

# The script will:
# 1. Install Docker Desktop and .NET 9.0 Runtime
# 2. Download and install CruiseShip.UpdateAgent
# 3. Configure ship-specific settings
# 4. Register ship with Shore Command Center  
# 5. Start the Update Agent Windows Service
# 6. Deploy initial Employee Management containers
# 7. Verify all services are running
```

### **Manual Installation Steps** (If automated script fails)

---

## 🪟 Windows Server Installation (Manual Steps)

### **Step 1: Install Docker Desktop**
```powershell
# Option A: Download and install manually
# 1. Download Docker Desktop from: https://www.docker.com/products/docker-desktop/
# 2. Run installer with administrative privileges
# 3. Enable WSL 2 backend when prompted

# Option B: Install via winget (recommended)
winget install Docker.DockerDesktop

# Verify installation
docker --version
docker-compose --version
```

### **Step 2: Install .NET 9.0 Runtime**
```powershell
# Download and install .NET 9.0 Runtime
winget install Microsoft.DotNet.Runtime.9

# Verify installation
dotnet --version
# Should output: 9.0.x
```

### **Step 3: Install Additional Tools**
```powershell
# Install Git for Windows (for deployment scripts)
winget install Git.Git

# Install PowerShell 7+ (for advanced scripting)
winget install Microsoft.PowerShell

# Install SQL Server Express (for local database)
winget install Microsoft.SQLServer.2022.Express

# Install text editor (optional)
winget install Microsoft.VisualStudioCode
```

### **Step 4: Configure Windows Features**
```powershell
# Enable Hyper-V (required for Docker)
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All

# Enable Windows Subsystem for Linux (for Docker)
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux

# Restart when prompted
Restart-Computer
```

### **Step 5: Configure Docker**
```powershell
# Start Docker service
Start-Service docker

# Configure Docker to start automatically
Set-Service docker -StartupType Automatic

# Test Docker installation
docker run hello-world

# Create Docker network for ship containers
docker network create ship-network --driver bridge
```

### **Step 6: Download and Install CruiseShip.UpdateAgent**
```powershell
# Create installation directory
New-Item -ItemType Directory -Path "C:\CruiseShip" -Force

# Download Update Agent from Shore Command Center
$ShoreApiUrl = "https://fleet-command.cruiseline.com"
Invoke-WebRequest -Uri "$ShoreApiUrl/downloads/CruiseShip.UpdateAgent.zip" -OutFile "C:\CruiseShip\UpdateAgent.zip"

# Extract Update Agent
Expand-Archive -Path "C:\CruiseShip\UpdateAgent.zip" -DestinationPath "C:\CruiseShip\UpdateAgent" -Force

# Install as Windows Service
New-Service -Name "CruiseShipUpdateAgent" -BinaryPathName "C:\CruiseShip\UpdateAgent\CruiseShip.UpdateAgent.exe" -StartupType Automatic -Description "Cruise Ship Container Update Agent"
```

### **Step 7: Configure Update Agent**
```powershell
# Create configuration file
$config = @{
    ShipId = "SHIP-001"
    ShipName = "Ocean Explorer" 
    ShoreApiUrl = "https://fleet-command.cruiseline.com"
    ApiKey = "your-ship-api-key-here"
    DatabaseConnectionString = "Server=localhost\SQLEXPRESS;Database=EmployeeManagement;Trusted_Connection=true;TrustServerCertificate=true;"
    DockerNetwork = "ship-network"
    ContainerRegistry = "fleet-registry.cruiseline.com"
    HealthCheckIntervalMinutes = 5
    UpdateCheckIntervalMinutes = 15
}

$config | ConvertTo-Json | Out-File -FilePath "C:\CruiseShip\UpdateAgent\appsettings.json" -Encoding UTF8
```

---

## 🐧 Ubuntu Linux Installation

### **Step 1: Update System**
```bash
# Update package lists and system
sudo apt update && sudo apt upgrade -y

# Install prerequisites
sudo apt install -y apt-transport-https ca-certificates curl gnupg lsb-release
```

### **Step 2: Install Docker Engine**
```bash
# Add Docker's official GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Set up Docker repository
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Start and enable Docker
sudo systemctl start docker
sudo systemctl enable docker

# Add current user to docker group
sudo usermod -aG docker $USER
newgrp docker

# Test Docker installation
docker --version
docker compose version
docker run hello-world
```

### **Step 3: Install .NET 9.0 Runtime**
```bash
# Add Microsoft package repository
curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo apt-add-repository https://packages.microsoft.com/ubuntu/$(lsb_release -rs)/prod

# Install .NET 9.0 runtime
sudo apt update
sudo apt install -y dotnet-runtime-9.0

# Verify installation
dotnet --version
```

### **Step 4: Install Additional Tools**
```bash
# Install Git
sudo apt install -y git

# Install PowerShell (for automation scripts)
wget -q "https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb"
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y powershell

# Install SQL Server (for local database)
sudo apt install -y mssql-server

# Configure SQL Server
sudo /opt/mssql/bin/mssql-conf setup

# Install network tools
sudo apt install -y curl wget telnet net-tools

# Install monitoring tools
sudo apt install -y htop iotop

# Install text editor (optional)
sudo apt install -y nano vim
```

### **Step 5: Download and Install CruiseShip.UpdateAgent**
```bash
# Create installation directory
sudo mkdir -p /opt/cruise-ship/update-agent

# Download Update Agent from Shore Command Center  
SHORE_API_URL="https://fleet-command.cruiseline.com"
curl -L "$SHORE_API_URL/downloads/CruiseShip.UpdateAgent.linux.tar.gz" -o /tmp/update-agent.tar.gz

# Extract Update Agent
sudo tar -xzf /tmp/update-agent.tar.gz -C /opt/cruise-ship/update-agent

# Make executable
sudo chmod +x /opt/cruise-ship/update-agent/CruiseShip.UpdateAgent

# Create systemd service
sudo tee /etc/systemd/system/cruise-ship-update-agent.service > /dev/null <<EOF
[Unit]
Description=Cruise Ship Update Agent
Requires=docker.service
After=docker.service

[Service]
Type=simple
User=root
WorkingDirectory=/opt/cruise-ship/update-agent
ExecStart=/opt/cruise-ship/update-agent/CruiseShip.UpdateAgent
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Enable service
sudo systemctl daemon-reload
sudo systemctl enable cruise-ship-update-agent.service
```

### **Step 6: Configure Update Agent**
```bash
# Create configuration file
sudo tee /opt/cruise-ship/update-agent/appsettings.json > /dev/null <<EOF
{
  "ShipId": "SHIP-001",
  "ShipName": "Ocean Explorer",
  "ShoreApiUrl": "https://fleet-command.cruiseline.com",
  "ApiKey": "your-ship-api-key-here",
  "DatabaseConnectionString": "Server=localhost;Database=EmployeeManagement;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;",
  "DockerNetwork": "ship-network",
  "ContainerRegistry": "fleet-registry.cruiseline.com",
  "HealthCheckIntervalMinutes": 5,
  "UpdateCheckIntervalMinutes": 15
}
EOF
```

---

## 🔧 Post-Installation Configuration

### **Step 1: Configure Docker for Production**
```bash
# Create Docker daemon configuration
sudo mkdir -p /etc/docker

# Configure Docker daemon (Linux)
sudo tee /etc/docker/daemon.json > /dev/null <<EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "storage-driver": "overlay2",
  "data-root": "/var/lib/docker",
  "default-address-pools": [
    {
      "base": "172.20.0.0/16",
      "size": 24
    }
  ]
}
EOF

# Restart Docker (Linux)
sudo systemctl restart docker
```

For Windows, create `C:\ProgramData\Docker\config\daemon.json`:
```json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "data-root": "C:\\ProgramData\\Docker"
}
```

### **Step 2: Create Directory Structure**
```bash
# Create application directories
mkdir -p /opt/cruise-ship/{config,logs,data,backups}

# Set permissions (Linux)
sudo chown -R $USER:$USER /opt/cruise-ship
chmod -R 755 /opt/cruise-ship

# Windows equivalent
# New-Item -ItemType Directory -Path "C:\CruiseShip\{config,logs,data,backups}" -Force
```

### **Step 3: Configure Firewall**

**Linux (UFW):**
```bash
# Enable UFW
sudo ufw enable

# Allow SSH (if needed for remote management)
sudo ufw allow 22/tcp

# Allow HTTP/HTTPS for web application
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow monitoring port
sudo ufw allow 9090/tcp

# Check status
sudo ufw status
```

**Windows (PowerShell):**
```powershell
# Enable Windows Firewall
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled True

# Allow HTTP/HTTPS
New-NetFirewallRule -DisplayName "Allow HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "Allow HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Allow monitoring
New-NetFirewallRule -DisplayName "Allow Monitoring" -Direction Inbound -Protocol TCP -LocalPort 9090 -Action Allow
```

### **Step 4: Configure System Services**

**Linux (systemd) - Update Agent Service:**
```bash
# Start and enable the Update Agent service
sudo systemctl start cruise-ship-update-agent.service
sudo systemctl enable cruise-ship-update-agent.service

# Check service status
sudo systemctl status cruise-ship-update-agent.service

# Create Docker network for containers
docker network create ship-network --driver bridge

# Create Employee Management service (will be managed by Update Agent)
sudo tee /etc/systemd/system/cruise-ship-app.service > /dev/null <<EOF
[Unit]
Description=Cruise Ship Employee Management
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/cruise-ship
ExecStart=/usr/bin/docker compose -f docker-compose.cruise.yml up -d
ExecStop=/usr/bin/docker compose -f docker-compose.cruise.yml down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
EOF

# Enable application service (Update Agent will manage this)
sudo systemctl daemon-reload
sudo systemctl enable cruise-ship-app.service
```

**Windows (Services and Task Scheduler):**
```powershell
# Start the Update Agent Windows Service
Start-Service "CruiseShipUpdateAgent"

# Configure service to start automatically
Set-Service "CruiseShipUpdateAgent" -StartupType Automatic

# Check service status
Get-Service "CruiseShipUpdateAgent"

# Create scheduled task for container management (backup to Update Agent)
$Action = New-ScheduledTaskAction -Execute "docker-compose" -Argument "-f C:\CruiseShip\docker-compose.cruise.yml up -d" -WorkingDirectory "C:\CruiseShip"
$Trigger = New-ScheduledTaskTrigger -AtStartup  
$Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
$Principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount

Register-ScheduledTask -TaskName "CruiseShipEmployeeManagement" -Action $Action -Trigger $Trigger -Settings $Settings -Principal $Principal
```

---

## 🔒 Security Hardening

### **Step 1: User Account Configuration**
```bash
# Create dedicated service user (Linux)
sudo adduser --system --group --no-create-home cruiseship
sudo usermod -aG docker cruiseship

# Configure sudo access for management
echo "cruiseship ALL=(ALL) NOPASSWD: /usr/bin/docker, /usr/bin/docker-compose" | sudo tee /etc/sudoers.d/cruiseship
```

### **Step 2: SSH Configuration (Linux)**
```bash
# Harden SSH configuration
sudo tee -a /etc/ssh/sshd_config > /dev/null <<EOF
# Cruise Ship Security Settings
PermitRootLogin no
PasswordAuthentication no
PubkeyAuthentication yes
MaxAuthTries 3
ClientAliveInterval 300
ClientAliveCountMax 2
EOF

# Restart SSH service
sudo systemctl restart sshd
```

### **Step 3: System Updates Configuration**
```bash
# Configure automatic security updates (Linux)
sudo apt install -y unattended-upgrades

# Configure unattended upgrades
sudo tee /etc/apt/apt.conf.d/50unattended-upgrades > /dev/null <<EOF
Unattended-Upgrade::Allowed-Origins {
    "\${distro_id}:\${distro_codename}-security";
};
Unattended-Upgrade::AutoFixInterruptedDpkg "true";
Unattended-Upgrade::MinimalSteps "true";
Unattended-Upgrade::Remove-Unused-Dependencies "true";
Unattended-Upgrade::Automatic-Reboot "false";
EOF
```

---

## ✅ Installation Verification

### **Step 1: Verify Docker Installation**
```bash
# Check Docker version
docker --version
docker compose version

# Test Docker functionality  
docker run --rm hello-world

# Check Docker system info
docker system info

# Verify ship network exists
docker network ls | grep ship-network
```

### **Step 2: Verify Update Agent Installation**

**Windows:**
```powershell
# Check Update Agent service status
Get-Service "CruiseShipUpdateAgent"

# Check service logs
Get-EventLog -LogName Application -Source "CruiseShipUpdateAgent" -Newest 10

# Test Update Agent API (if running)
Invoke-RestMethod -Uri "http://localhost:9090/health" -Method GET
```

**Linux:**
```bash
# Check Update Agent service status
sudo systemctl status cruise-ship-update-agent.service

# Check service logs
sudo journalctl -u cruise-ship-update-agent.service -f

# Test Update Agent API (if running)
curl -X GET http://localhost:9090/health
```

### **Step 3: Verify Shore Command Center Connectivity**
```bash
# Test connection to Shore Command Center
curl -X GET https://fleet-command.cruiseline.com/api/health

# Test ship registration (replace with actual ship details)
curl -X POST https://fleet-command.cruiseline.com/api/ships/register \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-api-key" \
  -d '{
    "shipId": "SHIP-001",
    "shipName": "Ocean Explorer",
    "agentVersion": "1.0.0",
    "capabilities": ["ContainerManagement", "HealthReporting"],
    "latitude": 25.7617,
    "longitude": -80.1918,
    "timeZone": "America/New_York"
  }'
```

### **Step 4: System Resource Check**
```bash
# Check available resources
free -h                    # Memory (Linux)
df -h                      # Disk space
nproc                      # CPU cores (Linux)

# Windows equivalents:
# Get-WmiObject -Class Win32_PhysicalMemory | Measure-Object -Property capacity -Sum
# Get-WmiObject -Class Win32_LogicalDisk | Select-Object Size,FreeSpace
# Get-WmiObject -Class Win32_ComputerSystem | Select-Object NumberOfProcessors

# Check system load
uptime                     # Linux
# Get-Counter "\Processor(_Total)\% Processor Time"  # Windows
```

### **Step 5: Verify Database Connectivity**
```bash
# Test SQL Server connection (Linux)
sqlcmd -S localhost -U sa -P YourPassword123 -Q "SELECT @@VERSION"

# Test database creation
sqlcmd -S localhost -U sa -P YourPassword123 -Q "CREATE DATABASE EmployeeManagement"
```

**Windows:**
```powershell
# Test SQL Server connection
Invoke-Sqlcmd -ServerInstance "localhost\SQLEXPRESS" -Query "SELECT @@VERSION"

# Test database creation
Invoke-Sqlcmd -ServerInstance "localhost\SQLEXPRESS" -Query "CREATE DATABASE EmployeeManagement"
```

---

## 📝 Quick Installation Scripts

### **Windows Auto-Install Script:**
```powershell
# cruise-ship-setup.ps1 - Automated installation script for Windows

param(
    [Parameter(Mandatory=$true)]
    [string]$ShipId,
    
    [Parameter(Mandatory=$true)]
    [string]$ShipName,
    
    [string]$ShoreApiUrl = "https://fleet-command.cruiseline.com",
    
    [string]$ApiKey = "your-api-key-here"
)

Write-Host "🚢 Starting Cruise Ship VM Setup for $ShipName ($ShipId)..." -ForegroundColor Green

# Install required software
winget install Docker.DockerDesktop Microsoft.DotNet.Runtime.9 Git.Git Microsoft.PowerShell Microsoft.SQLServer.2022.Express -h

# Create directories
New-Item -ItemType Directory -Path "C:\CruiseShip\config", "C:\CruiseShip\logs", "C:\CruiseShip\data", "C:\CruiseShip\backups" -Force

# Download and install Update Agent
Invoke-WebRequest -Uri "$ShoreApiUrl/downloads/CruiseShip.UpdateAgent.zip" -OutFile "C:\CruiseShip\UpdateAgent.zip"
Expand-Archive -Path "C:\CruiseShip\UpdateAgent.zip" -DestinationPath "C:\CruiseShip\UpdateAgent" -Force

# Create configuration
$config = @{
    ShipId = $ShipId
    ShipName = $ShipName
    ShoreApiUrl = $ShoreApiUrl
    ApiKey = $ApiKey
    DatabaseConnectionString = "Server=localhost\SQLEXPRESS;Database=EmployeeManagement;Trusted_Connection=true;TrustServerCertificate=true;"
    DockerNetwork = "ship-network"
    ContainerRegistry = "fleet-registry.cruiseline.com"
    HealthCheckIntervalMinutes = 5
    UpdateCheckIntervalMinutes = 15
}
$config | ConvertTo-Json | Out-File -FilePath "C:\CruiseShip\UpdateAgent\appsettings.json" -Encoding UTF8

# Install Windows Service
New-Service -Name "CruiseShipUpdateAgent" -BinaryPathName "C:\CruiseShip\UpdateAgent\CruiseShip.UpdateAgent.exe" -StartupType Automatic -Description "Cruise Ship Container Update Agent"

# Configure Windows Firewall
New-NetFirewallRule -DisplayName "Allow HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "Allow HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
New-NetFirewallRule -DisplayName "Allow Update Agent" -Direction Inbound -Protocol TCP -LocalPort 9090 -Action Allow

# Create Docker network
docker network create ship-network --driver bridge

# Start services
Start-Service "CruiseShipUpdateAgent"

Write-Host "✅ Cruise Ship VM setup completed!" -ForegroundColor Green
Write-Host "Ship $ShipName ($ShipId) is ready for remote deployment." -ForegroundColor Yellow
Write-Host "Update Agent is running and will automatically register with Shore Command Center." -ForegroundColor Cyan
```

### **Ubuntu Auto-Install Script:**
```bash
#!/bin/bash
# cruise-ship-setup.sh - Automated installation script for Ubuntu

set -e

SHIP_ID="${1:-SHIP-001}"
SHIP_NAME="${2:-Ocean Explorer}"
SHORE_API_URL="${3:-https://fleet-command.cruiseline.com}"
API_KEY="${4:-your-api-key-here}"

echo "🚢 Starting Cruise Ship VM Setup for $SHIP_NAME ($SHIP_ID)..."

# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Install .NET Runtime
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-runtime-9.0

# Install additional tools
sudo apt install -y git powershell mssql-server curl wget htop

# Configure SQL Server
sudo /opt/mssql/bin/mssql-conf setup

# Create directories
sudo mkdir -p /opt/cruise-ship/{config,logs,data,backups,update-agent}

# Download and install Update Agent
curl -L "$SHORE_API_URL/downloads/CruiseShip.UpdateAgent.linux.tar.gz" -o /tmp/update-agent.tar.gz
sudo tar -xzf /tmp/update-agent.tar.gz -C /opt/cruise-ship/update-agent
sudo chmod +x /opt/cruise-ship/update-agent/CruiseShip.UpdateAgent

# Create configuration
sudo tee /opt/cruise-ship/update-agent/appsettings.json > /dev/null <<EOF
{
  "ShipId": "$SHIP_ID",
  "ShipName": "$SHIP_NAME", 
  "ShoreApiUrl": "$SHORE_API_URL",
  "ApiKey": "$API_KEY",
  "DatabaseConnectionString": "Server=localhost;Database=EmployeeManagement;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;",
  "DockerNetwork": "ship-network",
  "ContainerRegistry": "fleet-registry.cruiseline.com",
  "HealthCheckIntervalMinutes": 5,
  "UpdateCheckIntervalMinutes": 15
}
EOF

# Create systemd service
sudo tee /etc/systemd/system/cruise-ship-update-agent.service > /dev/null <<EOF
[Unit]
Description=Cruise Ship Update Agent
Requires=docker.service
After=docker.service

[Service]
Type=simple
User=root
WorkingDirectory=/opt/cruise-ship/update-agent
ExecStart=/opt/cruise-ship/update-agent/CruiseShip.UpdateAgent
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Configure firewall
sudo ufw enable
sudo ufw allow 80,443,9090/tcp

# Create Docker network
docker network create ship-network --driver bridge

# Start services
sudo systemctl daemon-reload
sudo systemctl enable cruise-ship-update-agent.service
sudo systemctl start cruise-ship-update-agent.service

# Set permissions
sudo chown -R $USER:$USER /opt/cruise-ship

echo "✅ Cruise Ship VM setup completed!"
echo "Ship $SHIP_NAME ($SHIP_ID) is ready for remote deployment."
echo "Update Agent is running and will automatically register with Shore Command Center."
echo "Please reboot the system to complete Docker group membership: sudo reboot"
```

---

## 🚀 Next Steps

After completing the installation:

1. **Reboot the VM** to ensure all services start properly
2. **Verify Update Agent Registration**: Check Shore Command Center dashboard to confirm ship appears
3. **Initial Container Deployment**: Update Agent will automatically pull and deploy Employee Management containers
4. **Monitor Health Status**: Use Shore Command Center to monitor ship health metrics
5. **Test Offline Operation**: Verify system continues operating when disconnected from shore

### **Post-Installation Checklist**

✅ **Docker Desktop/Engine running**  
✅ **.NET 9.0 Runtime installed**  
✅ **CruiseShip.UpdateAgent service running**  
✅ **SQL Server database accessible**  
✅ **Ship registered with Shore Command Center**  
✅ **Employee Management containers deployed**  
✅ **Health metrics being reported**  
✅ **Network connectivity to shore (when available)**

### **Service URLs (After Deployment)**

- **Employee Management App**: `http://localhost:80`
- **Update Agent Health**: `http://localhost:9090/health`
- **Database**: `localhost:1433` (SQL Server)
- **Shore Command Center**: `https://fleet-command.cruiseline.com`

### **Monitoring and Logs**

**Windows:**
```powershell
# Check Update Agent logs
Get-EventLog -LogName Application -Source "CruiseShipUpdateAgent" -Newest 20

# Check container status
docker ps

# Check container logs
docker logs employee-management-web
```

**Linux:**
```bash
# Check Update Agent logs
sudo journalctl -u cruise-ship-update-agent.service -f

# Check container status
docker ps

# Check container logs
docker logs employee-management-web
```

## 📞 Support

If you encounter issues during installation:

- **TROUBLESHOOTING_GUIDE.md** - Common problems and solutions
- **Shore Command Center Dashboard** - Real-time ship status and diagnostics
- **Update Agent Logs** - Detailed service execution logs
- **Docker Logs** - Container-specific error messages
- **System Requirements** - Verify minimum specifications are met

### **Common Issues**

1. **Update Agent Won't Start**: Check .NET runtime installation and configuration file
2. **Ship Not Appearing in Fleet**: Verify API key and Shore Command Center URL
3. **Container Deployment Fails**: Check Docker service and network connectivity
4. **Database Connection Issues**: Verify SQL Server installation and connection string
5. **Health Metrics Not Reporting**: Check Update Agent configuration and firewall rules

### **Emergency Procedures**

**If Update Agent Fails:**
```powershell
# Windows: Restart service
Restart-Service "CruiseShipUpdateAgent"

# Linux: Restart service  
sudo systemctl restart cruise-ship-update-agent.service
```

**If Containers Stop:**
```bash
# Restart all containers
docker-compose -f docker-compose.cruise.yml restart

# Force pull latest images
docker-compose -f docker-compose.cruise.yml pull
docker-compose -f docker-compose.cruise.yml up -d
```

**If Shore Communication Lost:**
The system is designed to operate offline. Employee Management will continue running with the local database. Updates will resume automatically when connectivity is restored.
