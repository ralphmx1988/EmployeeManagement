using CruiseShip.UpdateAgent.Models;

namespace CruiseShip.UpdateAgent.Interfaces;

public interface IShoreApiClient
{
    Task<List<UpdateRequest>?> GetPendingUpdatesAsync(string shipId, CancellationToken cancellationToken = default);
    Task<bool> SendUpdateStatusAsync(string shipId, string updateId, UpdateStatusRequest status, CancellationToken cancellationToken = default);
    Task<bool> SendHealthMetricsAsync(string shipId, HealthMetrics metrics, CancellationToken cancellationToken = default);
    Task<bool> TestConnectivityAsync(CancellationToken cancellationToken = default);
    Task<bool> RegisterShipAsync(string shipId, string shipName, CancellationToken cancellationToken = default);
}
