# Uninstall CruiseShip Update Agent Windows Service
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "CruiseShip Update Agent",
    
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\CruiseShip\UpdateAgent",
    
    [Parameter(Mandatory=$false)]
    [switch]$RemoveFiles,
    
    [Parameter(Mandatory=$false)]
    [switch]$KeepLogs
)

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ This script must be run as Administrator" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

Write-Host "🗑️ Uninstalling CruiseShip Update Agent..." -ForegroundColor Yellow

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $existingService) {
    Write-Host "⚠️ Service '$ServiceName' not found" -ForegroundColor Yellow
} else {
    # Stop the service
    if ($existingService.Status -eq 'Running') {
        Write-Host "🛑 Stopping service..." -ForegroundColor Blue
        Stop-Service -Name $ServiceName -Force
        
        # Wait for service to stop
        $timeout = 30
        $elapsed = 0
        while ((Get-Service -Name $ServiceName).Status -eq 'Running' -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 1
            $elapsed++
        }
        
        if ((Get-Service -Name $ServiceName).Status -eq 'Running') {
            Write-Host "⚠️ Service did not stop gracefully, forcing..." -ForegroundColor Yellow
        } else {
            Write-Host "✅ Service stopped" -ForegroundColor Green
        }
    }
    
    # Remove the service
    Write-Host "🗑️ Removing Windows Service..." -ForegroundColor Blue
    $result = sc.exe delete $ServiceName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Service removed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to remove service" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
    }
}

# Remove files if requested
if ($RemoveFiles) {
    if (Test-Path $InstallPath) {
        Write-Host "🗂️ Removing installation files..." -ForegroundColor Blue
        
        if ($KeepLogs) {
            # Move logs to a backup location
            $backupPath = Join-Path $env:TEMP "CruiseShip.UpdateAgent.Logs.$(Get-Date -Format 'yyyy-MM-dd-HH-mm-ss')"
            $logsPath = Join-Path $InstallPath "logs"
            $backupsPath = Join-Path $InstallPath "backups"
            
            if (Test-Path $logsPath) {
                New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
                Copy-Item "$logsPath\*" -Destination $backupPath -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "📄 Logs backed up to: $backupPath" -ForegroundColor Cyan
            }
            
            if (Test-Path $backupsPath) {
                $backupsBackupPath = Join-Path $env:TEMP "CruiseShip.UpdateAgent.Backups.$(Get-Date -Format 'yyyy-MM-dd-HH-mm-ss')"
                New-Item -ItemType Directory -Path $backupsBackupPath -Force | Out-Null
                Copy-Item "$backupsPath\*" -Destination $backupsBackupPath -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "💾 Container backups moved to: $backupsBackupPath" -ForegroundColor Cyan
            }
        }
        
        # Remove installation directory
        try {
            Remove-Item $InstallPath -Recurse -Force
            Write-Host "✅ Installation files removed" -ForegroundColor Green
        } catch {
            Write-Host "⚠️ Some files could not be removed (may be in use): $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️ Installation path not found: $InstallPath" -ForegroundColor Yellow
    }
}

# Clean up Windows Event Log source (optional)
try {
    if ([System.Diagnostics.EventLog]::SourceExists("CruiseShip.UpdateAgent")) {
        Write-Host "🧹 Removing Event Log source..." -ForegroundColor Blue
        [System.Diagnostics.EventLog]::DeleteEventSource("CruiseShip.UpdateAgent")
        Write-Host "✅ Event Log source removed" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ Could not remove Event Log source (may require elevated permissions)" -ForegroundColor Yellow
}

Write-Host "`n📊 Uninstallation Summary:" -ForegroundColor Cyan

# Check final status
$finalService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($finalService) {
    Write-Host "❌ Service still exists (may require system restart)" -ForegroundColor Red
} else {
    Write-Host "✅ Service successfully removed" -ForegroundColor Green
}

if ($RemoveFiles) {
    if (Test-Path $InstallPath) {
        Write-Host "⚠️ Some installation files still exist" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Installation files removed" -ForegroundColor Green
    }
} else {
    Write-Host "📁 Installation files preserved at: $InstallPath" -ForegroundColor Cyan
}

Write-Host "`n💡 Cleanup Notes:" -ForegroundColor Yellow
if (-not $RemoveFiles) {
    Write-Host "- Run with -RemoveFiles to delete installation directory" -ForegroundColor White
}
if ($RemoveFiles -and -not $KeepLogs) {
    Write-Host "- Logs and backups were deleted with installation files" -ForegroundColor White
} elseif ($RemoveFiles -and $KeepLogs) {
    Write-Host "- Logs and backups were moved to TEMP directory" -ForegroundColor White
}
Write-Host "- Check Windows Event Viewer for any remaining log entries" -ForegroundColor White

Write-Host "`n🗑️ Uninstallation completed!" -ForegroundColor Green
