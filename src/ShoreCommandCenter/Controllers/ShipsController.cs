using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShoreCommandCenter.Models;
using ShoreCommandCenter.Services;

namespace ShoreCommandCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShipsController : ControllerBase
{
    private readonly IShipService _shipService;
    private readonly IDeploymentService _deploymentService;
    private readonly IHealthMetricsService _healthMetricsService;
    private readonly ILogger<ShipsController> _logger;

    public ShipsController(
        IShipService shipService,
        IDeploymentService deploymentService,
        IHealthMetricsService healthMetricsService,
        ILogger<ShipsController> logger)
    {
        _shipService = shipService;
        _deploymentService = deploymentService;
        _healthMetricsService = healthMetricsService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous] // Ships need to register before getting auth
    public async Task<IActionResult> RegisterShip([FromBody] ShipRegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Ship registration request for {ShipId}: {ShipName}", request.ShipId, request.ShipName);
            
            var ship = await _shipService.RegisterShipAsync(request);
            
            _logger.LogInformation("Ship {ShipId} registered successfully", request.ShipId);
            return Ok(new { status = "registered", shipId = request.ShipId, ship });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register ship {ShipId}", request.ShipId);
            return BadRequest(new { error = "Failed to register ship", message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllShips()
    {
        try
        {
            var ships = await _shipService.GetAllShipsAsync();
            return Ok(ships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ships list");
            return StatusCode(500, new { error = "Failed to retrieve ships" });
        }
    }

    [HttpGet("{shipId}")]
    public async Task<IActionResult> GetShip(string shipId)
    {
        try
        {
            var ship = await _shipService.GetShipAsync(shipId);
            if (ship == null)
                return NotFound(new { error = "Ship not found" });

            return Ok(ship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ship {ShipId}", shipId);
            return StatusCode(500, new { error = "Failed to retrieve ship" });
        }
    }

    [HttpGet("{shipId}/updates/pending")]
    public async Task<IActionResult> GetPendingUpdates(string shipId)
    {
        try
        {
            var updates = await _deploymentService.GetPendingUpdatesAsync(shipId);
            return Ok(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending updates for ship {ShipId}", shipId);
            return StatusCode(500, new { error = "Failed to retrieve pending updates" });
        }
    }

    [HttpPost("{shipId}/deploy")]
    public async Task<IActionResult> DeployToShip(string shipId, [FromBody] DeploymentRequest request)
    {
        try
        {
            _logger.LogInformation("Deployment request for ship {ShipId}: {ContainerImage}", shipId, request.ContainerImage);
            
            var deployment = await _deploymentService.CreateDeploymentAsync(shipId, request);
            
            _logger.LogInformation("Deployment {DeploymentId} created for ship {ShipId}", deployment.Id, shipId);
            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create deployment for ship {ShipId}", shipId);
            return BadRequest(new { error = "Failed to create deployment", message = ex.Message });
        }
    }

    [HttpPost("{shipId}/updates/{updateId}/status")]
    public async Task<IActionResult> UpdateStatus(string shipId, string updateId, [FromBody] UpdateStatusRequest status)
    {
        try
        {
            await _deploymentService.UpdateDeploymentStatusAsync(Guid.Parse(updateId), status.Status, status.Message);
            
            _logger.LogInformation("Update status received for ship {ShipId}, update {UpdateId}: {Status}", 
                shipId, updateId, status.Status);
            
            return Ok(new { message = "Status updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status for ship {ShipId}, update {UpdateId}", shipId, updateId);
            return BadRequest(new { error = "Failed to update status", message = ex.Message });
        }
    }

    [HttpPost("{shipId}/health")]
    public async Task<IActionResult> ReceiveHealthMetrics(string shipId, [FromBody] HealthMetricsRequest metrics)
    {
        try
        {
            await _healthMetricsService.RecordHealthMetricsAsync(shipId, metrics);
            await _shipService.UpdateLastSeenAsync(shipId);
            
            _logger.LogTrace("Health metrics received for ship {ShipId}", shipId);
            return Ok(new { message = "Health metrics received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record health metrics for ship {ShipId}", shipId);
            return BadRequest(new { error = "Failed to record health metrics", message = ex.Message });
        }
    }

    [HttpPost("{shipId}/rollback")]
    public async Task<IActionResult> RollbackShip(string shipId, [FromBody] RollbackRequest request)
    {
        try
        {
            _logger.LogWarning("Rollback request for ship {ShipId} to version {TargetVersion}. Reason: {Reason}", 
                shipId, request.TargetVersion, request.Reason);
            
            var deployment = await _deploymentService.CreateRollbackAsync(shipId, request);
            
            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create rollback for ship {ShipId}", shipId);
            return BadRequest(new { error = "Failed to create rollback", message = ex.Message });
        }
    }

    [HttpPost("{shipId}/emergency-command")]
    public async Task<IActionResult> SendEmergencyCommand(string shipId, [FromBody] EmergencyCommandRequest request)
    {
        try
        {
            _logger.LogWarning("Emergency command for ship {ShipId}: {Command}. Reason: {Reason}", 
                shipId, request.Command, request.Reason);
            
            // In a real implementation, this would queue the command for the ship
            // For now, we'll just log it and return success
            
            return Ok(new { message = "Emergency command queued", commandId = Guid.NewGuid() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send emergency command to ship {ShipId}", shipId);
            return BadRequest(new { error = "Failed to send emergency command", message = ex.Message });
        }
    }

    [HttpGet("outdated")]
    public async Task<IActionResult> GetOutdatedShips()
    {
        try
        {
            var ships = await _shipService.GetOutdatedShipsAsync();
            return Ok(ships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get outdated ships");
            return StatusCode(500, new { error = "Failed to retrieve outdated ships" });
        }
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class RollbackRequest
{
    public string Reason { get; set; } = string.Empty;
    public string TargetVersion { get; set; } = string.Empty;
    public bool Emergency { get; set; }
}

public class EmergencyCommandRequest
{
    public string Command { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool RequiresConfirmation { get; set; }
}
