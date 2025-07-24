namespace CruiseShip.UpdateAgent.Models;

public class ShipConfiguration
{
    public string ShipId { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public int CheckIntervalSeconds { get; set; } = 300;
    public int HealthCheckIntervalSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;
    public int RollbackTimeoutMinutes { get; set; } = 10;
    public CommunicationConfig Communication { get; set; } = new();
    public List<MaintenanceWindow> MaintenanceWindows { get; set; } = new();
    public DeploymentConfig Deployment { get; set; } = new();
    public MonitoringConfig Monitoring { get; set; } = new();
}

public class CommunicationConfig
{
    public string ShoreEndpoint { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string CaCertPath { get; set; } = string.Empty;
}

public class DeploymentConfig
{
    public bool AutoApply { get; set; } = true;
    public bool RequireManualApproval { get; set; } = false;
    public int BackupRetentionDays { get; set; } = 30;
}

public class MonitoringConfig
{
    public bool Enabled { get; set; } = true;
    public int ReportIntervalSeconds { get; set; } = 300;
    public List<string> MetricsToCollect { get; set; } = new();
}
