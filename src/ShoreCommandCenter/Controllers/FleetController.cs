using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShoreCommandCenter.Models;
using ShoreCommandCenter.Services;

namespace ShoreCommandCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FleetController : ControllerBase
{
    private readonly IFleetService _fleetService;
    private readonly IDeploymentService _deploymentService;
    private readonly ILogger<FleetController> _logger;

    public FleetController(
        IFleetService fleetService,
        IDeploymentService deploymentService,
        ILogger<FleetController> logger)
    {
        _fleetService = fleetService;
        _deploymentService = deploymentService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetFleetOverview()
    {
        try
        {
            var overview = await _fleetService.GetFleetOverviewAsync();
            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get fleet overview");
            return StatusCode(500, new { error = "Failed to retrieve fleet overview" });
        }
    }

    [HttpPost("deploy")]
    public async Task<IActionResult> DeployToFleet([FromBody] FleetDeploymentRequest request)
    {
        try
        {
            _logger.LogInformation("Fleet deployment request: {ContainerImage} to {ShipCount} ships", 
                request.ContainerImage, request.ShipFilter?.IncludeShips?.Length ?? 0);
            
            var fleetDeployment = await _deploymentService.CreateFleetDeploymentAsync(request);
            
            _logger.LogInformation("Fleet deployment {FleetDeploymentId} created", fleetDeployment.Id);
            return Ok(fleetDeployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create fleet deployment");
            return BadRequest(new { error = "Failed to create fleet deployment", message = ex.Message });
        }
    }

    [HttpPost("rollback")]
    public async Task<IActionResult> FleetRollback([FromBody] FleetRollbackRequest request)
    {
        try
        {
            _logger.LogWarning("Fleet rollback request to version {TargetVersion}. Reason: {Reason}", 
                request.TargetVersion, request.Reason);
            
            var rollbackDeployment = await _deploymentService.CreateFleetRollbackAsync(request);
            
            return Ok(rollbackDeployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create fleet rollback");
            return BadRequest(new { error = "Failed to create fleet rollback", message = ex.Message });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetFleetHealth()
    {
        try
        {
            var health = await _fleetService.GetFleetHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get fleet health");
            return StatusCode(500, new { error = "Failed to retrieve fleet health" });
        }
    }

    [HttpGet("deployments/active")]
    public async Task<IActionResult> GetActiveDeployments()
    {
        try
        {
            var deployments = await _deploymentService.GetActiveDeploymentsAsync();
            return Ok(deployments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active deployments");
            return StatusCode(500, new { error = "Failed to retrieve active deployments" });
        }
    }

    [HttpGet("alerts/critical")]
    public async Task<IActionResult> GetCriticalAlerts()
    {
        try
        {
            var alerts = await _fleetService.GetCriticalAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get critical alerts");
            return StatusCode(500, new { error = "Failed to retrieve critical alerts" });
        }
    }
}

public class FleetRollbackRequest
{
    public string Reason { get; set; } = string.Empty;
    public string TargetVersion { get; set; } = string.Empty;
    public bool Emergency { get; set; }
    public ShipFilterCriteria? ShipFilter { get; set; }
}
