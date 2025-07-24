using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CruiseShip.UpdateAgent.Interfaces;
using CruiseShip.UpdateAgent.Models;
using System.Diagnostics;
using System.Management;

namespace CruiseShip.UpdateAgent.Services;

public class HealthMonitor : IHealthMonitor
{
    private readonly ILogger<HealthMonitor> _logger;
    private readonly ShipConfiguration _config;
    private readonly IDockerService _dockerService;

    public HealthMonitor(ILogger<HealthMonitor> logger, IOptions<ShipConfiguration> config, IDockerService dockerService)
    {
        _logger = logger;
        _config = config.Value;
        _dockerService = dockerService;
    }

    public async Task<HealthMetrics> CollectHealthMetricsAsync()
    {
        _logger.LogTrace("Collecting health metrics for ship {ShipId}", _config.ShipId);

        var metrics = new HealthMetrics
        {
            ShipId = _config.ShipId,
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = await GetCpuUsageAsync(),
            MemoryUsagePercent = GetMemoryUsage(),
            DiskSpaceAvailableGB = GetAvailableDiskSpace(),
            DockerStatus = await GetDockerStatusAsync(),
            RunningContainers = await GetRunningContainersStatusAsync(),
            SystemUptime = GetSystemUptime(),
            NetworkConnectivity = await CheckNetworkConnectivityAsync()
        };

        _logger.LogTrace("Health metrics collected: CPU={CpuUsage:F1}%, Memory={MemoryUsage:F1}%, Disk={DiskSpace:F1}GB", 
            metrics.CpuUsagePercent, metrics.MemoryUsagePercent, metrics.DiskSpaceAvailableGB);

        return metrics;
    }

    private async Task<double> GetCpuUsageAsync()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "cpu get loadpercentage /value",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var lines = output.Split('\n');
            var loadLine = lines.FirstOrDefault(l => l.StartsWith("LoadPercentage="));
            
            if (loadLine != null && double.TryParse(loadLine.Split('=')[1].Trim(), out var cpuUsage))
            {
                return cpuUsage;
            }

            // Fallback method using PerformanceCounter
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // First call always returns 0
            await Task.Delay(1000); // Wait 1 second for accurate reading
            return cpuCounter.NextValue();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get CPU usage");
            return 0;
        }
    }

    private double GetMemoryUsage()
    {
        try
        {
            var totalMemory = GC.GetTotalMemory(false);
            var workingSet = Environment.WorkingSet;
            
            // Get total physical memory using WMI
            using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            using var results = searcher.Get();
            
            var totalPhysicalMemory = 0UL;
            foreach (ManagementObject result in results)
            {
                totalPhysicalMemory = (ulong)result["TotalPhysicalMemory"];
                break;
            }

            if (totalPhysicalMemory > 0)
            {
                var memoryInfo = new PerformanceCounter("Memory", "Available Bytes");
                var availableMemory = memoryInfo.NextValue();
                var usedMemory = totalPhysicalMemory - (ulong)availableMemory;
                
                return (double)usedMemory / totalPhysicalMemory * 100;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get memory usage");
            return 0;
        }
    }

    private double GetAvailableDiskSpace()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .ToList();

            if (drives.Any())
            {
                var primaryDrive = drives.First();
                return (double)primaryDrive.AvailableFreeSpace / 1024 / 1024 / 1024; // Convert to GB
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get disk space");
            return 0;
        }
    }

    private async Task<string> GetDockerStatusAsync()
    {
        try
        {
            var isRunning = await _dockerService.IsDockerRunningAsync();
            return isRunning ? "running" : "stopped";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Docker status");
            return "unknown";
        }
    }

    private async Task<List<ContainerHealthInfo>> GetRunningContainersStatusAsync()
    {
        try
        {
            var containers = await _dockerService.GetRunningContainersAsync();
            
            return containers.Select(c => new ContainerHealthInfo
            {
                Id = c.Id,
                Name = c.Name,
                Image = c.Image,
                Status = c.Status,
                HealthStatus = DetermineContainerHealth(c),
                Uptime = ExtractUptimeFromStatus(c.Status)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get running containers status");
            return new List<ContainerHealthInfo>();
        }
    }

    private string DetermineContainerHealth(ContainerInfo container)
    {
        if (container.Status.Contains("healthy", StringComparison.OrdinalIgnoreCase))
            return "healthy";
        
        if (container.Status.Contains("unhealthy", StringComparison.OrdinalIgnoreCase))
            return "unhealthy";
        
        if (container.Status.StartsWith("Up", StringComparison.OrdinalIgnoreCase))
            return "running";
        
        return "unknown";
    }

    private TimeSpan ExtractUptimeFromStatus(string status)
    {
        try
        {
            // Docker status format: "Up 2 hours", "Up 3 minutes", "Up 1 day", etc.
            if (status.StartsWith("Up ", StringComparison.OrdinalIgnoreCase))
            {
                var uptimePart = status.Substring(3);
                
                if (uptimePart.Contains("second"))
                {
                    var seconds = ExtractNumber(uptimePart);
                    return TimeSpan.FromSeconds(seconds);
                }
                else if (uptimePart.Contains("minute"))
                {
                    var minutes = ExtractNumber(uptimePart);
                    return TimeSpan.FromMinutes(minutes);
                }
                else if (uptimePart.Contains("hour"))
                {
                    var hours = ExtractNumber(uptimePart);
                    return TimeSpan.FromHours(hours);
                }
                else if (uptimePart.Contains("day"))
                {
                    var days = ExtractNumber(uptimePart);
                    return TimeSpan.FromDays(days);
                }
            }
            
            return TimeSpan.Zero;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    private int ExtractNumber(string text)
    {
        var numbers = new string(text.Where(char.IsDigit).ToArray());
        return int.TryParse(numbers, out var result) ? result : 0;
    }

    private TimeSpan GetSystemUptime()
    {
        try
        {
            return TimeSpan.FromMilliseconds(Environment.TickCount64);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get system uptime");
            return TimeSpan.Zero;
        }
    }

    private async Task<bool> CheckNetworkConnectivityAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            // Try to reach a reliable endpoint
            var response = await client.GetAsync("https://www.google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // Also try shore command center
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
                var response = await client.GetAsync(_config.ShoreCommandUrl);
                return true; // If we can reach shore command, we have connectivity
            }
            catch
            {
                return false;
            }
        }
    }
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
