# Update Agent Implementation Options

## Overview
This document provides multiple implementation options for the Ship Update Receiver Agent, allowing you to choose the best technology stack for your cruise ship deployment.

## Option A: .NET-based Update Agent (Recommended)

### Benefits
- **Native Windows integration**
- **Leverages existing .NET skills**
- **Strong typing and debugging**
- **Excellent Docker SDK support**
- **Easy integration with existing codebase**

### Implementation

#### Program.cs - Main Entry Point
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CruiseShip.UpdateAgent.Services;
using CruiseShip.UpdateAgent.Models;

namespace CruiseShip.UpdateAgent
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Enables Windows Service support
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ShipConfiguration>(
                        hostContext.Configuration.GetSection("ShipConfig"));
                    
                    services.AddHttpClient<IShoreApiClient, ShoreApiClient>();
                    services.AddSingleton<IDockerService, DockerService>();
                    services.AddSingleton<IHealthMonitor, HealthMonitor>();
                    services.AddSingleton<IUpdateOrchestrator, UpdateOrchestrator>();
                    
                    services.AddHostedService<UpdateReceiverService>();
                    services.AddHostedService<HealthMonitorService>();
                });
    }
}
```

#### UpdateReceiverService.cs - Main Service
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseShip.UpdateAgent.Models;
using System.Text.Json;

namespace CruiseShip.UpdateAgent.Services
{
    public class UpdateReceiverService : BackgroundService
    {
        private readonly ILogger<UpdateReceiverService> _logger;
        private readonly ShipConfiguration _config;
        private readonly IShoreApiClient _shoreClient;
        private readonly IUpdateOrchestrator _updateOrchestrator;
        private readonly IHealthMonitor _healthMonitor;

        public UpdateReceiverService(
            ILogger<UpdateReceiverService> logger,
            IOptions<ShipConfiguration> config,
            IShoreApiClient shoreClient,
            IUpdateOrchestrator updateOrchestrator,
            IHealthMonitor healthMonitor)
        {
            _logger = logger;
            _config = config.Value;
            _shoreClient = shoreClient;
            _updateOrchestrator = updateOrchestrator;
            _healthMonitor = healthMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Update Receiver Service starting for ship {ShipId}", _config.ShipId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForUpdates();
                    await SendHealthMetrics();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogDebug("No internet connection available: {Message}", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in update receiver service");
                }

                await Task.Delay(TimeSpan.FromSeconds(_config.CheckIntervalSeconds), stoppingToken);
            }
        }

        private async Task CheckForUpdates()
        {
            var pendingUpdates = await _shoreClient.GetPendingUpdatesAsync(_config.ShipId);
            
            if (pendingUpdates?.Any() == true)
            {
                _logger.LogInformation("Found {Count} pending updates", pendingUpdates.Count);
                
                foreach (var update in pendingUpdates)
                {
                    if (IsMaintenanceWindow())
                    {
                        await _updateOrchestrator.ProcessUpdateAsync(update);
                    }
                    else
                    {
                        _logger.LogInformation("Update {UpdateId} scheduled for maintenance window", update.Id);
                    }
                }
            }
        }

        private async Task SendHealthMetrics()
        {
            var metrics = await _healthMonitor.CollectMetricsAsync();
            await _shoreClient.SendHealthMetricsAsync(_config.ShipId, metrics);
        }

        private bool IsMaintenanceWindow()
        {
            var now = DateTime.UtcNow;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;

            return _config.MaintenanceWindows.Any(window =>
                window.Days.Contains(currentDay) &&
                currentTime >= window.StartTime &&
                currentTime <= window.EndTime);
        }
    }
}
```

