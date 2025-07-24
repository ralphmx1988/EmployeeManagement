using Microsoft.AspNetCore.SignalR;
using ShoreCommandCenter.Models;

namespace ShoreCommandCenter.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<FleetMonitoringHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<FleetMonitoringHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyFleetStatusUpdateAsync(FleetStatus fleetStatus)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("FleetStatusUpdate", fleetStatus);
            _logger.LogDebug("Sent fleet status update notification");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending fleet status update notification");
        }
    }

    public async Task NotifyShipStatusUpdateAsync(string shipId, string status)
    {
        try
        {
            var notification = new
            {
                ShipId = shipId,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("ShipStatusUpdate", notification);
            _logger.LogDebug("Sent ship status update notification for ship {ShipId}", shipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ship status update notification for ship {ShipId}", shipId);
        }
    }

    public async Task NotifyDeploymentUpdateAsync(string deploymentId, string status, string? shipId = null)
    {
        try
        {
            var notification = new
            {
                DeploymentId = deploymentId,
                ShipId = shipId,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("DeploymentUpdate", notification);
            _logger.LogDebug("Sent deployment update notification for deployment {DeploymentId}", deploymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending deployment update notification for deployment {DeploymentId}", deploymentId);
        }
    }

    public async Task NotifyFleetDeploymentUpdateAsync(string fleetDeploymentId, DeploymentProgress progress)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("FleetDeploymentUpdate", new
            {
                FleetDeploymentId = fleetDeploymentId,
                Progress = progress,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogDebug("Sent fleet deployment update notification for fleet deployment {FleetDeploymentId}", fleetDeploymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending fleet deployment update notification for fleet deployment {FleetDeploymentId}", fleetDeploymentId);
        }
    }

    public async Task NotifyHealthMetricsUpdateAsync(string shipId, ShipMetrics metrics)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("HealthMetricsUpdate", new
            {
                ShipId = shipId,
                Metrics = new
                {
                    metrics.CpuUsage,
                    metrics.MemoryUsage,
                    metrics.DiskUsage,
                    metrics.NetworkStatus,
                    metrics.ContainerStatus,
                    metrics.DatabaseStatus,
                    metrics.Timestamp
                }
            });
            
            _logger.LogDebug("Sent health metrics update notification for ship {ShipId}", shipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending health metrics update notification for ship {ShipId}", shipId);
        }
    }

    public async Task NotifyAlertAsync(string shipId, AlertLevel level, string message)
    {
        try
        {
            var alert = new
            {
                ShipId = shipId,
                Level = level.ToString(),
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("Alert", alert);
            _logger.LogInformation("Sent {Level} alert for ship {ShipId}: {Message}", level, shipId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert notification for ship {ShipId}", shipId);
        }
    }

    public async Task NotifyShipConnectionAsync(string shipId, bool isConnected)
    {
        try
        {
            var notification = new
            {
                ShipId = shipId,
                IsConnected = isConnected,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("ShipConnection", notification);
            _logger.LogDebug("Sent ship connection notification for ship {ShipId}: {IsConnected}", shipId, isConnected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ship connection notification for ship {ShipId}", shipId);
        }
    }

    public async Task NotifySystemEventAsync(string eventType, string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                EventType = eventType,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("SystemEvent", notification);
            _logger.LogInformation("Sent system event notification: {EventType} - {Message}", eventType, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system event notification: {EventType}", eventType);
        }
    }
}

public enum AlertLevel
{
    Info,
    Warning,
    Error,
    Critical
}
