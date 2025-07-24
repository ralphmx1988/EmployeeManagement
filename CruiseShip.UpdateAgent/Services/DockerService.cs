using Microsoft.Extensions.Logging;
using Docker.DotNet;
using Docker.DotNet.Models;
using CruiseShip.UpdateAgent.Interfaces;
using CruiseShip.UpdateAgent.Models;
using System.Text;
using System.IO.Compression;

namespace CruiseShip.UpdateAgent.Services;

public class DockerService : IDockerService
{
    private readonly ILogger<DockerService> _logger;
    private readonly DockerClient _dockerClient;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            await _dockerClient.System.PingAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker is not running or not accessible");
            return false;
        }
    }

    public async Task<List<ContainerInfo>> GetRunningContainersAsync()
    {
        try
        {
            var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = false // Only running containers
            });

            return containers.Select(c => new ContainerInfo
            {
                Id = c.ID,
                Name = c.Names?.FirstOrDefault()?.TrimStart('/') ?? "unnamed",
                Image = c.Image,
                Status = c.Status,
                Ports = c.Ports?.Select(p => $"{p.PublicPort}:{p.PrivatePort}").ToList() ?? new List<string>(),
                Labels = c.Labels ?? new Dictionary<string, string>()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get running containers");
            return new List<ContainerInfo>();
        }
    }

    public async Task<bool> StopContainerAsync(string containerId)
    {
        try
        {
            _logger.LogInformation("🛑 Stopping container {ContainerId}", containerId);
            
            await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 30 // Grace period
            });
            
            _logger.LogInformation("✅ Container {ContainerId} stopped successfully", containerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop container {ContainerId}", containerId);
            return false;
        }
    }

    public async Task<bool> StartContainerAsync(string containerId)
    {
        try
        {
            _logger.LogInformation("▶️ Starting container {ContainerId}", containerId);
            
            await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
            
            _logger.LogInformation("✅ Container {ContainerId} started successfully", containerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start container {ContainerId}", containerId);
            return false;
        }
    }

    public async Task<bool> PullImageAsync(string imageName, string tag = "latest", IProgress<JSONMessage>? progress = null)
    {
        try
        {
            _logger.LogInformation("⬇️ Pulling image {ImageName}:{Tag}", imageName, tag);
            
            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = imageName,
                    Tag = tag
                },
                new AuthConfig(), // Add authentication if needed
                progress ?? new Progress<JSONMessage>(message => 
                {
                    if (!string.IsNullOrEmpty(message.Status))
                        _logger.LogTrace("Docker pull: {Status}", message.Status);
                }));
            
            _logger.LogInformation("✅ Image {ImageName}:{Tag} pulled successfully", imageName, tag);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pull image {ImageName}:{Tag}", imageName, tag);
            return false;
        }
    }

    public async Task<string> CreateContainerBackupAsync(string containerId)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupPath = Path.Combine("backups", $"container_{containerId[..12]}_{timestamp}.tar");
            
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
            
            _logger.LogInformation("💾 Creating backup of container {ContainerId} to {BackupPath}", containerId, backupPath);
            
            // Export the container to a tar file
            using var exportStream = await _dockerClient.Containers.ExportContainerAsync(containerId);
            using var fileStream = File.Create(backupPath);
            await exportStream.CopyToAsync(fileStream);
            
            _logger.LogInformation("✅ Container backup created: {BackupPath}", backupPath);
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for container {ContainerId}", containerId);
            throw;
        }
    }

    public async Task<bool> RestoreContainerBackupAsync(string backupPath, string newContainerName)
    {
        try
        {
            _logger.LogInformation("🔄 Restoring container from backup {BackupPath} as {ContainerName}", backupPath, newContainerName);
            
            if (!File.Exists(backupPath))
            {
                _logger.LogError("Backup file not found: {BackupPath}", backupPath);
                return false;
            }
            
            // Import the container from tar file
            using var fileStream = File.OpenRead(backupPath);
            
            var importResponse = await _dockerClient.Images.LoadImageAsync(fileStream);
            
            // Parse the import response to get the image ID
            // Note: This is a simplified example. In practice, you might need more sophisticated parsing
            
            _logger.LogInformation("✅ Container restored from backup: {BackupPath}", backupPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore container from backup {BackupPath}", backupPath);
            return false;
        }
    }

    public async Task<bool> RemoveContainerAsync(string containerId)
    {
        try
        {
            _logger.LogInformation("🗑️ Removing container {ContainerId}", containerId);
            
            await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
            {
                Force = true, // Force removal even if running
                RemoveVolumes = false // Preserve volumes for data safety
            });
            
            _logger.LogInformation("✅ Container {ContainerId} removed successfully", containerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove container {ContainerId}", containerId);
            return false;
        }
    }

    public async Task<bool> CreateAndStartContainerAsync(string imageName, string containerName, Dictionary<string, string>? environmentVariables = null, Dictionary<string, string>? portMappings = null)
    {
        try
        {
            _logger.LogInformation("🚀 Creating and starting container {ContainerName} from image {ImageName}", containerName, imageName);
            
            var createParams = new CreateContainerParameters
            {
                Image = imageName,
                Name = containerName,
                Env = environmentVariables?.Select(kv => $"{kv.Key}={kv.Value}").ToList() ?? new List<string>(),
                HostConfig = new HostConfig
                {
                    PortBindings = portMappings?.ToDictionary(
                        kv => kv.Key,
                        kv => (IList<PortBinding>)new List<PortBinding> { new() { HostPort = kv.Value } }
                    ) ?? new Dictionary<string, IList<PortBinding>>(),
                    RestartPolicy = new RestartPolicy { Name = RestartPolicyKind.UnlessStopped }
                }
            };
            
            var response = await _dockerClient.Containers.CreateContainerAsync(createParams);
            
            if (response.Warnings?.Any() == true)
            {
                foreach (var warning in response.Warnings)
                {
                    _logger.LogWarning("Docker warning: {Warning}", warning);
                }
            }
            
            // Start the container
            await _dockerClient.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
            
            _logger.LogInformation("✅ Container {ContainerName} created and started with ID {ContainerId}", containerName, response.ID);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create and start container {ContainerName}", containerName);
            return false;
        }
    }

    public void Dispose()
    {
        _dockerClient?.Dispose();
    }
}
