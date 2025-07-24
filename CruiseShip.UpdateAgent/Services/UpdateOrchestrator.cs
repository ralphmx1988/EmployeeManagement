using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseShip.UpdateAgent.Interfaces;
using CruiseShip.UpdateAgent.Models;
using System.IO.Compression;
using System.Text.Json;

namespace CruiseShip.UpdateAgent.Services;

public class UpdateOrchestrator : IUpdateOrchestrator
{
    private readonly ILogger<UpdateOrchestrator> _logger;
    private readonly ShipConfiguration _config;
    private readonly IDockerService _dockerService;
    private readonly IShoreApiClient _shoreClient;
    private readonly string _workingDirectory;
    private readonly string _backupDirectory;

    public UpdateOrchestrator(
        ILogger<UpdateOrchestrator> logger,
        IOptions<ShipConfiguration> config,
        IDockerService dockerService,
        IShoreApiClient shoreClient)
    {
        _logger = logger;
        _config = config.Value;
        _dockerService = dockerService;
        _shoreClient = shoreClient;
        
        _workingDirectory = Path.Combine(AppContext.BaseDirectory, "temp");
        _backupDirectory = Path.Combine(AppContext.BaseDirectory, "backups");
        
        Directory.CreateDirectory(_workingDirectory);
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<bool> ProcessUpdateAsync(UpdateRequest updateRequest, CancellationToken cancellationToken = default)
    {
        var updateId = updateRequest.Id;
        _logger.LogInformation("🔄 Starting update process for {UpdateId}: {Description}", updateId, updateRequest.Description);

        try
        {
            // Step 1: Download update package
            _logger.LogInformation("📦 Step 1: Downloading update package...");
            var packageData = await DownloadUpdatePackageAsync(updateRequest, cancellationToken);
            if (packageData == null)
            {
                _logger.LogError("Failed to download update package for {UpdateId}", updateId);
                return false;
            }

            // Step 2: Validate and extract package
            _logger.LogInformation("✅ Step 2: Validating and extracting package...");
            var extractedPath = await ExtractUpdatePackageAsync(updateId, packageData, cancellationToken);
            if (extractedPath == null)
            {
                _logger.LogError("Failed to extract update package for {UpdateId}", updateId);
                return false;
            }

            // Step 3: Parse update instructions
            _logger.LogInformation("📋 Step 3: Parsing update instructions...");
            var instructions = await ParseUpdateInstructionsAsync(extractedPath, cancellationToken);
            if (instructions == null)
            {
                _logger.LogError("Failed to parse update instructions for {UpdateId}", updateId);
                return false;
            }

            // Step 4: Create backups of affected containers
            _logger.LogInformation("💾 Step 4: Creating container backups...");
            var backupPaths = await CreateContainerBackupsAsync(instructions.ContainersToUpdate, cancellationToken);

            // Step 5: Execute update
            _logger.LogInformation("🚀 Step 5: Executing update...");
            var success = await ExecuteUpdateAsync(instructions, extractedPath, cancellationToken);

            if (success)
            {
                _logger.LogInformation("✅ Update {UpdateId} completed successfully", updateId);
                await CleanupTempFilesAsync(extractedPath, cancellationToken);
                return true;
            }
            else
            {
                _logger.LogError("❌ Update {UpdateId} failed, attempting rollback...", updateId);
                await RollbackUpdateAsync(instructions, backupPaths, cancellationToken);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during update {UpdateId}", updateId);
            return false;
        }
    }

    private async Task<byte[]?> DownloadUpdatePackageAsync(UpdateRequest updateRequest, CancellationToken cancellationToken)
    {
        try
        {
            var progress = new Progress<DownloadProgress>(p =>
            {
                if (p.PercentComplete > 0)
                    _logger.LogInformation("Download progress: {Progress:F1}% ({Downloaded}/{Total} MB)",
                        p.PercentComplete,
                        p.DownloadedBytes / 1024.0 / 1024.0,
                        p.TotalBytes / 1024.0 / 1024.0);
            });

            return await _shoreClient.DownloadUpdatePackageAsync(updateRequest.PackageUrl, progress, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download update package from {PackageUrl}", updateRequest.PackageUrl);
            return null;
        }
    }

    private async Task<string?> ExtractUpdatePackageAsync(string updateId, byte[] packageData, CancellationToken cancellationToken)
    {
        try
        {
            var extractPath = Path.Combine(_workingDirectory, updateId);
            
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);
            
            Directory.CreateDirectory(extractPath);

            // Save package to temp file
            var tempPackagePath = Path.Combine(_workingDirectory, $"{updateId}.zip");
            await File.WriteAllBytesAsync(tempPackagePath, packageData, cancellationToken);

            // Extract package
            ZipFile.ExtractToDirectory(tempPackagePath, extractPath);
            File.Delete(tempPackagePath);

            _logger.LogInformation("Package extracted to {ExtractPath}", extractPath);
            return extractPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract update package for {UpdateId}", updateId);
            return null;
        }
    }

    private async Task<UpdateInstructions?> ParseUpdateInstructionsAsync(string extractedPath, CancellationToken cancellationToken)
    {
        try
        {
            var instructionsPath = Path.Combine(extractedPath, "update.json");
            
            if (!File.Exists(instructionsPath))
            {
                _logger.LogError("Update instructions file not found: {InstructionsPath}", instructionsPath);
                return null;
            }

            var json = await File.ReadAllTextAsync(instructionsPath, cancellationToken);
            var instructions = JsonSerializer.Deserialize<UpdateInstructions>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("Parsed update instructions: {ContainerCount} containers to update",
                instructions?.ContainersToUpdate?.Count ?? 0);

            return instructions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse update instructions from {ExtractedPath}", extractedPath);
            return null;
        }
    }

    private async Task<Dictionary<string, string>> CreateContainerBackupsAsync(List<ContainerUpdateInfo>? containersToUpdate, CancellationToken cancellationToken)
    {
        var backupPaths = new Dictionary<string, string>();

        if (containersToUpdate == null || !containersToUpdate.Any())
            return backupPaths;

        try
        {
            var runningContainers = await _dockerService.GetRunningContainersAsync();

            foreach (var containerUpdate in containersToUpdate)
            {
                var container = runningContainers.FirstOrDefault(c => 
                    c.Name.Equals(containerUpdate.Name, StringComparison.OrdinalIgnoreCase) ||
                    c.Id.StartsWith(containerUpdate.Name, StringComparison.OrdinalIgnoreCase));

                if (container != null)
                {
                    try
                    {
                        _logger.LogInformation("💾 Creating backup for container {ContainerName}", container.Name);
                        var backupPath = await _dockerService.CreateContainerBackupAsync(container.Id);
                        backupPaths[container.Id] = backupPath;
                        _logger.LogInformation("✅ Backup created for {ContainerName}: {BackupPath}", container.Name, backupPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create backup for container {ContainerName}", container.Name);
                    }
                }
                else
                {
                    _logger.LogWarning("Container {ContainerName} not found for backup", containerUpdate.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container backups");
        }

        return backupPaths;
    }

    private async Task<bool> ExecuteUpdateAsync(UpdateInstructions instructions, string extractedPath, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var containerUpdate in instructions.ContainersToUpdate ?? new List<ContainerUpdateInfo>())
            {
                _logger.LogInformation("🔄 Updating container {ContainerName} to image {NewImage}",
                    containerUpdate.Name, containerUpdate.NewImage);

                // Pull new image
                if (!await _dockerService.PullImageAsync(containerUpdate.NewImage, containerUpdate.NewTag ?? "latest"))
                {
                    _logger.LogError("Failed to pull new image {NewImage}:{NewTag}",
                        containerUpdate.NewImage, containerUpdate.NewTag ?? "latest");
                    return false;
                }

                // Stop existing container
                var runningContainers = await _dockerService.GetRunningContainersAsync();
                var existingContainer = runningContainers.FirstOrDefault(c =>
                    c.Name.Equals(containerUpdate.Name, StringComparison.OrdinalIgnoreCase));

                if (existingContainer != null)
                {
                    if (!await _dockerService.StopContainerAsync(existingContainer.Id))
                    {
                        _logger.LogError("Failed to stop existing container {ContainerName}", containerUpdate.Name);
                        return false;
                    }

                    if (!await _dockerService.RemoveContainerAsync(existingContainer.Id))
                    {
                        _logger.LogError("Failed to remove existing container {ContainerName}", containerUpdate.Name);
                        return false;
                    }
                }

                // Create and start new container
                var fullImageName = $"{containerUpdate.NewImage}:{containerUpdate.NewTag ?? "latest"}";
                if (!await _dockerService.CreateAndStartContainerAsync(
                    fullImageName,
                    containerUpdate.Name,
                    containerUpdate.EnvironmentVariables,
                    containerUpdate.PortMappings))
                {
                    _logger.LogError("Failed to create and start new container {ContainerName}", containerUpdate.Name);
                    return false;
                }

                _logger.LogInformation("✅ Container {ContainerName} updated successfully", containerUpdate.Name);
            }

            // Execute custom scripts if provided
            if (instructions.CustomScripts?.Any() == true)
            {
                foreach (var script in instructions.CustomScripts)
                {
                    await ExecuteCustomScriptAsync(script, extractedPath, cancellationToken);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing update");
            return false;
        }
    }

    private async Task ExecuteCustomScriptAsync(string scriptPath, string extractedPath, CancellationToken cancellationToken)
    {
        try
        {
            var fullScriptPath = Path.Combine(extractedPath, scriptPath);
            
            if (!File.Exists(fullScriptPath))
            {
                _logger.LogWarning("Custom script not found: {ScriptPath}", fullScriptPath);
                return;
            }

            _logger.LogInformation("🔧 Executing custom script: {ScriptPath}", scriptPath);

            using var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{fullScriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("✅ Custom script executed successfully: {ScriptPath}", scriptPath);
                if (!string.IsNullOrEmpty(output))
                    _logger.LogDebug("Script output: {Output}", output);
            }
            else
            {
                _logger.LogError("❌ Custom script failed: {ScriptPath}, Exit Code: {ExitCode}", scriptPath, process.ExitCode);
                if (!string.IsNullOrEmpty(error))
                    _logger.LogError("Script error: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing custom script: {ScriptPath}", scriptPath);
        }
    }

    private async Task RollbackUpdateAsync(UpdateInstructions instructions, Dictionary<string, string> backupPaths, CancellationToken cancellationToken)
    {
        _logger.LogWarning("🔙 Starting rollback process...");

        try
        {
            foreach (var kvp in backupPaths)
            {
                var containerId = kvp.Key;
                var backupPath = kvp.Value;

                _logger.LogInformation("🔄 Restoring container from backup: {BackupPath}", backupPath);
                
                // This is a simplified rollback - in practice, you might need more sophisticated logic
                await _dockerService.RestoreContainerBackupAsync(backupPath, $"restored_{containerId[..12]}");
            }

            _logger.LogInformation("✅ Rollback completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during rollback process");
        }
    }

    private async Task CleanupTempFilesAsync(string extractedPath, CancellationToken cancellationToken)
    {
        try
        {
            if (Directory.Exists(extractedPath))
            {
                Directory.Delete(extractedPath, true);
                _logger.LogInformation("🧹 Cleaned up temporary files: {ExtractedPath}", extractedPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup temporary files: {ExtractedPath}", extractedPath);
        }
    }

    public async Task CleanupOldBackupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupDirectory, "*.tar")
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < DateTime.Now.AddDays(-_config.BackupRetentionDays))
                .ToList();

            foreach (var backupFile in backupFiles)
            {
                backupFile.Delete();
                _logger.LogInformation("🗑️ Deleted old backup: {BackupFile}", backupFile.Name);
            }

            if (backupFiles.Any())
            {
                _logger.LogInformation("✅ Cleaned up {Count} old backup files", backupFiles.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old backups");
        }
    }
}

public class UpdateInstructions
{
    public List<ContainerUpdateInfo>? ContainersToUpdate { get; set; }
    public List<string>? CustomScripts { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ContainerUpdateInfo
{
    public string Name { get; set; } = string.Empty;
    public string NewImage { get; set; } = string.Empty;
    public string? NewTag { get; set; }
    public Dictionary<string, string>? EnvironmentVariables { get; set; }
    public Dictionary<string, string>? PortMappings { get; set; }
}
