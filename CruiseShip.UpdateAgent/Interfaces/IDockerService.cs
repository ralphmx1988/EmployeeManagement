using Docker.DotNet.Models;

namespace CruiseShip.UpdateAgent.Interfaces;

public interface IDockerService
{
    Task<bool> PullImageAsync(string imageTag, CancellationToken cancellationToken = default);
    Task<string?> CreateContainerAsync(string imageTag, string containerName, CreateContainerParameters parameters, CancellationToken cancellationToken = default);
    Task<bool> StartContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task<bool> StopContainerAsync(string containerId, int timeoutSeconds = 30, CancellationToken cancellationToken = default);
    Task<bool> RemoveContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task<bool> RenameContainerAsync(string containerId, string newName, CancellationToken cancellationToken = default);
    Task<ContainerInspectResponse?> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task<bool> HealthCheckAsync(string containerId, TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<IList<ContainerListResponse>> ListContainersAsync(ContainerListParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<ContainerStats?> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken = default);
}
