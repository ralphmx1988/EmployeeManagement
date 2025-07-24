using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseShip.UpdateAgent.Models;
using CruiseShip.UpdateAgent.Interfaces;

namespace CruiseShip.UpdateAgent.Services;

public class UpdateReceiverService : BackgroundService
{
    private readonly ILogger<UpdateReceiverService> _logger;
    private readonly ShipConfiguration _config;
    private readonly IShoreApiClient _shoreClient;
    private readonly IUpdateOrchestrator _updateOrchestrator;

    public UpdateReceiverService(
        ILogger<UpdateReceiverService> logger,
        IOptions<ShipConfiguration> config,
        IShoreApiClient shoreClient,
        IUpdateOrchestrator updateOrchestrator)
    {
        _logger = logger;
        _config = config.Value;
        _shoreClient = shoreClient;
        _updateOrchestrator = updateOrchestrator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚢 Update Receiver Service starting for ship {ShipId} ({ShipName})", 
            _config.ShipId, _config.ShipName);

        // Register ship with shore command on startup
        try
        {
            await _shoreClient.RegisterShipAsync(_config.ShipId, _config.ShipName, stoppingToken);
            _logger.LogInformation("✅ Ship registered with shore command center");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register ship with shore command (will retry later)");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForUpdatesAsync(stoppingToken);
                await CleanupOldBackupsAsync(stoppingToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDebug("No internet connection available: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update receiver service");
            }

            var delay = TimeSpan.FromSeconds(_config.CheckIntervalSeconds);
            _logger.LogTrace("Next update check in {Delay}", delay);
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("🛑 Update Receiver Service stopping");
    }

    private async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking for pending updates...");
        
        var pendingUpdates = await _shoreClient.GetPendingUpdatesAsync(_config.ShipId, cancellationToken);
        
        if (pendingUpdates?.Any() == true)
        {
            _logger.LogInformation("📦 Found {Count} pending updates", pendingUpdates.Count);
            
            foreach (var update in pendingUpdates.OrderBy(u => u.Priority == "high" ? 0 : 1))
            {
                if (IsMaintenanceWindow() || update.Priority == "emergency")
                {
                    _logger.LogInformation("🔄 Processing update {UpdateId}: {Description}", 
                        update.Id, update.Description);
                    
                    var success = await _updateOrchestrator.ProcessUpdateAsync(update, cancellationToken);
                    
                    var status = new UpdateStatusRequest
                    {
                        Status = success ? "completed" : "failed",
                        Message = success ? "Update completed successfully" : "Update failed",
                        ShipId = _config.ShipId,
                        Timestamp = DateTime.UtcNow
                    };
                    
                    await _shoreClient.SendUpdateStatusAsync(_config.ShipId, update.Id, status, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("⏰ Update {UpdateId} scheduled for next maintenance window", update.Id);
                }
            }
        }
        else
        {
            _logger.LogTrace("No pending updates found");
        }
    }

    private bool IsMaintenanceWindow()
    {
        var now = DateTime.UtcNow;
        
        foreach (var window in _config.MaintenanceWindows)
        {
            if (window.IsInWindow(now))
            {
                _logger.LogTrace("Currently in maintenance window: {Days} {StartTime}-{EndTime} {TimeZone}",
                    string.Join(",", window.Days), window.StartTime, window.EndTime, window.TimeZone);
                return true;
            }
        }
        
        return false;
    }

    private async Task CleanupOldBackupsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _updateOrchestrator.CleanupOldBackupsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old backups");
        }
    }
}
