using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseShip.UpdateAgent.Models;
using CruiseShip.UpdateAgent.Interfaces;

namespace CruiseShip.UpdateAgent.Services;

public class HealthReportingService : BackgroundService
{
    private readonly ILogger<HealthReportingService> _logger;
    private readonly ShipConfiguration _config;
    private readonly IShoreApiClient _shoreClient;
    private readonly IHealthMonitor _healthMonitor;

    public HealthReportingService(
        ILogger<HealthReportingService> logger,
        IOptions<ShipConfiguration> config,
        IShoreApiClient shoreClient,
        IHealthMonitor healthMonitor)
    {
        _logger = logger;
        _config = config.Value;
        _shoreClient = shoreClient;
        _healthMonitor = healthMonitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📊 Health Reporting Service starting for ship {ShipId}", _config.ShipId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectAndSendHealthMetricsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in health reporting service");
            }

            var delay = TimeSpan.FromSeconds(_config.HealthReportIntervalSeconds);
            _logger.LogTrace("Next health report in {Delay}", delay);
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("🛑 Health Reporting Service stopping");
    }

    private async Task CollectAndSendHealthMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogTrace("Collecting health metrics...");
            var metrics = await _healthMonitor.CollectHealthMetricsAsync();

            _logger.LogTrace("Sending health metrics to shore command...");
            var success = await _shoreClient.SendHealthMetricsAsync(_config.ShipId, metrics, cancellationToken);

            if (success)
            {
                _logger.LogTrace("✅ Health metrics sent successfully");
            }
            else
            {
                _logger.LogDebug("Failed to send health metrics (likely no internet connection)");
            }

            // Log critical health issues locally
            LogCriticalHealthIssues(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting and sending health metrics");
        }
    }

    private void LogCriticalHealthIssues(HealthMetrics metrics)
    {
        var issues = new List<string>();

        if (metrics.CpuUsagePercent > 90)
            issues.Add($"High CPU usage: {metrics.CpuUsagePercent:F1}%");

        if (metrics.MemoryUsagePercent > 90)
            issues.Add($"High memory usage: {metrics.MemoryUsagePercent:F1}%");

        if (metrics.DiskSpaceAvailableGB < 5)
            issues.Add($"Low disk space: {metrics.DiskSpaceAvailableGB:F1}GB remaining");

        if (metrics.DockerStatus != "running")
            issues.Add($"Docker is not running: {metrics.DockerStatus}");

        var unhealthyContainers = metrics.RunningContainers?
            .Where(c => c.HealthStatus == "unhealthy")
            .ToList();

        if (unhealthyContainers?.Any() == true)
        {
            foreach (var container in unhealthyContainers)
            {
                issues.Add($"Unhealthy container: {container.Name} ({container.Image})");
            }
        }

        if (issues.Any())
        {
            _logger.LogWarning("⚠️ Critical health issues detected: {Issues}", string.Join(", ", issues));
        }
        else
        {
            _logger.LogTrace("💚 All health metrics within normal ranges");
        }
    }
}
