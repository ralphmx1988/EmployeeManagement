using Microsoft.EntityFrameworkCore;
using ShoreCommandCenter.Data;
using ShoreCommandCenter.Models;

namespace ShoreCommandCenter.Services;

public class DeploymentService : IDeploymentService
{
    private readonly FleetDbContext _context;
    private readonly ILogger<DeploymentService> _logger;

    public DeploymentService(FleetDbContext context, ILogger<DeploymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Deployment> CreateDeploymentAsync(DeploymentRequest request)
    {
        var deployment = new Deployment
        {
            Id = Guid.NewGuid().ToString(),
            ShipId = request.ShipId,
            Version = request.Version,
            Status = "Pending",
            StartedAt = DateTime.UtcNow,
            ContainerImage = request.ContainerImage,
            ConfigurationJson = request.ConfigurationJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Deployments.Add(deployment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created deployment {DeploymentId} for ship {ShipId} with version {Version}", 
            deployment.Id, request.ShipId, request.Version);

        return deployment;
    }

    public async Task<FleetDeployment> CreateFleetDeploymentAsync(FleetDeploymentRequest request)
    {
        var fleetDeployment = new FleetDeployment
        {
            Id = Guid.NewGuid().ToString(),
            Version = request.Version,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow,
            TotalShips = request.ShipIds?.Count ?? 0,
            ContainerImage = request.ContainerImage,
            ConfigurationJson = request.ConfigurationJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FleetDeployments.Add(fleetDeployment);
        await _context.SaveChangesAsync();

        // Create individual deployments for each ship
        if (request.ShipIds != null)
        {
            foreach (var shipId in request.ShipIds)
            {
                var deployment = new Deployment
                {
                    Id = Guid.NewGuid().ToString(),
                    ShipId = shipId,
                    Version = request.Version,
                    Status = "Pending",
                    StartedAt = DateTime.UtcNow,
                    ContainerImage = request.ContainerImage,
                    ConfigurationJson = request.ConfigurationJson,
                    FleetDeploymentId = fleetDeployment.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Deployments.Add(deployment);
            }

            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Created fleet deployment {FleetDeploymentId} for {ShipCount} ships with version {Version}", 
            fleetDeployment.Id, request.ShipIds?.Count, request.Version);

        return fleetDeployment;
    }

    public async Task<Deployment?> GetDeploymentAsync(string deploymentId)
    {
        return await _context.Deployments
            .Include(d => d.Ship)
            .FirstOrDefaultAsync(d => d.Id == deploymentId);
    }

    public async Task<IEnumerable<Deployment>> GetShipDeploymentsAsync(string shipId)
    {
        return await _context.Deployments
            .Where(d => d.ShipId == shipId)
            .OrderByDescending(d => d.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Deployment>> GetActiveDeploymentsAsync()
    {
        return await _context.Deployments
            .Include(d => d.Ship)
            .Where(d => d.Status == "Pending" || d.Status == "InProgress" || d.Status == "Downloading")
            .OrderBy(d => d.StartedAt)
            .ToListAsync();
    }

    public async Task UpdateDeploymentStatusAsync(string deploymentId, string status, string? errorMessage = null)
    {
        var deployment = await _context.Deployments.FindAsync(deploymentId);
        if (deployment != null)
        {
            deployment.Status = status;
            deployment.ErrorMessage = errorMessage;
            deployment.UpdatedAt = DateTime.UtcNow;

            if (status == "Completed")
            {
                deployment.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Update fleet deployment status if this deployment belongs to one
            if (!string.IsNullOrEmpty(deployment.FleetDeploymentId))
            {
                await UpdateFleetDeploymentStatusAsync(deployment.FleetDeploymentId);
            }

            _logger.LogInformation("Updated deployment {DeploymentId} status to {Status}", deploymentId, status);
        }
    }

    public async Task<FleetDeployment?> GetFleetDeploymentAsync(string fleetDeploymentId)
    {
        return await _context.FleetDeployments
            .Include(fd => fd.Deployments)
                .ThenInclude(d => d.Ship)
            .FirstOrDefaultAsync(fd => fd.Id == fleetDeploymentId);
    }

    public async Task<IEnumerable<FleetDeployment>> GetRecentFleetDeploymentsAsync(int count = 10)
    {
        return await _context.FleetDeployments
            .Include(fd => fd.Deployments)
                .ThenInclude(d => d.Ship)
            .OrderByDescending(fd => fd.StartedAt)
            .Take(count)
            .ToListAsync();
    }

    private async Task UpdateFleetDeploymentStatusAsync(string fleetDeploymentId)
    {
        var fleetDeployment = await _context.FleetDeployments
            .Include(fd => fd.Deployments)
            .FirstOrDefaultAsync(fd => fd.Id == fleetDeploymentId);

        if (fleetDeployment == null) return;

        var deployments = fleetDeployment.Deployments.ToList();
        var completedCount = deployments.Count(d => d.Status == "Completed");
        var failedCount = deployments.Count(d => d.Status == "Failed");
        var totalCount = deployments.Count;

        fleetDeployment.CompletedShips = completedCount;
        fleetDeployment.FailedShips = failedCount;
        fleetDeployment.UpdatedAt = DateTime.UtcNow;

        // Determine overall status
        if (completedCount + failedCount == totalCount)
        {
            fleetDeployment.Status = failedCount == 0 ? "Completed" : "PartiallyFailed";
            fleetDeployment.CompletedAt = DateTime.UtcNow;
        }
        else if (failedCount > 0)
        {
            fleetDeployment.Status = "PartiallyFailed";
        }

        await _context.SaveChangesAsync();
    }
}
