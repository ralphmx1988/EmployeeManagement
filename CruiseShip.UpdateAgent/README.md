# .NET Update Agent Project

## Overview
This is a complete .NET 9.0 Windows Service implementation of the Cruise Ship Update Agent. It provides automatic container updates with health monitoring, rollback capabilities, and secure communication with the shore command center.

## Project Structure

```
CruiseShip.UpdateAgent/
├── CruiseShip.UpdateAgent.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Production.json
├── Models/
│   ├── ShipConfiguration.cs
│   ├── UpdateRequest.cs
│   ├── HealthMetrics.cs
│   └── MaintenanceWindow.cs
├── Services/
│   ├── UpdateReceiverService.cs
│   ├── DockerService.cs
│   ├── ShoreApiClient.cs
│   ├── HealthMonitor.cs
│   └── UpdateOrchestrator.cs
├── Interfaces/
│   ├── IDockerService.cs
│   ├── IShoreApiClient.cs
│   ├── IHealthMonitor.cs
│   └── IUpdateOrchestrator.cs
└── Dockerfile
```

## How to Use

### 1. Build and Deploy
```bash
# Build the project
dotnet build --configuration Release

# Publish for deployment
dotnet publish --configuration Release --output ./publish

# Install as Windows Service
sc create "CruiseShipUpdateAgent" binPath= "C:\CruiseShip\UpdateAgent\CruiseShip.UpdateAgent.exe"
sc start "CruiseShipUpdateAgent"
```

### 2. Configuration
Configure the `appsettings.Production.json` file with your ship-specific settings.

### 3. Monitoring
Monitor the service through Windows Event Viewer or log files in the configured directory.

## Features
- ✅ Automatic container updates from shore command
- ✅ Health monitoring and metrics reporting
- ✅ Maintenance window scheduling
- ✅ Automatic rollback on failures
- ✅ Secure API communication
- ✅ Windows Service integration
- ✅ Comprehensive logging
