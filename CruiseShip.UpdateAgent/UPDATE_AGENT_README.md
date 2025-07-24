# CruiseShip Update Agent

A .NET 9.0 Windows Service that automatically manages container updates for cruise ship deployments.

## Features

- 🚢 **Ship Registration**: Automatically registers with shore command center
- 📦 **Container Updates**: Downloads and applies container updates during maintenance windows
- 💾 **Backup & Rollback**: Creates container backups before updates with automatic rollback on failure
- 📊 **Health Monitoring**: Continuously monitors system and container health
- 🔄 **Maintenance Windows**: Respects configured maintenance schedules
- 🌐 **Offline Support**: Works without internet, applies updates when connection is available
- 🔐 **Secure Communication**: API key-based authentication with shore command

## Architecture

```
Shore Command Center
        ↓ (Updates via API)
CruiseShip Update Agent
        ↓ (Manages)
    Docker Containers
        ↓ (Access)
Local Ship Resources (Database, IIS, etc.)
```

## Quick Start

### 1. Build the Project

```powershell
cd CruiseShip.UpdateAgent
dotnet build -c Release
```

### 2. Configure Settings

Edit `appsettings.Production.json`:

```json
{
  "ShipConfiguration": {
    "ShipId": "SHIP001",
    "ShipName": "Ocean Explorer",
    "ShoreCommandUrl": "https://shore-command.cruiseline.com",
    "ApiKey": "your-secure-api-key",
    "CheckIntervalSeconds": 300,
    "HealthReportIntervalSeconds": 60,
    "BackupRetentionDays": 7,
    "MaintenanceWindows": [
      {
        "Days": ["Sunday", "Wednesday"],
        "StartTime": "02:00",
        "EndTime": "04:00",
        "TimeZone": "UTC"
      }
    ]
  }
}
```

### 3. Install as Windows Service

```powershell
# Publish the application
dotnet publish -c Release -o "C:\CruiseShip\UpdateAgent"

# Install as Windows Service
sc create "CruiseShip Update Agent" binPath="C:\CruiseShip\UpdateAgent\CruiseShip.UpdateAgent.exe" start=auto
sc description "CruiseShip Update Agent" "Manages container updates for cruise ship deployments"
sc start "CruiseShip Update Agent"
```

### 4. Verify Installation

Check Windows Event Logs or application logs:

```powershell
# View service status
sc query "CruiseShip Update Agent"

# View recent logs
Get-EventLog -LogName Application -Source "CruiseShip.UpdateAgent" -Newest 10
```

## Configuration

### Ship Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| `ShipId` | Unique identifier for this ship | Required |
| `ShipName` | Human-readable ship name | Required |
| `ShoreCommandUrl` | Base URL of shore command API | Required |
| `ApiKey` | Authentication key for shore API | Required |
| `CheckIntervalSeconds` | How often to check for updates | 300 (5 min) |
| `HealthReportIntervalSeconds` | How often to send health reports | 60 (1 min) |
| `BackupRetentionDays` | How long to keep container backups | 7 days |

### Maintenance Windows

Configure when updates can be applied:

```json
{
  "MaintenanceWindows": [
    {
      "Days": ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
      "StartTime": "02:00",
      "EndTime": "04:00",
      "TimeZone": "UTC"
    },
    {
      "Days": ["Wednesday"],
      "StartTime": "14:00",
      "EndTime": "16:00",
      "TimeZone": "America/New_York"
    }
  ]
}
```

## Update Package Format

Updates are delivered as ZIP files containing:

### update.json
```json
{
  "containersToUpdate": [
    {
      "name": "employee-management-web",
      "newImage": "myregistry/employee-management",
      "newTag": "v2.1.0",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ConnectionStrings__DefaultConnection": "Server=localhost;Database=EmployeeDB;Trusted_Connection=true;"
      },
      "portMappings": {
        "80": "8080",
        "443": "8443"
      }
    }
  ],
  "customScripts": [
    "scripts/pre-update.ps1",
    "scripts/post-update.ps1"
  ],
  "metadata": {
    "version": "2.1.0",
    "description": "Employee Management System Update",
    "releaseNotes": "Bug fixes and performance improvements"
  }
}
```

## Development

### Prerequisites
- .NET 9.0 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code

### Build & Test
```powershell
# Restore packages
dotnet restore

# Build
dotnet build

# Run locally
dotnet run --environment Development
```

## Security Considerations

- API keys should be stored securely
- Service runs with minimal permissions
- Container backups may contain sensitive data
- Network traffic to shore command should use HTTPS

## License

This project is licensed under the MIT License - see the LICENSE file for details.
