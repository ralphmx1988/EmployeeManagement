using System.ComponentModel.DataAnnotations;

namespace ShoreCommandCenter.Models;

public class Ship
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Offline";
    public DateTime? LastSeen { get; set; }
    public string? CurrentVersion { get; set; }
    public string? TargetVersion { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ConfigurationJson { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
}

public class Deployment
{
    public Guid Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string ContainerImage { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime? ScheduledFor { get; set; }
    public DateTime? DeployedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ContainerName { get; set; }
    public string? ContainerConfig { get; set; }
    public string? Description { get; set; }
    public bool IsEmergency { get; set; }
}

public class FleetDeployment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContainerImage { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? ShipFilter { get; set; }
    public string Status { get; set; } = "Planning";
    public DateTime? ScheduledFor { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RolloutStrategy { get; set; }
    public int? BatchSize { get; set; }
    public TimeSpan? DelayBetweenBatches { get; set; }
}

public class ShipMetrics
{
    public long Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal? CpuPercent { get; set; }
    public decimal? MemoryPercent { get; set; }
    public decimal? DiskPercent { get; set; }
    public int? ContainerCount { get; set; }
    public string? MetricsJson { get; set; }
    public bool NetworkConnectivity { get; set; }
    public string? DockerStatus { get; set; }
    public TimeSpan? SystemUptime { get; set; }
}

public class UpdateRequest
{
    public Guid Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string PackageUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // Normal, High, Emergency
    public string Status { get; set; } = "Pending"; // Pending, Downloaded, Applied, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Description { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresMaintenanceWindow { get; set; } = true;
}

public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? ShipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Permissions { get; set; }
}

// DTOs
public class ShipRegistrationRequest
{
    [Required]
    public string ShipId { get; set; } = string.Empty;
    
    [Required]
    public string ShipName { get; set; } = string.Empty;
    
    public string? AgentVersion { get; set; }
    public DateTime LastSeen { get; set; }
    public string[]? Capabilities { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
}

public class DeploymentRequest
{
    [Required]
    public string ContainerImage { get; set; } = string.Empty;
    
    [Required]
    public string ContainerName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public ContainerConfiguration? ContainerConfig { get; set; }
    public bool IsEmergency { get; set; }
}

public class ContainerConfiguration
{
    public string[]? Environment { get; set; }
    public Dictionary<string, object>? Ports { get; set; }
    public string[]? Networks { get; set; }
    public RestartPolicy? RestartPolicy { get; set; }
    public Dictionary<string, string>? Volumes { get; set; }
}

public class RestartPolicy
{
    public string Name { get; set; } = string.Empty;
    public int MaximumRetryCount { get; set; }
}

public class FleetDeploymentRequest
{
    [Required]
    public string ContainerImage { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public ShipFilterCriteria? ShipFilter { get; set; }
    public RolloutStrategy? RolloutStrategy { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public ContainerConfiguration? ContainerConfig { get; set; }
}

public class ShipFilterCriteria
{
    public string[]? Regions { get; set; }
    public string[]? ExcludeShips { get; set; }
    public string? MinVersion { get; set; }
    public string? MaxVersion { get; set; }
    public string[]? IncludeShips { get; set; }
    public string[]? ShipTypes { get; set; }
}

public class RolloutStrategy
{
    public string Type { get; set; } = "rolling"; // rolling, canary, blue-green
    public int BatchSize { get; set; } = 1;
    public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.FromHours(2);
    public int MaxFailuresPerBatch { get; set; } = 0;
}

public class HealthMetricsRequest
{
    public string ShipId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskSpaceAvailableGB { get; set; }
    public string DockerStatus { get; set; } = string.Empty;
    public ContainerHealthInfo[]? RunningContainers { get; set; }
    public TimeSpan SystemUptime { get; set; }
    public bool NetworkConnectivity { get; set; }
    public DateTime? LastUpdateCheck { get; set; }
    public DateTime? LastSuccessfulUpdate { get; set; }
}

public class ContainerHealthInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
}

public class FleetStatusResponse
{
    public int TotalShips { get; set; }
    public int OnlineShips { get; set; }
    public int PendingDeployments { get; set; }
    public int FailedDeployments { get; set; }
    public ShipStatusSummary[] Ships { get; set; } = Array.Empty<ShipStatusSummary>();
}

public class ShipStatusSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastSeen { get; set; }
    public string? CurrentVersion { get; set; }
    public int PendingUpdates { get; set; }
    public string? Location { get; set; }
    public HealthMetricsSummary? HealthMetrics { get; set; }
}

public class HealthMetricsSummary
{
    public double? CpuUsagePercent { get; set; }
    public double? MemoryUsagePercent { get; set; }
    public double? DiskSpaceAvailableGB { get; set; }
    public int? ContainerCount { get; set; }
    public bool NetworkConnectivity { get; set; }
}
