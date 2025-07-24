using Microsoft.AspNetCore.SignalR;

namespace ShoreCommandCenter.Services;

public class FleetMonitoringHub : Hub
{
    private readonly ILogger<FleetMonitoringHub> _logger;

    public FleetMonitoringHub(ILogger<FleetMonitoringHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected to FleetMonitoringHub: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from FleetMonitoringHub: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with exception: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    // Client can join specific groups for targeted notifications
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    // Clients can subscribe to specific ship updates
    public async Task SubscribeToShip(string shipId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ship-{shipId}");
        _logger.LogDebug("Client {ConnectionId} subscribed to ship {ShipId}", Context.ConnectionId, shipId);
    }

    public async Task UnsubscribeFromShip(string shipId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ship-{shipId}");
        _logger.LogDebug("Client {ConnectionId} unsubscribed from ship {ShipId}", Context.ConnectionId, shipId);
    }

    // Clients can subscribe to deployment updates
    public async Task SubscribeToDeployment(string deploymentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"deployment-{deploymentId}");
        _logger.LogDebug("Client {ConnectionId} subscribed to deployment {DeploymentId}", Context.ConnectionId, deploymentId);
    }

    public async Task UnsubscribeFromDeployment(string deploymentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"deployment-{deploymentId}");
        _logger.LogDebug("Client {ConnectionId} unsubscribed from deployment {DeploymentId}", Context.ConnectionId, deploymentId);
    }
}