#### DockerService.cs - Docker Management
```csharp
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace CruiseShip.UpdateAgent.Services
{
    public interface IDockerService
    {
        Task<bool> PullImageAsync(string imageTag);
        Task<string> CreateContainerAsync(string imageTag, string containerName, CreateContainerParameters parameters);
        Task<bool> StartContainerAsync(string containerId);
        Task<bool> StopContainerAsync(string containerId, int timeoutSeconds = 30);
        Task<bool> RemoveContainerAsync(string containerId);
        Task<bool> RenameContainerAsync(string containerId, string newName);
        Task<ContainerInspectResponse> InspectContainerAsync(string containerId);
        Task<bool> HealthCheckAsync(string containerId, TimeSpan timeout);
    }

    public class DockerService : IDockerService
    {
        private readonly DockerClient _dockerClient;
        private readonly ILogger<DockerService> _logger;

        public DockerService(ILogger<DockerService> logger)
        {
            _logger = logger;
            _dockerClient = new DockerClientConfiguration().CreateClient();
        }

        public async Task<bool> PullImageAsync(string imageTag)
        {
            try
            {
                _logger.LogInformation("Pulling image: {ImageTag}", imageTag);
                
                await _dockerClient.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageTag,
                        Tag = "latest"
                    },
                    null,
                    new Progress<JSONMessage>(message => 
                        _logger.LogDebug("Pull progress: {Status}", message.Status)));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to pull image {ImageTag}", imageTag);
                return false;
            }
        }

        public async Task<string> CreateContainerAsync(string imageTag, string containerName, CreateContainerParameters parameters)
        {
            try
            {
                var response = await _dockerClient.Containers.CreateContainerAsync(parameters);
                _logger.LogInformation("Created container {ContainerName} with ID {ContainerId}", 
                    containerName, response.ID);
                return response.ID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create container {ContainerName}", containerName);
                return null;
            }
        }

        public async Task<bool> StartContainerAsync(string containerId)
        {
            try
            {
                await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
                _logger.LogInformation("Started container {ContainerId}", containerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start container {ContainerId}", containerId);
                return false;
            }
        }

        public async Task<bool> StopContainerAsync(string containerId, int timeoutSeconds = 30)
        {
            try
            {
                await _dockerClient.Containers.StopContainerAsync(containerId, 
                    new ContainerStopParameters { WaitBeforeKillSeconds = (uint)timeoutSeconds });
                _logger.LogInformation("Stopped container {ContainerId}", containerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop container {ContainerId}", containerId);
                return false;
            }
        }

        public async Task<bool> HealthCheckAsync(string containerId, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;
            
            while (DateTime.UtcNow - startTime < timeout)
            {
                try
                {
                    var inspection = await InspectContainerAsync(containerId);
                    if (inspection?.State?.Running == true)
                    {
                        // Perform application-specific health check here
                        // For example, HTTP health check endpoint
                        using var httpClient = new HttpClient();
                        var response = await httpClient.GetAsync("http://localhost:8080/health");
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Health check attempt failed: {Message}", ex.Message);
                }

                await Task.Delay(10000); // Wait 10 seconds between checks
            }

            return false;
        }

        // Additional methods implementation...
    }
}
```

## Option B: PowerShell-based Update Agent

### Benefits
- **Native Windows tooling**
- **Built-in Docker management**
- **Easy customization**
- **No additional runtime dependencies**
- **Excellent system integration**

### Implementation

