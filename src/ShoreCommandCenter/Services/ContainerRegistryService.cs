using Microsoft.EntityFrameworkCore;
using ShoreCommandCenter.Data;
using ShoreCommandCenter.Models;
using System.Security.Cryptography;
using System.Text;

namespace ShoreCommandCenter.Services;

public class ContainerRegistryService : IContainerRegistryService
{
    private readonly FleetDbContext _context;
    private readonly ILogger<ContainerRegistryService> _logger;
    private readonly IConfiguration _configuration;

    public ContainerRegistryService(
        FleetDbContext context, 
        ILogger<ContainerRegistryService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<UpdateRequest> CreateUpdateRequestAsync(UpdateRequestDto request)
    {
        var updateRequest = new UpdateRequest
        {
            Id = Guid.NewGuid().ToString(),
            Version = request.Version,
            ContainerImage = request.ContainerImage,
            ConfigurationJson = request.ConfigurationJson,
            Priority = request.Priority,
            ScheduledAt = request.ScheduledAt,
            ExpiresAt = request.ExpiresAt,
            Checksum = CalculateChecksum(request.ContainerImage + request.Version),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.UpdateRequests.Add(updateRequest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created update request {UpdateRequestId} for version {Version}", 
            updateRequest.Id, request.Version);

        return updateRequest;
    }

    public async Task<UpdateRequest?> GetLatestUpdateRequestAsync(string? currentVersion = null)
    {
        var query = _context.UpdateRequests
            .Where(ur => ur.Status == "Pending" && ur.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(currentVersion))
        {
            query = query.Where(ur => ur.Version != currentVersion);
        }

        return await query
            .OrderByDescending(ur => ur.Priority)
            .ThenByDescending(ur => ur.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UpdateRequest>> GetPendingUpdateRequestsAsync()
    {
        return await _context.UpdateRequests
            .Where(ur => ur.Status == "Pending" && ur.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(ur => ur.Priority)
            .ThenBy(ur => ur.ScheduledAt)
            .ToListAsync();
    }

    public async Task<UpdateRequest?> GetUpdateRequestAsync(string updateRequestId)
    {
        return await _context.UpdateRequests.FindAsync(updateRequestId);
    }

    public async Task<bool> ValidateContainerImageAsync(string containerImage)
    {
        try
        {
            // Basic validation - check if image name follows expected format
            if (string.IsNullOrWhiteSpace(containerImage))
            {
                return false;
            }

            // Check if image follows registry/repository:tag format
            var parts = containerImage.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            var imageName = parts[0];
            var tag = parts[1];

            // Validate image name format
            if (string.IsNullOrWhiteSpace(imageName) || string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            // Additional validation logic can be added here
            // For example, checking if the image exists in the registry
            
            _logger.LogDebug("Container image validation passed for {ContainerImage}", containerImage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating container image {ContainerImage}", containerImage);
            return false;
        }
    }

    public async Task<ContainerRegistryInfo> GetRegistryInfoAsync()
    {
        var registryUrl = _configuration["ContainerRegistry:Url"] ?? "localhost:5000";
        var registryType = _configuration["ContainerRegistry:Type"] ?? "Docker";
        var isSecure = bool.Parse(_configuration["ContainerRegistry:IsSecure"] ?? "false");

        // Get statistics
        var totalUpdateRequests = await _context.UpdateRequests.CountAsync();
        var pendingUpdates = await _context.UpdateRequests
            .CountAsync(ur => ur.Status == "Pending" && ur.ExpiresAt > DateTime.UtcNow);
        
        var recentUpdates = await _context.UpdateRequests
            .Where(ur => ur.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .CountAsync();

        // Get unique versions
        var availableVersions = await _context.UpdateRequests
            .Where(ur => ur.Status == "Pending" && ur.ExpiresAt > DateTime.UtcNow)
            .Select(ur => ur.Version)
            .Distinct()
            .OrderByDescending(v => v)
            .ToListAsync();

        return new ContainerRegistryInfo
        {
            RegistryUrl = registryUrl,
            RegistryType = registryType,
            IsSecure = isSecure,
            TotalUpdateRequests = totalUpdateRequests,
            PendingUpdates = pendingUpdates,
            RecentUpdates = recentUpdates,
            AvailableVersions = availableVersions,
            LastChecked = DateTime.UtcNow
        };
    }

    public async Task MarkUpdateRequestAsProcessedAsync(string updateRequestId, string status = "Processed")
    {
        var updateRequest = await _context.UpdateRequests.FindAsync(updateRequestId);
        if (updateRequest != null)
        {
            updateRequest.Status = status;
            updateRequest.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked update request {UpdateRequestId} as {Status}", 
                updateRequestId, status);
        }
    }

    public async Task CleanupExpiredRequestsAsync()
    {
        var expiredRequests = await _context.UpdateRequests
            .Where(ur => ur.ExpiresAt <= DateTime.UtcNow && ur.Status == "Pending")
            .ToListAsync();

        if (expiredRequests.Any())
        {
            foreach (var request in expiredRequests)
            {
                request.Status = "Expired";
                request.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Marked {Count} expired update requests as expired", 
                expiredRequests.Count);
        }
    }

    public async Task<IEnumerable<string>> GetAvailableVersionsAsync()
    {
        return await _context.UpdateRequests
            .Where(ur => ur.Status == "Pending" && ur.ExpiresAt > DateTime.UtcNow)
            .Select(ur => ur.Version)
            .Distinct()
            .OrderByDescending(v => v)
            .ToListAsync();
    }

    public async Task<bool> IsVersionAvailableAsync(string version)
    {
        return await _context.UpdateRequests
            .AnyAsync(ur => ur.Version == version && 
                           ur.Status == "Pending" && 
                           ur.ExpiresAt > DateTime.UtcNow);
    }

    private static string CalculateChecksum(string input)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
