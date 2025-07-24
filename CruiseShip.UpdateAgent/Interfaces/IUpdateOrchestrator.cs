using CruiseShip.UpdateAgent.Models;

namespace CruiseShip.UpdateAgent.Interfaces;

public interface IUpdateOrchestrator
{
    Task<bool> ProcessUpdateAsync(UpdateRequest update, CancellationToken cancellationToken = default);
    Task<bool> RollbackUpdateAsync(string containerId, string backupContainerId, CancellationToken cancellationToken = default);
    Task<bool> ValidateUpdateAsync(UpdateRequest update, CancellationToken cancellationToken = default);
    Task CleanupOldBackupsAsync(CancellationToken cancellationToken = default);
}