#### UpdateAgent.ps1 - Main PowerShell Script
```powershell
#Requires -Version 7.0

param(
    [string]$ConfigPath = "C:\CruiseShip\config\ship-config.json"
)

# Import required modules
Import-Module Microsoft.PowerShell.Utility

# Global configuration
$Global:Config = Get-Content $ConfigPath | ConvertFrom-Json
$Global:LogPath = "C:\CruiseShip\logs\update-agent.log"

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    
    Write-Host $logEntry
    Add-Content -Path $Global:LogPath -Value $logEntry
}

function Test-InternetConnection {
    try {
        $response = Invoke-WebRequest -Uri $Global:Config.communication.shoreEndpoint -Method HEAD -TimeoutSec 10
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

function Get-PendingUpdates {
    try {
        $uri = "$($Global:Config.communication.shoreEndpoint)/api/ships/$($Global:Config.shipId)/updates"
        $headers = @{
            'Authorization' = "Bearer $($Global:Config.communication.apiToken)"
            'Content-Type' = 'application/json'
        }
        
        $response = Invoke-RestMethod -Uri $uri -Headers $headers -Method GET
        return $response.pendingUpdates
    }
    catch {
        Write-Log "Failed to get pending updates: $($_.Exception.Message)" "ERROR"
        return $null
    }
}

function Invoke-ContainerUpdate {
    param(
        [object]$Update
    )
    
    Write-Log "Starting update $($Update.id): $($Update.description)"
    
    try {
        # Pull new image
        Write-Log "Pulling image: $($Update.containerImage)"
        docker pull $Update.containerImage
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to pull image"
        }
        
        # Stop current container
        Write-Log "Stopping container: $($Update.containerName)"
        docker stop $Update.containerName
        
        # Rename for backup
        $backupName = "$($Update.containerName)-backup-$(Get-Date -Format 'yyyyMMddHHmmss')"
        docker rename $Update.containerName $backupName
        
        # Start new container
        Write-Log "Starting new container: $($Update.containerName)"
        $dockerArgs = @(
            "run", "-d",
            "--name", $Update.containerName,
            "--network", "cruise-network"
        )
        
        # Add environment variables
        foreach ($env in $Update.containerConfig.environment) {
            $dockerArgs += "-e", $env
        }
        
        # Add port mappings
        foreach ($port in $Update.containerConfig.ports.PSObject.Properties) {
            $dockerArgs += "-p", "$($port.Value):$($port.Name)"
        }
        
        $dockerArgs += $Update.containerImage
        
        & docker @dockerArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to start new container"
        }
        
        # Health check
        if (Test-ContainerHealth -ContainerName $Update.containerName -TimeoutMinutes 5) {
            # Success - remove backup
            docker rm $backupName
            Send-UpdateStatus -UpdateId $Update.id -Status "success" -Message "Update completed successfully"
            Write-Log "Update $($Update.id) completed successfully"
        }
        else {
            # Failure - rollback
            Invoke-Rollback -ContainerName $Update.containerName -BackupName $backupName
            Send-UpdateStatus -UpdateId $Update.id -Status "failed" -Message "Health check failed, rolled back"
            Write-Log "Update $($Update.id) failed health check, rolled back" "ERROR"
        }
    }
    catch {
        Write-Log "Update $($Update.id) failed: $($_.Exception.Message)" "ERROR"
        Send-UpdateStatus -UpdateId $Update.id -Status "error" -Message $_.Exception.Message
    }
}

function Test-ContainerHealth {
    param(
        [string]$ContainerName,
        [int]$TimeoutMinutes = 5
    )
    
    $timeout = (Get-Date).AddMinutes($TimeoutMinutes)
    
    while ((Get-Date) -lt $timeout) {
        try {
            # Check if container is running
            $containerStatus = docker inspect --format='{{.State.Running}}' $ContainerName
            if ($containerStatus -eq "true") {
                # Check application health endpoint
                $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -TimeoutSec 10
                if ($response.StatusCode -eq 200) {
                    return $true
                }
            }
        }
        catch {
            Write-Log "Health check attempt failed: $($_.Exception.Message)" "DEBUG"
        }
        
        Start-Sleep -Seconds 30
    }
    
    return $false
}

function Invoke-Rollback {
    param(
        [string]$ContainerName,
        [string]$BackupName
    )
    
    Write-Log "Rolling back container: $ContainerName"
    
    try {
        # Stop and remove failed container
        docker stop $ContainerName
        docker rm $ContainerName
        
        # Restore backup
        docker rename $BackupName $ContainerName
        docker start $ContainerName
        
        Write-Log "Rollback completed successfully"
    }
    catch {
        Write-Log "Rollback failed: $($_.Exception.Message)" "ERROR"
    }
}

function Send-UpdateStatus {
    param(
        [string]$UpdateId,
        [string]$Status,
        [string]$Message
    )
    
    try {
        $uri = "$($Global:Config.communication.shoreEndpoint)/api/ships/$($Global:Config.shipId)/updates/$UpdateId/status"
        $headers = @{
            'Authorization' = "Bearer $($Global:Config.communication.apiToken)"
            'Content-Type' = 'application/json'
        }
        
        $body = @{
            status = $Status
            message = $Message
            timestamp = (Get-Date).ToUniversalTime().ToString("o")
            shipId = $Global:Config.shipId
        } | ConvertTo-Json
        
        Invoke-RestMethod -Uri $uri -Headers $headers -Method POST -Body $body
    }
    catch {
        Write-Log "Failed to send update status: $($_.Exception.Message)" "ERROR"
    }
}

function Test-MaintenanceWindow {
    $now = Get-Date
    $currentTime = $now.TimeOfDay
    $currentDay = $now.DayOfWeek
    
    foreach ($window in $Global:Config.deployment.maintenanceWindows) {
        if ($window.days -contains $currentDay.ToString()) {
            $startTime = [TimeSpan]::Parse($window.start)
            $endTime = [TimeSpan]::Parse($window.end)
            
            if ($currentTime -ge $startTime -and $currentTime -le $endTime) {
                return $true
            }
        }
    }
    
    return $false
}

function Send-HealthMetrics {
    try {
        $metrics = Get-SystemMetrics
        
        $uri = "$($Global:Config.communication.shoreEndpoint)/api/ships/$($Global:Config.shipId)/metrics"
        $headers = @{
            'Authorization' = "Bearer $($Global:Config.communication.apiToken)"
            'Content-Type' = 'application/json'
        }
        
        $body = $metrics | ConvertTo-Json -Depth 10
        Invoke-RestMethod -Uri $uri -Headers $headers -Method POST -Body $body
    }
    catch {
        Write-Log "Failed to send health metrics: $($_.Exception.Message)" "DEBUG"
    }
}

function Get-SystemMetrics {
    $cpuPercent = (Get-Counter '\Processor(_Total)\% Processor Time').CounterSamples[0].CookedValue
    $memory = Get-CimInstance -ClassName Win32_OperatingSystem
    $memoryPercent = (($memory.TotalVisibleMemorySize - $memory.FreePhysicalMemory) / $memory.TotalVisibleMemorySize) * 100
    
    $diskC = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='C:'"
    $diskPercent = (($diskC.Size - $diskC.FreeSpace) / $diskC.Size) * 100
    
    $containers = docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Image}}" | ConvertFrom-Csv -Delimiter "`t"
    
    return @{
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
        shipId = $Global:Config.shipId
        system = @{
            cpu_percent = [math]::Round($cpuPercent, 2)
            memory_percent = [math]::Round($memoryPercent, 2)
            disk_percent = [math]::Round($diskPercent, 2)
        }
        containers = $containers
    }
}

