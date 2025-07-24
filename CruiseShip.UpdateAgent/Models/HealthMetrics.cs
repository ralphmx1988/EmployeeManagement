using System.Text.Json.Serialization;

namespace CruiseShip.UpdateAgent.Models;

public class HealthMetrics
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("shipId")]
    public string ShipId { get; set; } = string.Empty;
    
    [JsonPropertyName("system")]
    public SystemMetrics System { get; set; } = new();
    
    [JsonPropertyName("containers")]
    public List<ContainerMetrics> Containers { get; set; } = new();
    
    [JsonPropertyName("network")]
    public NetworkMetrics Network { get; set; } = new();
    
    [JsonPropertyName("updateAgent")]
    public UpdateAgentMetrics UpdateAgent { get; set; } = new();
}

public class SystemMetrics
{
    [JsonPropertyName("cpuPercent")]
    public double CpuPercent { get; set; }
    
    [JsonPropertyName("memoryPercent")]
    public double MemoryPercent { get; set; }
    
    [JsonPropertyName("memoryUsedMB")]
    public long MemoryUsedMB { get; set; }
    
    [JsonPropertyName("memoryTotalMB")]
    public long MemoryTotalMB { get; set; }
    
    [JsonPropertyName("diskPercent")]
    public double DiskPercent { get; set; }
    
    [JsonPropertyName("diskUsedGB")]
    public long DiskUsedGB { get; set; }
    
    [JsonPropertyName("diskTotalGB")]
    public long DiskTotalGB { get; set; }
    
    [JsonPropertyName("uptime")]
    public TimeSpan Uptime { get; set; }
}

public class ContainerMetrics
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
    
    [JsonPropertyName("cpuPercent")]
    public double CpuPercent { get; set; }
    
    [JsonPropertyName("memoryUsageMB")]
    public long MemoryUsageMB { get; set; }
    
    [JsonPropertyName("memoryLimitMB")]
    public long MemoryLimitMB { get; set; }
    
    [JsonPropertyName("networkRxMB")]
    public long NetworkRxMB { get; set; }
    
    [JsonPropertyName("networkTxMB")]
    public long NetworkTxMB { get; set; }
    
    [JsonPropertyName("uptime")]
    public TimeSpan Uptime { get; set; }
}

public class NetworkMetrics
{
    [JsonPropertyName("isConnected")]
    public bool IsConnected { get; set; }
    
    [JsonPropertyName("connectionType")]
    public string ConnectionType { get; set; } = string.Empty;
    
    [JsonPropertyName("latencyMs")]
    public int LatencyMs { get; set; }
    
    [JsonPropertyName("bandwidthMbps")]
    public double BandwidthMbps { get; set; }
    
    [JsonPropertyName("lastConnected")]
    public DateTime? LastConnected { get; set; }
}

public class UpdateAgentMetrics
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("lastUpdateCheck")]
    public DateTime? LastUpdateCheck { get; set; }
    
    [JsonPropertyName("pendingUpdates")]
    public int PendingUpdates { get; set; }
    
    [JsonPropertyName("successfulUpdates")]
    public int SuccessfulUpdates { get; set; }
    
    [JsonPropertyName("failedUpdates")]
    public int FailedUpdates { get; set; }
}
