# CruiseShip Update Agent Build Script
# This script builds and packages the Update Agent for deployment

param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\dist",
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateInstaller
)

Write-Host "🚀 Building CruiseShip Update Agent..." -ForegroundColor Green

# Clean previous builds
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "✅ Cleaned previous builds" -ForegroundColor Yellow
}

# Restore dependencies
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Blue
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host "🔨 Building project..." -ForegroundColor Blue
dotnet build -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}

# Publish the application
Write-Host "📤 Publishing application..." -ForegroundColor Blue
dotnet publish -c $Configuration -o $OutputPath --no-build --self-contained false
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publish failed" -ForegroundColor Red
    exit 1
}

# Copy additional files
Write-Host "📋 Copying additional files..." -ForegroundColor Blue
Copy-Item "UPDATE_AGENT_README.md" "$OutputPath\README.md" -Force
Copy-Item "install-service.ps1" "$OutputPath\" -Force
Copy-Item "uninstall-service.ps1" "$OutputPath\" -Force

# Create configuration template
$configTemplate = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "EventLog": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "ShipConfiguration": {
    "ShipId": "CHANGE_ME",
    "ShipName": "CHANGE_ME",
    "ShoreCommandUrl": "https://shore-command.cruiseline.com",
    "ApiKey": "CHANGE_ME",
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
"@

$configTemplate | Out-File "$OutputPath\appsettings.Template.json" -Encoding UTF8

# Create deployment package
Write-Host "📦 Creating deployment package..." -ForegroundColor Blue
$zipPath = ".\CruiseShip.UpdateAgent.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$OutputPath\*" -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host "✅ Build completed successfully!" -ForegroundColor Green
Write-Host "📍 Output directory: $OutputPath" -ForegroundColor Cyan
Write-Host "📦 Deployment package: $zipPath" -ForegroundColor Cyan

# Display next steps
Write-Host "`n🔧 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy the deployment package to the cruise ship server" -ForegroundColor White
Write-Host "2. Extract the package to C:\CruiseShip\UpdateAgent\" -ForegroundColor White
Write-Host "3. Edit appsettings.Production.json with ship-specific configuration" -ForegroundColor White
Write-Host "4. Run install-service.ps1 as Administrator to install the Windows Service" -ForegroundColor White
Write-Host "5. Start the service: sc start `"CruiseShip Update Agent`"" -ForegroundColor White

if ($CreateInstaller) {
    Write-Host "`n🔧 Creating Windows Installer..." -ForegroundColor Blue
    # Here you could integrate with WiX Toolset or other installer tools
    Write-Host "💡 Windows Installer creation requires WiX Toolset (not implemented in this script)" -ForegroundColor Yellow
}
