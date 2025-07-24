using CruiseShip.UpdateAgent.Models;

namespace CruiseShip.UpdateAgent.Interfaces;

public interface IHealthMonitor
{
    Task<HealthMetrics> CollectMetricsAsync(CancellationToken cancellationToken = default);
    Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken cancellationToken = default);
    Task<List<ContainerMetrics>> GetContainerMetricsAsync(CancellationToken cancellationToken = default);
    Task<NetworkMetrics> GetNetworkMetricsAsync(CancellationToken cancellationToken = default);
    Task<UpdateAgentMetrics> GetUpdateAgentMetricsAsync(CancellationToken cancellationToken = default);
}