# Main execution loop
function Start-UpdateAgent {
    Write-Log "Starting Update Agent for ship $($Global:Config.shipId)"
    
    while ($true) {
        try {
            if (Test-InternetConnection) {
                $pendingUpdates = Get-PendingUpdates
                
                if ($pendingUpdates) {
                    Write-Log "Found $($pendingUpdates.Count) pending updates"
                    
                    foreach ($update in $pendingUpdates) {
                        if (Test-MaintenanceWindow) {
                            Invoke-ContainerUpdate -Update $update
                        }
                        else {
                            Write-Log "Update $($update.id) scheduled for maintenance window"
                        }
                    }
                }
                
                Send-HealthMetrics
            }
            else {
                Write-Log "No internet connection available" "DEBUG"
            }
        }
        catch {
            Write-Log "Error in main loop: $($_.Exception.Message)" "ERROR"
        }
        
        Start-Sleep -Seconds $Global:Config.updateAgent.checkInterval
    }
}

# Start the agent
Start-UpdateAgent
```

## Option C: Docker-based Update Agent

### Benefits
- **Consistent deployment**
- **Isolated execution**
- **Easy updates**
- **Cross-platform compatibility**

### Implementation

#### Dockerfile for .NET Update Agent
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine

WORKDIR /app

# Install Docker CLI
RUN apk add --no-cache docker-cli curl

# Copy application
COPY bin/Release/net9.0/publish/ .

# Create non-root user
RUN addgroup -g 1001 updateagent && \
    adduser -D -s /bin/sh -u 1001 -G updateagent updateagent

USER updateagent

ENTRYPOINT ["dotnet", "CruiseShip.UpdateAgent.dll"]
```

#### docker-compose.update-agent.yml
```yaml
version: '3.8'

services:
  update-agent:
    image: cruisefleetregistry.azurecr.io/update-agent:latest
    container_name: cruise-update-agent
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - ./config:/app/config:ro
      - ./logs:/app/logs
    environment:
      - DOTNET_ENVIRONMENT=Production
      - ShipConfig__ShipId=${SHIP_ID}
      - ShipConfig__ShipName=${SHIP_NAME}
    restart: unless-stopped
    networks:
      - cruise-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

networks:
  cruise-network:
    external: true
```

## Recommendation

**I recommend Option A (.NET-based Update Agent)** for the following reasons:

1. **Technology Alignment**: Matches your existing .NET Employee Management System
2. **Team Skills**: Leverages your existing .NET development expertise  
3. **Integration**: Easy to share models, utilities, and patterns with main application
4. **Debugging**: Familiar debugging tools and techniques
5. **Windows Native**: Excellent Windows Service integration
6. **Docker SDK**: Robust .NET Docker SDK for container management
7. **Async/Await**: Native async programming for better performance
8. **Type Safety**: Strong typing reduces runtime errors
9. **Tooling**: Excellent Visual Studio integration and IntelliSense

The .NET option provides the best balance of functionality, maintainability, and team productivity for your cruise ship deployment scenario.

Would you like me to create the complete .NET Update Agent project structure with all the necessary files?
