# Install CruiseShip Update Agent as Windows Service
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "CruiseShip Update Agent",
    
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\CruiseShip\UpdateAgent",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceAccount = "LocalSystem"
)

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ This script must be run as Administrator" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

Write-Host "🚢 Installing CruiseShip Update Agent..." -ForegroundColor Green

# Create installation directory
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Host "✅ Created installation directory: $InstallPath" -ForegroundColor Yellow
}

# Copy files to installation directory
if (Test-Path ".\CruiseShip.UpdateAgent.exe") {
    Write-Host "📂 Copying files to installation directory..." -ForegroundColor Blue
    Copy-Item ".\*" -Destination $InstallPath -Recurse -Force
    Write-Host "✅ Files copied successfully" -ForegroundColor Yellow
} else {
    Write-Host "❌ CruiseShip.UpdateAgent.exe not found in current directory" -ForegroundColor Red
    Write-Host "Make sure you're running this script from the published output directory" -ForegroundColor Yellow
    exit 1
}

# Check if service already exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Host "⚠️ Service '$ServiceName' already exists" -ForegroundColor Yellow
    Write-Host "Stopping existing service..." -ForegroundColor Blue
    
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    
    Write-Host "Removing existing service..." -ForegroundColor Blue
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

# Install the service
Write-Host "🔧 Installing Windows Service..." -ForegroundColor Blue
$exePath = Join-Path $InstallPath "CruiseShip.UpdateAgent.exe"

$result = sc.exe create $ServiceName binPath= $exePath start= auto obj= $ServiceAccount
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Service installed successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to install service" -ForegroundColor Red
    Write-Host "Error: $result" -ForegroundColor Red
    exit 1
}

# Set service description
sc.exe description $ServiceName "Manages container updates for cruise ship deployments. Automatically downloads and applies updates during maintenance windows." | Out-Null

# Set service failure actions (restart on failure)
sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000 | Out-Null

# Configure service to restart on failure
Write-Host "🔧 Configuring service recovery options..." -ForegroundColor Blue

# Create folders for logs and data
$logsPath = Join-Path $InstallPath "logs"
$backupsPath = Join-Path $InstallPath "backups"
$tempPath = Join-Path $InstallPath "temp"

@($logsPath, $backupsPath, $tempPath) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
    }
}

Write-Host "✅ Created working directories" -ForegroundColor Yellow

# Check configuration file
$configPath = Join-Path $InstallPath "appsettings.Production.json"
if (-not (Test-Path $configPath)) {
    $templatePath = Join-Path $InstallPath "appsettings.Template.json"
    if (Test-Path $templatePath) {
        Copy-Item $templatePath $configPath
        Write-Host "⚠️ Created appsettings.Production.json from template" -ForegroundColor Yellow
        Write-Host "📝 IMPORTANT: Edit $configPath with your ship-specific configuration!" -ForegroundColor Red
    } else {
        Write-Host "⚠️ No configuration template found" -ForegroundColor Yellow
    }
}

# Start the service
Write-Host "▶️ Starting service..." -ForegroundColor Blue
$startResult = Start-Service -Name $ServiceName -PassThru -ErrorAction SilentlyContinue

if ($startResult.Status -eq 'Running') {
    Write-Host "✅ Service started successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️ Service installed but failed to start" -ForegroundColor Yellow
    Write-Host "This is normal if configuration is not yet complete" -ForegroundColor Yellow
}

# Display status
Write-Host "`n📊 Service Status:" -ForegroundColor Cyan
Get-Service -Name $ServiceName | Format-Table Name, Status, StartType -AutoSize

Write-Host "`n🔧 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Edit configuration file: $configPath" -ForegroundColor White
Write-Host "   - Set ShipId and ShipName" -ForegroundColor White
Write-Host "   - Set ShoreCommandUrl" -ForegroundColor White
Write-Host "   - Set ApiKey" -ForegroundColor White
Write-Host "   - Configure MaintenanceWindows" -ForegroundColor White
Write-Host "2. Restart the service: Restart-Service '$ServiceName'" -ForegroundColor White
Write-Host "3. Check service logs in Windows Event Viewer (Application)" -ForegroundColor White

Write-Host "`n🎯 Management Commands:" -ForegroundColor Cyan
Write-Host "Start:   Start-Service '$ServiceName'" -ForegroundColor White
Write-Host "Stop:    Stop-Service '$ServiceName'" -ForegroundColor White
Write-Host "Restart: Restart-Service '$ServiceName'" -ForegroundColor White
Write-Host "Status:  Get-Service '$ServiceName'" -ForegroundColor White
Write-Host "Logs:    Get-EventLog -LogName Application -Source 'CruiseShip.UpdateAgent' -Newest 10" -ForegroundColor White

Write-Host "`n🚢 Installation completed!" -ForegroundColor Green
