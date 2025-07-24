# Cruise Ship Container Deployment Script (PowerShell)
# This script deploys the Employee Management System to a cruise ship

param(
    [string]$ShipId = "Ship001",
    [string]$ShipName = "Cruise Ship Alpha"
)

$ErrorActionPreference = "Stop"

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "🚢 Deploying Employee Management System to $ShipName ($ShipId)" -ForegroundColor Green

# Check prerequisites
function Test-Prerequisites {
    Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow
    
    if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Host "❌ Docker is not installed" -ForegroundColor Red
        exit 1
    }
    
    if (!(Get-Command docker-compose -ErrorAction SilentlyContinue)) {
        Write-Host "❌ Docker Compose is not installed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Prerequisites check passed" -ForegroundColor Green
}

# Create ship-specific configuration
function New-ShipConfig {
    Write-Host "⚙️ Creating ship-specific configuration..." -ForegroundColor Yellow
    
    # Create .env file from template
    $envTemplate = Get-Content "$ProjectRoot\.env.template"
    $envContent = $envTemplate -replace "SHIP_ID=Ship001", "SHIP_ID=$ShipId"
    $envContent = $envContent -replace 'SHIP_NAME="Cruise Ship Alpha"', "SHIP_NAME=`"$ShipName`""
    $envContent | Set-Content "$ProjectRoot\.env"
    
    # Create directories
    @("config", "logs", "database\backups", "database\scripts", "nginx") | ForEach-Object {
        $dir = Join-Path $ProjectRoot $_
        if (!(Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }
    }
    
    # Create ship config JSON
    $shipConfig = @{
        ship_id = $ShipId
        ship_name = $ShipName
        services = @(
            @{
                name = "employeemanagement-web"
                image = "registry.cruiseline.com/employeemanagement:latest"
                update_policy = "auto"
                rollback_enabled = $true
            }
        )
        update_window = @{
            start = "02:00"
            end = "04:00"
            timezone = "UTC"
        }
        auto_update_enabled = $true
        max_retries = 3
        health_check_timeout = 300
    }
    
    $shipConfig | ConvertTo-Json -Depth 5 | Set-Content "$ProjectRoot\config\ship-config.json"
    
    Write-Host "✅ Ship configuration created" -ForegroundColor Green
}

# Build container images
function Build-Images {
    Write-Host "🔨 Building container images..." -ForegroundColor Yellow
    
    Push-Location $ProjectRoot
    
    try {
        # Build main application
        docker build -t "employeemanagement:local" .
        
        # Build update agent
        Push-Location "cruise-ship-tools"
        docker build -f Dockerfile.update-agent -t "update-agent:local" .
        Pop-Location
        
        Write-Host "✅ Container images built" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Create NGINX configuration
function New-NginxConfig {
    Write-Host "🌐 Creating NGINX configuration..." -ForegroundColor Yellow
    
    $nginxConfig = @'
events {
    worker_connections 1024;
}

http {
    upstream employeemanagement {
        server employeemanagement-web:8080;
    }
    
    server {
        listen 80;
        server_name _;
        
        # Health check endpoint
        location /health {
            proxy_pass http://employeemanagement/health;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
        }
        
        # Main application
        location / {
            proxy_pass http://employeemanagement;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            # WebSocket support for Blazor
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
        }
        
        # Static files caching
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
            proxy_pass http://employeemanagement;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
'@
    
    $nginxConfig | Set-Content "$ProjectRoot\nginx\nginx.conf"
    
    Write-Host "✅ NGINX configuration created" -ForegroundColor Green
}

# Deploy containers
function Deploy-Containers {
    Write-Host "🚀 Deploying containers..." -ForegroundColor Yellow
    
    Push-Location $ProjectRoot
    
    try {
        # Stop existing containers
        docker-compose -f docker-compose.cruise.yml down 2>$null
        
        # Start new deployment
        docker-compose -f docker-compose.cruise.yml up -d
        
        Write-Host "✅ Containers deployed" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Verify deployment
function Test-Deployment {
    Write-Host "🔍 Verifying deployment..." -ForegroundColor Yellow
    
    # Wait for services to start
    Start-Sleep -Seconds 30
    
    Push-Location $ProjectRoot
    
    try {
        # Check container status
        $containers = docker-compose -f docker-compose.cruise.yml ps
        if ($containers -match "Up") {
            Write-Host "✅ Containers are running" -ForegroundColor Green
        } else {
            Write-Host "❌ Some containers failed to start" -ForegroundColor Red
            docker-compose -f docker-compose.cruise.yml logs
            exit 1
        }
        
        # Check application health
        $healthCheckPassed = $false
        for ($i = 1; $i -le 10; $i++) {
            try {
                $response = Invoke-WebRequest -Uri "http://localhost/health" -UseBasicParsing -TimeoutSec 5
                if ($response.StatusCode -eq 200) {
                    Write-Host "✅ Application is healthy" -ForegroundColor Green
                    $healthCheckPassed = $true
                    break
                }
            }
            catch {
                Write-Host "⏳ Waiting for application to start... ($i/10)" -ForegroundColor Yellow
                Start-Sleep -Seconds 10
            }
        }
        
        if (!$healthCheckPassed) {
            Write-Host "❌ Application health check failed" -ForegroundColor Red
        }
    }
    finally {
        Pop-Location
    }
}

# Create maintenance scripts
function New-MaintenanceScripts {
    Write-Host "🔧 Creating maintenance scripts..." -ForegroundColor Yellow
    
    $scriptsDir = Join-Path $ProjectRoot "scripts"
    if (!(Test-Path $scriptsDir)) {
        New-Item -ItemType Directory -Path $scriptsDir -Force | Out-Null
    }
    
    # Backup script
    $backupScript = @'
# Database backup script for cruise ship
param([string]$Password = $env:DB_PASSWORD)

$BackupDir = "/var/opt/mssql/backups"
$Date = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFile = "EmployeeManagement_$Date.bak"

Write-Host "Creating database backup: $BackupFile"

docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd `
    -S localhost -U sa -P $Password `
    -Q "BACKUP DATABASE EmployeeManagement TO DISK = '/var/opt/mssql/backups/$BackupFile'"

Write-Host "Backup completed: $BackupFile"
'@
    
    $backupScript | Set-Content "$scriptsDir\backup.ps1"
    
    # Log rotation script
    $logRotationScript = @'
# Log rotation script for cruise ship
$LogDir = ".\logs"
$MaxSizeMB = 100

Get-ChildItem -Path $LogDir -Filter "*.log" | Where-Object { 
    $_.Length -gt ($MaxSizeMB * 1MB) 
} | ForEach-Object {
    Compress-Archive -Path $_.FullName -DestinationPath "$($_.FullName).zip"
    Remove-Item $_.FullName
}

Get-ChildItem -Path $LogDir -Filter "*.log.zip" | Where-Object {
    $_.LastWriteTime -lt (Get-Date).AddDays(-7)
} | Remove-Item

Write-Host "Log rotation completed"
'@
    
    $logRotationScript | Set-Content "$scriptsDir\rotate-logs.ps1"
    
    Write-Host "✅ Maintenance scripts created" -ForegroundColor Green
}

# Main deployment flow
function Main {
    Write-Host "🚢 Starting Cruise Ship Deployment for $ShipName ($ShipId)" -ForegroundColor Cyan
    Write-Host "==================================================" -ForegroundColor Cyan
    
    Test-Prerequisites
    New-ShipConfig
    New-NginxConfig
    Build-Images
    Deploy-Containers
    Test-Deployment
    New-MaintenanceScripts
    
    Write-Host ""
    Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Services:" -ForegroundColor Cyan
    Write-Host "   • Web Application: http://localhost"
    Write-Host "   • Health Monitor: http://localhost:9090"
    Write-Host "   • Database: localhost:1433"
    Write-Host ""
    Write-Host "📁 Important directories:" -ForegroundColor Cyan
    Write-Host "   • Configuration: .\config\"
    Write-Host "   • Logs: .\logs\"
    Write-Host "   • Database backups: .\database\backups\"
    Write-Host "   • Maintenance scripts: .\scripts\"
    Write-Host ""
    Write-Host "🔧 Management commands:" -ForegroundColor Cyan
    Write-Host "   • View logs: docker-compose -f docker-compose.cruise.yml logs"
    Write-Host "   • Stop services: docker-compose -f docker-compose.cruise.yml down"
    Write-Host "   • Restart services: docker-compose -f docker-compose.cruise.yml restart"
    Write-Host "   • Backup database: .\scripts\backup.ps1"
    Write-Host ""
}

# Run main function
Main
