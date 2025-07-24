using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseShip.UpdateAgent.Interfaces;
using CruiseShip.UpdateAgent.Models;
using System.Text.Json;
using System.Text;

namespace CruiseShip.UpdateAgent.Services;

public class ShoreApiClient : IShoreApiClient
{
    private readonly ILogger<ShoreApiClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly ShipConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public ShoreApiClient(ILogger<ShoreApiClient> logger, HttpClient httpClient, IOptions<ShipConfiguration> config)
    {
        _logger = logger;
        _httpClient = httpClient;
        _config = config.Value;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_config.ShoreCommandUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Add authentication headers if configured
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);
        }
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"CruiseShip-UpdateAgent/{_config.ShipId}");
    }

    public async Task<bool> RegisterShipAsync(string shipId, string shipName, CancellationToken cancellationToken = default)
    {
        try
        {
            var registrationData = new
            {
                ShipId = shipId,
                ShipName = shipName,
                AgentVersion = "1.0.0",
                LastSeen = DateTime.UtcNow,
                Capabilities = new[]
                {
                    "docker-updates",
                    "health-monitoring",
                    "remote-deployment"
                }
            };

            var json = JsonSerializer.Serialize(registrationData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Registering ship {ShipId} with shore command", shipId);
            
            var response = await _httpClient.PostAsync("/api/ships/register", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Ship {ShipId} registered successfully", shipId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to register ship {ShipId}. Status: {StatusCode}, Error: {Error}", 
                    shipId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No internet connection available for ship registration");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering ship {ShipId}", shipId);
            return false;
        }
    }

    public async Task<List<UpdateRequest>?> GetPendingUpdatesAsync(string shipId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogTrace("Checking for pending updates for ship {ShipId}", shipId);
            
            var response = await _httpClient.GetAsync($"/api/ships/{shipId}/updates/pending", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var updates = JsonSerializer.Deserialize<List<UpdateRequest>>(json, _jsonOptions);
                
                _logger.LogDebug("Retrieved {Count} pending updates for ship {ShipId}", updates?.Count ?? 0, shipId);
                return updates ?? new List<UpdateRequest>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogTrace("No pending updates found for ship {ShipId}", shipId);
                return new List<UpdateRequest>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to get pending updates for ship {ShipId}. Status: {StatusCode}, Error: {Error}", 
                    shipId, response.StatusCode, errorContent);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogTrace(ex, "No internet connection available for update check");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending updates for ship {ShipId}", shipId);
            return null;
        }
    }

    public async Task<bool> SendUpdateStatusAsync(string shipId, string updateId, UpdateStatusRequest status, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(status, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending update status for ship {ShipId}, update {UpdateId}: {Status}", 
                shipId, updateId, status.Status);
            
            var response = await _httpClient.PostAsync($"/api/ships/{shipId}/updates/{updateId}/status", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Update status sent successfully for update {UpdateId}", updateId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to send update status for update {UpdateId}. Status: {StatusCode}, Error: {Error}", 
                    updateId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No internet connection available to send update status");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending update status for update {UpdateId}", updateId);
            return false;
        }
    }

    public async Task<bool> SendHealthMetricsAsync(string shipId, HealthMetrics metrics, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(metrics, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogTrace("Sending health metrics for ship {ShipId}", shipId);
            
            var response = await _httpClient.PostAsync($"/api/ships/{shipId}/health", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogTrace("Health metrics sent successfully for ship {ShipId}", shipId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Failed to send health metrics for ship {ShipId}. Status: {StatusCode}, Error: {Error}", 
                    shipId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogTrace(ex, "No internet connection available to send health metrics");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending health metrics for ship {ShipId}", shipId);
            return false;
        }
    }

    public async Task<byte[]?> DownloadUpdatePackageAsync(string packageUrl, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⬇️ Downloading update package from {PackageUrl}", packageUrl);
            
            using var response = await _httpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to download update package. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            
            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var downloadedBytes = 0L;
            
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memoryStream = new MemoryStream();
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                downloadedBytes += bytesRead;
                
                progress?.Report(new DownloadProgress
                {
                    TotalBytes = totalBytes,
                    DownloadedBytes = downloadedBytes,
                    PercentComplete = totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100 : 0
                });
            }
            
            _logger.LogInformation("✅ Update package downloaded successfully ({TotalMB:F2} MB)", 
                memoryStream.Length / 1024.0 / 1024.0);
            
            return memoryStream.ToArray();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No internet connection available for package download");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading update package from {PackageUrl}", packageUrl);
            return null;
        }
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ShipId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class DownloadProgress
{
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public double PercentComplete { get; set; }
}
