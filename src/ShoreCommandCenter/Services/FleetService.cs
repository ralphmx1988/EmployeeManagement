using Microsoft.EntityFrameworkCore;
using ShoreCommandCenter.Data;
using ShoreCommandCenter.Models;

namespace ShoreCommandCenter.Services;

public class FleetService : IFleetService
{
    private readonly FleetDbContext _context;
    private readonly IDeploymentService _deploymentService;
    private readonly ILogger<FleetService> _logger;

    public FleetService(
        FleetDbContext context, 
        IDeploymentService deploymentService,
        ILogger<FleetService> logger)
    {
        _context = context;
        _deploymentService = deploymentService;
        _logger = logger;
    }

    public async Task<FleetStatus> GetFleetStatusAsync()
    {
        var ships = await _context.Ships.ToListAsync();
        var cutoffTime = DateTime.UtcNow.AddMinutes(-10); // Consider ships offline after 10 minutes

        var totalShips = ships.Count;
        var onlineShips = ships.Count(s => s.LastSeen >= cutoffTime);
        var offlineShips = totalShips - onlineShips;

        // Get active deployments
        var activeDeployments = await _context.Deployments
            .Where(d => d.Status == "Pending" || d.Status == "InProgress" || d.Status == "Downloading")
            .CountAsync();

        // Get version distribution
        var versionGroups = ships
            .Where(s => !string.IsNullOrEmpty(s.CurrentVersion))
            .GroupBy(s => s.CurrentVersion)
            .Select(g => new { Version = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new FleetStatus
        {
            TotalShips = totalShips,
            OnlineShips = onlineShips,
            OfflineShips = offlineShips,
            ActiveDeployments = activeDeployments,
            VersionDistribution = versionGroups.ToDictionary(x => x.Version!, x => x.Count),
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<FleetDeployment> DeployToFleetAsync(FleetDeploymentRequest request)
    {
        // Get target ships
        var targetShips = new List<string>();
        
        if (request.ShipIds != null && request.ShipIds.Any())
        {
            // Deploy to specific ships
            targetShips = request.ShipIds.ToList();
        }
        else
        {
            // Deploy to all online ships
            var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
            var onlineShips = await _context.Ships
                .Where(s => s.LastSeen >= cutoffTime)
                .Select(s => s.Id)
                .ToListAsync();
            
            targetShips = onlineShips;
        }

        if (!targetShips.Any())
        {
            throw new InvalidOperationException("No target ships found for deployment");
        }

        // Create fleet deployment with target ships
        var fleetRequest = new FleetDeploymentRequest
        {
            Version = request.Version,
            ContainerImage = request.ContainerImage,
            ConfigurationJson = request.ConfigurationJson,
            ShipIds = targetShips
        };

        var fleetDeployment = await _deploymentService.CreateFleetDeploymentAsync(fleetRequest);

        _logger.LogInformation("Initiated fleet deployment {FleetDeploymentId} to {ShipCount} ships", 
            fleetDeployment.Id, targetShips.Count);

        return fleetDeployment;
    }

    public async Task<IEnumerable<Ship>> GetShipsByStatusAsync(string status)
    {
        var query = _context.Ships.AsQueryable();
        
        if (status.ToLower() == "online")
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
            query = query.Where(s => s.LastSeen >= cutoffTime);
        }
        else if (status.ToLower() == "offline")
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
            query = query.Where(s => s.LastSeen < cutoffTime);
        }
        else
        {
            query = query.Where(s => s.Status == status);
        }

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ship>> GetShipsByVersionAsync(string version)
    {
        return await _context.Ships
            .Where(s => s.CurrentVersion == version)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetVersionDistributionAsync()
    {
        var versionGroups = await _context.Ships
            .Where(s => !string.IsNullOrEmpty(s.CurrentVersion))
            .GroupBy(s => s.CurrentVersion)
            .Select(g => new { Version = g.Key, Count = g.Count() })
            .ToListAsync();

        return versionGroups.ToDictionary(x => x.Version!, x => x.Count);
    }

    public async Task<FleetMetricsSummary> GetFleetMetricsSummaryAsync()
    {
        // Get latest metrics for each ship
        var latestMetrics = await _context.ShipMetrics
            .GroupBy(m => m.ShipId)
            .Select(g => g.OrderByDescending(m => m.Timestamp).First())
            .ToListAsync();

        if (!latestMetrics.Any())
        {
            return new FleetMetricsSummary
            {
                TotalShips = await _context.Ships.CountAsync(),
                ShipsWithMetrics = 0,
                AverageCpuUsage = 0,
                AverageMemoryUsage = 0,
                AverageDiskUsage = 0,
                HealthyShips = 0,
                UnhealthyShips = 0,
                LastUpdated = DateTime.UtcNow
            };
        }

        var totalShips = await _context.Ships.CountAsync();
        var shipsWithMetrics = latestMetrics.Count;
        var avgCpuUsage = latestMetrics.Average(m => m.CpuUsage ?? 0);
        var avgMemoryUsage = latestMetrics.Average(m => m.MemoryUsage ?? 0);
        var avgDiskUsage = latestMetrics.Average(m => m.DiskUsage ?? 0);

        var healthyShips = latestMetrics.Count(m => 
            (m.CpuUsage ?? 0) < 80 && 
            (m.MemoryUsage ?? 0) < 80 && 
            (m.DiskUsage ?? 0) < 85 &&
            m.DatabaseStatus == "Healthy" &&
            m.ContainerStatus == "Running");

        var unhealthyShips = latestMetrics.Count - healthyShips;

        return new FleetMetricsSummary
        {
            TotalShips = totalShips,
            ShipsWithMetrics = shipsWithMetrics,
            AverageCpuUsage = avgCpuUsage,
            AverageMemoryUsage = avgMemoryUsage,
            AverageDiskUsage = avgDiskUsage,
            HealthyShips = healthyShips,
            UnhealthyShips = unhealthyShips,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<Ship>> GetShipsRequiringUpdatesAsync(string targetVersion)
    {
        return await _context.Ships
            .Where(s => s.CurrentVersion != targetVersion || string.IsNullOrEmpty(s.CurrentVersion))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> IsFleetDeploymentCompleteAsync(string fleetDeploymentId)
    {
        var fleetDeployment = await _context.FleetDeployments
            .Include(fd => fd.Deployments)
            .FirstOrDefaultAsync(fd => fd.Id == fleetDeploymentId);

        if (fleetDeployment == null)
        {
            return false;
        }

        var totalDeployments = fleetDeployment.Deployments.Count;
        var completedDeployments = fleetDeployment.Deployments.Count(d => 
            d.Status == "Completed" || d.Status == "Failed");

        return completedDeployments == totalDeployments;
    }

    public async Task<DeploymentProgress> GetFleetDeploymentProgressAsync(string fleetDeploymentId)
    {
        var fleetDeployment = await _context.FleetDeployments
            .Include(fd => fd.Deployments)
                .ThenInclude(d => d.Ship)
            .FirstOrDefaultAsync(fd => fd.Id == fleetDeploymentId);

        if (fleetDeployment == null)
        {
            throw new ArgumentException($"Fleet deployment {fleetDeploymentId} not found");
        }

        var deployments = fleetDeployment.Deployments.ToList();
        var total = deployments.Count;
        var completed = deployments.Count(d => d.Status == "Completed");
        var failed = deployments.Count(d => d.Status == "Failed");
        var inProgress = deployments.Count(d => d.Status == "InProgress" || d.Status == "Downloading");
        var pending = deployments.Count(d => d.Status == "Pending");

        var progressPercentage = total > 0 ? (double)(completed + failed) / total * 100 : 0;

        return new DeploymentProgress
        {
            FleetDeploymentId = fleetDeploymentId,
            TotalShips = total,
            CompletedShips = completed,
            FailedShips = failed,
            InProgressShips = inProgress,
            PendingShips = pending,
            ProgressPercentage = progressPercentage,
            StartedAt = fleetDeployment.StartedAt,
            IsComplete = (completed + failed) == total,
            Ships = deployments.Select(d => new ShipDeploymentStatus
            {
                ShipId = d.ShipId,
                ShipName = d.Ship?.Name ?? "Unknown",
                Status = d.Status,
                StartedAt = d.StartedAt,
                CompletedAt = d.CompletedAt,
                ErrorMessage = d.ErrorMessage
            }).ToList()
        };
    }
}
