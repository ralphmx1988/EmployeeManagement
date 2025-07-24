using System.Text.Json.Serialization;

namespace CruiseShip.UpdateAgent.Models;

public class UpdateRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("containerImage")]
    public string ContainerImage { get; set; } = string.Empty;
    
    [JsonPropertyName("containerName")]
    public string ContainerName { get; set; } = string.Empty;
    
    [JsonPropertyName("containerConfig")]
    public ContainerConfig ContainerConfig { get; set; } = new();
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "normal";
    
    [JsonPropertyName("scheduledFor")]
    public DateTime? ScheduledFor { get; set; }
    
    [JsonPropertyName("rollbackVersion")]
    public string? RollbackVersion { get; set; }
}

public class ContainerConfig
{
    [JsonPropertyName("environment")]
    public List<string> Environment { get; set; } = new();
    
    [JsonPropertyName("ports")]
    public Dictionary<string, object> Ports { get; set; } = new();
    
    [JsonPropertyName("volumes")]
    public List<string> Volumes { get; set; } = new();
    
    [JsonPropertyName("networks")]
    public List<string> Networks { get; set; } = new();
    
    [JsonPropertyName("restartPolicy")]
    public RestartPolicy RestartPolicy { get; set; } = new();
    
    [JsonPropertyName("healthCheck")]
    public HealthCheckConfig? HealthCheck { get; set; }
}

public class RestartPolicy
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "unless-stopped";
    
    [JsonPropertyName("MaximumRetryCount")]
    public int MaximumRetryCount { get; set; } = 3;
}

public class HealthCheckConfig
{
    [JsonPropertyName("test")]
    public List<string> Test { get; set; } = new();
    
    [JsonPropertyName("interval")]
    public string Interval { get; set; } = "30s";
    
    [JsonPropertyName("timeout")]
    public string Timeout { get; set; } = "10s";
    
    [JsonPropertyName("retries")]
    public int Retries { get; set; } = 3;
    
    [JsonPropertyName("startPeriod")]
    public string StartPeriod { get; set; } = "60s";
}

public class UpdateStatusRequest
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("shipId")]
    public string ShipId { get; set; } = string.Empty;
    
    [JsonPropertyName("details")]
    public Dictionary<string, object>? Details { get; set; }
}
