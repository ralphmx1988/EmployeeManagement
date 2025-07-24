using ShoreCommandCenter.Models;

namespace ShoreCommandCenter.Services;

public interface IShipService
{
    Task<Ship> RegisterShipAsync(ShipRegistrationRequest request);
    Task<Ship?> GetShipAsync(string shipId);
    Task<IEnumerable<Ship>> GetAllShipsAsync();
    Task<IEnumerable<Ship>> GetOutdatedShipsAsync();
    Task UpdateLastSeenAsync(string shipId);
    Task UpdateShipStatusAsync(string shipId, string status);
    Task UpdateShipVersionAsync(string shipId, string? currentVersion, string? targetVersion);
}

public interface IDeploymentService
{
    Task<Deployment> CreateDeploymentAsync(string shipId, DeploymentRequest request);
    Task<FleetDeployment> CreateFleetDeploymentAsync(FleetDeploymentRequest request);
    Task<Deployment> CreateRollbackAsync(string shipId, RollbackRequest request);
    Task<FleetDeployment> CreateFleetRollbackAsync(FleetRollbackRequest request);
    Task<IEnumerable<UpdateRequest>> GetPendingUpdatesAsync(string shipId);
    Task<IEnumerable<Deployment>> GetActiveDeploymentsAsync();
    Task UpdateDeploymentStatusAsync(Guid deploymentId, string status, string? message);
}

public interface IHealthMetricsService
{
    Task RecordHealthMetricsAsync(string shipId, HealthMetricsRequest metrics);
    Task<HealthMetricsSummary?> GetCurrentHealthAsync(string shipId);
    Task<IEnumerable<ShipMetrics>> GetHealthHistoryAsync(string shipId, int days);
}

public interface IFleetService
{
    Task<FleetStatusResponse> GetFleetStatusAsync();
    Task<object> GetFleetOverviewAsync();
    Task<object> GetFleetHealthAsync();
    Task<IEnumerable<object>> GetCriticalAlertsAsync();
}

public interface IContainerRegistryService
{
    Task<bool> ImageExistsAsync(string imageName, string tag);
    Task<string> GetImageDigestAsync(string imageName, string tag);
    Task<IEnumerable<string>> GetAvailableTagsAsync(string imageName);
}

public interface INotificationService
{
    Task SendFleetNotificationAsync(string message, string severity = "info");
    Task SendShipNotificationAsync(string shipId, string message, string severity = "info");
}
