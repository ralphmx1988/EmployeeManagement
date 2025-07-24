namespace ShoreCommandCenter.Models;

// Additional DTOs for various operations
public class FleetStatus
{
    public int TotalShips { get; set; }
    public int OnlineShips { get; set; }
    public int OfflineShips { get; set; }
    public int ActiveDeployments { get; set; }
    public Dictionary<string, int> VersionDistribution { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class FleetHealthSummary
{
    public int TotalShips { get; set; }
    public int OnlineShips { get; set; }
    public int OfflineShips { get; set; }
    public int WarningShips { get; set; }
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public double AverageDiskUsage { get; set; }
    public int ActiveDeployments { get; set; }
    public int RecentDeployments { get; set; }
    public int FailedDeployments { get; set; }
    public DateTime Timestamp { get; set; }
}

public class FleetMetricsSummary
{
    public int TotalShips { get; set; }
    public int ShipsWithMetrics { get; set; }
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public double AverageDiskUsage { get; set; }
    public int HealthyShips { get; set; }
    public int UnhealthyShips { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class DeploymentProgress
{
    public string FleetDeploymentId { get; set; } = string.Empty;
    public int TotalShips { get; set; }
    public int CompletedShips { get; set; }
    public int FailedShips { get; set; }
    public int InProgressShips { get; set; }
    public int PendingShips { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public bool IsComplete { get; set; }
    public List<ShipDeploymentStatus> Ships { get; set; } = new();
}

public class ShipDeploymentStatus
{
    public string ShipId { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ContainerRegistryInfo
{
    public string RegistryUrl { get; set; } = string.Empty;
    public string RegistryType { get; set; } = string.Empty;
    public bool IsSecure { get; set; }
    public int TotalUpdateRequests { get; set; }
    public int PendingUpdates { get; set; }
    public int RecentUpdates { get; set; }
    public List<string> AvailableVersions { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

public class UpdateRequestDto
{
    public string Version { get; set; } = string.Empty;
    public string ContainerImage { get; set; } = string.Empty;
    public string? ConfigurationJson { get; set; }
    public int Priority { get; set; } = 1;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class HealthMetricsRequest
{
    public double? CpuUsage { get; set; }
    public double? MemoryUsage { get; set; }
    public double? DiskUsage { get; set; }
    public string NetworkStatus { get; set; } = "Unknown";
    public string ContainerStatus { get; set; } = "Unknown";
    public string DatabaseStatus { get; set; } = "Unknown";
    public string? AdditionalMetricsJson { get; set; }
}
