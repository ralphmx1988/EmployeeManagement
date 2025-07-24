using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CruiseShip.UpdateAgent.Services;
using CruiseShip.UpdateAgent.Interfaces;
using CruiseShip.UpdateAgent.Models;

namespace CruiseShip.UpdateAgent;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        // Log startup information
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("🚢 Cruise Ship Update Agent starting...");
        
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "CruiseShipUpdateAgent";
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddEventLog();
                
                // Add file logging
                logging.AddFile("C:\\CruiseShip\\logs\\update-agent-{Date}.log");
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Configuration
                services.Configure<ShipConfiguration>(
                    hostContext.Configuration.GetSection("ShipConfig"));
                
                // HTTP Clients
                services.AddHttpClient<IShoreApiClient, ShoreApiClient>(client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    client.DefaultRequestHeaders.Add("User-Agent", "CruiseShip-UpdateAgent/1.0");
                });
                
                // Core Services
                services.AddSingleton<IDockerService, DockerService>();
                services.AddSingleton<IHealthMonitor, HealthMonitor>();
                services.AddSingleton<IUpdateOrchestrator, UpdateOrchestrator>();
                
                // Background Services
                services.AddHostedService<UpdateReceiverService>();
                services.AddHostedService<HealthMonitorService>();
            });
}
