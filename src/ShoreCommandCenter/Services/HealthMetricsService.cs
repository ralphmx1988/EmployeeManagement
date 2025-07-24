using Microsoft.EntityFrameworkCore;
using ShoreCommandCenter.Data;
using ShoreCommandCenter.Models;

namespace ShoreCommandCenter.Services;

public class HealthMetricsService : IHealthMetricsService
{
    private readonly FleetDbContext _context;
    private readonly ILogger<HealthMetricsService> _logger;

    public HealthMetricsService(FleetDbContext context, ILogger<HealthMetricsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RecordHealthMetricsAsync(string shipId, HealthMetricsRequest metrics)
    {
        var shipMetrics = new ShipMetrics
        {
            Id = Guid.NewGuid().ToString(),
            ShipId = shipId,
            CpuUsage = metrics.CpuUsage,
            MemoryUsage = metrics.MemoryUsage,
            DiskUsage = metrics.DiskUsage,
            NetworkStatus = metrics.NetworkStatus,
            ContainerStatus = metrics.ContainerStatus,
            DatabaseStatus = metrics.DatabaseStatus,
            Timestamp = DateTime.UtcNow,
            AdditionalMetricsJson = metrics.AdditionalMetricsJson
        };

        _context.ShipMetrics.Add(shipMetrics);
        await _context.SaveChangesAsync();

        // Update ship's last seen timestamp
        var ship = await _context.Ships.FindAsync(shipId);
        if (ship != null)
        {
            ship.LastSeen = DateTime.UtcNow;
            ship.Status = DetermineShipStatus(metrics);
            ship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogDebug("Recorded health metrics for ship {ShipId}", shipId);
    }

    public async Task<IEnumerable<ShipMetrics>> GetShipMetricsAsync(string shipId, DateTime? since = null)
    {
        var query = _context.ShipMetrics.Where(m => m.ShipId == shipId);
        
        if (since.HasValue)
        {
            query = query.Where(m => m.Timestamp >= since.Value);
        }

        return await query
            .OrderByDescending(m => m.Timestamp)
            .Take(100) // Limit to last 100 metrics
            .ToListAsync();
    }

    public async Task<IEnumerable<ShipMetrics>> GetLatestMetricsForAllShipsAsync()
    {
        // Get the latest metric for each ship
        var latestMetrics = await _context.ShipMetrics
            .GroupBy(m => m.ShipId)
            .Select(g => g.OrderByDescending(m => m.Timestamp).First())
            .ToListAsync();

        return latestMetrics;
    }

    public async Task<FleetHealthSummary> GetFleetHealthSummaryAsync()
    {
        var ships = await _context.Ships.ToListAsync();
        var totalShips = ships.Count;
        var onlineShips = ships.Count(s => s.Status == "Online");
        var offlineShips = ships.Count(s => s.Status == "Offline");
        var warningShips = ships.Count(s => s.Status == "Warning");

        // Get recent metrics for health analysis
        var recentCutoff = DateTime.UtcNow.AddHours(-1);
        var recentMetrics = await _context.ShipMetrics
            .Where(m => m.Timestamp >= recentCutoff)
            .GroupBy(m => m.ShipId)
            .Select(g => g.OrderByDescending(m => m.Timestamp).First())
            .ToListAsync();

        var avgCpuUsage = recentMetrics.Any() ? recentMetrics.Average(m => m.CpuUsage ?? 0) : 0;
        var avgMemoryUsage = recentMetrics.Any() ? recentMetrics.Average(m => m.MemoryUsage ?? 0) : 0;
        var avgDiskUsage = recentMetrics.Any() ? recentMetrics.Average(m => m.DiskUsage ?? 0) : 0;

        // Get deployment statistics
        var activeDeployments = await _context.Deployments
            .Where(d => d.Status == "Pending" || d.Status == "InProgress" || d.Status == "Downloading")
            .CountAsync();

        var recentDeployments = await _context.Deployments
            .Where(d => d.StartedAt >= DateTime.UtcNow.AddDays(-7))
            .CountAsync();

        var failedDeployments = await _context.Deployments
            .Where(d => d.Status == "Failed" && d.StartedAt >= DateTime.UtcNow.AddDays(-7))
            .CountAsync();

        return new FleetHealthSummary
        {
            TotalShips = totalShips,
            OnlineShips = onlineShips,
            OfflineShips = offlineShips,
            WarningShips = warningShips,
            AverageCpuUsage = avgCpuUsage,
            AverageMemoryUsage = avgMemoryUsage,
            AverageDiskUsage = avgDiskUsage,
            ActiveDeployments = activeDeployments,
            RecentDeployments = recentDeployments,
            FailedDeployments = failedDeployments,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<ShipMetrics>> GetShipMetricsInRangeAsync(string shipId, DateTime startTime, DateTime endTime)
    {
        return await _context.ShipMetrics
            .Where(m => m.ShipId == shipId && m.Timestamp >= startTime && m.Timestamp <= endTime)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task CleanupOldMetricsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        
        var oldMetrics = await _context.ShipMetrics
            .Where(m => m.Timestamp < cutoffDate)
            .ToListAsync();

        if (oldMetrics.Any())
        {
            _context.ShipMetrics.RemoveRange(oldMetrics);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old metrics older than {CutoffDate}", 
                oldMetrics.Count, cutoffDate);
        }
    }

    private static string DetermineShipStatus(HealthMetricsRequest metrics)
    {
        // Determine ship status based on health metrics
        var criticalThresholds = new
        {
            CpuUsage = 90.0,
            MemoryUsage = 90.0,
            DiskUsage = 95.0
        };

        var warningThresholds = new
        {
            CpuUsage = 80.0,
            MemoryUsage = 80.0,
            DiskUsage = 85.0
        };

        // Check for critical conditions
        if ((metrics.CpuUsage >= criticalThresholds.CpuUsage) ||
            (metrics.MemoryUsage >= criticalThresholds.MemoryUsage) ||
            (metrics.DiskUsage >= criticalThresholds.DiskUsage) ||
            (metrics.DatabaseStatus != "Healthy") ||
            (metrics.ContainerStatus != "Running"))
        {
            return "Critical";
        }

        // Check for warning conditions
        if ((metrics.CpuUsage >= warningThresholds.CpuUsage) ||
            (metrics.MemoryUsage >= warningThresholds.MemoryUsage) ||
            (metrics.DiskUsage >= warningThresholds.DiskUsage) ||
            (metrics.NetworkStatus != "Connected"))
        {
            return "Warning";
        }

        return "Online";
    }
}
